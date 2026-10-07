using System.ComponentModel.DataAnnotations;

namespace GestaodePedidosAPI.DTOs;

public class LoginAdminDto
{
    [Required(ErrorMessage = "Informe o usuário.")]
    [StringLength(64, ErrorMessage = "Usuário ou senha inválidos.")]
    public string Usuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a senha.")]
    [StringLength(128, ErrorMessage = "Usuário ou senha inválidos.")]
    public string Senha { get; set; } = string.Empty;
}
