using Microsoft.Extensions.Caching.Memory;

namespace GestaodePedidosAPI.Services;

/// <summary>
/// Bloqueia o login de um usuário por 15 minutos após 5 senhas erradas seguidas.
/// Complementa o rate limit por IP (que pode ser contornado por quem troca de IP).
/// </summary>
public class LoginAttemptTracker
{
    private const int MaximoDeFalhas = 5;
    private static readonly TimeSpan Janela = TimeSpan.FromMinutes(15);

    private readonly IMemoryCache _cache;

    public LoginAttemptTracker(IMemoryCache cache)
    {
        _cache = cache;
    }

    private static string Chave(string? usuario)
    {
        var u = (usuario ?? string.Empty).Trim().ToLowerInvariant();
        if (u.Length > 64) u = u[..64];
        return "login:" + u;
    }

    public bool Bloqueado(string? usuario)
    {
        return _cache.TryGetValue(Chave(usuario), out int falhas) && falhas >= MaximoDeFalhas;
    }

    public void RegistrarFalha(string? usuario)
    {
        var chave = Chave(usuario);
        _cache.TryGetValue(chave, out int falhas);

        _cache.Set(chave, falhas + 1, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = Janela,
            Size = 1
        });
    }

    public void Limpar(string? usuario)
    {
        _cache.Remove(Chave(usuario));
    }
}
