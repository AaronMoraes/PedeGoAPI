using System.Text.Json.Serialization;

namespace GestaodePedidosAPI.Models;

public class ItemPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal Preco { get; set; }

    [JsonIgnore]
    public Pedido Pedido { get; set; } = null!;
    public Produto Produto { get; set; } = null!;
}

