using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;

namespace ProjetoFinalMinimalAPI.Dominio.Interfaces
{
    public interface IAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);

        void Incluir(Administrador administrador);

        List<Administrador> ListaTodos(int? pagina = 1);

        Administrador? BuscaPorId(int id);
    }
}