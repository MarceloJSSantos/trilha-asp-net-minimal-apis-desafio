using ProjetoFinalMinimalAPI.Dominio.Interfaces;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

namespace ProjetoFinalMinimalAPI.Dominio.Servicos
{
    public class AdministradorServico : IAdministradorService
    {
        private readonly DbContexto _contexto;

        public AdministradorServico(DbContexto db)
        {
            _contexto = db;
        }

        public Administrador? Login(LoginDTO loginDTO)
        {
            var adm = _contexto.Administradores.Where(a =>
                a.Email == loginDTO.Email &&
                a.Senha == loginDTO.Senha).FirstOrDefault();
            return adm;
        }

        public void Incluir(Administrador administrador)
        {
            _contexto.Administradores.Add(administrador);
            _contexto.SaveChanges();
        }

        public List<Administrador> ListaTodos(int? pagina = 1)
        {
            var query = _contexto.Administradores.AsQueryable();

            int itensPorPagina = 10;
            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }

        public Administrador? BuscaPorId(int id)
        {
            return _contexto.Administradores.Where(a => a.Id == id).FirstOrDefault();
        }
    }
}