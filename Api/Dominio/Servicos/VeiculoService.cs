using ProjetoFinalMinimalAPI.Dominio.Entidades;
using ProjetoFinalMinimalAPI.Dominio.Interfaces;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

namespace ProjetoFinalMinimalAPI.Dominio.Servicos
{
    public class VeiculoService : IVeiculoService
    {
        private readonly DbContexto _contexto;

        public VeiculoService(DbContexto db)
        {
            _contexto = db;
        }

        public void Apagar(Veiculo veiculo)
        {
            _contexto.Veiculos.Remove(veiculo);
            _contexto.SaveChanges();
        }

        public void Atualizar(Veiculo veiculo)
        {
            _contexto.Veiculos.Update(veiculo);
            _contexto.SaveChanges();
        }

        public Veiculo? BuscaPorId(int id)
        {
            return _contexto.Veiculos.Where(v => v.Id == id).FirstOrDefault();
        }

        public void Incluir(Veiculo veiculo)
        {
            _contexto.Veiculos.Add(veiculo);
            _contexto.SaveChanges();
        }

        public List<Veiculo> ListaTodos(int? pagina = 1, string? marca = null, string? modelo = null)
        {
            var query = _contexto.Veiculos.AsQueryable();
            if (!string.IsNullOrEmpty(marca))
            {
                query = query.Where(v => v.Marca.ToLower().Contains(marca));
                //query = query.Where(v => EF.Functions.Like(v.Marca.ToLower(), $"%{marca}%"));
            }

            int itensPorPagina = 10;
            if (pagina != null)
                query = query.Skip(((int)pagina - 1) * itensPorPagina).Take(itensPorPagina);

            return query.ToList();
        }
    }
}