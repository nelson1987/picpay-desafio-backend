using Moq;

namespace Simplified.UnitTests;
public class CriacaoTransferenciaConsumerTests
{
    private readonly Mock<IAutorizadorTransferenciaService> _autorizadorService;
    private readonly Mock<IUsuarioRepository> _usuarioRepository;
    private readonly Mock<IMovimentacaoRepository> _movimentacaoRepository;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly Usuario _debitante;
    private readonly Usuario _creditante;
    private readonly CriacaoTransferenciaConsumer _consumer;
    private readonly TransferenciaEvent _event;
    public CriacaoTransferenciaConsumerTests()
    {
        _debitante = new Usuario("Nome", "Cpf", "Email", "Senha") with { Saldo = 10.00M };
        _creditante = new Usuario("Nome", "Outro Cpf", "Outro Email", "Senha") with { Saldo = 0.00M };
        _usuarioRepository
            .Setup(x => x.GetById("123", _cancellationToken))
            .ReturnsAsync(_debitante);
        _usuarioRepository
            .Setup(x => x.GetById("456", _cancellationToken))
            .ReturnsAsync(_creditante);
        _autorizadorService
            .Setup(library => library.Get("https://run.mocky.io/v3/5794d450-d2e2-4412-8131-73d0293ac1cc", _cancellationToken))
            .ReturnsAsync(0);
        //_movimentacaoRepository
        //    .Setup(library => library.Create(_event, _cancellationToken))
        //    .ReturnsAsync(0);
        //_usuarioRepository
        //    .Setup(library => library.Update(_event.Debitante, _cancellationToken))
        //    .ReturnsAsync(0);
        //_usuarioRepository
        //    .Setup(library => library.Update(_event.Creditante, _cancellationToken))
        //    .ReturnsAsync(0);
        _event = new TransferenciaEvent(It.IsAny<Usuario>(), It.IsAny<Usuario>(), 10.00M);
        _consumer = new CriacaoTransferenciaConsumer(_autorizadorService.Object,
            _usuarioRepository.Object,
            _movimentacaoRepository.Object);

    }/*
    Dado_Consumir_Transferencia_Event_Autorizador_Retorna_Mensagem_Erro
    Dado_Consumir_Transferencia_Event_Autorizador_Retorna_Mensagem_Sucesso()*/
    [Fact]
    public async Task AutorizarTransferencia_ComSucessoAsync()
    {
        //Act
        var result = await _consumer.Handle(_event, CancellationToken.None);
        //Assert
        _usuarioRepository.Verify(library => library.GetById(_event.Creditante.Nome, _cancellationToken), Times.AtMostOnce());
        _usuarioRepository.Verify(library => library.GetById(_event.Debitante.Nome, _cancellationToken), Times.AtMostOnce());
        _autorizadorService.Verify(library => library.Get("https://run.mocky.io/v3/5794d450-d2e2-4412-8131-73d0293ac1cc", _cancellationToken), Times.AtMostOnce());
        _movimentacaoRepository.Verify(library => library.Create(_event, _cancellationToken), Times.AtMostOnce());
        _usuarioRepository.Verify(library => library.Update(_event.Debitante, _cancellationToken), Times.AtMostOnce());
        _usuarioRepository.Verify(library => library.Update(_event.Creditante, _cancellationToken), Times.AtMostOnce());

        Assert.Equal(0, result);
    }
}
public class TransferenciaServiceTests
{

    private readonly Mock<IUsuarioRepository> _usuarioRepository;
    private readonly Mock<ICriacaoTransferenciaConsumer> _criacaoTransferenciaProducer;
    private readonly CancellationToken _cancellationToken = CancellationToken.None;
    private readonly TransferenciaService _service;
    private readonly Usuario _debitante;
    private readonly Usuario _creditante;
    public TransferenciaServiceTests()
    {
        _usuarioRepository = new Mock<IUsuarioRepository>();
        _criacaoTransferenciaProducer = new Mock<ICriacaoTransferenciaConsumer>();
        _debitante = new Usuario("Nome", "Cpf", "Email", "Senha") with { Saldo = 10.00M };
        _creditante = new Usuario("Nome", "Outro Cpf", "Outro Email", "Senha") with { Saldo = 0.00M };
        _usuarioRepository
            .Setup(x => x.GetById("123", _cancellationToken))
            .ReturnsAsync(_debitante);
        _usuarioRepository
            .Setup(x => x.GetById("456", _cancellationToken))
            .ReturnsAsync(_creditante);
        _service = new TransferenciaService(_usuarioRepository.Object, _criacaoTransferenciaProducer.Object);
    }

    [Fact]
    public async Task Dado_Realizar_Transferencia_Entre_Usuario_E_Usuario_Com_Sucesso()
    {
        //Act
        var resultado = await _service.RealizarTransferencia("123", "456", 10.00M, _cancellationToken);
        //Assert
        _usuarioRepository.Verify(library => library.GetById("123", _cancellationToken), Times.AtMostOnce());
        _usuarioRepository.Verify(library => library.GetById("456", _cancellationToken), Times.AtMostOnce());
        _criacaoTransferenciaProducer.Verify(library => library.Handle(It.IsAny<TransferenciaEvent>(), _cancellationToken), Times.AtMostOnce());
        Assert.Equal(0, resultado);
    }

    [Fact]
    public async Task Dado_Realizar_Transferencia_Entre_Lojista_E_Lojista_Com_Erro()
    {
        //Arrange
        var debitante = new Lojista("Nome", "Cpf", "Email", "Senha") with { Saldo = 100.00M };
        _usuarioRepository
            .Setup(x => x.GetById("123", _cancellationToken))
            .ReturnsAsync(debitante);
        //Act
        var resultado = await _service.RealizarTransferencia("123", "456", 10.00M, _cancellationToken);
        //Assert
        _usuarioRepository.Verify(library => library.GetById("123", _cancellationToken), Times.AtMostOnce());
        Assert.Equal(-1, resultado);
    }

    [Fact]
    public async Task Dado_Saldo_Usuario_Menor_Que_Valor_Transferencia_Realizar_Transferencia_Com_Erro()
    {
        //Act
        var resultado = await _service.RealizarTransferencia("123", "456", 11.00M, _cancellationToken);
        //Assert
        _usuarioRepository.Verify(library => library.GetById("123", _cancellationToken), Times.AtMostOnce());
        Assert.Equal(-2, resultado);
    }
    /* 
    Dado_Consumir_Transferencia_Event_Autorizador_Retorna_Mensagem_Erro
    Dado_Consumir_Transferencia_Event_Autorizador_Retorna_Mensagem_Sucesso()
    Dado_Autorizador_Retornar_Mensagem_Erro_Concluir_Transferencia_Com_Erro
    Dado_Autorizador_Retornar_Mensagem_Sucesso_Concluir_Transferencia_Com_Sucesso
    Dado_Realizar_Transferencia_Entre_Usuario_E_Lojista_Com_Sucesso
    Dado_Realizar_Transferencia_Entre_Lojista_E_Usuario_Com_Erro
    Dado_Saldo_Usuario_Maior_Que_Valor_Transferencia_Realizar_Transferencia_Com_Sucesso
    Dado_Saldo_Usuario_Igual_Valor_Transferencia_Realizar_Transferencia_Com_Sucesso
    Dado_Haja_Problema_Durante_Transferencia_Retornar_Saldo_Inicial_Das_Contas
    Dado_Pagamento_Recebido_Receber_Notificacao_Atraves_Servico_Externo_Com_Sucesso
    Dado_Pagamento_Recebido_Receber_Notificacao_Atraves_Servico_Externo_Com_Erro
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
    
    
    
    Dado_Usuario_Com_Nome_Cpf_Email_Senha_Confirmar_Dados_Validos
    Dado_Lojista_Com_Nome_Cpf_Email_Senha_Confirmar_Dados_Validos
    Dado_Usuario_Sem_Nome_Cpf_Email_Senha_Confirmar_Dados_Invalidos
    Dado_Lojista_Sem_Nome_Cpf_Email_Senha_Confirmar_Dados_Invalidos
    Dado_Usuario_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Cpf
    Dado_Lojista_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Cnpj
    Dado_Usuario_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Email
    Dado_Lojista_Cadastrado_Nao_Cadastrar_Novo_usuario_Com_Mesmo_Email

*/
}
public record Usuario(string Nome, string Cpf, string Email, string Senha)
{
    public decimal Saldo { get; set; }
};
public record Lojista(string Nome, string Cpf, string Email, string Senha) : Usuario(Nome, Cpf, Email, Senha);
public record TransferenciaEvent(Usuario Debitante, Usuario Creditante, decimal Valor);
public interface IUsuarioRepository
{
    Task<Usuario> GetById(string numeroConta, CancellationToken cancellationToken);
    Task Update(Usuario debitante, CancellationToken cancellationToken);
}
public interface IMovimentacaoRepository
{
    Task Create(TransferenciaEvent @event, CancellationToken cancellationToken);
}
public interface ICriacaoTransferenciaConsumer
{
    Task<int> Handle(TransferenciaEvent @event,
        CancellationToken cancellationToken);
}
public interface IAutorizadorTransferenciaService
{
    Task<int> Get(string uri,
        CancellationToken cancellationToken);
}
public class AutorizadorTransferenciaService : IAutorizadorTransferenciaService
{
    public Task<int> Get(string uri, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
public class TransferenciaService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICriacaoTransferenciaConsumer _criaTransferenciaProducer;
    public TransferenciaService(IUsuarioRepository usuarioRepository,
        ICriacaoTransferenciaConsumer transferenciaRepository)
    {
        _usuarioRepository = usuarioRepository;
        _criaTransferenciaProducer = transferenciaRepository;
    }

    public async Task<int> RealizarTransferencia(string contaDebitante, string contaCreditante, decimal valor,
        CancellationToken cancellationToken)
    {
        var debitante = await _usuarioRepository.GetById(contaDebitante, cancellationToken);
        if (debitante is Lojista)
            return -1;
        if (debitante.Saldo < valor)
            return -2;

        var creditante = await _usuarioRepository.GetById(contaCreditante, cancellationToken);

        await _criaTransferenciaProducer.Handle(new TransferenciaEvent(debitante, creditante, valor), cancellationToken);

        return 0;
    }
}
public class CriacaoTransferenciaConsumer : ICriacaoTransferenciaConsumer
{
    private readonly IAutorizadorTransferenciaService _httpClient;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IMovimentacaoRepository _movimentacaoRepository;

    public CriacaoTransferenciaConsumer(IAutorizadorTransferenciaService httpClient,
        IUsuarioRepository usuarioRepository,
        IMovimentacaoRepository movimentacaoRepository)
    {
        _httpClient = httpClient;
        _usuarioRepository = usuarioRepository;
        _movimentacaoRepository = movimentacaoRepository;
    }

    public async Task<int> Handle(TransferenciaEvent @event, CancellationToken cancellationToken)
    {

        Usuario creditante = await _usuarioRepository.GetById(@event.Creditante.Nome, cancellationToken);
        Usuario debitante = await _usuarioRepository.GetById(@event.Debitante.Nome, cancellationToken);
        var autorizador = await _httpClient.Get("https://run.mocky.io/v3/5794d450-d2e2-4412-8131-73d0293ac1cc", cancellationToken);
        if (autorizador != 0)
            return -3;

        await _movimentacaoRepository.Create(@event, cancellationToken);

        debitante.Saldo -= @event.Valor;
        creditante.Saldo += @event.Valor;

        await _usuarioRepository.Update(@event.Debitante, cancellationToken);
        await _usuarioRepository.Update(@event.Creditante, cancellationToken);

        return 0;
    }
}
public class RecebePagamento
{
    private readonly IAutorizadorTransferenciaService _httpClient;

    public RecebePagamento(IAutorizadorTransferenciaService httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> Handle(CancellationToken cancellationToken)
    {
        var notificacao = await _httpClient.Get("https://run.mocky.io/v3/54dc2cf1-3add-45b5-b5a9-6bf7e7f1f4a6", cancellationToken);
        return 0;
    }
}
