using System.ComponentModel.DataAnnotations;
using ProjetoFinalMinimalAPI.Dominio.Entidades;

namespace Test.Dominio.Entidades
{
    [TestClass]
    public class AdministradorTest
    {
        // Método auxiliar para validar atributos de anotação
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationContext = new ValidationContext(model, null, null);
            var validationResults = new List<ValidationResult>();
            Validator.TryValidateObject(model, validationContext, validationResults, true);
            return validationResults;
        }

        // --- Testes de Construtor e Propriedades ---
        [TestMethod]
        public void Administrador_DeveSerInstanciadoComSucesso()
        {
            // Arrange & Act
            var administrador = new Administrador();

            // Assert
            Assert.IsNotNull(administrador);
            Assert.AreEqual(default(int), administrador.Id);
            // Os valores de string devem ser null (ou o que for definido por 'default!')
            Assert.IsNull(administrador.Email);
            Assert.IsNull(administrador.Senha);
            Assert.IsNull(administrador.Perfil);
        }

        [TestMethod]
        public void Administrador_PropriedadesDevemFuncionar()
        {
            // Arrange
            int idEsperado = 1;
            string emailEsperado = "teste@dominio.com";
            string senhaEsperada = "senha123";
            string perfilEsperado = "MASTER";

            // Act
            var administrador = new Administrador
            {
                Id = idEsperado,
                Email = emailEsperado,
                Senha = senhaEsperada,
                Perfil = perfilEsperado
            };

            // Assert
            Assert.AreEqual(idEsperado, administrador.Id);
            Assert.AreEqual(emailEsperado, administrador.Email);
            Assert.AreEqual(senhaEsperada, administrador.Senha);
            Assert.AreEqual(perfilEsperado, administrador.Perfil);
        }

        // --- Testes de Data Annotations (Validação) ---
        [TestMethod]
        public void Administrador_ModeloValido_NaoDeveGerarErros()
        {
            // Arrange
            var administrador = new Administrador
            {
                Email = "teste@valido.com",
                Senha = "senha",
                Perfil = "ADMIN"
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            Assert.AreEqual(0, resultados.Count, "O modelo não deveria ter erros de validação.");
        }

        // --- Testes de Validação [Required] ---
        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Administrador_EmailObrigatorio_DeveGerarErroSeNullOuVazio(string email)
        {
            // Arrange
            var administrador = new Administrador
            {
                Email = email, // Vai ser testado como null ou ""
                Senha = "senhaValida",
                Perfil = "ADMIN"
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            var erroEmail = resultados.FirstOrDefault(r => r.MemberNames.Contains(nameof(Administrador.Email)));
            Assert.IsNotNull(erroEmail, "O campo Email deveria ter um erro de validação [Required].");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Administrador_SenhaObrigatoria_DeveGerarErroSeNullOuVazio(string senha)
        {
            // Arrange
            var administrador = new Administrador
            {
                Email = "teste@valido.com",
                Senha = senha, // Vai ser testado como null ou ""
                Perfil = "ADMIN"
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            var erroSenha = resultados.FirstOrDefault(r => r.MemberNames.Contains(nameof(Administrador.Senha)));
            Assert.IsNotNull(erroSenha, "O campo Senha deveria ter um erro de validação [Required].");
        }

        [DataTestMethod]
        [DataRow(null)]
        [DataRow("")]
        public void Administrador_PerfilObrigatorio_DeveGerarErroSeNullOuVazio(string perfil)
        {
            // Arrange
            var administrador = new Administrador
            {
                Email = "teste@valido.com",
                Senha = "senhaValida",
                Perfil = perfil // Vai ser testado como null ou ""
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            var erroPerfil = resultados.FirstOrDefault(r => r.MemberNames.Contains(nameof(Administrador.Perfil)));
            Assert.IsNotNull(erroPerfil, "O campo Perfil deveria ter um erro de validação [Required].");
        }

        // --- Testes de Validação [StringLength] ---

        [TestMethod]
        public void Administrador_EmailMaximo255_DeveGerarErroSeMuitoLongo()
        {
            // Arrange
            // 255 é o máximo permitido. 256 excede o limite.
            var administrador = new Administrador
            {
                Email = new string('a', 256),
                Senha = "senhaValida",
                Perfil = "ADMIN"
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            var erroEmail = resultados.FirstOrDefault(r => r.MemberNames.Contains(nameof(Administrador.Email)));
            Assert.IsNotNull(erroEmail, "O Email deveria falhar por exceder 255 caracteres.");
        }

        [TestMethod]
        public void Administrador_SenhaMaximo50_DeveGerarErroSeMuitoLongo()
        {
            // Arrange
            // 50 é o máximo permitido. 51 excede o limite.
            var administrador = new Administrador
            {
                Email = "teste@valido.com",
                Senha = new string('x', 51),
                Perfil = "ADMIN"
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            var erroSenha = resultados.FirstOrDefault(r => r.MemberNames.Contains(nameof(Administrador.Senha)));
            Assert.IsNotNull(erroSenha, "A Senha deveria falhar por exceder 50 caracteres.");
        }

        [TestMethod]
        public void Administrador_PerfilMaximo10_DeveGerarErroSeMuitoLongo()
        {
            // Arrange
            // 10 é o máximo permitido. 11 excede o limite.
            var administrador = new Administrador
            {
                Email = "teste@valido.com",
                Senha = "senhaValida",
                Perfil = new string('z', 11)
            };

            // Act
            var resultados = ValidateModel(administrador);

            // Assert
            var erroPerfil = resultados.FirstOrDefault(r => r.MemberNames.Contains(nameof(Administrador.Perfil)));
            Assert.IsNotNull(erroPerfil, "O Perfil deveria falhar por exceder 10 caracteres.");
        }
    }
}