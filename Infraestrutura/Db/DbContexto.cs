using Microsoft.EntityFrameworkCore;
using ProjetoFinalMinimalAPI.Dominio.Entidades;

namespace ProjetoFinalMinimalAPI.Infraestrutura.Db;

public class DbContexto : DbContext
{
    private readonly IConfiguration _configuracaoAppSetting;

    public DbContexto(IConfiguration configuracaoAppSetting)
    {
        _configuracaoAppSetting = configuracaoAppSetting;
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

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var stringConexao = _configuracaoAppSetting.GetConnectionString("mysql")?.ToString();
            if (!string.IsNullOrEmpty(stringConexao))
            {
                optionsBuilder.UseMySql(
                    stringConexao,
                    ServerVersion.AutoDetect(stringConexao)
                );
            }
        }
    }
}