using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using projeto_final_minimal_api.Dominio.ModelViews;
using ProjetoFinalMinimalAPI.Dominio.DTOs;
using ProjetoFinalMinimalAPI.Dominio.Enuns;
using ProjetoFinalMinimalAPI.Dominio.ModelViews;
using Test.Helpers;

namespace Test.Requests
{
    [TestClass]
    public class AdministradorRequestTest
    {
        // Variável para armazenar o token JWT para requisições autorizadas
        private static string _jwtToken = string.Empty;

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            Setup.ClassInit(testContext);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            Setup.ClassCleanup();
        }

        /// <summary>
        /// Método de teste para obter um token JWT válido de um administrador (ADM)
        /// e armazená-lo para ser usado nos testes subsequentes que exigem autorização.
        /// </summary>
        [TestMethod]
        [Priority(1)] // Garante que este teste rode primeiro para obter o token
        public async Task PostLoginComEmailESenhaValidosDeveRetornar200OkEObterToken()
        {
            // Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com", // Usuário com perfil ADM
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            // Act
            var response = await Setup.client.PostAsync("/administradores/login", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admLogado = JsonSerializer.Deserialize<AdministradorLogado>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admLogado?.Token);
            _jwtToken = admLogado.Token; // Armazena o token para uso posterior

            // Adicionalmente, verifica outras propriedades como no teste original
            Assert.IsNotNull(admLogado.Email);
            Assert.IsNotNull(admLogado.Perfil);
        }

        /// <summary>
        /// Testa o login com uma senha incorreta. Espera-se 401 Unauthorized.
        /// </summary>
        [TestMethod]
        [Priority(3)]
        public async Task PostLoginComSenhaInvalidaDeveRetornar401Unauthorized()
        {
            //Arrange
            var loginDTO = new LoginDTO
            {
                Email = "adm@teste.com",
                Senha = "senha_incorreta" // Senha incorreta
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            //act
            var response = await Setup.client.PostAsync("/administradores/login", content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode); // 401
        }

        /// <summary>
        /// Testa o login com um email não cadastrado. Espera-se 401 Unauthorized.
        /// </summary>
        [TestMethod]
        [Priority(4)]
        public async Task PostLoginComEmailNaoCadastradoDeveRetornar401Unauthorized()
        {
            //Arrange
            var loginDTO = new LoginDTO
            {
                Email = "nao_existe@teste.com", // Email que não deve estar no banco de dados de teste
                Senha = "123456"
            };

            var content = new StringContent(JsonSerializer.Serialize(loginDTO), Encoding.UTF8, "Application/json");

            //act
            var response = await Setup.client.PostAsync("/administradores/login", content);

            //Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode); // 401
        }

        // ----------------------------------------------------------------------
        // TESTES PARA POST /administradores (Inclusão de novo Administrador)
        // Requer Autorização: Perfil ADM
        // ----------------------------------------------------------------------

        /// <summary>
        /// Testa a inclusão de um novo administrador com dados válidos, requerendo token de autorização.
        /// </summary>
        [TestMethod]
        [Priority(6)]
        public async Task PostAdministradorComDadosValidosDeveRetornar201Created()
        {
            // Arrange
            // Garante que o token foi obtido. Se não, falha ou usa um token obtido em outro teste.
            if (string.IsNullOrEmpty(_jwtToken))
            {
                // Tenta executar o teste de login para obter o token, se necessário.
                await PostLoginComEmailESenhaValidosDeveRetornar200OkEObterToken();
            }

            var novoAdministrador = new AdministradorDTO
            {
                Email = "novo_adm_test@teste.com",
                Senha = "password123",
                Perfil = Perfil.EDITOR // Exemplo
            };

            var content = new StringContent(JsonSerializer.Serialize(novoAdministrador), Encoding.UTF8, "Application/json");

            // Adiciona o cabeçalho de autorização
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Act
            var response = await Setup.client.PostAsync("/administradores", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var admCriado = JsonSerializer.Deserialize<AdministradorModelView>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(admCriado?.Id);
            Assert.AreEqual(novoAdministrador.Email, admCriado.Email);
            Assert.AreEqual(novoAdministrador.Perfil.ToString(), admCriado.Perfil);
        }

        /// <summary>
        /// Testa a inclusão de novo administrador sem token de autorização. Espera-se 401 Unauthorized.
        /// </summary>
        [TestMethod]
        [Priority(7)]
        public async Task PostAdministradorSemTokenDeveRetornar401Unauthorized()
        {
            // Arrange
            var novoAdministrador = new AdministradorDTO
            {
                Email = "sem_token@teste.com",
                Senha = "password123",
                Perfil = Perfil.EDITOR
            };
            var content = new StringContent(JsonSerializer.Serialize(novoAdministrador), Encoding.UTF8, "Application/json");

            // Remove o cabeçalho de autorização se houver
            Setup.client.DefaultRequestHeaders.Authorization = null;

            // Act
            var response = await Setup.client.PostAsync("/administradores", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Testa a inclusão com dados inválidos (ex: email vazio). Espera-se 400 Bad Request.
        /// </summary>
        [TestMethod]
        [Priority(8)]
        public async Task PostAdministradorComDadosInvalidosDeveRetornar400BadRequest()
        {
            // Arrange
            // Garante que o token foi obtido
            if (string.IsNullOrEmpty(_jwtToken)) await PostLoginComEmailESenhaValidosDeveRetornar200OkEObterToken();

            var admInvalido = new AdministradorDTO
            {
                Email = "", // Dado inválido
                Senha = "password123",
                Perfil = Perfil.EDITOR
            };
            var content = new StringContent(JsonSerializer.Serialize(admInvalido), Encoding.UTF8, "Application/json");

            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Act
            var response = await Setup.client.PostAsync("/administradores", content);

            // Assert
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // ----------------------------------------------------------------------
        // TESTES PARA GET /administradores (Listagem)
        // Requer Autorização: Perfil ADM
        // ----------------------------------------------------------------------

        /// <summary>
        /// Testa a listagem de administradores com token válido. Espera-se 200 OK e uma lista.
        /// </summary>
        [TestMethod]
        [Priority(9)]
        public async Task GetAdministradoresComTokenValidoDeveRetornar200Ok()
        {
            // Arrange
            if (string.IsNullOrEmpty(_jwtToken)) await PostLoginComEmailESenhaValidosDeveRetornar200OkEObterToken();

            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Act
            var response = await Setup.client.GetAsync("/administradores");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var lista = JsonSerializer.Deserialize<List<AdministradorModelView>>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(lista);
            Assert.IsTrue(lista.Count >= 1); // Deve haver pelo menos o ADM de teste e o recém-criado
        }

        /// <summary>
        /// Testa a listagem de administradores sem token de autorização. Espera-se 401 Unauthorized.
        /// </summary>
        [TestMethod]
        [Priority(10)]
        public async Task GetAdministradoresSemTokenDeveRetornar401Unauthorized()
        {
            // Arrange
            Setup.client.DefaultRequestHeaders.Authorization = null; // Remove o token

            // Act
            var response = await Setup.client.GetAsync("/administradores");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // ----------------------------------------------------------------------
        // TESTES PARA GET /administradores/{id} (Busca por ID)
        // Requer Autorização: Perfil ADM
        // ----------------------------------------------------------------------

        /// <summary>
        /// Testa a busca por ID de um administrador existente com token válido. Espera-se 200 OK.
        /// </summary>
        [TestMethod]
        [Priority(11)]
        public async Task GetAdministradorPorIdExistenteDeveRetornar200Ok()
        {
            // Arrange
            if (string.IsNullOrEmpty(_jwtToken)) await PostLoginComEmailESenhaValidosDeveRetornar200OkEObterToken();

            // O ID 1 é assumido como o ADM padrão (adm@teste.com)
            const int idExistente = 1;
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{idExistente}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadAsStringAsync();
            var adm = JsonSerializer.Deserialize<AdministradorModelView>(result, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.IsNotNull(adm);
            Assert.AreEqual(idExistente, adm.Id);
            Assert.AreEqual("adm@teste.com", adm.Email);
        }

        /// <summary>
        /// Testa a busca por ID de um administrador não existente. Espera-se 404 Not Found.
        /// </summary>
        [TestMethod]
        [Priority(12)]
        public async Task GetAdministradorPorIdInexistenteDeveRetornar404NotFound()
        {
            // Arrange
            if (string.IsNullOrEmpty(_jwtToken)) await PostLoginComEmailESenhaValidosDeveRetornar200OkEObterToken();

            // Assumindo que este ID não existe no banco de dados de teste
            const int idInexistente = 9999;
            Setup.client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _jwtToken);

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{idInexistente}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        /// <summary>
        /// Testa a busca por ID de um administrador sem token de autorização. Espera-se 401 Unauthorized.
        /// </summary>
        [TestMethod]
        public async Task GetAdministradorPorIdSemTokenDeveRetornar401Unauthorized()
        {
            // Arrange
            const int idQualquer = 1;
            Setup.client.DefaultRequestHeaders.Authorization = null; // Remove o token

            // Act
            var response = await Setup.client.GetAsync($"/administradores/{idQualquer}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}