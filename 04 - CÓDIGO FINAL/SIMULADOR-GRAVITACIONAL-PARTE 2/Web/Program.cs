using System.Globalization;
using SimuladorGravitacional_N2;
using TrabalhoN2;

/* Este projeto implementa uma simulação de um universo com corpos celestes, permitindo a criação, manipulação e exportação de estados do universo. 
 * A aplicação é construída utilizando ASP.NET Core e fornece uma API RESTful para interação com o universo simulado. */
var builder = WebApplication.CreateBuilder(args);

/* Configura o caminho de armazenamento dos universos salvos. Se não for especificado, utiliza o diretório pai do diretório de conteúdo da aplicação. */
builder.WebHost.UseUrls("http://localhost:5080");

/* Configura o diretório raiz para servir arquivos estáticos, como HTML, CSS e JavaScript. */
builder.WebHost.UseWebRoot(Path.Combine(builder.Environment.ContentRootPath, "wwwroot"));

/* Configura a aplicação para aceitar e retornar dados JSON com nomes de propriedades insensíveis a maiúsculas e minúsculas. */
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNameCaseInsensitive = true);

/* Configura o diretório de armazenamento dos universos salvos. Se não for especificado, utiliza o diretório pai do diretório de conteúdo da aplicação. */
var app = builder.Build();
var storage = Path.GetFullPath(builder.Configuration["UniverseStoragePath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, ".."));
var recorder = new GravadorArquivoTexto();
var fileLock = new object();

/* Middleware para tratamento de exceções e retorno de erros em JSON.Captura ArgumentException, FormatException, OverflowException, InvalidDataException e IndexOutOfRangeException como erros 400 (Bad Request). Captura IOException como erro 409 (Conflict) com mensagem genérica. */
app.Use(async (context, next) =>
{
    try { await next(context); }
    catch (Exception ex) when (ex is ArgumentException or FormatException or OverflowException or InvalidDataException or IndexOutOfRangeException)
    {
        /* Retorna um erro 400 (Bad Request) com a mensagem da exceção. */
        context.Response.StatusCode = 400;
        await context.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
    catch (IOException)
    {
        /* Retorna um erro 409 (Conflict) com uma mensagem genérica para evitar expor detalhes do sistema de arquivos. */
        context.Response.StatusCode = 409;
        await context.Response.WriteAsJsonAsync(new { error = "Não foi possível acessar o arquivo. Tente novamente." });
    }
});
/*  * Middleware para habilitar CORS (Cross-Origin Resource Sharing) para permitir requisições de qualquer origem. */
app.UseDefaultFiles();
app.UseStaticFiles();

/*  * Rotas da API: */
app.MapGet("/api/health", (HttpResponse response) =>
{
    response.Headers.CacheControl = "no-store";
    return new { status = "ok", application = "TrabalhoN1", storagePath = storage, processId = Environment.ProcessId };
});

/*  * Rota para criar um novo universo com corpos aleatórios. */
app.MapPost("/api/universes/create", (CreateRequest request) =>
{
    /* Valida se a requisição contém um número válido de corpos para criar. */
    if (request.Count < 1) throw new ArgumentException("A quantidade de corpos deve ser maior que zero.");
    var universe = new Universo { QuantidadeIteracoes = 10000, TempoEntreIteracoes = 3600 };

    /* Gera corpos aleatórios no universo e calcula as forças entre eles. */
    universe.GerarCorposAleatorios(request.Count);
    universe.CalcularForcas();
    return Snapshot.From(universe, 0, 0);
});

/* Rota para avançar a simulação em um número de passos. */
app.MapPost("/api/simulation/step", (StepRequest request) =>
{
    ArgumentNullException.ThrowIfNull(request.State);

    /* Converte o estado do universo recebido na requisição para um objeto Universo. */
    var universe = request.State.ToUniverse();
    if (request.Steps < 1 || request.Steps > 100) throw new ArgumentException("Quantidade de passos inválida.");

    /* Calcula o número de passos a serem executados, garantindo que não ultrapasse o número total de iterações do universo. */
    var steps = Math.Min(request.Steps, universe.QuantidadeIteracoes - request.State.Iteration);
    for (var i = 0; i < steps; i++)
    {
        /* Calcula as forças entre os corpos, atualiza suas posições e trata possíveis colisões. */
        universe.CalcularForcas();
        universe.AtualizarPosicoes(universe.TempoEntreIteracoes);
        universe.TratarColisoes();
        ValidateBodies(universe.Corpos);
    }

    /* Retorna o estado atualizado do universo após os passos executados. */
    universe.CalcularForcas();
    return Snapshot.From(universe, request.State.Iteration + steps,
        request.State.Elapsed + steps * universe.TempoEntreIteracoes);
});

/*  Rota para listar os universos salvos no diretório de armazenamento. */
app.MapGet("/api/universes", () => Directory.GetFiles(storage, "universo_*.txt")
    .Select(path => new { name = Path.GetFileName(path), modified = File.GetLastWriteTimeUtc(path), size = new FileInfo(path).Length })
    .OrderByDescending(file => file.modified));

/*  Rota para salvar o estado atual do universo em um arquivo. */
app.MapPost("/api/universes/save", (Snapshot state) =>
{
    /* Converte o estado do universo recebido na requisição para um objeto Universo. */
    var universe = state.ToUniverse();
    lock (fileLock)
    {
        var number = 1;
        while (File.Exists(Path.Combine(storage, $"universo_{number}.txt"))) number++;
        var name = $"universo_{number}.txt";
        recorder.Salvar(universe, Path.Combine(storage, name));
        return Results.Ok(new { name });
    }
});

/*  Rota para carregar um universo salvo a partir de um arquivo. */
app.MapGet("/api/universes/{name}", (string name) =>
{
    /* Valida o nome do arquivo e garante que ele siga o padrão "universo_{número}.txt". */
    var path = SafePath(name);
    if (!File.Exists(path)) return Results.NotFound(new { error = "Universo não encontrado." });
    var universe = recorder.Carregar(path);

    /* Valida os corpos do universo carregado para garantir que todos os parâmetros estejam dentro dos limites aceitáveis. */
    ValidateBodies(universe.Corpos);
    var state = Snapshot.From(universe, 0, 0);
    state.ToUniverse();
    return Results.Ok(state);
});

/*  Rota para deletar um universo salvo a partir de um arquivo. */
app.MapDelete("/api/universes/{name}", (string name) =>
{
    /* Valida o nome do arquivo e garante que ele siga o padrão "universo_{número}.txt". */
    var path = SafePath(name);
    lock (fileLock)
    {
        /* Verifica se o arquivo existe antes de tentar deletá-lo. Se não existir, retorna um erro 404 (Not Found). */
        if (!File.Exists(path)) return Results.NotFound(new { error = "Universo não encontrado." });
        File.Delete(path);
    }
    return Results.NoContent();
});

/*  Rota para exportar o estado atual do universo em um arquivo de texto. */
app.MapPost("/api/universes/export", (Snapshot state) =>
{
    var universe = state.ToUniverse();

    /* Cria uma lista de linhas para o arquivo de exportação, começando com o cabeçalho contendo a quantidade de corpos, quantidade de iterações e tempo entre iterações. */
    var lines = new List<string> { $"{universe.Corpos.Count};{universe.QuantidadeIteracoes};{universe.TempoEntreIteracoes.ToString("R", CultureInfo.InvariantCulture)}" };

    /* Adiciona uma linha para cada corpo no universo, contendo os dados do corpo separados por ponto e vírgula. */
    lines.AddRange(universe.Corpos.Select(c => string.Join(';', c.Nome,
        c.Massa.ToString("R", CultureInfo.InvariantCulture), c.Densidade.ToString("R", CultureInfo.InvariantCulture),
        c.PosX.ToString("R", CultureInfo.InvariantCulture), c.PosY.ToString("R", CultureInfo.InvariantCulture),
        c.VelX.ToString("R", CultureInfo.InvariantCulture), c.VelY.ToString("R", CultureInfo.InvariantCulture))));

    /* Retorna o arquivo de exportação como um resultado de arquivo, com o conteúdo codificado em UTF-8 e o tipo MIME definido como "text/plain". */
    return Results.File(System.Text.Encoding.UTF8.GetBytes(string.Join('\n', lines)), "text/plain; charset=utf-8", "universo_exportado.txt");
});

/*  Rota para importar um universo a partir de um arquivo de texto enviado no corpo da requisição. */
app.MapPost("/api/universes/import", async (HttpRequest request) =>
{
    /* Lê o conteúdo do corpo da requisição como uma string. */
    using var reader = new StreamReader(request.Body);
    var content = await reader.ReadToEndAsync();

    /* Valida se o tamanho do arquivo é maior que 100 KB. */
    if (content.Length > 100000) throw new ArgumentException("Arquivo muito grande (máximo 100 KB).");
    var lines = content.Trim().Split('\n', StringSplitOptions.RemoveEmptyEntries);

    /* Valida se o arquivo possui pelo menos duas linhas (uma para o cabeçalho e pelo menos uma para um corpo). */
    if (lines.Length < 2) throw new ArgumentException("Arquivo de universo inválido.");
    var header = lines[0].Trim().Split(';');

    /* Valida se o cabeçalho possui exatamente três campos (quantidade de corpos, quantidade de iterações e tempo entre iterações). */
    if (header.Length != 3) throw new ArgumentException("Cabeçalho inválido.");
    var count = int.Parse(header[0]);

    /* Valida se a quantidade de corpos é válida (entre 1 e 100) e se o número de linhas no arquivo corresponde à quantidade de corpos especificada. */
    if (count < 1 || lines.Length != count + 1) throw new ArgumentException("Quantidade de corpos inválida no arquivo.");
    var state = new Snapshot([], int.Parse(header[1]), Parse(header[2]), 0, 0);

    /* Itera sobre cada linha do arquivo (exceto o cabeçalho) para criar os corpos e adicioná-los ao estado do universo. */
    foreach (var line in lines.Skip(1))
    {
        var fields = line.Trim().Split(';');
        if (fields.Length != 7) throw new ArgumentException("Cada corpo deve ter sete campos.");
        state.Corpos.Add(new Corpo(fields[0], Parse(fields[1]), Parse(fields[2]), Parse(fields[3]), Parse(fields[4]), Parse(fields[5]), Parse(fields[6])));
    }
    /* Converte o estado do universo para um objeto Universo, calcula as forças e retorna o estado atualizado. */
    var universe = state.ToUniverse();
    universe.CalcularForcas();
    return Snapshot.From(universe, 0, 0);
});

/* Inicia a aplicação web. */
app.Run();

/* Funções auxiliares. */
double Parse(string value) => double.Parse(value.Trim().Replace(',', '.'), CultureInfo.InvariantCulture);

/* Função para validar o nome do arquivo e garantir que ele siga o padrão "universo_{número}.txt". */
string SafePath(string name)
{
    if (!System.Text.RegularExpressions.Regex.IsMatch(name, @"^universo_[0-9]+\.txt$"))
        throw new ArgumentException("Nome de arquivo inválido.");
    return Path.Combine(storage, name);
}

/* Função para validar os corpos do universo, garantindo que todos os parâmetros estejam dentro dos limites aceitáveis. */
static void ValidateBodies(List<Corpo> bodies) => Snapshot.ValidateBodies(bodies);

/* Classes de requisição para a API. */
record CreateRequest(int Count);

/* Classe de requisição para avançar a simulação em um número de passos. */
record StepRequest(Snapshot State, int Steps);

/* Classe que representa o estado de um universo em um determinado ponto da simulação. */
record Snapshot(List<Corpo> Corpos, int QuantidadeIteracoes, double TempoEntreIteracoes, int Iteration, double Elapsed)
{
    public static Snapshot From(Universo u, int iteration, double elapsed)
    {
        ValidateBodies(u.Corpos);
        if (u.Corpos.Any(c => new[] { c.ForcaX, c.ForcaY, c.AceleracaoX, c.AceleracaoY }.Any(v => !double.IsFinite(v))))
            throw new ArgumentException("O cálculo excedeu a precisão numérica. Reduza o passo de tempo ou aumente a distância entre os corpos.");
        return new(u.Corpos, u.QuantidadeIteracoes, u.TempoEntreIteracoes, iteration, elapsed);
    }
    /* Converte o Snapshot de volta para um Universo, validando os dados. */
    public Universo ToUniverse()
    {
        /* Valida os parâmetros do Snapshot antes de criar o Universo. */
        if (QuantidadeIteracoes < 1 || QuantidadeIteracoes > 1000000) throw new ArgumentException("Use de 1 a 1.000.000 iterações.");

        if (!double.IsFinite(TempoEntreIteracoes) || TempoEntreIteracoes < .001 || TempoEntreIteracoes > 86400)
            throw new ArgumentException("O passo de tempo deve estar entre 0,001 e 86.400 segundos.");

        /* Valida o estado da simulação (iteração e tempo decorrido). */
        if (Iteration < 0 || Iteration > QuantidadeIteracoes || !double.IsFinite(Elapsed) || Elapsed < 0)
            throw new ArgumentException("Estado da simulação inválido.");
        ValidateBodies(Corpos);

        /*  Cria um novo Universo com os corpos e parâmetros validados. */
        var u = new Universo { QuantidadeIteracoes = QuantidadeIteracoes, TempoEntreIteracoes = TempoEntreIteracoes };
        foreach (var c in Corpos) u.AdicionarCorpo(c);
        return u;
    }
    /* Valida os corpos do universo, garantindo que todos os parâmetros estejam dentro dos limites aceitáveis. */
    public static void ValidateBodies(List<Corpo>? bodies)
    {
        /* Aqui é pra lançar um aviso que deve ter pelo menos um corpo */
        if (bodies is null || bodies.Count < 1) throw new ArgumentException("O universo deve ter pelo menos um corpo.");
        foreach (var c in bodies)
        {
            /* Valida o nome do corpo, garantindo que não seja nulo, vazio, maior que 60 caracteres ou contenha ponto e vírgula. */
            if (c is null || string.IsNullOrWhiteSpace(c.Nome) || c.Nome.Length > 60 || c.Nome.IndexOfAny([';', '\r', '\n']) >= 0)
                throw new ArgumentException("Nome de corpo inválido (até 60 caracteres, sem ponto e vírgula).");

            /* Valida a massa e densidade do corpo, garantindo que estejam dentro dos limites aceitáveis. */
            if (!double.IsFinite(c.Massa) || c.Massa <= 0 || c.Massa > 1e35 || !double.IsFinite(c.Densidade) || c.Densidade <= 0 || c.Densidade > 1e18)
                throw new ArgumentException("Massa ou densidade fora do intervalo permitido.");

            /* Valida o raio do corpo, garantindo que seja finito e positivo. */
            if (!double.IsFinite(c.Raio) || c.Raio <= 0)
                throw new ArgumentException("A massa e a densidade produzem um raio fora do intervalo numérico.");

            /* Valida a posição e velocidade do corpo, garantindo que estejam dentro dos limites aceitáveis. */
            if (new[] { c.PosX, c.PosY, c.VelX, c.VelY }.Any(v => !double.IsFinite(v) || Math.Abs(v) > 1e20))
                throw new ArgumentException("A simulação ultrapassou o intervalo numérico. Reduza o passo de tempo ou ajuste os corpos.");
        }
    }
}
