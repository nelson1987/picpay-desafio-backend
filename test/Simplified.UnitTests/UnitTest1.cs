namespace Simplified.UnitTests;

public class TransferenciaServiceTests
{
    [Fact]
    public void Test1()
    {

    }
}
public record Usuario(string Nome, string Cpf, string Email, string Senha)
{
    public decimal Saldo{get;set;}
};
public record Lojistas(string Nome, string Cpf, string Email, string Senha) : Usuario(Nome, Cpf, Email, Senha);
public record TransferenciaEvent(Usuario Debitante, Usuario Creditante, decimal Valor);
public interface IUsuarioRepository
{
    Task<Usuario> GetById(string numeroConta, 
        CancellationToken cancellationToken);
}
public interface IBus
{
    Task<int> Send(TransferenciaEvent @event, 
        CancellationToken cancellationToken);
}
public interface IHttpClientSimplified 
{
    Task<int> Get(string uri, 
        CancellationToken cancellationToken);
}
public class TransferenciaService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IBus _transferenciaRepository;
    private readonly IHttpClientSimplified _httpClient;
    public TransferenciaService(IUsuarioRepository usuarioRepository,
        IBus transferenciaRepository, 
        IHttpClientSimplified httpClient)
    {
        _usuarioRepository = usuarioRepository;
        _transferenciaRepository = transferenciaRepository;
        _httpClient = httpClient;
    }

    public async Task<int> RealizarTransferencia(string contaDebitante, string contaCreditante, decimal valor, 
        CancellationToken cancellationToken)
    {
        var debitante = await _usuarioRepository.GetById(contaDebitante, cancellationToken);
        if(debitante is Lojistas)
            return -1;
        var creditante = await _usuarioRepository.GetById(contaCreditante, cancellationToken);
        if(debitante.Saldo < valor)
            return -1;
        var autorizador = await _httpClient.Get("https://run.mocky.io/v3/5794d450-d2e2-4412-8131-73d0293ac1cc", cancellationToken);
        if(autorizador !=0)
            return -1;
        await _transferenciaRepository.Send(new TransferenciaEvent(debitante, creditante, valor), cancellationToken);

        return 0;
    }
}
public class RecebePagamento
{
    private readonly IHttpClientSimplified _httpClient;

    public RecebePagamento(IHttpClientSimplified httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> Handle(CancellationToken cancellationToken)
    {
        var notificacao = await _httpClient.Get("https://run.mocky.io/v3/54dc2cf1-3add-45b5-b5a9-6bf7e7f1f4a6",cancellationToken);
        return 0;
    }
}
/*
## Objetivo: PicPay Simplificado

Temos 2 tipos de usuários, os comuns e lojistas, ambos têm carteira com dinheiro e 
realizam transferências entre eles. Vamos nos atentar **somente** ao fluxo de transferência entre dois usuários.

Requisitos:

-   Para ambos tipos de usuário, precisamos do Nome Completo, CPF, e-mail e Senha. 
    CPF/CNPJ e e-mails devem ser únicos no sistema. 
    Sendo assim, seu sistema deve permitir apenas um cadastro com o mesmo CPF ou endereço de e-mail.
-   A operação de transferência deve ser uma transação (ou seja, revertida em qualquer caso de inconsistência) 
    e o dinheiro deve voltar para a carteira do usuário que envia.
-   Este serviço deve ser RESTFul.

### Payload

Faça uma **proposta** :heart: de payload, se preferir, temos uma exemplo aqui:

POST /transaction

```json
{
    "value": 100.0,
    "payer": 4,
    "payee": 15
}
```
*/