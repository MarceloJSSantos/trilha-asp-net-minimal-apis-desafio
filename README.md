# Projeto Final - Minimal API

Este projeto é uma API REST minimalista desenvolvida em .NET 9, utilizando Entity Framework Core e autenticação JWT. O objetivo é gerenciar administradores e veículos, com endpoints protegidos por perfis de acesso.

## Estrutura do Projeto

```
Api/
  ├── Dominio/
  │   ├── DTOs/
  │   ├── Entidades/
  │   ├── Enuns/
  │   ├── Interfaces/
  │   ├── ModelViews/
  │   └── Servicos/
  ├── Infraestrutura/
  │   └── Db/
  ├── Migrations/
  ├── Program.cs
  ├── Startup.cs
  ├── appsettings.json
  └── projeto-final-minimal-api.csproj
Test/
  ├── Helpers/
  ├── Mocks/
  ├── Requests/
  ├── Dominio/
  ├── MSTestSettings.cs
  └── Test.csproj
```

## Principais Tecnologias

- .NET 9
- Entity Framework Core (MySQL)
- Autenticação JWT
- Swagger para documentação
- MSTest para testes automatizados

## Como Executar

1. **Configuração do Banco de Dados**

   - Configure a string de conexão MySQL em [`Api/appsettings.json`](Api/appsettings.json).

2. **Executando a API**

   - No terminal, navegue até a pasta `Api` e execute:
     ```sh
     dotnet run
     ```
   - Acesse a documentação Swagger em [http://localhost:5116/swagger](http://localhost:5116/swagger).

3. **Executando os Testes**
   - Navegue até a pasta `Test` e execute:
     ```sh
     dotnet test
     ```

## Endpoints Principais

- **Administradores**

  - `POST /administradores/login` — Autenticação e geração de token JWT
  - `GET /administradores` — Listagem (requer perfil ADM)
  - `POST /administradores` — Inclusão (requer perfil ADM)
  - `GET /administradores/{id}` — Busca por ID (requer perfil ADM)

- **Veículos**
  - `GET /veiculos` — Listagem (ADM/EDITOR)
  - `POST /veiculos` — Inclusão (ADM/EDITOR)
  - `PUT /veiculos/{id}` — Atualização (ADM)
  - `DELETE /veiculos/{id}` — Exclusão (ADM)

## Testes Automatizados

Os testes estão localizados em [`Test/`](Test/) e cobrem:

- **Administradores**

  - Validação de entidades
  - Serviços de domínio
  - Requisições HTTP simuladas (incluindo autenticação JWT)

- **Veiculos**
  - Não desenvolvidos os testes ainda

## Autor

- [Marcelo Santos](https://github.com/MarceloJSSantos)

---

Projeto para demonstração de Minimal API com autenticação e testes automatizados desenvolvido como projeto final do **Bootcamp DIO Akad Fullstack Developer**.
