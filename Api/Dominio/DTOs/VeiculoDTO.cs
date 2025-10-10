namespace ProjetoFinalMinimalAPI.Dominio.DTOs
{
    public record VeiculoDTO
    {
        public string Marca { get; set; } = default!;

        public string Modelo { get; set; } = default!;

        public int Ano { get; set; } = default!;
    }
}