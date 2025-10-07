using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;

namespace projeto_final_minimal_api.Dominio.Interfaces
{
    public interface IAdministradorService
    {
        Administrador? Login(LoginDTO loginDTO);
    }
}