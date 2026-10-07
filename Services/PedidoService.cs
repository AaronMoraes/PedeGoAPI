using GestaodePedidosAPI.Data;
using GestaodePedidosAPI.Models;
using GestaodePedidosAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GestaodePedidosAPI.Services;

public class PedidoService
{
    private const int MaximoDeItens = 50;
    private const int MaximoPorItem = 99;

    private readonly AppDbContext _context;

    public PedidoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Pedido>> ListarTodos()
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }

    public async Task<Pedido?> BuscarPorId(int id)
    {
        return await _context.Pedidos
            .Include(p => p.Itens)
            .ThenInclude(i => i.Produto)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Pedido> Criar(CriarPedidoDto dto)
    {
        if (dto.Itens == null || dto.Itens.Count == 0)
            throw new ArgumentException("O pedido deve possuir pelo menos um item.");

        if (dto.Itens.Count > MaximoDeItens)
            throw new ArgumentException("O pedido possui itens demais.");

        if (dto.FormaPagamento != "pix" && dto.FormaPagamento != "cartao")
            throw new ArgumentException("Forma de pagamento inválida.");

        if (dto.Status != "Pendente")
            throw new ArgumentException("Status do pedido inválido.");

        if (dto.Itens.Any(i => i.Quantidade <= 0))
            throw new ArgumentException("A quantidade deve ser maior que zero.");

        // Soma quantidades se o mesmo produto vier repetido.
        var agrupados = dto.Itens
            .GroupBy(i => i.ProdutoId)
            .Select(g => new { ProdutoId = g.Key, Quantidade = g.Sum(i => (long)i.Quantidade) })
            .ToList();

        if (agrupados.Any(i => i.Quantidade > MaximoPorItem))
            throw new ArgumentException($"A quantidade máxima por item é {MaximoPorItem}.");

        var ids = agrupados.Select(i => i.ProdutoId).ToList();

        var produtos = await _context.Produtos
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id);

        var pedido = new Pedido
        {
            Data = DateTime.UtcNow,
            Status = dto.Status,
            FormaPagamento = dto.FormaPagamento
        };

        foreach (var item in agrupados)
        {
            // O preço SEMPRE vem do banco, nunca do cliente.
            if (!produtos.TryGetValue(item.ProdutoId, out var produto) || !produto.Ativo)
                throw new ArgumentException("Um dos produtos do pedido não está mais disponível. Atualize o carrinho.");

            pedido.Itens.Add(new ItemPedido
            {
                ProdutoId = produto.Id,
                Produto = produto,
                Quantidade = (int)item.Quantidade,
                Preco = produto.Preco
            });
        }

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return pedido;
    }

    public async Task<bool> Excluir(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido == null)
            return false;

        _context.Pedidos.Remove(pedido);

        await _context.SaveChangesAsync();

        return true;
    }
}
