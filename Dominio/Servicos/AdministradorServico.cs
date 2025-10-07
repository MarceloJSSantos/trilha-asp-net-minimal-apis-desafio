using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using projeto_final_minimal_api.Dominio.Interfaces;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

namespace projeto_final_minimal_api.Dominio.Servicos
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
    }
}