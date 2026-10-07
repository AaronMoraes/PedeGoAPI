using GestaodePedidosAPI.Models;
using GestaodePedidosAPI.Services;
using GestaodePedidosAPI.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace GestaodePedidosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _service;

    public PedidosController(PedidoService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<Pedido>>> ListarTodos()
    {
        var pedidos = await _service.ListarTodos();

        return Ok(pedidos);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<Pedido>> BuscarPorId(int id)
    {
        var pedido = await _service.BuscarPorId(id);

        if (pedido == null)
            return NotFound();

        return Ok(pedido);
    }

    // Público (o cliente não tem login), por isso limitado por IP.
    [HttpPost]
    [EnableRateLimiting("pedido")]
    public async Task<ActionResult<Pedido>> Criar(CriarPedidoDto dto)
    {
        var pedido = await _service.Criar(dto);

        // Não expõe a rota de consulta (que é só do admin) para o cliente.
        return StatusCode(StatusCodes.Status201Created, pedido);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Excluir(int id)
    {
        var excluido = await _service.Excluir(id);

        if (!excluido)
            return NotFound();

        return NoContent();
    }
}
