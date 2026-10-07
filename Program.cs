using System.Text;
using System.Threading.RateLimiting;
using GestaodePedidosAPI.Data;
using GestaodePedidosAPI.Middleware;
using GestaodePedidosAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;

// Carrega o arquivo .env (se existir) sem sobrescrever variáveis que já estejam definidas no sistema.
CarregarEnv(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// ---------- Configuração obrigatória ----------
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Length < 32)
    throw new InvalidOperationException("JWT_KEY não configurada ou curta demais (mínimo de 32 caracteres).");

var dbPath = Environment.GetEnvironmentVariable("DB_PATH");
if (string.IsNullOrWhiteSpace(dbPath))
    dbPath = "gestaodepedidos.db";

var origensPermitidas = (Environment.GetEnvironmentVariable("ALLOWED_ORIGINS") ?? string.Empty)
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

// Ative BEHIND_PROXY=true somente se a API estiver atrás de um proxy reverso (Nginx, Caddy, Cloudflare...).
// Sem proxy, confiar em X-Forwarded-For permitiria falsificar o IP e burlar o limite de requisições.
var atrasDeProxy = string.Equals(Environment.GetEnvironmentVariable("BEHIND_PROXY"), "true", StringComparison.OrdinalIgnoreCase);

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
    builder.WebHost.UseUrls("http://0.0.0.0:5000");

// ---------- Serviços ----------
builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Erros de validação no mesmo formato { message } que o front já espera.
        options.InvalidModelStateResponseFactory = context =>
        {
            var mensagem = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Dados inválidos.";

            return new BadRequestObjectResult(new { message = mensagem });
        };
    });

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
            ValidateIssuer = true,
            ValidIssuer = JwtService.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtService.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();

// Em produção defina ALLOWED_ORIGINS (ex.: https://meusite.com.br). Sem ela, só localhost é aceito.
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        if (origensPermitidas.Length > 0)
            policy.WithOrigins(origensPermitidas);
        else
            policy.SetIsOriginAllowed(origin =>
                Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                (uri.Host == "localhost" || uri.Host == "127.0.0.1"));

        policy
            .WithHeaders("Content-Type", "Authorization")
            .WithMethods("GET", "POST", "PUT", "DELETE");
    });
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\":\"Muitas requisições. Aguarde um instante e tente novamente.\"}", token);
    };

    options.AddPolicy("login", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 10,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));

    options.AddPolicy("pedido", http => RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 15,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        }));
});

// Atrás de proxy, o IP real do cliente vem em X-Forwarded-For.
if (atrasDeProxy)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
}

builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

builder.Services.AddScoped<ProdutoService>();
builder.Services.AddScoped<PedidoService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<ImagemService>();
builder.Services.AddSingleton<LoginAttemptTracker>();

var app = builder.Build();

// ---------- Banco e admin inicial ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();

    var demo = Environment.GetEnvironmentVariable("CARDAPIO_DEMO");
    if (!string.Equals(demo, "false", StringComparison.OrdinalIgnoreCase))
        await CardapioInicial.PopularSeVazio(db);

    var adminService = scope.ServiceProvider.GetRequiredService<AdminService>();

    var usuario = Environment.GetEnvironmentVariable("ADMIN_USUARIO");
    var senha = Environment.GetEnvironmentVariable("ADMIN_SENHA");

    if (!string.IsNullOrWhiteSpace(usuario) && !string.IsNullOrWhiteSpace(senha))
        await adminService.CriarAdminInicial(usuario.Trim(), senha);
    else
        app.Logger.LogWarning("ADMIN_USUARIO/ADMIN_SENHA não definidos: nenhum admin será criado.");
}

if (origensPermitidas.Length == 0)
    app.Logger.LogWarning("ALLOWED_ORIGINS não definida: CORS liberado apenas para localhost.");

var pastaUploads = Path.GetFullPath(ImagemService.PastaUploads(app.Environment));
Directory.CreateDirectory(pastaUploads);

// ---------- Pipeline ----------
if (atrasDeProxy)
    app.UseForwardedHeaders();

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    if (context.Request.IsHttps)
        headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
    await next();
});

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("Frontend");

// Imagens enviadas pelo admin. Só esta pasta é pública.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(pastaUploads),
    RequestPath = "/uploads",
    OnPrepareResponse = ctx =>
    {
        ctx.Context.Response.Headers["X-Content-Type-Options"] = "nosniff";
        ctx.Context.Response.Headers["Content-Security-Policy"] = "default-src 'none'";
        ctx.Context.Response.Headers["Cache-Control"] = "public, max-age=604800, immutable";
    }
});

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "Gestão de Pedidos API funcionando!");

app.Run();

static void CarregarEnv(string caminho)
{
    if (!File.Exists(caminho))
        return;

    foreach (var linha in File.ReadAllLines(caminho))
    {
        var texto = linha.Trim();

        if (texto.Length == 0 || texto.StartsWith('#'))
            continue;

        var igual = texto.IndexOf('=');
        if (igual <= 0)
            continue;

        var chave = texto[..igual].Trim();
        var valor = texto[(igual + 1)..].Trim().Trim('"');

        if (Environment.GetEnvironmentVariable(chave) == null)
            Environment.SetEnvironmentVariable(chave, valor);
    }
}
