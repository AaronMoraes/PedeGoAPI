using System.Text.Json;

namespace GestaodePedidosAPI.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ArgumentException ex)
        {
            await Responder(context, StatusCodes.Status400BadRequest, ex.Message);
        }
        catch (BadHttpRequestException ex)
        {
            await Responder(context, ex.StatusCode, "Requisição inválida ou grande demais.");
        }
        catch (Exception ex)
        {
            // O detalhe fica só no log do servidor; o cliente nunca vê a mensagem interna.
            _logger.LogError(ex, "Erro não tratado em {Metodo} {Caminho}",
                context.Request.Method, context.Request.Path);

            await Responder(context, StatusCodes.Status500InternalServerError,
                "Ocorreu um erro interno no servidor.");
        }
    }

    private static async Task Responder(HttpContext context, int status, string mensagem)
    {
        if (context.Response.HasStarted)
            return;

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(JsonSerializer.Serialize(new { message = mensagem }));
    }
}
