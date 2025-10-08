using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using projeto_final_minimal_api.Dominio.DTOs;
using projeto_final_minimal_api.Dominio.Interfaces;
using projeto_final_minimal_api.Dominio.ModelViews;
using projeto_final_minimal_api.Dominio.Servicos;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;
using ProjetoFinalMinimalAPI.Dominio.ModelViews;
using ProjetoFinalMinimalAPI.Dominio.Servicos;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAdministradorService, AdministradorServico>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<DbContexto>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("mysql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("mysql"))
    );
}
);

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administradores
app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
{
    if (administradorService.Login(loginDTO) != null)
    {
        return Results.Ok("Login efetuado com sucesso!");
    }
    else
    {
        return Results.Unauthorized();
    }
}).WithTags("Administradores");
#endregion

#region Veículos


ErrosDeValidacao validaDTO(VeiculoDTO veiculoDTO)
{
    var validacoes = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(veiculoDTO.Marca))
        validacoes.Mensagens.Add("A marca não pode ser null ou vazia!");

    if (string.IsNullOrEmpty(veiculoDTO.Modelo))
        validacoes.Mensagens.Add("O modelo não pode ser null ou vazia!");

    if (veiculoDTO.Ano < 1950)
        validacoes.Mensagens.Add("O ano deve ser maior que 1950!");

    return validacoes;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var validacoes = validaDTO(veiculoDTO);

    if (validacoes.Mensagens.Count > 0)
        return Results.BadRequest(validacoes);

    var veiculo = new Veiculo
    {
        Marca = veiculoDTO.Marca,
        Modelo = veiculoDTO.Modelo,
        Ano = veiculoDTO.Ano
    };
    veiculoService.Incluir(veiculo);

    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);

}).WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) =>
{
    var listaVeiculos = veiculoService.ListaTodos(pagina);

    return Results.Ok(listaVeiculos);
}).WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();

    return Results.Ok(veiculo);
}).WithTags("Veículos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();

    var validacoes = validaDTO(veiculoDTO);

    if (validacoes.Mensagens.Count > 0)
        return Results.BadRequest(validacoes);

    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Modelo = veiculoDTO.Modelo;
    veiculo.Ano = veiculoDTO.Ano;
    veiculoService.Atualizar(veiculo);

    return Results.Ok(veiculo);
}).WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
{
    var veiculo = veiculoService.BuscaPorId(id);

    if (veiculo == null)
        return Results.NotFound();

    veiculoService.Apagar(veiculo);

    return Results.NoContent();
}).WithTags("Veículos");
#endregion

#region App
app.UseSwagger();
app.UseSwaggerUI();

app.Run();
#endregion