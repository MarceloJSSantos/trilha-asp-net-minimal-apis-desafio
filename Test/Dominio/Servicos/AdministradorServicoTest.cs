using Microsoft.EntityFrameworkCore;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Entidades;
using ProjetoFinalMinimalAPI.Dominio.Servicos;
using ProjetoFinalMinimalAPI.Infraestrutura.Db;

namespace Test.Dominio.Servicos
{
    [TestClass]
    public class AdministradorServicoTest
    {
        // Variável para armazenar o nome único do banco de dados para cada classe/teste, se necessário.
        // Usaremos a abordagem de recriar o contexto diretamente nos métodos.

        // Método auxiliar para criar um DbContexto com um novo banco de dados em memória
        private DbContexto GetPersistedInMemoryDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<DbContexto>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;

            var context = new DbContexto(options);
            context.Database.EnsureCreated(); // Não deleta, apenas garante que exista
            return context;
        }

        // --- Testes para o método Incluir ---
        [TestMethod]
        public void IncluirDeveAdicionarAdministradorECommitar()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            // Usa o método que não limpa, apenas cria
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);
            var novoAdm = new Administrador { Email = "teste@email.com", Senha = "senha123", Perfil = "ADM" };

            // Act
            service.Incluir(novoAdm);

            // Assert
            // Requer um novo contexto para verificar a persistência (simulando um novo acesso ao BD)
            using var validationContext = GetPersistedInMemoryDbContext(dbName);
            var admNoBanco = validationContext.Administradores.FirstOrDefault(a => a.Email == "teste@email.com");

            Assert.IsNotNull(admNoBanco, "O administrador deve ser encontrado no banco de dados.");
            Assert.AreEqual("teste@email.com", admNoBanco.Email, "O email do administrador deve ser o esperado.");
        }

        // --- Testes para o método Login ---
        [TestMethod]
        public void LoginComCredenciaisCorretasDeveRetornarAdministrador()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            var admExistente = new Administrador { Email = "login@teste.com", Senha = "senhaSegura", Perfil = "ADM" };
            context.Administradores.Add(admExistente);
            context.SaveChanges();

            var loginDTO = new LoginDTO { Email = "login@teste.com", Senha = "senhaSegura" };

            // Act
            var resultado = service.Login(loginDTO);

            // Assert
            Assert.IsNotNull(resultado, "O login deve retornar um administrador.");
            Assert.AreEqual(admExistente.Email, resultado.Email, "O email retornado deve ser o do administrador logado.");
        }

        [TestMethod]
        public void LoginComSenhaIncorretaDeveRetornarNulo()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            var admExistente = new Administrador { Email = "login@teste.com", Senha = "senhaCorreta", Perfil = "ADM" };
            context.Administradores.Add(admExistente);
            context.SaveChanges();

            var loginDTO = new LoginDTO { Email = "login@teste.com", Senha = "senhaErrada" };

            // Act
            var resultado = service.Login(loginDTO);

            // Assert
            Assert.IsNull(resultado, "O login deve retornar nulo com senha incorreta.");
        }

        // --- Testes para o método BuscaPorId ---
        [TestMethod]
        public void BuscaPorIdComIdExistenteDeveRetornarAdministrador()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            var adm = new Administrador { Id = 5, Email = "busca@id.com", Senha = "123", Perfil = "ADM" };
            context.Administradores.Add(adm);
            context.SaveChanges();

            // Act
            var resultado = service.BuscaPorId(5);

            // Assert
            Assert.IsNotNull(resultado, "A busca por ID deve retornar um administrador.");
            Assert.AreEqual(5, resultado.Id, "O ID do administrador retornado deve ser o esperado.");
        }

        [TestMethod]
        public void BuscaPorIdComIdInexistenteDeveRetornarNulo()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            // Act
            var resultado = service.BuscaPorId(999);

            // Assert
            Assert.IsNull(resultado, "A busca por ID inexistente deve retornar nulo.");
        }

        // --- Testes para o método ListaTodos (PAGINAÇÃO) ---
        [TestMethod]
        public void ListaTodosSemPaginacaoDeveRetornarTodosAdministradores()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            // Usa o método que limpa o DB
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            context.Administradores.AddRange(
                new Administrador { Email = "a1@a.com", Senha = "1", Perfil = "ADM" },
                new Administrador { Email = "a2@a.com", Senha = "2", Perfil = "ADM" },
                new Administrador { Email = "a3@a.com", Senha = "3", Perfil = "ADM" }
            );
            context.SaveChanges();

            // Act
            var resultado = service.ListaTodos(null); // Sem parâmetro de página

            // Assert
            Assert.AreEqual(3, resultado.Count, "Deve retornar exatamente 3 administradores.");
            Assert.IsTrue(resultado.Any(a => a.Email == "a1@a.com"), "A lista deve conter o administrador 'a1@a.com'.");
        }

        [TestMethod]
        public void ListaTodosComPaginacaoPaginaUmDeveRetornarPrimeirosDez()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            // Adiciona 15 administradores
            for (int i = 1; i <= 15; i++)
            {
                context.Administradores.Add(new Administrador { Id = i, Email = $"adm{i}@test.com", Senha = i.ToString(), Perfil = "ADM" });
            }
            context.SaveChanges();

            // Act
            var resultado = service.ListaTodos(1);

            // Assert
            Assert.AreEqual(10, resultado.Count, "A primeira página deve retornar 10 itens.");
            Assert.IsTrue(resultado.Any(a => a.Email == "adm1@test.com"));
            Assert.IsTrue(resultado.Any(a => a.Email == "adm10@test.com"));
            Assert.IsFalse(resultado.Any(a => a.Email == "adm11@test.com"), "O 11º item não deve estar na primeira página.");
        }

        [TestMethod]
        public void ListaTodosComPaginacaoPaginaDoisDeveRetornarOsRestantes()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            // Adiciona 15 administradores
            for (int i = 1; i <= 15; i++)
            {
                context.Administradores.Add(new Administrador { Id = i, Email = $"adm{i}@test.com", Senha = i.ToString(), Perfil = "ADM" });
            }
            context.SaveChanges();

            // Act
            var resultado = service.ListaTodos(2);

            // Assert
            Assert.AreEqual(5, resultado.Count, "A segunda página deve retornar os 5 itens restantes.");
            Assert.IsFalse(resultado.Any(a => a.Email == "adm10@test.com"));
            Assert.IsTrue(resultado.Any(a => a.Email == "adm11@test.com"), "O 11º item deve estar na segunda página.");
            Assert.IsTrue(resultado.Any(a => a.Email == "adm15@test.com"));
        }

        [TestMethod]
        public void ListaTodosComPaginacaoPaginaInvalidaDeveRetornarVazio()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            using var context = GetPersistedInMemoryDbContext(dbName);
            var service = new AdministradorServico(context);

            VerificaELimpaBDEmMemoria(context);

            // Adiciona 5 administradores (menos de 10)
            for (int i = 1; i <= 5; i++)
            {
                context.Administradores.Add(new Administrador { Id = i, Email = $"adm{i}@test.com", Senha = i.ToString(), Perfil = "ADM" });
            }
            context.SaveChanges();

            // Act
            var resultado = service.ListaTodos(2); // Página 2 deve estar vazia

            // Assert
            Assert.IsTrue(resultado.Count == 0, "A lista deve estar vazia para uma página além dos dados existentes.");
        }

        private static void VerificaELimpaBDEmMemoria(DbContexto context)
        {
            if (context.Administradores.Any())
            {
                context.Administradores.RemoveRange(context.Administradores);
                context.SaveChanges();
            }
        }
    }
}