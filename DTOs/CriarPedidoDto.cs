namespace GestaodePedidosAPI.DTOs;

public class CriarPedidoDto
{
    public string Status { get; set; } = string.Empty;
    public string FormaPagamento { get; set; } = string.Empty;
    public List<CriarItemPedidoDto> Itens { get; set; } = new();
}