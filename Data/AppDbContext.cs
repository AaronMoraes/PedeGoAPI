using GestaodePedidosAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GestaodePedidosAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<Admin> Admins => Set<Admin>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admin>()
            .HasIndex(admin => admin.Usuario)
            .IsUnique();

        modelBuilder.Entity<ItemPedido>()
            .HasOne(item => item.Pedido)
            .WithMany(pedido => pedido.Itens)
            .HasForeignKey(item => item.PedidoId);

        // Restrict: um produto que já foi vendido nunca é apagado do banco (apenas arquivado),
        // senão o histórico de pedidos seria destruído.
        modelBuilder.Entity<ItemPedido>()
            .HasOne(item => item.Produto)
            .WithMany()
            .HasForeignKey(item => item.ProdutoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
