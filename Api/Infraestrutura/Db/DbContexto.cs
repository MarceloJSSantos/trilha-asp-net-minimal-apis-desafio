using Microsoft.EntityFrameworkCore;
using ProjetoFinalMinimalAPI.Dominio.Entidades;

namespace ProjetoFinalMinimalAPI.Infraestrutura.Db;

public class DbContexto : DbContext
{
    public DbContexto(DbContextOptions<DbContexto> options) : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrador>().HasData(
            new Administrador
            {
                Id = 1,
                Email = "administrador@email.com",
                Senha = "123456",
                Perfil = "ADM"
            }
        );
    }

    public DbSet<Administrador> Administradores { get; set; } = default!;

    public DbSet<Veiculo> Veiculos { get; set; } = default!;

}