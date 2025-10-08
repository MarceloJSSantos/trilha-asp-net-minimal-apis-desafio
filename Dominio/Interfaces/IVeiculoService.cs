using ProjetoFinalMinimalAPI.Dominio.Entidades;

namespace projeto_final_minimal_api.Dominio.Interfaces
{
    public interface IVeiculoService
    {
        List<Veiculo> ListaTodos(int? pagina = 1, string? marca = null, string? modelo = null);

        Veiculo? BuscaPorId(int id);

        void Incluir(Veiculo veiculo);

        void Atualizar(Veiculo veiculo);

        void Apagar(Veiculo veiculo);


    }
}