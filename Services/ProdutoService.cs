using System.Text.RegularExpressions;
using GestaodePedidosAPI.Data;
using GestaodePedidosAPI.DTOs;
using GestaodePedidosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaodePedidosAPI.Services;

public class ProdutoService
{
    private static readonly string[] Categorias = { "lanche", "bebida", "combo" };

    // Só aceita imagens do próprio front ("imagens/...") ou enviadas pelo admin ("/uploads/<guid>.ext").
    private static readonly Regex ImagemValida = new(
        @"^(imagens/[A-Za-z0-9_\-\. /]+\.(jpg|jpeg|png|webp)|/uploads/[a-f0-9]{32}\.(jpg|png|webp))$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private readonly AppDbContext _context;

    public ProdutoService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Produto>> Listar(string? categoria, bool incluirInativos)
    {
        var query = _context.Produtos.AsNoTracking().AsQueryable();

        if (!incluirInativos)
            query = query.Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            var cat = categoria.Trim().ToLowerInvariant();
            query = query.Where(p => p.Categoria == cat);
        }

        return await query.OrderBy(p => p.Id).ToListAsync();
    }

    public async Task<Produto?> GetProdutoPorId(int id)
    {
        return await _context.Produtos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Produto> CriarProduto(ProdutoDto dto)
    {
        var produto = new Produto();
        Aplicar(produto, dto);

        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        return produto;
    }

    public async Task<Produto?> AtualizarProduto(int id, ProdutoDto dto)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return null;

        Aplicar(produto, dto);
        await _context.SaveChangesAsync();

        return produto;
    }

    /// <summary>
    /// Se o produto já foi vendido, ele é apenas arquivado (Ativo = false) para não destruir o
    /// histórico de pedidos. Caso contrário, é removido de vez.
    /// </summary>
    public async Task<(bool Encontrado, bool Arquivado)> ExcluirProduto(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);

        if (produto == null)
            return (false, false);

        var temPedidos = await _context.ItensPedido.AnyAsync(i => i.ProdutoId == id);

        if (temPedidos)
        {
            produto.Ativo = false;
            await _context.SaveChangesAsync();
            return (true, true);
        }

        _context.Produtos.Remove(produto);
        await _context.SaveChangesAsync();
        return (true, false);
    }

    private static void Aplicar(Produto produto, ProdutoDto dto)
    {
        var nome = dto.Nome.Trim();
        if (nome.Length < 2)
            throw new ArgumentException("O nome do produto é obrigatório.");

        if (dto.Preco <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.");

        var categoria = dto.Categoria.Trim().ToLowerInvariant();
        if (!Categorias.Contains(categoria))
            throw new ArgumentException("Categoria inválida. Use lanche, bebida ou combo.");

        var imagem = dto.ImagemUrl.Trim();
        if (imagem.Length > 0 && (imagem.Contains("..") || !ImagemValida.IsMatch(imagem)))
            throw new ArgumentException("Imagem inválida.");

        var linhas = dto.Descricao
            .Replace("\r", "")
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.Length > 0);

        produto.Nome = nome;
        produto.Preco = Math.Round(dto.Preco, 2);
        produto.Categoria = categoria;
        produto.Descricao = string.Join("\n", linhas);
        produto.ImagemUrl = imagem;
        produto.Selo = dto.Selo.Trim();
        produto.NaHome = dto.NaHome;
        produto.Ativo = dto.Ativo;
    }
}
