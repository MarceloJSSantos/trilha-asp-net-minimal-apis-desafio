using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Enuns;
using ProjetoFinalMinimalAPI.Dominio.Interfaces;
using ProjetoFinalMinimalAPI.Dominio.ModelViews;
using ProjetoFinalMinimalAPI.Dominio.Servicos;
using ProjetoFinalMinimalAPI.Dominio.Entidades;
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
ErrosDeValidacao validaAdministradorDTO(AdministradorDTO administradorDTO)
{
    var validacoes = new ErrosDeValidacao
    {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacoes.Mensagens.Add("O email não pode ser null ou vazio!");

    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacoes.Mensagens.Add("A senha não pode ser null ou vazia!");

    if (string.IsNullOrEmpty(administradorDTO.Perfil.ToString()))
        validacoes.Mensagens.Add("O perfil não pode ser null ou vazio!");

    return validacoes;
}

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

app.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
{
    var listaAdministradores = administradorService.ListaTodos(pagina);
    var listaAdministradoresModelView = new List<AdministradorModelView>();

    foreach (var adm in listaAdministradores)
    {
        listaAdministradoresModelView.Add(new AdministradorModelView
        {
            Id = adm.Id,
            Email = adm.Email,
            Perfil = adm.Perfil
        });
    }

    return Results.Ok(listaAdministradoresModelView);
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
{
    var validacoes = validaAdministradorDTO(administradorDTO);

    if (validacoes.Mensagens.Count > 0)
        return Results.BadRequest(validacoes);

    var administrador = new Administrador
    {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.EDITOR.ToString()
    };
    administradorService.Incluir(administrador);

    var AdministradorModelView = new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    };

    return Results.Created($"/administradores/{AdministradorModelView.Id}", AdministradorModelView);

}).WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
{
    var administrador = administradorService.BuscaPorId(id);
    if (administrador == null)
        return Results.NotFound();

    var AdministradorModelView = new AdministradorModelView
    {
        Id = administrador.Id,
        Email = administrador.Email,
        Perfil = administrador.Perfil
    };

    return Results.Ok(AdministradorModelView);
}).WithTags("Administradores");
#endregion

#region Veículos
ErrosDeValidacao validaVeiculoDTO(VeiculoDTO veiculoDTO)
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
    var validacoes = validaVeiculoDTO(veiculoDTO);

    if (validacoes.Mensagens.Count > 0)
        return Results.BadRequest(validacoes);

    var veiculo = new Veiculo
    {
        Marca = veiculoDTO.Marca,
        Modelo = veiculoDTO.Modelo,
        Ano = veiculoDTO.Ano
    };
    veiculoService.Incluir(veiculo);

    return Results.Created($"/veiculos/{veiculo.Id}", veiculo);

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

    var validacoes = validaVeiculoDTO(veiculoDTO);

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