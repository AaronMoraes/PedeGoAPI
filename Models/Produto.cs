namespace GestaodePedidosAPI.Models;

public class Produto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }

    /// <summary>"lanche", "bebida" ou "combo".</summary>
    public string Categoria { get; set; } = string.Empty;

    /// <summary>Uma linha por item (ingredientes / itens do combo).</summary>
    public string Descricao { get; set; } = string.Empty;

    /// <summary>"imagens/..." (arquivos do front) ou "/uploads/..." (enviadas pelo admin).</summary>
    public string ImagemUrl { get; set; } = string.Empty;

    /// <summary>Selo opcional exibido no card (ex.: "Novo", "Mais Pedido").</summary>
    public string Selo { get; set; } = string.Empty;

    /// <summary>Aparece na seção "Mais pedidos" da home.</summary>
    public bool NaHome { get; set; }

    /// <summary>Produto visível no cardápio. Falso = fora do ar (esgotado/arquivado).</summary>
    public bool Ativo { get; set; } = true;
}
