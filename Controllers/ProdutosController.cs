using GestaodePedidosAPI.DTOs;
using GestaodePedidosAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestaodePedidosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly ProdutoService _produtoService;
    private readonly ImagemService _imagemService;

    public ProdutosController(ProdutoService produtoService, ImagemService imagemService)
    {
        _produtoService = produtoService;
        _imagemService = imagemService;
    }

    // ----- Público: cardápio (somente produtos ativos) -----

    [HttpGet]
    public async Task<IActionResult> GetProdutos([FromQuery] string? categoria)
    {
        return Ok(await _produtoService.Listar(categoria, incluirInativos: false));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetProdutoPorId(int id)
    {
        var produto = await _produtoService.GetProdutoPorId(id);

        if (produto == null || !produto.Ativo)
            return NotFound();

        return Ok(produto);
    }

    // ----- Admin -----

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetProdutosAdmin()
    {
        return Ok(await _produtoService.Listar(null, incluirInativos: true));
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CriarProduto(ProdutoDto produtoDto)
    {
        var produto = await _produtoService.CriarProduto(produtoDto);

        return CreatedAtAction(
            nameof(GetProdutoPorId),
            new { id = produto.Id },
            produto
        );
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AtualizarProduto(int id, ProdutoDto produtoDto)
    {
        var produto = await _produtoService.AtualizarProduto(id, produtoDto);

        if (produto == null)
            return NotFound();

        return Ok(produto);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ExcluirProduto(int id)
    {
        var (encontrado, arquivado) = await _produtoService.ExcluirProduto(id);

        if (!encontrado)
            return NotFound();

        return Ok(new
        {
            arquivado,
            message = arquivado
                ? "Este produto já foi vendido, então foi removido do cardápio e arquivado para preservar o histórico de pedidos."
                : "Produto excluído."
        });
    }

    [HttpPost("imagem")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(4 * 1024 * 1024)]
    public async Task<IActionResult> EnviarImagem(IFormFile? arquivo)
    {
        var url = await _imagemService.Salvar(arquivo);

        return Ok(new { url });
    }
}
