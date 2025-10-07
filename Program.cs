using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projeto_final_minimal_api.Dominio.Interfaces;
using projeto_final_minimal_api.Dominio.Servicos;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorService, AdministradorServico>();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
}
);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapPost("/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    if (administradorService.Login(loginDTO) != null)
    {
        return Results.Ok("Login efetuado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
});

app.Run();