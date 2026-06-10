using System.Text.Json;
using System.Text.Json.Serialization;
using ChroniclesRPG;
using ChroniclesRPG.Web;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// ════════════════════════════════════════════════════════════════════════════
//  CHRONICLES RPG — SERVIDOR WEB
//  Expõe a lógica do backend via Minimal API HTTP para o frontend JavaScript.
// ════════════════════════════════════════════════════════════════════════════

var builder = WebApplication.CreateBuilder(args);

// Serialização JSON: camelCase + ignora null
builder.Services.ConfigureHttpJsonOptions(opt =>
{
    opt.SerializerOptions.PropertyNamingPolicy        = JsonNamingPolicy.CamelCase;
    opt.SerializerOptions.DefaultIgnoreCondition      = JsonIgnoreCondition.WhenWritingNull;
    opt.SerializerOptions.WriteIndented               = false;
});

// Sessão de jogo como singleton (um jogo por servidor)
var sessao = new SessaoWebJogo();
sessao.NovoJogo();

var app = builder.Build();

// ── Arquivos estáticos (wwwroot) ──────────────────────────────────────────
app.UseStaticFiles();

// ════════════════════════════════════════════════════════════════════════════
//  ENDPOINTS DA API
// ════════════════════════════════════════════════════════════════════════════

// POST /api/jogo/novo — reinicia o jogo
app.MapPost("/api/jogo/novo", () =>
{
    sessao.NovoJogo();
    return Results.Ok(sessao.CriarResposta());
});

// GET /api/jogo/estado — retorna estado sem modificar
app.MapGet("/api/jogo/estado", () =>
    Results.Ok(sessao.CriarResposta()));

// POST /api/jogo/personagem — cria personagem com classe e nome
app.MapPost("/api/jogo/personagem", (CriarPersonagemDto dto) =>
{
    sessao.CriarPersonagem(dto.Classe ?? "paladino", dto.Nome);
    return Results.Ok(sessao.CriarResposta());
});

// POST /api/jogo/avancar — avança do mapa para o encontro com inimigo
app.MapPost("/api/jogo/avancar", () =>
{
    sessao.AvancarEstagio();
    return Results.Ok(sessao.CriarResposta());
});

// POST /api/jogo/combate — inicia combate (do encontro → combate)
app.MapPost("/api/jogo/combate", () =>
{
    sessao.IniciarCombate();
    return Results.Ok(sessao.CriarResposta());
});

// POST /api/jogo/acao — executa ação de combate
app.MapPost("/api/jogo/acao", (AcaoDto dto) =>
{
    sessao.ExecutarAcao(dto.Acao ?? "encerrar");
    return Results.Ok(sessao.CriarResposta());
});

// POST /api/jogo/evento — trata eventos (continuar, descanso, loja)
app.MapPost("/api/jogo/evento", (EventoDto dto) =>
{
    sessao.ExecutarEvento(dto.Acao ?? "continuar");
    return Results.Ok(sessao.CriarResposta());
});

// Rota padrão — serve o index.html
app.MapFallbackToFile("index.html");

app.Run();

// ════════════════════════════════════════════════════════════════════════════
//  DTOs DE ENTRADA
// ════════════════════════════════════════════════════════════════════════════
record CriarPersonagemDto(string? Classe, string? Nome);
record AcaoDto(string? Acao);
record EventoDto(string? Acao, string? Item = null);
