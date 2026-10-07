using GestaodePedidosAPI.Data;
using GestaodePedidosAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaodePedidosAPI.Services;

public class AdminService
{
    // Hash de mentira, usado para gastar o mesmo tempo quando o usuário não existe
    // (evita descobrir usuários válidos medindo o tempo de resposta do login).
    private static readonly Lazy<string> HashFalso = new(() =>
        new PasswordHasher<Admin>().HashPassword(new Admin(), Guid.NewGuid().ToString("N")));

    private readonly AppDbContext _context;
    private readonly PasswordHasher<Admin> _passwordHasher;

    public AdminService(AppDbContext context)
    {
        _context = context;
        _passwordHasher = new PasswordHasher<Admin>();
    }

    public async Task<Admin?> BuscarPorUsuario(string usuario)
    {
        return await _context.Admins
            .FirstOrDefaultAsync(a => a.Usuario == usuario);
    }

    public string GerarHashSenha(Admin admin, string senha)
    {
        return _passwordHasher.HashPassword(admin, senha);
    }

    public bool VerificarSenha(Admin admin, string senha)
    {
        var resultado = _passwordHasher.VerifyHashedPassword(
            admin,
            admin.SenhaHash,
            senha
        );

        return resultado == PasswordVerificationResult.Success ||
               resultado == PasswordVerificationResult.SuccessRehashNeeded;
    }

    public bool SimularVerificacao(string senha)
    {
        _passwordHasher.VerifyHashedPassword(new Admin(), HashFalso.Value, senha);
        return false;
    }

    public async Task CriarAdminInicial(string usuario, string senha)
    {
        if (senha.Length < 10)
            throw new InvalidOperationException("ADMIN_SENHA deve ter pelo menos 10 caracteres.");

        var adminExistente = await BuscarPorUsuario(usuario);

        if (adminExistente != null)
            return;

        var admin = new Admin
        {
            Usuario = usuario
        };

        admin.SenhaHash = GerarHashSenha(admin, senha);

        _context.Admins.Add(admin);

        await _context.SaveChangesAsync();
    }
}
