# GestaodePedidosAPI

API do sistema de pedidos (ASP.NET Core 8 + SQLite + JWT).

## Rodar na sua máquina

Pré-requisito: .NET 8 SDK.

```
copy .env.example .env        (Windows)   |   cp .env.example .env   (Linux/Mac)
```

Edite o `.env`: defina `JWT_KEY`, `ADMIN_USUARIO` e `ADMIN_SENHA` (os outros podem ficar como estão). Depois:

```
dotnet run
```

A API sobe em http://localhost:5000 e lê o `.env` sozinha. Teste: http://localhost:5000/api/Produtos
(devem vir 29 produtos de demonstração na primeira execução).

Se já existir um `gestaodepedidos.db` de versões anteriores, **apague-o antes**. Ele foi criado com o
histórico antigo de migrations e não é compatível com a migration inicial atual.

## Login do administrador

Não existe usuário nem senha padrão no código. O admin é criado na primeira execução com `ADMIN_USUARIO`
e `ADMIN_SENHA` (mínimo 10 caracteres). Mudar a senha no `.env` depois NÃO altera um admin já criado:
para trocar, apague o banco (ou a linha na tabela `Admins`) e suba de novo.
A tela de login do site é `Admin.html`.

## Banco de dados e cardápio inicial

- Há uma única migration (`Inicial`), só com a estrutura das tabelas. Ela é aplicada ao subir a API.
- O cardápio de demonstração (29 produtos) é inserido **somente se a tabela de produtos estiver vazia**
  (`CARDAPIO_DEMO=true`). Para um cliente começar do zero use `CARDAPIO_DEMO=false`.
- Produto já vendido nunca é apagado: ao "excluir" ele é arquivado, para preservar o histórico de pedidos.

## Variáveis de ambiente

| Variável | Para quê |
|---|---|
| `JWT_KEY` | Obrigatória. Mínimo 32 caracteres, única por cliente. |
| `ADMIN_USUARIO`, `ADMIN_SENHA` | Login do admin (senha com 10+ caracteres). |
| `ALLOWED_ORIGINS` | Endereços do site permitidos (CORS), separados por vírgula. Vazio = só localhost. |
| `BEHIND_PROXY` | `true` só se houver proxy reverso na frente (Nginx, Caddy, Cloudflare). |
| `CARDAPIO_DEMO` | `false` para não criar o cardápio de demonstração. |
| `DB_PATH`, `UPLOADS_PATH` | Onde ficam o banco e as imagens enviadas (o Docker usa `/data`). |

## Produção (checklist)

1. Servidor com Docker. Copie `.env.example` para `.env` e preencha (`ALLOWED_ORIGINS=https://seusite.com.br`, `BEHIND_PROXY=true`, `CARDAPIO_DEMO` a seu critério).
2. `docker compose up -d --build`. A API fica em 127.0.0.1:5000, acessível só pelo proxy.
3. Coloque um proxy com **HTTPS** na frente (Caddy ou Nginx + Let's Encrypt) apontando para a porta 5000.
4. **Backup** do volume `dados` (contém `gestaodepedidos.db` e as imagens enviadas).
5. Nunca versionar o `.env`. Use uma `JWT_KEY` diferente para cada cliente (`openssl rand -base64 48`).
6. Se mudar de modelo de dados no futuro, crie migrations novas (`dotnet ef migrations add NomeDaMudanca`); não edite a `Inicial`.

## Segurança já implementada

JWT com emissor/destinatário/papel, rotas de admin protegidas, limite de tentativas por IP e bloqueio de
usuário após 5 senhas erradas, CORS restrito, validação de entrada, preço sempre calculado no servidor,
upload de imagem validado pelo conteúdo do arquivo, cabeçalhos de segurança, erros internos só no log.

## Se algo der errado com a migration

Rode `dotnet build` e depois `dotnet ef migrations add Verificacao`. O método `Up` gerado deve vir **vazio**
(isso confirma que modelo e migration batem). Remova com `dotnet ef migrations remove`.
Se não vier vazio ou a API falhar ao criar o banco, apague a pasta `Migrations`, apague o `.db` e gere de novo:
`dotnet ef migrations add Inicial`.
