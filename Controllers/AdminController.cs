using GestaodePedidosAPI.DTOs;
using GestaodePedidosAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace GestaodePedidosAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly AdminService _adminService;
    private readonly JwtService _jwtService;
    private readonly LoginAttemptTracker _tentativas;

    public AdminController(AdminService adminService, JwtService jwtService, LoginAttemptTracker tentativas)
    {
        _adminService = adminService;
        _jwtService = jwtService;
        _tentativas = tentativas;
    }

    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginAdminDto dto)
    {
        if (_tentativas.Bloqueado(dto.Usuario))
            return StatusCode(StatusCodes.Status429TooManyRequests,
                new { message = "Muitas tentativas. Tente novamente em alguns minutos." });

        var admin = await _adminService.BuscarPorUsuario(dto.Usuario);

        // Mesma mensagem e mesmo tempo de resposta para "usuário não existe" e "senha errada".
        var senhaValida = admin != null
            ? _adminService.VerificarSenha(admin, dto.Senha)
            : _adminService.SimularVerificacao(dto.Senha);

        if (admin == null || !senhaValida)
        {
            _tentativas.RegistrarFalha(dto.Usuario);
            return Unauthorized(new { message = "Usuário ou senha inválidos." });
        }

        _tentativas.Limpar(dto.Usuario);

        return Ok(new
        {
            message = "Login realizado com sucesso.",
            token = _jwtService.GerarToken(admin)
        });
    }
}
