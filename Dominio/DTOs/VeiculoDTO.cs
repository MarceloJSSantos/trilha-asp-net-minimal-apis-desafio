using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace projeto_final_minimal_api.Dominio.DTOs
{
    public record VeiculoDTO
    {
        public string Marca { get; set; } = default!;

        public string Modelo { get; set; } = default!;

        public int Ano { get; set; } = default!;
    }
}