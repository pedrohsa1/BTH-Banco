# Arquitetura

A aplicação é um monolito modular. Sobe um processo só, com dois agregados bem definidos. `Accounts` cuida do saldo, do cheque especial, da situação e dos limites da conta. `Transfers` cuida do andamento da transferência, do limite por hora e dos agendamentos. Cada módulo tem o próprio domínio, o próprio schema e os próprios endpoints. A API apenas reúne, dentro do módulo, cada caso de uso fica junto: comando, validação e endpoint.

O valor sai de uma conta e entra na outra na mesma transação do PostgreSQL. Dois microsserviços, neste momento, trocariam essa transação por uma saga.

O encontro dos dois módulos acontece num lugar só, o `Composition`. Ele abre os dois contextos na mesma conexão, trava as contas e confirma o débito e o crédito juntos. No restante, um módulo não entra no domínio do outro. O que precisa ser avisado depois do commit vai para a outbox, na mesma transação. O Hangfire publica esse evento e dispara a transferência agendada. Publicação e agendamento passam por interface, então o Hangfire pode dar lugar a uma fila, como o RabbitMQ, SQS, Kafka entre outros, sem alterar a regra.

O agendamento entra na fila do Hangfire. O horário é marcado por `ITransferScheduler`, na classe `HangfireTransferScheduler`. Os eventos que saem da outbox seguem por `IEventBus`.

Na hora combinada, `ExecuteScheduledTransferJob` executa a transferência. O `DispatchDueTransfersJob` roda a cada unidade de tempo e pega o que já venceu e não chegou a executar.

A transferência imediata e a agendada leem o header `Idempotency-Key`. A chave fica num índice único e o pedido guarda um hash. Se o mesmo POST voltar, a API devolve a transferência que já existe e não debita de novo.

# Ferramentas

| Ferramenta | Uso |
| --- | --- |
| .NET 10 | API |
| PostgreSQL 16 | Banco de dados |
| React, TypeScript e Vite | Interface |
| Docker | Subir a API, a interface e o banco  |
| Hangfire | Agendamento das transferências e publicação dos eventos |

## Com Docker

Na pasta `bth`:

```bash
docker compose up --build
```

- Interface: http://localhost:8080
- Swagger: http://localhost:5080/swagger
- Hangfire: http://localhost:5080/hangfire

O PostgreSQL fica disponível na porta 5433, com usuário e senha `postgres` e banco `bth_banking`.

Na primeira execução, a API cria as tabelas e as contas abaixo:

- Ana Souza, documento 11111111111, saldo de R$ 500 e cheque especial de R$ 1.000
- Bruno Lima, documento 22222222222, saldo de R$ 2.500
- Carla Dias, documento 33333333333, saldo de R$ 800 e cheque especial de R$ 500
- Diego Rocha, documento 44444444444, saldo de R$ 1.500, conta inativa

## Sem Docker

É necessário ter o SDK do .NET 10, o Node.js e um PostgreSQL 16 em `localhost:5433`, no banco `bth_banking`, com usuário e senha `postgres`.

Para subir apenas o banco:

```bash
docker compose up postgres -d
```

Para subir a API:

```bash
dotnet run --project src/API/Bth.Api
```

O Swagger fica em http://localhost:5080/swagger.

Para subir a interface, em outro terminal:

```bash
cd src/Web
npm install
npm run dev
```

A interface abre em http://localhost:5173.

## Testes

```bash
dotnet test
```

Os testes de integração usam o Docker para subir um PostgreSQL. O Docker precisa estar em execução.
