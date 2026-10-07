namespace GestaodePedidosAPI.Services;

/// <summary>
/// Salva imagens enviadas pelo admin. Não confia no nome nem no Content-Type enviados:
/// o formato é detectado pelos primeiros bytes do arquivo e o nome final é sempre um GUID.
/// </summary>
public class ImagemService
{
    public const long TamanhoMaximo = 3 * 1024 * 1024;

    private readonly string _pasta;

    public ImagemService(IWebHostEnvironment env)
    {
        _pasta = PastaUploads(env);
    }

    public static string PastaUploads(IWebHostEnvironment env)
    {
        var configurada = Environment.GetEnvironmentVariable("UPLOADS_PATH");

        return string.IsNullOrWhiteSpace(configurada)
            ? Path.Combine(env.ContentRootPath, "uploads")
            : configurada;
    }

    public async Task<string> Salvar(IFormFile? arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            throw new ArgumentException("Selecione uma imagem.");

        if (arquivo.Length > TamanhoMaximo)
            throw new ArgumentException("A imagem deve ter no máximo 3 MB.");

        using var memoria = new MemoryStream();
        await arquivo.CopyToAsync(memoria);
        var bytes = memoria.ToArray();

        var extensao = DetectarExtensao(bytes);
        if (extensao == null)
            throw new ArgumentException("Formato inválido. Envie uma imagem JPG, PNG ou WebP.");

        Directory.CreateDirectory(_pasta);

        var nome = Guid.NewGuid().ToString("N") + extensao;
        await File.WriteAllBytesAsync(Path.Combine(_pasta, nome), bytes);

        return "/uploads/" + nome;
    }

    private static string? DetectarExtensao(byte[] b)
    {
        if (b.Length >= 3 && b[0] == 0xFF && b[1] == 0xD8 && b[2] == 0xFF)
            return ".jpg";

        if (b.Length >= 8 && b[0] == 0x89 && b[1] == 0x50 && b[2] == 0x4E && b[3] == 0x47 &&
            b[4] == 0x0D && b[5] == 0x0A && b[6] == 0x1A && b[7] == 0x0A)
            return ".png";

        if (b.Length >= 12 && b[0] == 0x52 && b[1] == 0x49 && b[2] == 0x46 && b[3] == 0x46 &&
            b[8] == 0x57 && b[9] == 0x45 && b[10] == 0x42 && b[11] == 0x50)
            return ".webp";

        return null;
    }
}
