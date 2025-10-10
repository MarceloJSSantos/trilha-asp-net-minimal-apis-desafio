
using ProjetoFinalMinimalAPI.Dominio.Enuns;
using System.Text.Json.Serialization;

namespace ProjetoFinalMinimalAPI.Dominio.DTOs
{
    public record AdministradorDTO
    {
        public string Email { get; set; } = default!;

        public string Senha { get; set; } = default!;

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Perfil Perfil { get; set; } = default!;
    }
}