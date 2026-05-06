using ChroniclesRPG;
using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Itens;
using ChroniclesRPG.Entidades.Classes;
using ChroniclesRPG.Entidades.Habilidades;
using ChroniclesRPG.Combate;

Console.OutputEncoding = System.Text.Encoding.UTF8;
Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
Console.WriteLine("║         CHRONICLES RPG — SIMULAÇÃO DE COMBATE           ║");
Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

// ── [1] DADOS DO MUNDO ─────────────────────────────────────────
Console.WriteLine("\n[1] Carregando dados do mundo...");
ScriptInicial.CarregarDados();
Console.WriteLine("    Dados carregados com sucesso!");

// ── [2] CRIANDO PERSONAGENS ────────────────────────────────────
Console.WriteLine("\n[2] Criando a Party e o Inimigo...");

FichaPersonagem magnus = new FichaPersonagem("Magnus (Guerreiro)", new Guerreiro()) { Raca = "Humano" };
FichaPersonagem eldrin = new FichaPersonagem("Eldrin (Mago)",      new Mago())      { Raca = "Elfo"   };
FichaPersonagem arthur = new FichaPersonagem("Arthur (Paladino)",  new Paladino())  { Raca = "Anão"   };
FichaPersonagem orcChefe = new FichaPersonagem("Orc Chefe",        new Guerreiro()) { Raca = "Orc"    };

// ── [3] SUBINDO PARA NÍVEL 6 ───────────────────────────────────
Console.WriteLine("\n[3] Subindo a Party para o Nível 6...");
foreach (var p in new[] { magnus, eldrin, arthur, orcChefe })
{
    p.GanharXP(1500);
    p.SubirNivel();
}
// Orc recebe HP extra para sobreviver mais de 1 rodada
orcChefe.HpMaximo = 180;
orcChefe.HpAtual  = 180;

// ── [4] EQUIPAMENTOS ───────────────────────────────────────────
Console.WriteLine("\n[4] Equipando a Party...");
magnus.EquiparArmadura(ScriptInicial.Armaduras.Find(a => a.Nome == "Cota de Malha")!);
magnus.EquiparArma(ScriptInicial.Armas.Find(a => a.Nome == "Machado Grande")!);

eldrin.EquiparArmadura(ScriptInicial.Armaduras.Find(a => a.Nome == "Armadura Acolchoada")!);
eldrin.EquiparArma(ScriptInicial.Armas.Find(a => a.Nome == "Cajado de Mago")!);

arthur.EquiparArmadura(ScriptInicial.Armaduras.Find(a => a.Nome == "Cota de Talas")!);
arthur.EquiparArma(ScriptInicial.Armas.Find(a => a.Nome == "Espada Longa")!);

orcChefe.EquiparArmadura(ScriptInicial.Armaduras.Find(a => a.Nome == "Brunea")!);
orcChefe.EquiparArma(ScriptInicial.Armas.Find(a => a.Nome == "Machado de Batalha")!);

Console.WriteLine("\n\n══════════════════════════════════════════════════════════════");
Console.WriteLine("                       Escolha o modo de teste");
Console.WriteLine("══════════════════════════════════════════════════════════════");

Console.WriteLine("1. Teste completo(2 rodadas testando diferentes habilidades)");
Console.WriteLine("2. Teste de loop(ate um dos adversarios morrer)");

int menu = Convert.ToInt32(Console.ReadLine());

switch (menu)
{
    case 1:{

        Console.WriteLine("══════════════════════════════════════════════════════════════");
        Console.WriteLine("                       Teste completo 1");
        Console.WriteLine("══════════════════════════════════════════════════════════════");

        // ── INICIANDO O COMBATE ────────────────────────────────────
        var combate = new ControladorDeCombate();
        combate.IniciarCombate(magnus, eldrin, arthur, orcChefe);

        // Helper para achar habilidade por nome
        Habilidade? H(FichaPersonagem p, string nome) => p.HabilidadesConhecidas.Find(h => h.Nome == nome);

        // ══════════════════════════════════════════════════════════════
        //                        RODADA 1
        // ══════════════════════════════════════════════════════════════
        combate.IniciarRodada();

        // ── Turno do Mago ──────────────────────────────────────────────
        combate.ProcessarInicioTurno(eldrin);
        // Ação Bônus: Ataque Certeiro (carrega vantagem para o próximo ataque)
        combate.Agir(eldrin, H(eldrin, "Ataque Certeiro")!, orcChefe);
        // Ação Principal: Proteção Arcana (cria o escudo e ganha +2 CA)
        combate.Agir(eldrin, H(eldrin, "Proteção Arcana")!);
        combate.ProcessarFimTurno(eldrin);

        // ── Turno do Paladino ──────────────────────────────────────────
        combate.ProcessarInicioTurno(arthur);
        // Ação Bônus: Escudo da Fé no Guerreiro (+2 CA)
        combate.Agir(arthur, H(arthur, "Escudo da Fé")!, magnus);
        // Ação Principal: Destruição Divina contra o Orc
        combate.Agir(arthur, H(arthur, "Destruição Divina")!, orcChefe);
        combate.ProcessarFimTurno(arthur);
        combate.VerificarMortos();

        // ── Turno do Guerreiro ─────────────────────────────────────────
        combate.ProcessarInicioTurno(magnus);
        // Ação Principal: Ataque Básico (2 ataques, nível 5+, com bônus de Escudo da Fé na CA)
        combate.Agir(magnus, H(magnus, "Ataque Básico")!, orcChefe);
        // Ação Principal Extra: Surto de Ação → abre Ação Extra
        combate.Agir(magnus, H(magnus, "Surto de Ação")!);
        // Segunda Ação (via Surto): mais 2 ataques
        combate.Agir(magnus, H(magnus, "Ataque Básico")!, orcChefe);
        combate.ProcessarFimTurno(magnus);
        combate.VerificarMortos();

        // ── Turno do Inimigo ───────────────────────────────────────────
        combate.ProcessarInicioTurno(orcChefe);
        // Ação Principal: Ataque Básico (2 ataques) contra o Mago — escudo deve absorver!
        Console.WriteLine("  O Orc Chefe avança furiosamente em direção ao Mago!");
        combate.Agir(orcChefe, H(orcChefe, "Ataque Básico")!, eldrin);
        combate.ProcessarFimTurno(orcChefe);
        combate.VerificarMortos();

        combate.ExibirResumo();

        // ══════════════════════════════════════════════════════════════
        //                        RODADA 2
        // ══════════════════════════════════════════════════════════════
        combate.IniciarRodada();

        // ── Turno do Mago (Rodada 2) ───────────────────────────────────
        combate.ProcessarInicioTurno(eldrin);
        // Ação Principal: Proteção contra Lâminas (resistência física por 1 turno)
        combate.Agir(eldrin, H(eldrin, "Proteção contra Lâminas")!);
        // Sem ação bônus sobrando — tenta usar uma segunda ação principal (deve ser bloqueado)
        Console.WriteLine("\n  [Teste] Eldrin tenta conjurar novamente...");
        combate.Agir(eldrin, H(eldrin, "Raio de Fogo")!, orcChefe);
        combate.ProcessarFimTurno(eldrin); // Resistência expira aqui

        // ── Turno do Paladino (Rodada 2) ──────────────────────────────
        combate.ProcessarInicioTurno(arthur);
        // Canalizar Divindade: Onda de Vitalidade no Mago (cura progressiva por 3 turnos)
        combate.Agir(arthur, H(arthur, "Onda de Vitalidade")!);
        // Ação Principal: Destruição Divina
        combate.Agir(arthur, H(arthur, "Destruição Divina")!, orcChefe);
        combate.ProcessarFimTurno(arthur);
        combate.VerificarMortos();

        // ── Turno do Guerreiro (Rodada 2) ─────────────────────────────
        combate.ProcessarInicioTurno(magnus);
        combate.Agir(magnus, H(magnus, "Ataque Básico")!, orcChefe);
        combate.ProcessarFimTurno(magnus);
        combate.VerificarMortos();

        // ── Turno do Inimigo (Rodada 2) ───────────────────────────────
        combate.ProcessarInicioTurno(orcChefe);
        // Sem escudo ativo agora — o dano vai direto no HP do Mago
        Console.WriteLine("  O Orc Chefe ataca o Mago novamente!");
        combate.Agir(orcChefe, H(orcChefe, "Ataque Básico")!, eldrin);
        combate.ProcessarFimTurno(orcChefe);
        combate.VerificarMortos();

        combate.ExibirResumo();

        // ── STATUS FINAL ───────────────────────────────────────────────
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 STATUS PÓS-COMBATE                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        magnus.ExibirStatus();
        eldrin.ExibirStatus();
        arthur.ExibirStatus();
        orcChefe.ExibirStatus();
    }

    break;


    case 2:{
        Console.WriteLine("══════════════════════════════════════════════════════════════");
        Console.WriteLine("                       Teste de loop 1");
        Console.WriteLine("══════════════════════════════════════════════════════════════");

        // ── [5] INICIANDO O COMBATE ────────────────────────────────────
        var combate = new ControladorDeCombate();
        combate.IniciarCombate(magnus, arthur);

        // Helper para achar habilidade por nome
        Habilidade? H(FichaPersonagem p, string nome) => p.HabilidadesConhecidas.Find(h => h.Nome == nome);

        int rodada = 0;
        
        // O loop continua enquanto AMBOS estiverem vivos (HP > 0)
        while(arthur.HpAtual > 0 && magnus.HpAtual > 0)
        {
            rodada++;
            Console.WriteLine($"\n═════════════════════ RODADA {rodada} ═════════════════════");
            combate.IniciarRodada();

            // ── Turno do Guerreiro (Magnus) ─────────────────────────────
            combate.ProcessarInicioTurno(magnus);
            Console.WriteLine(" O Guerreiro Magnus avança para o Ataque Básico!");
            combate.Agir(magnus, H(magnus, "Ataque Básico")!, arthur);
            combate.ProcessarFimTurno(magnus);
            combate.VerificarMortos();

            // Interrompe a rodada se Arthur morrer com esse ataque
            if (arthur.HpAtual <= 0) 
            {
                Console.WriteLine("\n[Combate Encerrado] Arthur caiu em batalha!");
                break;
            }

            // ── Turno do Paladino (Arthur) ──────────────────────────────
            combate.ProcessarInicioTurno(arthur);
            Console.WriteLine(" O Paladino Arthur revida com um Ataque Básico!");
            combate.Agir(arthur, H(arthur, "Ataque Básico")!, magnus);
            combate.ProcessarFimTurno(arthur);
            combate.VerificarMortos();

            // Interrompe a rodada se Magnus morrer com esse ataque
            if (magnus.HpAtual <= 0)
            {
                Console.WriteLine("\n[Combate Encerrado] Magnus caiu em batalha!");
                break;
            }

            // Exibe o resumo do final da rodada
            combate.ExibirResumo();
        }

        // ── STATUS FINAL ───────────────────────────────────────────────
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 STATUS FINAL DO DUELO                    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
        magnus.ExibirStatus();
        arthur.ExibirStatus();
    }
    break;

    default:
    Console.WriteLine("Opção inválida!");
    break;
}

