using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using projeto_final_minimal_api.Dominio.ModelViews;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;
using ProjetoFinalMinimalAPI.Dominio.Enuns;
using ProjetoFinalMinimalAPI.Dominio.Interfaces;
using ProjetoFinalMinimalAPI.Dominio.ModelViews;
using ProjetoFinalMinimalAPI.Dominio.Servicos;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        jwtKey = Configuration?["Jwt:Key"] ?? "123456";
    }

    private string jwtKey;

    public IConfiguration Configuration { get; set; } = default!;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateLifetime = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateIssuer = false,
                ValidateAudience = false
            };
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser() // Requer um usuário autenticado
                .Build();
        });

        services.AddScoped<IAdministradorService, AdministradorServico>();
        services.AddScoped<IVeiculoService, VeiculoService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Insira o seu token JWT aqui"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
            });
        });

        services.AddDbContext<DbContexto>(options =>
        {
            options.UseMySql(
                Configuration.GetConnectionString("MySql"),
                ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql"))
            );
        }
        );

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {

            #region Home
            endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home").AllowAnonymous();
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
            string GerarTokenJwt(Administrador administrador)
            {
                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>()
                {
                    new Claim("Email", administrador.Email),
                    new Claim("Perfil", administrador.Perfil),
                    new Claim(ClaimTypes.Role, administrador.Perfil)
                };

                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: credentials
                );

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
;

            endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorService administradorService) =>
            {
                var adm = administradorService.Login(loginDTO);
                if (adm != null)
                {
                    string token = GerarTokenJwt(adm);
                    return Results.Ok(new AdministradorLogado
                    {
                        Email = adm.Email,
                        Perfil = adm.Perfil,
                        Token = token
                    });
                }
                else
                {
                    return Results.Unauthorized();
                }
            }).WithTags("Administradores").AllowAnonymous();

            endpoints.MapGet("/administradores", ([FromQuery] int? pagina, IAdministradorService administradorService) =>
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
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
            .WithTags("Administradores");

            endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorService administradorService) =>
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

            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
            .WithTags("Administradores");

            endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorService administradorService) =>
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
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
            .WithTags("Administradores");
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

            endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
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

            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" })
            .WithTags("Veículos");

            endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, IVeiculoService veiculoService) =>
            {
                var listaVeiculos = veiculoService.ListaTodos(pagina);

                return Results.Ok(listaVeiculos);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" })
            .WithTags("Veículos");

            endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
            {
                var veiculo = veiculoService.BuscaPorId(id);

                if (veiculo == null)
                    return Results.NotFound();

                return Results.Ok(veiculo);
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM,EDITOR" })
            .WithTags("Veículos");

            endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoService veiculoService) =>
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
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
            .WithTags("Veículos");

            endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoService veiculoService) =>
            {
                var veiculo = veiculoService.BuscaPorId(id);

                if (veiculo == null)
                    return Results.NotFound();

                veiculoService.Apagar(veiculo);

                return Results.NoContent();
            })
            .RequireAuthorization(new AuthorizeAttribute { Roles = "ADM" })
            .WithTags("Veículos");
            #endregion
        });
    }
}