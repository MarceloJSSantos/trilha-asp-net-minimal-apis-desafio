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

    public DbSet<Administrador> Admnistradores { get; set; } = default!;

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