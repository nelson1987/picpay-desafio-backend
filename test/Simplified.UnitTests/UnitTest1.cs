namespace Simplified.UnitTests;

public class TransferenciaServiceTests
{
    [Fact]
    public void Dado_Usuario_Com_Saldo_Realizar_Transferencia_Com_Sucesso()
    {

    }
    [Fact]
    public void Dado_Usuario_Sem_Saldo_Realizar_Transferencia_Com_Erro()
    {

    }
    [Fact]
    public void Dado_Lojista_Com_Saldo_Realizar_Transferencia_Com_Erro()
    {

    }
    
    
    /*
    Dado_Usuario_Com_Nome_Cpf_Email_Senha_Confirmar_Dados_Validos
    Dado_Lojista_Com_Nome_Cpf_Email_Senha_Confirmar_Dados_Validos
    Dado_Usuario_Sem_Nome_Cpf_Email_Senha_Confirmar_Dados_Invalidos
    Dado_Lojista_Sem_Nome_Cpf_Email_Senha_Confirmar_Dados_Invalidos
    Dado_Usuario_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Cpf
    Dado_Lojista_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Cnpj
    Dado_Usuario_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Email
    Dado_Lojista_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Email
    Dado_Realizar_Transferencia_Entre_Usuario_E_Usuario_Com_Sucesso
    Dado_Realizar_Transferencia_Entre_Usuario_E_Lojista_Com_Sucesso
    Dado_Realizar_Transferencia_Entre_Lojista_E_Lojista_Com_Erro
    Dado_Realizar_Transferencia_Entre_Lojista_E_Usuario_Com_Erro
    Dado_Saldo_Usuario_Menor_Que_Valor_Transferencia_Realizar_Transferencia_Com_Erro
    Dado_Saldo_Usuario_Maior_Que_Valor_Transferencia_Realizar_Transferencia_Com_Sucesso
    Dado_Saldo_Usuario_Igual_Valor_Transferencia_Realizar_Transferencia_Com_Sucesso
    Dado_Autorizador_Retornar_Mensagem_Erro_Concluir_Transferencia_Com_Erro
    Dado_Autorizador_Retornar_Mensagem_Sucesso_Concluir_Transferencia_Com_Sucesso
    Dado_Haja_Problema_Durante_Transferencia_Retornar_Saldo_Inicial_Das_Contas
    Dado_Pagamento_Recebido_Receber_Notificacao_Atraves_Servico_Externo_Com_Sucesso
    Dado_Pagamento_Recebido_Receber_Notificacao_Atraves_Servico_Externo_Com_Erro
*/
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