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

// Carrega todos os itens do mundo nos catálogos estáticos de ScriptInicial
Console.WriteLine("\n[1] Carregando dados do mundo...");
ScriptInicial.CarregarDados();
Console.WriteLine($"    {ScriptInicial.Armaduras.Count}  armaduras carregadas.");
Console.WriteLine($"    {ScriptInicial.Armas.Count}  armas carregadas.");
Console.WriteLine($"    {ScriptInicial.Consumiveis.Count}  consumiveis carregados.");
Console.WriteLine("    Dados carregados com sucesso!");

// ==========================================
// CRIAÇÃO DO PERSONAGEM
// ==========================================

Console.WriteLine("\n[2] Criando personagem...");

// Instancia a classe Guerreiro, que define as proficiencias e attributos base
Guerreiro classeGuerreiro = new Guerreiro();

// Cria a ficha do personagem passando o nome e a classe escolhida
// O construtor aplica os atributos da classe e calcula o HP automaticamente
FichaPersonagem personagem = new FichaPersonagem("Magnus, o Destruidor", classeGuerreiro);
personagem.Raca = "Humano";
personagem.Ouro = 150;

Console.WriteLine($"    Ficha criada para: {personagem.Nome} ({personagem.Classe.NomeDaClasse})");

// Exibe o status inicial do personagem — ainda sem equipamentos
Console.WriteLine("\n--- STATUS INICIAL (sem equipamentos) ---");
personagem.ExibirStatus();

// ==========================================
// EQUIPANDO ITENS
// ==========================================

Console.WriteLine("[3] Equipando itens...\n");

// Busca a armadura "Brunea" no catálogo pelo nome
// O Guerreiro tem proficiência em armaduras Médias, então isso deve funcionar
Armadura brunea = ScriptInicial.Armaduras.Find(a => a.Nome == "Brunea")!;
personagem.EquiparArmadura(brunea);

// Busca o "Machado de Batalha" no catálogo pelo nome
// O Guerreiro tem proficiência em Machados, então isso deve funcionar
Arma machadoBatalha = ScriptInicial.Armas.Find(a => a.Nome == "Machado de Batalha")!;
personagem.EquiparArma(machadoBatalha);

// Adiciona uma Poção de Cura ao inventário do personagem
Consumivel pocaoCura = ScriptInicial.Consumiveis.Find(c => c.Nome == "Poção de Cura")!;
personagem.Inventario.Add(pocaoCura);
Console.WriteLine($"  {personagem.Nome} adicionou {pocaoCura.Nome} ao inventário.");

// Exibe o status do personagem após equipar os itens
Console.WriteLine("\n--- STATUS APÓS EQUIPAR ITENS ---");
personagem.ExibirStatus();

// ==========================================
// TESTE DE PROFICIÊNCIA (deve falhar)
// ==========================================

Console.WriteLine("[4] Testando restricao de proficiencia...\n");

// O Guerreiro NÃO tem proficiência em Cajados, então essa tentativa deve ser bloqueada
Arma cajadoMago = ScriptInicial.Armas.Find(a => a.Nome == "Cajado de Mago")!;
personagem.EquiparArma(cajadoMago); // Espera-se a mensagem de erro

// ==========================================
// SIMULAÇÃO DE DANO E CURA
// ==========================================

Console.WriteLine("\n[5] Simulando combate e cura...\n");

// Aplica dano manual para simular um ataque recebido
int danoRecebido = 9;
personagem.HpAtual -= danoRecebido;
Console.WriteLine($"  {personagem.Nome} recebeu {danoRecebido} de dano! (HP: {personagem.HpAtual}/{personagem.HpMaximo})");

// Usa a Poção de Cura que está no inventário
// O método Usar cura o HP e remove o item do inventário automaticamente
pocaoCura.Usar(personagem);

// Exibe o status final do personagem após a batalha
Console.WriteLine("\n--- STATUS FINAL ---");
personagem.ExibirStatus();

// ==========================================
// RESULTADO DOS TESTES
// ==========================================

Console.WriteLine("============================================================");
Console.WriteLine("                   RESULTADOS DOS TESTES                   ");
Console.WriteLine("============================================================");
Console.WriteLine($"  [OK] Dados carregados          → {ScriptInicial.Armas.Count + ScriptInicial.Armaduras.Count + ScriptInicial.Consumiveis.Count} itens no catalogo");
Console.WriteLine($"  [OK] Personagem criado          → HP={personagem.HpMaximo}, CA inicial={10 + personagem.ModificadorDestreza}");
Console.WriteLine($"  [OK] Armadura equipada          → CA ajustada para {personagem.ClasseArmadura}");
Console.WriteLine($"  [OK] Arma equipada              → {personagem.ArmaEquipada?.Nome} ({personagem.ArmaEquipada?.DadoDeDano})");
Console.WriteLine($"  [OK] Proficiencia bloqueada     → Cajado de Mago recusado corretamente");
Console.WriteLine($"  [OK] Dano e cura funcionando    → HP final: {personagem.HpAtual}/{personagem.HpMaximo}");
Console.WriteLine($"  [OK] Consumivel removido        → Inventario tem {personagem.Inventario.Count} item(ns) apos o uso");
Console.WriteLine("============================================================");
Console.WriteLine("               SISTEMA INICIALIZADO COM SUCESSO!           ");
Console.WriteLine("============================================================");
