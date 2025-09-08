# ASP.NET Core Web API Serverless Application

This project shows how to run an ASP.NET Core Web API project as an AWS Lambda exposed through Amazon API Gateway. The NuGet package [Amazon.Lambda.AspNetCoreServer](https://www.nuget.org/packages/Amazon.Lambda.AspNetCoreServer) contains a Lambda function that is used to translate requests from API Gateway into the ASP.NET Core framework and then the responses from ASP.NET Core back to API Gateway.


For more information about how the Amazon.Lambda.AspNetCoreServer package works and how to extend its behavior view its [README](https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.AspNetCoreServer/README.md) file in GitHub.


### Configuring for API Gateway HTTP API ###

API Gateway supports the original REST API and the new HTTP API. In addition HTTP API supports 2 different
payload formats. When using the 2.0 format the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.
For the 1.0 payload format the base class is the same as REST API which is `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction`.
**Note:** when using the `AWS::Serverless::Function` CloudFormation resource with an event type of `HttpApi` the default payload
format is 2.0 so the base class of `LambdaEntryPoint` must be `Amazon.Lambda.AspNetCoreServer.APIGatewayHttpApiV2ProxyFunction`.


### Configuring for Application Load Balancer ###

To configure this project to handle requests from an Application Load Balancer instead of API Gateway change
the base class of `LambdaEntryPoint` from `Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction` to 
`Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction`.

### Project Files ###

* serverless.template - an AWS CloudFormation Serverless Application Model template file for declaring your Serverless functions and other AWS resources
* aws-lambda-tools-defaults.json - default argument settings for use with Visual Studio and command line deployment tools for AWS
* LambdaEntryPoint.cs - class that derives from **Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction**. The code in 
this file bootstraps the ASP.NET Core hosting framework. The Lambda function is defined in the base class.
Change the base class to **Amazon.Lambda.AspNetCoreServer.ApplicationLoadBalancerFunction** when using an 
Application Load Balancer.
* LocalEntryPoint.cs - for local development this contains the executable Main function which bootstraps the ASP.NET Core hosting framework with Kestrel, as for typical ASP.NET Core applications.
* Startup.cs - usual ASP.NET Core Startup class used to configure the services ASP.NET Core will use.
* appsettings.json - used for local development.
* Controllers\ValuesController - example Web API controller

You may also have a test project depending on the options selected.

## Here are some steps to follow from Visual Studio:

To deploy your Serverless application, right click the project in Solution Explorer and select *Publish to AWS Lambda*.

To view your deployed application open the Stack View window by double-clicking the stack name shown beneath the AWS CloudFormation node in the AWS Explorer tree. The Stack View also displays the root URL to your published application.

## Here are some steps to follow to get started from the command line:

Once you have edited your template and code you can deploy your application using the [Amazon.Lambda.Tools Global Tool](https://github.com/aws/aws-extensions-for-dotnet-cli#aws-lambda-amazonlambdatools) from the command line.

Install Amazon.Lambda.Tools Global Tools if not already installed.
```
    dotnet tool install -g Amazon.Lambda.Tools
```

If already installed check if new version is available.
```
    dotnet tool update -g Amazon.Lambda.Tools
```

Execute unit tests
```
    cd "LambdaApi/test/LambdaApi.Tests"
    dotnet test
```

Deploy application
```
    cd "LambdaApi/src/LambdaApi"
    dotnet lambda deploy-serverless
```

# Helps:
Create AWS IAM User:
1. Login console AWS or create account.
2. Acesse AWS service IAM -> Users -> Create user.
3. Create user
4. Define user
5. Select Add user to group -> Next
6. Create user
7. Acesse User -> Security credentials -> Access keys -> Create access key -> Command Line Interface (CLI) -> Create access key.

Copy Key and Secret:
1. Copy AWS Access Key ID and AWS Secret Access Key.

Create User groups:
1. Create group
2. Permissions -> Add permissions -> AWSLambda_FullAccess, AmazonDynamoDBFullAccess
3. In user create -> Groups -> Add user to groups -> Add Group to user
5. Add permissions
    AmazonDynamoDBFullAccess
    AmazonS3FullAccess
    AWSCloudFormationFullAccess
    AWSLambda_FullAccess

Configure AWS local machine:
1. aws configure
2. AWS_Access_Key_ID
3. AWS_Secret_Access_Key
4. sa-east-1
5. yaml
6. Teste:aws sts get-caller-identity
    Account: '89XXXXXXXXXX'
    Arn: arn:aws:iam::89XXXXXXXXXX:user/UserCriado

Create table DynamoDB:
aws dynamodb create-table \
    --table-name MessagesTable \
    --attribute-definitions AttributeName=Id,AttributeType=S \
    --key-schema AttributeName=Id,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

Create bucket S3 para deploy:
aws s3 mb s3://deploy-whatsapp-bucket --region sa-east-1

Tools to Deploy da Lambda:
dotnet tool install -g Amazon.Lambda.Tools

Deploy APP in AWS:
1. Na raiz do seu projeto cd \lambdaapi
2. Run:
    ```
    dotnet lambda deploy-serverless
    ```
3. Enter CloudFormation Stack Name: (CloudFormation stack name for an AWS Serverless application)
    ```
    cf-deploy-whatsapp
    ```
4. Enter CloudFormation Template: (Path to the CloudFormation template)
    ```
    serverless.template
    ```
5. Enter S3 Bucket: (S3 bucket to upload the build output)
    ```
    deploy-whatsapp-bucket
    ```

Install templates:
1. dotnet new install Amazon.Lambda.Templates
2. dotnet tool install -g Amazon.Lambda.Tools
3. dotnet tool update -g Amazon.Lambda.Tools
4. dotnet tool uninstall -g Amazon.Lambda.Tools

Create projects:
dotnet new serverless.AspNetCoreWebAPI --name LambdaApi
dotnet new worker -n LambdaApi.Workers

Packages Add:
dotnet add package AWSSDK.DynamoDBv2
dotnet add package Swashbuckle.AspNetCore
dotnet add package AWSSDK.SQS
dotnet add package Amazon.Lambda.SQSEvents

Run APP with SLN:
1. dotnet restore LambdaApi.sln
2. dotnet build LambdaApi.sln
3. cd folder API
4. dotnet run --project LambdaApi.csproj


🔹 Fluxo Técnico de Mensagens
[Remetente (App)] 
       |
       | 1️⃣ Criptografa mensagem (Signal / E2EE)
       |    messageId, senderId, recipientId, content (criptografado)
       v
[Servidor WhatsApp / Backend]
       |
       | Recebe mensagem → armazena metadados
       | Retorna ACK ao remetente
       v
[Remetente] -----> marca ✓ (enviado)
       |
       | 2️⃣ Entrega para destinatário (quando online)
       v
[Destinatário (App)]
       |
       | Recebe mensagem criptografada
       | Envia delivery receipt (criptografado) ao servidor
       v
[Servidor] 
       |
       | Atualiza status entrega → envia update para remetente
       v
[Remetente] -----> marca ✓✓ (entregue)
       |
       | 3️⃣ Destinatário abre conversa
       v
[Destinatário]
       |
       | Envia read receipt (criptografado) ao servidor
       v
[Servidor]
       |
       | Atualiza status leitura → envia update para remetente
       v
[Remetente] -----> marca ✓✓ azul (lida)


🔹 Metadados por mensagem //TODO - Metadados por mensagem

| Metadado      | Função                          | Armazenamento / Segurança                   |
| ------------- | ------------------------------- | ------------------------------------------- |
| `messageId`   | Identificador único da mensagem | Gerado no app, usado para todos os receipts |
| `senderId`    | Quem enviou a mensagem          | Criptografado ou apenas ID no servidor      |
| `recipientId` | Quem vai receber                | Criptografado                               |
| `sentAt`      | Timestamp do envio              | Gravado no servidor                         |
| `deliveredAt` | Timestamp da entrega            | Recebido via delivery receipt               |
| `readAt`      | Timestamp da leitura            | Recebido via read receipt, opcional         |
| `status`      | `sent`, `delivered`, `read`     | Atualizado pelo servidor                    |


✅ Resumo técnico seguro e robusto: //TODO - Seguro e robusto

Mensagem criptografada ponta a ponta → servidor nunca lê.
✓: servidor recebeu a mensagem (ACK).
✓✓: destinatário recebeu a mensagem (delivery receipt).
✓✓ azul: destinatário leu a mensagem (read receipt).
Logs, timestamps e metadados são gravados de forma auditável, sem violar a privacidade.


1️⃣ Mensagem enviada (✓)

Como tratar tecnicamente:
Criptografia ponta a ponta (E2EE):
Cada mensagem é criptografada no dispositivo do remetente usando uma chave única compartilhada com o destinatário (ex.: protocolo Signal).
Envio ao servidor:
A mensagem vai somente criptografada, sem possibilidade do servidor ler.
Confirmação de recebimento do servidor:
O servidor grava a mensagem no banco e retorna um ACK ao cliente remetente.
Esse ACK é o que gera o primeiro tique (✓).
Boas práticas:
Uso de mensagens idempotentes: cada mensagem tem um messageId único para evitar duplicatas.
Logs de auditoria de recebimento, sem armazenar conteúdo.


2️⃣ Mensagem entregue (✓✓)

Como tratar tecnicamente:
Quando o destinatário estiver online, o servidor envia a mensagem para o app dele.
O app do destinatário reconhece a mensagem e envia de volta um delivery receipt criptografado.
O servidor registra internamente que a mensagem foi entregue e envia um update para o remetente (✓✓ cinza).
Boas práticas:
Retry automático em caso de falha (destinatário offline).
Garantir ordem de mensagens via timestamps e sequência.
Persistência temporária: mensagens pendentes devem expirar após certo tempo se não forem entregues.


3️⃣ Mensagem lida (✓✓ azul)

Como tratar tecnicamente:
Quando o destinatário abre a conversa:
O app envia um read receipt ao servidor (também criptografado).
O servidor repassa a confirmação de leitura para o remetente.
O remetente só marca os tiques azuis após receber esse recibo.
Boas práticas:
Respeitar configurações de privacidade (ex.: desativar confirmações de leitura).
Garantir que read receipts sejam criptografados e vinculados à mensagem correta.
Registrar hora da leitura como metadado, sem acessar o conteúdo da mensagem.


4️⃣ Casos especiais

Modo avião / sem internet:
Mensagem permanece com ✓ até que o dispositivo do destinatário receba.
Bloqueio:
Servidor rejeita a entrega, o remetente só vê ✓.
Confirmação de leitura desativada:
Mensagens entregues → ✓✓ cinza; nunca aparecem azuis.
Grupos:
✓✓ cinza: todos receberam.
✓✓ azul: todos leram.
O servidor mantém tracking de delivery/read por usuário em grupo.


5️⃣ Armazenamento de metadados seguro

Para implementar de forma robusta:
Metadado	Função	Segurança / Observações
messageId	Identificação única da mensagem	Criptografia ponta a ponta
senderId	ID do remetente	Sem expor no corpo da mensagem
recipientId	ID do destinatário	Criptografado
sentAt	Timestamp de envio	Servidor confiável
deliveredAt	Timestamp de entrega	Mantido sem acessar conteúdo
readAt	Timestamp de leitura	Opcional, respeitando privacidade
status	sent/delivered/read	Atualizado com recibos criptografados


Solução robusta Arquitetura
Flutter (persist local + queue) 
   ⇩ (HTTPS)
API Gateway -> Lambda API (.NET) -> DynamoDB (Messages table)
                                 ↳ SQS FIFO (delivery pipeline) -> Lambda (deliver to device via push/WebSocket)
                                 ↳ DynamoDB Streams -> Lambda (reconciliação / auditing)
SNS / FCM / APNs for push
CloudWatch + DLQ + alarms

Principais garantias:
Idempotência: evitar duplicação de mensagens/ops.
Entrega com retries + DLQ: SQS FIFO + DLQ para at-least-once + ordenação por conversa.
Controle de versão / concorrência otimista: evitar sobrescritas de status regressivas.
Reconciliation: job que reconcilia mensagens em estados inconsistentes.
Client-first durability: app persiste mensagens localmente até confirmação (ACK).


Modelagem da tabela (DynamoDB):

[DynamoDBTable("MessagesTable")]
public class Message
[DynamoDBHashKey]
public string MessageId { get; set; } = Guid.NewGuid().ToString();

public string ConversationId { get; set; }
public string SenderId { get; set; }
public string RecipientId { get; set; }

// O conteúdo é criptografado no cliente; aqui é apenas um blob cifrado
public string Content { get; set; }

public DateTime SentAt { get; set; } = DateTime.UtcNow;
public DateTime? DeliveredAt { get; set; }
public DateTime? ReadAt { get; set; }

public string Status { get; set; } = "sent"; // sent, delivered, read, dead

public int Version { get; set; } = 0; // optimistic concurrency
public string IdempotencyKey { get; set; }
public int DeliveryAttempts { get; set; } = 0;

// TTL (Unix epoch seconds) - DynamoDB TTL enabled on ExpiresAtUnix
public long? ExpiresAtUnix { get; set; }


Idempotência no envio (server-side)
Cliente sempre gera MessageId e IdempotencyKey (podem ser iguais).
No servidor, use PutItem com ConditionExpression para attribute_not_exists(MessageId) — evita duplicatas se o cliente reenviar.
Exemplo com PutItemAsync (low-level) ou DynamoDBContext com TransactWrite:

// usando low-level PutItem com condição
var request = new PutItemRequest
{
    TableName = "MessagesTable",
    Item = new Dictionary<string, AttributeValue>
    {
        ["MessageId"] = new AttributeValue { S = message.MessageId },
        ["ConversationId"] = new AttributeValue { S = message.ConversationId },
        // ... outros atributos
        ["IdempotencyKey"] = new AttributeValue { S = message.IdempotencyKey }
    },
    ConditionExpression = "attribute_not_exists(MessageId)"
};

try
{
    await dynamoDb.PutItemAsync(request);
}
catch (ConditionalCheckFailedException)
{
    // já existe -> idempotente: retornar existente (ou OK)
}



Atualização de status com controle de versão (evitar regressões)
Use Version (integer) ou timestamps e ConditionExpression para permitir apenas upgrades de status (sent → delivered → read).
Exemplo: só atualize para delivered se Status <> 'delivered' AND Status <> 'read' ou se DeliveredAt < :new.
(ajuste :read e :zero como necessário). A ideia: condição que impede downgrade.

var updateRequest = new UpdateItemRequest
{
    TableName = "MessagesTable",
    Key = new Dictionary<string, AttributeValue> { ["MessageId"] = new AttributeValue { S = id } },
    ExpressionAttributeNames = new Dictionary<string,string>
    {
        ["#S"] = "Status",
        ["#DA"] = "DeliveredAt",
        ["#V"] = "Version"
    },
    ExpressionAttributeValues = new Dictionary<string,AttributeValue>
    {
        [":del"] = new AttributeValue { S = "delivered" },
        [":now"] = new AttributeValue { S = DateTime.UtcNow.ToString("o") },
        [":one"] = new AttributeValue { N = "1" }
    },
    UpdateExpression = "SET #S = :del, #DA = :now, #V = if_not_exists(#V, :zero) + :one",
    ConditionExpression = "attribute_not_exists(#S) OR #S <> :del AND #S <> :read",
};


Pipeline de entrega confiável (SQS FIFO + DLQ):

Em vez de enviar diretamente ao dispositivo, publique um evento (delivery job) em SQS FIFO com MessageGroupId = ConversationId e MessageDeduplicationId = MessageId/IdempotencyKey.
Um consumer Lambda processa fila:
Tenta enviar via push (FCM/APNs) ou WebSocket.
Se sucesso → chama endpoint /{id}/delivered (ou atualiza DynamoDB).
Se falha → reenqueue (SQS faz retry) até maxReceiveCount → DLQ.
SQS FIFO garante ordenação por conversa e deduplicação.


Garantia no client (Flutter):
Persistir mensagens em storage local (sqflite) com status local: queued → sending → sent → delivered → read.
Gerenciar uma fila local: se envio falhar, retry com backoff + jitter; reenvio idempotente usando MessageId.
Ao criar mensagem, somente remove local quando receber ACK do servidor (200 + persisted).
Use WebSockets (API Gateway WebSocket) ou push notifications para receber delivery/read updates “em tempo real”.


Reconciliation e recuperação:
Job (Lambda) periódico:
Escaneia mensagens com status = sent ou delivered e DeliveryAttempts < N → reenqueue para SQS.
Marca dead após TTL / limite de tentativas (e coloca em DLQ para auditoria).
Use DynamoDB Streams para acionar auditoria e sincronização entre replicas se tiver mais de uma store.


Observabilidade e alarmes:
Publique métricas no CloudWatch:
mensagens recebidas/salvas
entregas bem-sucedidas
mensagens em DLQ
latência de entrega
Configure alarms para: aumento de mensagens em DLQ, taxa de falhas de envio, crescimento de DeliveryAttempts.


Segurança e privacidade:
Conteúdo sempre criptografado no cliente; servidor só armazena blob cifrado.
Receipts e metadados: armazenar mínimos necessários e aplicar retenção/TTL.
IAM: Lambdas só com permissões necessárias (principle of least privilege).
GDPR / LGPD: TTL para remoção e endpoints para exclusão de mensagens.


Exemplos concretos (C#) — snippets práticos:
SendMessage com idempotência (DynamoDB Put com condição) + enfileirar SQS
MarkDelivered com condição (evitar regressão)


Grupo / multi-recipient
Mantenha uma tabela DeliveryStatus por (MessageId, RecipientId) para registrar status por usuário em grupo.
Use transações (DynamoDB TransactWrite) para gravar mensagem + registros de delivery atomically se necessário.


Recomendações operacionais rápidas:
Use SQS FIFO para pipeline de entrega (ordenado + dedup).
Use DLQ para não perder mensagens e auditar falhas.
TTL (DynamoDB) para mensagens expiradas (ex.: ExpiresAtUnix).
Backoff exponencial no client e no worker com jitter.
Testes de falha: simule dispositivos offline, perda de push, latência, e verifique reconciler.


Este documento contém o conjunto de alterações sugeridas para transformar seu LambdaApi em uma solução robusta de mensagens com idempotência, SQS FIFO para entrega confiável, reconciliação, e DynamoDB com controle de versão.

Arquivos incluídos:
src/LambdaApi/Models/Message.cs
src/LambdaApi/Controllers/MessagesController.cs
src/LambdaApi/Services/DeliveryQueueService.cs
src/LambdaApi/Workers/SqsDeliveryWorker.cs (Lambda handler skeleton)
src/LambdaApi/Startup.cs (ajustes)
sam-template.yaml (SAM: DynamoDB, SQS FIFO + DLQ, API Lambda, Delivery Lambda, roles)
deploy/README.md (instruções de deploy e testes)


📐 Arquitetura Atual – Sistema de Mensageria Estilo WhatsApp
🔹 Componentes Principais

1. API Gateway
    - Exposição pública da API HTTP.
    - Roteia chamadas dos clientes Flutter → para a Lambda de API.
* Lambda API (ASP.NET Core)
    - Implementada com serverless.AspNetCoreWebAPI.
1. Oferece endpoints REST para envio de mensagens e consulta de status.
- Publica mensagens em uma fila SQS (DeliveryQueue).
* Persiste o estado inicial da mensagem no DynamoDB (MessagesTable).

SQS (DeliveryQueue)

Fila de mensagens (FIFO para garantir ordem e idempotência).

Recebe mensagens da API Lambda.

Ativa o processamento assíncrono pelo Worker Lambda.

Lambda Worker (SqsDeliveryWorker)

Trigger do SQS.

Consome mensagens, processa regras de negócio, atualiza status no DynamoDB.

Pode reenfileirar mensagens em caso de falha ou enviar para DLQ (Dead Letter Queue).

DynamoDB (MessagesTable)

Banco de dados NoSQL.

Armazena mensagens com metadados: MessageId, SenderId, RecipientId, SentAt, DeliveredAt, ReadAt, Status.

S3

Armazena artefatos de deploy (pacotes Lambda).

Armazena também possíveis anexos de mensagens (fotos, vídeos, documentos).

CloudFormation / SAM

Infraestrutura como código.

Provisiona API Gateway, Lambdas, DynamoDB, SQS e permissões IAM.

Controla versões e rollback da infra.

🔹 Fluxo de Deploy

Desenvolvimento

Código em C# (.NET Core) → Lambda API e Worker.

Templates SAM/CloudFormation → definem toda a infra.

Build

dotnet publish gera o pacote das Lambdas.

Artefatos são enviados para S3.

Deploy

sam deploy --guided aplica o template.

CloudFormation cria/atualiza:

API Gateway

Lambda API

Lambda Worker

SQS

DynamoDB

permissões IAM

Resultado

Infra provisionada, versão controlada e pronta para receber tráfego.

🔹 Fluxo da Aplicação
1. Envio de mensagem

Cliente Flutter chama POST /messages no API Gateway.

API Gateway aciona a Lambda API.

Lambda API:

Gera MessageId.

Persiste no DynamoDB com status Sent.

Publica a mensagem na SQS DeliveryQueue.

Retorna 200 OK ao cliente.

2. Entrega

A Lambda Worker é acionada pela SQS.

Worker:

Lê mensagem da fila.

Atualiza no DynamoDB → status Delivered.

Pode acionar push notification (SNS ou Firebase).

3. Leitura

Quando o destinatário abre a mensagem, o app envia PUT /messages/{id}/read.

API Gateway → Lambda API.

Lambda API atualiza DynamoDB → status Read.

4. Consulta

Cliente pode consultar histórico via GET /messages.

Lambda API busca no DynamoDB e retorna.