using GestaodePedidosAPI.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace GestaodePedidosAPI.Services;

public class JwtService
{
    public const string Issuer = "GestaoPedidosAPI";
    public const string Audience = "GestaoPedidosAdmin";

    private readonly string _jwtKey;

    public JwtService()
    {
        _jwtKey = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? throw new InvalidOperationException("JWT_KEY não configurada");
    }

    public string GerarToken(Admin admin)
    {
        // Nomes curtos de claim + MapInboundClaims = false no Program.cs: "role" é lido como papel.
        var claims = new[]
        {
            new Claim("sub", admin.Id.ToString()),
            new Claim("name", admin.Usuario),
            new Claim("role", "Admin")
        };

        var chave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtKey));
        var credenciais = new SigningCredentials(chave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credenciais
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
