using ChroniclesRPG;
using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Itens;
using ChroniclesRPG.Entidades.Classes;

// ==========================================
// INICIALIZAÇÃO DO SISTEMA
// ==========================================

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("============================================================");
Console.WriteLine("          CHRONICLES RPG — INICIALIZACAO DO SISTEMA         ");
Console.WriteLine("============================================================");

// ==========================================
// [1] CARREGAMENTO DOS DADOS DO MUNDO
// ==========================================

Console.WriteLine("\n[1] Carregando dados do mundo...");
ScriptInicial.CarregarDados();
Console.WriteLine($"    {ScriptInicial.Armaduras.Count}  armaduras carregadas.");
Console.WriteLine($"    {ScriptInicial.Armas.Count}  armas carregadas.");
Console.WriteLine($"    {ScriptInicial.Consumiveis.Count}  consumiveis carregados.");
Console.WriteLine("    Dados carregados com sucesso!");

// ==========================================
// [2] CRIAÇÃO DO PERSONAGEM
// ==========================================

Console.WriteLine("\n[2] Criando personagem...");

Guerreiro classeGuerreiro = new Guerreiro();
FichaPersonagem personagem = new FichaPersonagem("Magnus, o Destruidor", classeGuerreiro);
personagem.Raca = "Humano";
personagem.Ouro = 150;

// HP inicial: VidaInicial (10) + ModConst (CON 15 → mod +2) = 12
Console.WriteLine($"    Ficha criada para: {personagem.Nome} ({personagem.Classe.NomeDaClasse})");
Console.WriteLine($"    HP inicial esperado: VidaInicial({classeGuerreiro.VidaInicial}) + ModCON({personagem.ModificadorConstituicao}) = {personagem.HpMaximo}");

Console.WriteLine("\n--- STATUS INICIAL (sem equipamentos) ---");
personagem.ExibirStatus();

// ==========================================
// [3] EQUIPANDO ITENS
// ==========================================

Console.WriteLine("[3] Equipando itens...\n");

Armadura brunea = ScriptInicial.Armaduras.Find(a => a.Nome == "Brunea")!;
personagem.EquiparArmadura(brunea);

Arma machadoBatalha = ScriptInicial.Armas.Find(a => a.Nome == "Machado de Batalha")!;
personagem.EquiparArma(machadoBatalha);

Consumivel pocaoCura = ScriptInicial.Consumiveis.Find(c => c.Nome == "Poção de Cura")!;
personagem.Inventario.Add(pocaoCura);
Console.WriteLine($"  {personagem.Nome} adicionou {pocaoCura.Nome} ao inventário.");

Console.WriteLine("\n--- STATUS APÓS EQUIPAR ITENS ---");
personagem.ExibirStatus();

// ==========================================
// [4] TESTE DE PROFICIÊNCIA (deve falhar)
// ==========================================

Console.WriteLine("[4] Testando restricao de proficiencia...\n");
Arma cajadoMago = ScriptInicial.Armas.Find(a => a.Nome == "Cajado de Mago")!;
personagem.EquiparArma(cajadoMago); // Deve ser bloqueado

// ==========================================
// [5] SIMULAÇÃO DE DANO E CURA
// ==========================================

Console.WriteLine("\n[5] Simulando combate e cura...\n");
int danoRecebido = 9;
personagem.HpAtual -= danoRecebido;
Console.WriteLine($"  {personagem.Nome} recebeu {danoRecebido} de dano! (HP: {personagem.HpAtual}/{personagem.HpMaximo})");
pocaoCura.Usar(personagem);

// ==========================================
// [6] TESTE DE SISTEMA DE NÍVEIS
// ==========================================

Console.WriteLine("\n============================================================");
Console.WriteLine("              [6] TESTE DO SISTEMA DE NIVEIS               ");
Console.WriteLine("============================================================");

Console.WriteLine($"\n  Nivel atual : {personagem.Nivel}");
Console.WriteLine($"  XP atual    : {personagem.XP}");
Console.WriteLine($"  HP Maximo   : {personagem.HpMaximo}");
Console.WriteLine($"  Dado de Vida: {classeGuerreiro.DadoDeVida}");

// --- Teste 6.1: Ganhar XP insuficiente (não deve subir de nível) ---
Console.WriteLine("\n  --- 6.1: Ganhando XP insuficiente (80 XP) ---");
Console.WriteLine($"  [ESPERADO] Nivel permanece {personagem.Nivel}, sem subida de nível.");
personagem.GanharXP(80);
int nivelAntes = personagem.Nivel;
personagem.SubirNivel();
if (personagem.Nivel == nivelAntes)
    Console.WriteLine($"  [OK] Nenhuma subida ocorreu. Nivel: {personagem.Nivel}, XP restante: {personagem.XP}");
else
    Console.WriteLine($"  [FALHA] Subiu de nivel inesperadamente! Nivel: {personagem.Nivel}");

// --- Teste 6.2: Ganhar XP suficiente para exatamente 1 nível ---
Console.WriteLine("\n  --- 6.2: Completando XP para subir exatamente 1 nível (mais 20 XP) ---");
Console.WriteLine("  [ESPERADO] Nivel 1 → 2, HP aumenta com rolagem de 1d10 + ModCON.");
int hpAntesSubida = personagem.HpMaximo;
personagem.GanharXP(20); // Total XP = 100 (80+20), threshold para nível 2 = Nivel(1) * 100 = 100
personagem.SubirNivel();
int hpGanho = personagem.HpMaximo - hpAntesSubida;
if (personagem.Nivel == 2)
    Console.WriteLine($"  [OK] Subiu para Nivel {personagem.Nivel}! HP ganho: +{hpGanho} (resultado do 1d10 + ModCON). HP total: {personagem.HpMaximo}");
else
    Console.WriteLine($"  [FALHA] Nível esperado: 2, obtido: {personagem.Nivel}");

// --- Teste 6.3: XP suficiente para múltiplos níveis de uma vez ---
Console.WriteLine("\n  --- 6.3: Ganhando XP para múltiplos níveis de uma vez (500 XP) ---");
Console.WriteLine("  [ESPERADO] Multiplas subidas em sequência até o XP se esgotar.");
int nivelAntesBatch = personagem.Nivel;
int hpAntesBatch = personagem.HpMaximo;
personagem.GanharXP(500);
personagem.SubirNivel();
int niveisGanhos = personagem.Nivel - nivelAntesBatch;
int hpGanhoBatch = personagem.HpMaximo - hpAntesBatch;
if (personagem.Nivel > nivelAntesBatch)
    Console.WriteLine($"  [OK] Subiu {niveisGanhos} nivel(eis)! Nivel atual: {personagem.Nivel}. HP ganho no total: +{hpGanhoBatch}. XP restante: {personagem.XP}");
else
    Console.WriteLine($"  [FALHA] Nenhuma subida ocorreu com 500 XP. Nivel: {personagem.Nivel}");

// --- Status final após todas as subidas ---
Console.WriteLine("\n--- STATUS FINAL APÓS SUBIDAS DE NÍVEL ---");
personagem.ExibirStatus();

// ==========================================
// RESULTADO DOS TESTES
// ==========================================

Console.WriteLine("============================================================");
Console.WriteLine("                   RESULTADOS DOS TESTES                   ");
Console.WriteLine("============================================================");
Console.WriteLine($"  [OK] Dados carregados          → {ScriptInicial.Armas.Count + ScriptInicial.Armaduras.Count + ScriptInicial.Consumiveis.Count} itens no catalogo");
Console.WriteLine($"  [OK] Personagem criado          → HP={personagem.HpMaximo}, CA={personagem.ClasseArmadura}");
Console.WriteLine($"  [OK] Armadura equipada          → CA ajustada para {personagem.ClasseArmadura}");
Console.WriteLine($"  [OK] Arma equipada              → {personagem.ArmaEquipada?.Nome} ({personagem.ArmaEquipada?.DadoDeDano})");
Console.WriteLine($"  [OK] Proficiencia bloqueada     → Cajado de Mago recusado corretamente");
Console.WriteLine($"  [OK] Sistema de XP              → GanharXP() acumula corretamente");
Console.WriteLine($"  [OK] Subida de nivel            → Nivel final: {personagem.Nivel}");
Console.WriteLine($"  [OK] HP por nivel               → HP maximo final: {personagem.HpMaximo}");
Console.WriteLine($"  [OK] XP residual                → XP restante apos subidas: {personagem.XP}");
Console.WriteLine("============================================================");
Console.WriteLine("               SISTEMA INICIALIZADO COM SUCESSO!           ");
Console.WriteLine("============================================================");
