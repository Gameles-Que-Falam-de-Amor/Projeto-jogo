using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Classes;
using ChroniclesRPG.Entidades.Habilidades;
using ChroniclesRPG.Entidades.Inimigos;
using ChroniclesRPG.Entidades.Itens;
using ChroniclesRPG.Funções;

namespace ChroniclesRPG.Web
{
    // ══════════════════════════════════════════════════════
    //  SESSÃO WEB — usa o backend real do Gabriel
    // ══════════════════════════════════════════════════════
    public class SessaoWebJogo
    {
        // ── Estado da sessão ─────────────────────────────
        public string Fase { get; private set; } = "intro";    // intro | criacao | mapa | encontro | combate | evento | vitoria_estagio | fim
        private FichaPersonagem? _jogador;
        private FichaInimigo?    _inimigoAtual;
        private FichaPersonagem? _proxyInimigo;
        private int              _estagioAtual = 1;
        private const int        ESTAGIO_MAX   = 20;
        private int              _inimigosDerrotados = 0;
        private readonly List<string> _log = new();
        private readonly List<string> _caminhosPercorridos = new();

        // ── Estado de turno ──────────────────────────────
        private bool _acaoPrincipalUsada;
        private bool _acaoBonusUsada;
        private int  _acoesExtras;           // Surto de Ação do Guerreiro

        // ── Buffs / status ───────────────────────────────
        private int  _vantagemJogador;
        private int  _desvantagemInimigo;
        private int  _paralisiaInimigo;

        // ── Snapshots de recursos para restauração ───────
        private Dictionary<int, int> _slotsMaximos = new();
        private int _surtoDeAcaoMax;
        private int _canalizarDivindadeMax;
        private int _reservaCuraMaosMax;

        // ── Feedback visual ──────────────────────────────
        public  string FeedbackTipo  { get; private set; } = "neutro";
        public  string FeedbackTexto { get; private set; } = "";

        // ═══════════════════════════════════════════════════
        //  NOVO JOGO
        // ═══════════════════════════════════════════════════
        public void NovoJogo()
        {
            ScriptInicial.CarregarDados();
            _jogador            = null;
            _inimigoAtual       = null;
            _proxyInimigo       = null;
            _estagioAtual       = 1;
            _inimigosDerrotados = 0;
            _acaoPrincipalUsada = false;
            _acaoBonusUsada     = false;
            _acoesExtras        = 0;
            _vantagemJogador    = 0;
            _desvantagemInimigo = 0;
            _paralisiaInimigo   = 0;
            _log.Clear();
            _caminhosPercorridos.Clear();
            Fase = "criacao";
            SetFeedback("neutro", "Escolha seu herói");
            Log("A guilda Pluma de Prata recebe o chamado da Floresta de Letes.");
        }

        // ═══════════════════════════════════════════════════
        //  CRIAR PERSONAGEM
        // ═══════════════════════════════════════════════════
        public void CriarPersonagem(string classeId, string? nome)
        {
            ScriptInicial.CarregarDados();
            var classe = CriarClasse(classeId);
            string nomeHeroi = string.IsNullOrWhiteSpace(nome) ? NomePadrao(classe.NomeDaClasse) : nome.Trim();

            _jogador = new FichaPersonagem(nomeHeroi, classe);
            _jogador.Raca = "Humano";
            _jogador.Ouro = OuroInicial(classe.NomeDaClasse);
            EquiparInicial(classe.NomeDaClasse);
            _jogador.Inventario.Add(ScriptInicial.Consumiveis.First()); // poção inicial

            // Captura snapshots para restauração entre estágios
            _slotsMaximos          = new Dictionary<int, int>(_jogador.SlotsDeMagia);
            _surtoDeAcaoMax        = _jogador.UsosSurtoDeAcao;
            _canalizarDivindadeMax = _jogador.UsosCanalizarDivindade;
            _reservaCuraMaosMax    = _jogador.ReservaCuraPelasMaos;

            Log($"{_jogador.Nome}, {_jogador.Classe.NomeDaClasse}, parte da guilda Pluma de Prata.");
            Log("O Bosque dos Lamentos aguarda além da trilha principal.");
            Fase = "mapa";
            SetFeedback("jornada", "Jornada iniciada");
        }

        // ═══════════════════════════════════════════════════
        //  AVANÇAR PARA O PRÓXIMO ESTÁGIO (do mapa)
        // ═══════════════════════════════════════════════════
        public void AvancarEstagio()
        {
            if (Fase != "mapa" || _jogador == null) return;

            var inimigo = CatalogoDeEstagio.ObterInimigo(_estagioAtual);
            _inimigoAtual = inimigo;
            _proxyInimigo = CriarProxy(inimigo);

            string tipo = inimigo.Categoria is "Mini-Boss" or "Boss Final" ? "boss" : "comum";
            Log($"Estágio {_estagioAtual}: {inimigo.Nome}");
            _caminhosPercorridos.Add($"Estágio {_estagioAtual}: {inimigo.Nome}");

            Fase = "encontro"; // tela de apresentação do inimigo com fala
            SetFeedback(tipo, inimigo.Nome);
        }

        // ═══════════════════════════════════════════════════
        //  INICIAR COMBATE (do encontro)
        // ═══════════════════════════════════════════════════
        public void IniciarCombate()
        {
            if (Fase != "encontro" || _inimigoAtual == null || _jogador == null) return;

            ResetarTurno();
            Log($"Combate iniciado contra {_inimigoAtual.Nome}.");
            Log($"Iniciativa: {(_jogador.Iniciativa >= _inimigoAtual.Iniciativa ? _jogador.Nome : _inimigoAtual.Nome)} age primeiro!");
            Fase = "combate";
            SetFeedback("neutro", "Seu turno!");
        }

        // ═══════════════════════════════════════════════════
        //  EXECUTAR AÇÃO DE COMBATE
        // ═══════════════════════════════════════════════════
        public void ExecutarAcao(string acao)
        {
            if (Fase != "combate" || _inimigoAtual == null || _jogador == null) return;

            string a = acao.ToLowerInvariant().Trim();

            if (a == "encerrar")
            {
                EncerrarTurnoJogador();
                return;
            }

            bool ehBonus = EhAcaoBonus(a);

            if (ehBonus && _acaoBonusUsada)
            {
                SetFeedback("erro", "Ação bônus já usada");
                return;
            }
            if (!ehBonus && _acaoPrincipalUsada && _acoesExtras <= 0)
            {
                SetFeedback("erro", "Ação principal já usada");
                return;
            }

            if (ehBonus)
                ExecutarBonus(a);
            else
                ExecutarPrincipal(a);

            // Checa morte do inimigo
            if (_inimigoAtual is { HpAtual: <= 0 })
            {
                ConcluirVitoria();
                return;
            }
            // Checa morte do jogador
            if (_jogador.HpAtual <= 0)
            {
                Fase = "fim";
                Log("Derrota. O miasma ainda domina a floresta.");
                SetFeedback("derrota", "Você foi derrotado!");
            }
        }

        // ═══════════════════════════════════════════════════
        //  EXECUTAR EVENTO (descanso, etc.)
        // ═══════════════════════════════════════════════════
        public void ExecutarEvento(string acao)
        {
            if (_jogador == null) return;
            if (acao.ToLowerInvariant() == "continuar" && Fase == "vitoria_estagio")
            {
                ProximoEstagio();
            }
        }

        // ═══════════════════════════════════════════════════
        //  AÇÕES PRINCIPAIS POR CLASSE
        // ═══════════════════════════════════════════════════
        private void ExecutarPrincipal(string acao)
        {
            if (_jogador == null || _inimigoAtual == null) return;
            string classe = _jogador.Classe.NomeDaClasse;

            switch (acao)
            {
                // ── Universais ─────────────────────
                case "atacar":
                    AtacarComArma(usarSmite: false, dadoExtra: null, nome: "Ataque");
                    break;

                // ── Guerreiro ──────────────────────
                case "golpe":
                    if (classe != "Guerreiro") { SetFeedback("erro", "Apenas Guerreiros"); return; }
                    AtacarComArma(usarSmite: false, dadoExtra: "1d6", nome: "Golpe Preciso");
                    _desvantagemInimigo = 1;
                    Log("O inimigo fica em desvantagem no próximo ataque.");
                    break;

                // ── Paladino ───────────────────────
                case "smite":
                    if (classe != "Paladino") { SetFeedback("erro", "Apenas Paladinos"); return; }
                    if (!GastarSlot(1)) return;
                    AtacarComArma(usarSmite: true, dadoExtra: null, nome: "Destruição Divina");
                    break;

                // ── Mago ───────────────────────────
                case "rajada":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    AtacarMagico("Rajada Arcana", "1d10");
                    break;

                case "gelo":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    // Escala com nível 5+
                    string dadoGelo = _jogador.Nivel >= 5 ? "2d8" : "1d8";
                    AtacarMagico($"Raio de Gelo ({dadoGelo})", dadoGelo);
                    break;

                case "fogo":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    if (_jogador.Nivel < 6) { SetFeedback("erro", "Desbloqueia no nível 6"); return; }
                    string dadoFogo = _jogador.Nivel >= 5 ? "2d10" : "1d10";
                    AtacarMagico($"Raio de Fogo ({dadoFogo})", dadoFogo);
                    break;

                case "misil":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    if (!GastarSlot(1)) return;
                    int danoMisil = Dados.Rolar("3d4") + 3;
                    AplicarDanoInimigo(danoMisil, "Mísseis Mágicos");
                    SetFeedback("magia", $"-{danoMisil} HP");
                    break;

                case "paralisar":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    if (!GastarSlot(1)) return;
                    _paralisiaInimigo = 1;
                    Log($"{_jogador.Nome} prende {_inimigoAtual.Nome} com energia arcana.");
                    SetFeedback("status", "Paralisado!");
                    break;

                case "laminas":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    _jogador.TemResistenciaFisica = true;
                    Log($"{_jogador.Nome} ativa Proteção contra Lâminas. Resistência física por 1 turno.");
                    SetFeedback("defesa", "Resistência ativa");
                    break;

                default:
                    SetFeedback("erro", "Ação desconhecida");
                    return;
            }

            if (_acoesExtras > 0)
                _acoesExtras--;
            else
                _acaoPrincipalUsada = true;
        }

        // ═══════════════════════════════════════════════════
        //  AÇÕES BÔNUS
        // ═══════════════════════════════════════════════════
        private void ExecutarBonus(string acao)
        {
            if (_jogador == null) return;
            string classe = _jogador.Classe.NomeDaClasse;

            switch (acao)
            {
                case "cura":
                    if (classe != "Paladino") { SetFeedback("erro", "Apenas Paladinos"); return; }
                    if (_jogador.ReservaCuraPelasMaos <= 0) { SetFeedback("erro", "Reserva esgotada"); return; }
                    int curaQtd = Math.Min(5, _jogador.ReservaCuraPelasMaos);
                    curaQtd = Math.Min(curaQtd, _jogador.HpMaximo - _jogador.HpAtual);
                    _jogador.ReservaCuraPelasMaos -= curaQtd;
                    _jogador.HpAtual              += curaQtd;
                    Log($"{_jogador.Nome} recupera {curaQtd} HP pela Cura pelas Mãos.");
                    SetFeedback("cura", $"+{curaQtd} HP");
                    break;

                case "folego":
                    if (classe != "Guerreiro") { SetFeedback("erro", "Apenas Guerreiros"); return; }
                    int curaFolego = Dados.Rolar("1d10") + _jogador.Nivel;
                    int hpAntes = _jogador.HpAtual;
                    _jogador.HpAtual = Math.Min(_jogador.HpMaximo, _jogador.HpAtual + curaFolego);
                    Log($"{_jogador.Nome} retoma o fôlego: +{_jogador.HpAtual - hpAntes} HP.");
                    SetFeedback("cura", $"+{_jogador.HpAtual - hpAntes} HP");
                    break;

                case "surto":
                    if (classe != "Guerreiro") { SetFeedback("erro", "Apenas Guerreiros"); return; }
                    if (_jogador.UsosSurtoDeAcao <= 0) { SetFeedback("erro", "Surto esgotado"); return; }
                    _jogador.UsosSurtoDeAcao--;
                    _acoesExtras = 1;
                    _acaoPrincipalUsada = false; // libera ação principal
                    Log($"Surto de Ação! {_jogador.Nome} ganha 1 ação extra.");
                    SetFeedback("status", "Surto de Ação!");
                    break;

                case "certeiro":
                    if (classe != "Mago") { SetFeedback("erro", "Apenas Magos"); return; }
                    _vantagemJogador = 1;
                    Log($"{_jogador.Nome} assume Ataque Certeiro. Vantagem no próximo ataque.");
                    SetFeedback("status", "Vantagem ativa");
                    break;

                case "pocao":
                    var pocao = _jogador.Inventario.OfType<Consumivel>().FirstOrDefault();
                    if (pocao == null) { SetFeedback("erro", "Sem poções"); return; }
                    int hpPocao = _jogador.HpAtual;
                    pocao.Usar(_jogador);
                    _jogador.Inventario.Remove(pocao);
                    int curou = _jogador.HpAtual - hpPocao;
                    Log($"{_jogador.Nome} usa {pocao.Nome}: +{curou} HP.");
                    SetFeedback("cura", $"+{curou} HP");
                    break;

                default:
                    SetFeedback("erro", "Ação bônus desconhecida");
                    return;
            }
            _acaoBonusUsada = true;
        }

        // ═══════════════════════════════════════════════════
        //  ENCERRAR TURNO — TURNO DO INIMIGO
        // ═══════════════════════════════════════════════════
        private void EncerrarTurnoJogador()
        {
            if (_inimigoAtual == null || _jogador == null) return;

            if (!_acaoPrincipalUsada && _acoesExtras <= 0)
            {
                SetFeedback("aviso", "Use uma ação principal antes de encerrar.");
                return;
            }

            // Efeitos de fim de turno
            if (_jogador.TemResistenciaFisica)
            {
                _jogador.TemResistenciaFisica = false;
                Log("Proteção contra Lâminas expirou.");
            }
            if (_vantagemJogador > 0 && _jogador.TemVantagemProximoAtaque)
            {
                _jogador.TemVantagemProximoAtaque = false;
                _vantagemJogador = 0;
                Log("Ataque Certeiro expirou sem ser usado.");
            }

            // Turno do inimigo
            if (_paralisiaInimigo > 0)
            {
                _paralisiaInimigo--;
                Log($"{_inimigoAtual.Nome} está paralisado e perde o turno.");
                SetFeedback("defesa", "Inimigo paralisado");
            }
            else
            {
                TurnoInimigo();
            }

            if (_jogador.HpAtual <= 0)
            {
                Fase = "fim";
                Log("Derrota. O miasma ainda domina a floresta.");
                SetFeedback("derrota", "Você foi derrotado!");
                return;
            }

            ResetarTurno();
        }

        // ═══════════════════════════════════════════════════
        //  TURNO DO INIMIGO (automático)
        // ═══════════════════════════════════════════════════
        private void TurnoInimigo()
        {
            if (_inimigoAtual == null || _jogador == null) return;

            int rolagem = Dados.RolarD20();
            bool temDesvantagem = _desvantagemInimigo > 0;
            if (temDesvantagem)
            {
                int r2 = Dados.RolarD20();
                rolagem = Math.Min(rolagem, r2);
                _desvantagemInimigo--;
                Log($"{_inimigoAtual.Nome} ataca com desvantagem (d20: {rolagem}).");
            }
            else
            {
                Log($"{_inimigoAtual.Nome} ataca: d20({rolagem}) + {_inimigoAtual.Ataque.BonusDeAcerto}.");
            }

            int totalAtaque = rolagem + _inimigoAtual.Ataque.BonusDeAcerto;

            if (rolagem == 1 || (rolagem != 20 && totalAtaque < _jogador.ClasseArmadura))
            {
                Log($"{_inimigoAtual.Nome} errou! CA {_jogador.ClasseArmadura}.");
                SetFeedback("defesa", "Inimigo errou!");
                return;
            }

            int dano = Dados.Rolar(_inimigoAtual.Ataque.DadoDeDano) + _inimigoAtual.Ataque.BonusDeDano;
            if (rolagem == 20)
            {
                dano += Dados.Rolar(_inimigoAtual.Ataque.DadoDeDano);
                Log("Crítico do inimigo!");
            }
            dano = Math.Max(1, dano);
            int danoReal = _jogador.ReceberDano(dano);
            Log($"{_jogador.Nome} sofreu {danoReal} de dano. HP: {Math.Max(0, _jogador.HpAtual)}/{_jogador.HpMaximo}.");
            SetFeedback("sofreu-dano", $"-{danoReal} HP");
        }

        // ═══════════════════════════════════════════════════
        //  CONCLUIR VITÓRIA DE ESTÁGIO
        // ═══════════════════════════════════════════════════
        private void ConcluirVitoria()
        {
            if (_inimigoAtual == null || _jogador == null) return;

            int xp = _inimigoAtual.Categoria switch
            {
                "Mini-Boss"  => 150,
                "Boss Final" => 500,
                _            => 50
            };

            Log($"{_inimigoAtual.Nome} foi derrotado! +{xp} XP.");
            _jogador.GanharXP(xp);
            _jogador.SubirNivel();
            _inimigosDerrotados++;

            // Atualiza snapshot de reserva caso tenha subido de nível
            if (_jogador.ReservaCuraPelasMaos > _reservaCuraMaosMax)
                _reservaCuraMaosMax = _jogador.ReservaCuraPelasMaos;

            bool foiBoss = _inimigoAtual.Categoria is "Mini-Boss" or "Boss Final";
            if (foiBoss)
                RestaurarDescansoLongo();
            else
                RestaurarEntreEstagios();

            if (_estagioAtual >= ESTAGIO_MAX)
            {
                Fase = "fim";
                Log("Vitória! O mal antigo é selado e a Floresta de Letes respira novamente.");
                SetFeedback("vitoria", "Campanha concluída!");
            }
            else
            {
                Fase = "vitoria_estagio";
                SetFeedback("vitoria", $"Estágio {_estagioAtual} concluído!");
            }

            _inimigoAtual = null;
            _proxyInimigo = null;
        }

        // ═══════════════════════════════════════════════════
        //  PRÓXIMO ESTÁGIO
        // ═══════════════════════════════════════════════════
        private void ProximoEstagio()
        {
            _estagioAtual++;
            Fase = "mapa";
            SetFeedback("jornada", $"Estágio {_estagioAtual}");
        }

        // ═══════════════════════════════════════════════════
        //  RESTAURAÇÃO DE RECURSOS
        // ═══════════════════════════════════════════════════
        private void RestaurarEntreEstagios()
        {
            if (_jogador == null) return;
            // Cura: metade do HP faltante
            int faltando = _jogador.HpMaximo - _jogador.HpAtual;
            int cura = (faltando + 1) / 2;
            if (cura > 0)
            {
                _jogador.HpAtual = Math.Min(_jogador.HpAtual + cura, _jogador.HpMaximo);
                Log($"Descanso rápido: +{cura} HP.");
            }
            RestaurarSlots(parcial: true);
        }

        private void RestaurarDescansoLongo()
        {
            if (_jogador == null) return;
            int recuperado = _jogador.HpMaximo - _jogador.HpAtual;
            _jogador.HpAtual = _jogador.HpMaximo;
            Log($"Descanso longo! HP totalmente restaurado (+{recuperado}).");
            RestaurarSlots(parcial: false);
            _slotsMaximos = new Dictionary<int, int>(_jogador.SlotsDeMagia);
        }

        private void RestaurarSlots(bool parcial)
        {
            if (_jogador == null) return;

            // Garante novos slots no snapshot
            foreach (var kv in _jogador.SlotsDeMagia)
                if (!_slotsMaximos.ContainsKey(kv.Key))
                    _slotsMaximos[kv.Key] = kv.Value;

            foreach (var kv in _slotsMaximos)
            {
                if (_jogador.SlotsDeMagia.ContainsKey(kv.Key) && _jogador.SlotsDeMagia[kv.Key] < kv.Value)
                    _jogador.SlotsDeMagia[kv.Key] = kv.Value;
            }
            Log("Slots de magia restaurados.");

            if (_jogador.UsosSurtoDeAcao < _surtoDeAcaoMax)
            {
                _jogador.UsosSurtoDeAcao = _surtoDeAcaoMax;
                Log("Surto de Ação restaurado.");
            }
            if (_jogador.UsosCanalizarDivindade < _canalizarDivindadeMax)
            {
                _jogador.UsosCanalizarDivindade = _canalizarDivindadeMax;
                Log("Canalizar Divindade restaurado.");
            }
            if (_reservaCuraMaosMax > 0 && _jogador.ReservaCuraPelasMaos < _reservaCuraMaosMax)
            {
                _jogador.ReservaCuraPelasMaos = _reservaCuraMaosMax;
                Log("Cura pelas Mãos restaurada.");
            }

            // Limpa buffs
            _jogador.TemBencao                   = false;
            _jogador.TemEscudoDaFe               = false;
            _jogador.TemArmaMagica               = false;
            _jogador.ProximoAtaqueTrovejante     = false;
            _jogador.TemResistenciaFisica        = false;
            _jogador.TemVantagemProximoAtaque    = false;
            _jogador.OndaVitalidadeTurnosRestantes = 0;
            _jogador.HpEscudoArcano              = 0;
            _vantagemJogador    = 0;
            _desvantagemInimigo = 0;
            _paralisiaInimigo   = 0;
        }

        // ═══════════════════════════════════════════════════
        //  MECÂNICAS DE COMBATE
        // ═══════════════════════════════════════════════════
        private void AtacarComArma(bool usarSmite, string? dadoExtra, string nome)
        {
            if (_jogador?.ArmaEquipada == null || _inimigoAtual == null) return;

            int nAtaques = _jogador.NumeroDeAtaques; // 1 ou 2 (nível 5+)
            for (int i = 0; i < nAtaques; i++)
            {
                int rolagem  = RolarD20Jogador();
                int mod      = _jogador.ArmaEquipada.UsaDestreza ? _jogador.ModificadorDestreza : _jogador.ModificadorForca;
                int bonus    = mod + BonusProficiencia(_jogador.Nivel);
                int total    = rolagem + bonus;

                Log($"{nome} (ataque {i + 1}): d20({rolagem})+{bonus}={total} vs CA {_inimigoAtual.ClasseArmadura}.");

                if (rolagem == 1)                                { SetFeedback("erro", "Erro crítico!"); continue; }
                if (rolagem != 20 && total < _inimigoAtual.ClasseArmadura) { SetFeedback("erro", "Errou!"); continue; }

                int dano = Dados.Rolar(_jogador.ArmaEquipada.DadoDeDano) + mod;
                if (!string.IsNullOrWhiteSpace(dadoExtra))
                    dano += Dados.Rolar(dadoExtra);
                if (rolagem == 20)
                {
                    dano += Dados.Rolar(_jogador.ArmaEquipada.DadoDeDano);
                    Log("Crítico!");
                    SetFeedback("critico", "Crítico!");
                }
                if (usarSmite)
                {
                    int danoSmite = Dados.Rolar("2d8");
                    dano += danoSmite;
                    Log($"Destruição Divina +{danoSmite} radiante!");
                    SetFeedback("smite", $"Smite +{danoSmite}");
                }

                AplicarDanoInimigo(Math.Max(1, dano), nome);
                if (_inimigoAtual.HpAtual <= 0) break;
            }
        }

        private void AtacarMagico(string nome, string dado)
        {
            if (_inimigoAtual == null || _jogador == null) return;

            int rolagem = RolarD20Jogador();
            int bonus   = _jogador.ModificadorInteligencia + BonusProficiencia(_jogador.Nivel);
            int total   = rolagem + bonus;
            Log($"{nome}: d20({rolagem})+{bonus}={total} vs CA {_inimigoAtual.ClasseArmadura}.");

            if (rolagem == 1 || (rolagem != 20 && total < _inimigoAtual.ClasseArmadura))
            {
                SetFeedback("erro", "Magia errou!");
                return;
            }

            int dano = Dados.Rolar(dado) + _jogador.ModificadorInteligencia;
            if (rolagem == 20) dano += Dados.Rolar(dado);
            AplicarDanoInimigo(Math.Max(1, dano), nome);
        }

        private void AplicarDanoInimigo(int dano, string fonte)
        {
            if (_inimigoAtual == null) return;
            _inimigoAtual.HpAtual = Math.Max(0, _inimigoAtual.HpAtual - dano);
            if (_proxyInimigo != null) _proxyInimigo.HpAtual = _inimigoAtual.HpAtual;
            Log($"{fonte}: {dano} de dano em {_inimigoAtual.Nome}. HP: {_inimigoAtual.HpAtual}/{_inimigoAtual.HpMaximo}.");
            if (FeedbackTipo is not ("critico" or "smite"))
                SetFeedback("dano", $"-{dano} HP");
        }

        // ═══════════════════════════════════════════════════
        //  HELPERS
        // ═══════════════════════════════════════════════════
        private int RolarD20Jogador()
        {
            if (_vantagemJogador > 0)
            {
                int a = Dados.RolarD20(), b = Dados.RolarD20();
                _vantagemJogador = 0;
                return Math.Max(a, b);
            }
            return Dados.RolarD20();
        }

        private bool GastarSlot(int nivel)
        {
            if (_jogador == null) return false;
            if (_jogador.GastarSlotDeMagia(nivel)) return true;
            SetFeedback("erro", $"Sem slot de nível {nivel}+");
            return false;
        }

        private void ResetarTurno()
        {
            _acaoPrincipalUsada = false;
            _acaoBonusUsada     = false;
            _acoesExtras        = 0;
        }

        private static bool EhAcaoBonus(string acao) =>
            acao is "cura" or "folego" or "surto" or "certeiro" or "pocao";

        private static int BonusProficiencia(int nivel) => nivel >= 5 ? 3 : 2;

        private static FichaPersonagem CriarProxy(FichaInimigo ini)
        {
            var proxy = new FichaPersonagem(ini.Nome, new ClasseVazia());
            proxy.Forca          = ini.Forca;
            proxy.Destreza       = ini.Destreza;
            proxy.Constituicao   = ini.Constituicao;
            proxy.HpMaximo       = ini.HpMaximo;
            proxy.HpAtual        = ini.HpAtual;
            proxy.ClasseArmadura = ini.ClasseArmadura;
            proxy.MargemCritico  = ini.MargemCritico;
            return proxy;
        }

        private static IClasseRPG CriarClasse(string? id) => (id ?? "paladino").ToLowerInvariant() switch
        {
            "guerreiro" => new Guerreiro(),
            "mago"      => new Mago(),
            _           => new Paladino()
        };

        private static string NomePadrao(string classe) => classe switch
        {
            "Guerreiro" => "Kael Varn",
            "Mago"      => "Eldrin",
            _           => "Sir Alden"
        };

        private static int OuroInicial(string classe) => classe switch
        {
            "Guerreiro" => 30,
            "Mago"      => 45,
            _           => 35
        };

        private void EquiparInicial(string classe)
        {
            if (_jogador == null) return;
            if (classe == "Mago")
            {
                _jogador.EquiparArmadura(ScriptInicial.Armaduras.First(a => a.Nome == "Armadura Acolchoada"));
                _jogador.EquiparArma(ScriptInicial.Armas.First(a => a.Nome == "Cajado de Mago"));
            }
            else if (classe == "Guerreiro")
            {
                _jogador.EquiparArmadura(ScriptInicial.Armaduras.First(a => a.Nome == "Cota de Malha"));
                _jogador.EquiparArma(ScriptInicial.Armas.First(a => a.Nome == "Espada Grande"));
            }
            else // Paladino
            {
                _jogador.EquiparArmadura(ScriptInicial.Armaduras.First(a => a.Nome == "Cota de Talas"));
                _jogador.EquiparArma(ScriptInicial.Armas.First(a => a.Nome == "Espada Longa"));
                _jogador.ReservaCuraPelasMaos = _jogador.Nivel * 5;
            }
        }

        private void SetFeedback(string tipo, string texto)
        {
            FeedbackTipo  = tipo;
            FeedbackTexto = texto;
        }

        private void Log(string msg) => _log.Add(msg);

        // ═══════════════════════════════════════════════════
        //  CONSTRUIR RESPOSTA JSON
        // ═══════════════════════════════════════════════════
        public object CriarResposta()
        {
            // Classes disponíveis para tela de criação
            var classes = new[]
            {
                new { id="guerreiro", nome="Guerreiro",  arquetipo="O Mestre de Armas",
                      descricao="Alta resistência e versatilidade marcial. Surto de Ação permite atacar duas vezes em um turno.",
                      imagem="assets/guerreiro.png",
                      habilidades = new[]{"Ataque Básico","Golpe Preciso (+1d6)","Retomar o Fôlego (bônus)","Surto de Ação (bônus)"},
                      hp="1d10 + CON", armadura="Pesada", dado="1d10" },
                new { id="mago",      nome="Mago",       arquetipo="O Erudito Arcano",
                      descricao="Frágil mas com magias poderosas. Proteção Arcana cria um escudo mágico que absorve dano.",
                      imagem="assets/mago.png",
                      habilidades = new[]{"Rajada Arcana","Raio de Gelo (truque)","Raio de Fogo (nível 6)","Mísseis Mágicos [Slot]","Paralisar [Slot]","Proteção c/ Lâminas","Ataque Certeiro (bônus)"},
                      hp="1d6 + CON", armadura="Leve", dado="1d6" },
                new { id="paladino",  nome="Paladino",   arquetipo="O Baluarte Sagrado",
                      descricao="Armadura alta e Cura pelas Mãos. Destruição Divina gasta slots de magia para dano radiante extra.",
                      imagem="assets/paladino.png",
                      habilidades = new[]{"Ataque Básico","Destruição Divina [Slot]","Cura pelas Mãos (bônus)"},
                      hp="1d10 + CON", armadura="Pesada", dado="1d10" }
            };

            // Dados do inimigo atual
            object? inimigoDto = null;
            if (_inimigoAtual != null)
            {
                bool ehBoss = _inimigoAtual.Categoria is "Mini-Boss" or "Boss Final";
                inimigoDto = new
                {
                    nome             = _inimigoAtual.Nome,
                    descricao        = _inimigoAtual.Descricao,
                    categoria        = _inimigoAtual.Categoria,
                    nd               = _inimigoAtual.NivelDeDesafio,
                    hpAtual          = _inimigoAtual.HpAtual,
                    hpMaximo         = _inimigoAtual.HpMaximo,
                    classeArmadura   = _inimigoAtual.ClasseArmadura,
                    ataque           = _inimigoAtual.Ataque.Nome,
                    descricaoAtaque  = _inimigoAtual.Ataque.Descricao,
                    dado             = _inimigoAtual.Ataque.DadoDeDano,
                    boss             = ehBoss,
                    fala             = FalaDoInimigo(_inimigoAtual.Nome),
                    imagem           = ImagemInimigo(_inimigoAtual.Nome)
                };
            }

            // Dados do jogador
            object? jogadorDto = null;
            if (_jogador != null)
            {
                int pocoes = _jogador.Inventario.OfType<Consumivel>().Count();
                _jogador.SlotsDeMagia.TryGetValue(1, out int s1);
                _jogador.SlotsDeMagia.TryGetValue(2, out int s2);
                _jogador.SlotsDeMagia.TryGetValue(3, out int s3);

                jogadorDto = new
                {
                    nome           = _jogador.Nome,
                    classe         = _jogador.Classe.NomeDaClasse,
                    nivel          = _jogador.Nivel,
                    xp             = _jogador.XP,
                    xpProximoNivel = _jogador.Nivel * 100,
                    hpAtual        = Math.Max(0, _jogador.HpAtual),
                    hpMaximo       = _jogador.HpMaximo,
                    classeArmadura = _jogador.ClasseArmadura,
                    ouro           = _jogador.Ouro,
                    curaPelasMaos  = _jogador.ReservaCuraPelasMaos,
                    slotsNivel1    = s1,
                    slotsNivel2    = s2,
                    slotsNivel3    = s3,
                    pocoes         = pocoes,
                    arma           = _jogador.ArmaEquipada?.Nome ?? "Nenhuma",
                    armadura       = _jogador.ArmaduraVestida?.Nome ?? "Nenhuma",
                    surtoDeAcao    = _jogador.UsosSurtoDeAcao,
                    atributos      = new { forca=_jogador.Forca, destreza=_jogador.Destreza,
                                           constituicao=_jogador.Constituicao, inteligencia=_jogador.Inteligencia,
                                           sabedoria=_jogador.Sabedoria, carisma=_jogador.Carisma },
                    inventario     = _jogador.Inventario.Select(i => i.Nome).ToList()
                };
            }

            return new
            {
                fase               = Fase,
                estagioAtual       = _estagioAtual,
                estagioMax         = ESTAGIO_MAX,
                vitoria            = Fase == "fim" && _jogador?.HpAtual > 0,
                derrota            = Fase == "fim" && (_jogador == null || _jogador.HpAtual <= 0),
                jogador            = jogadorDto,
                inimigo            = inimigoDto,
                turno              = new { acaoPrincipalUsada=_acaoPrincipalUsada, acaoBonusUsada=_acaoBonusUsada,
                                           acoesExtras=_acoesExtras, paralisiaInimigo=_paralisiaInimigo > 0,
                                           vantagemJogador=_vantagemJogador > 0 },
                feedback           = new { tipo=FeedbackTipo, texto=FeedbackTexto },
                classes            = classes,
                log                = _log.TakeLast(20).ToList(),
                inimigosDerrotados = _inimigosDerrotados,
                caminhosPercorridos= _caminhosPercorridos.ToList(),
                // Mapa de estágios para a tela de mapa
                mapa               = GerarMapa()
            };
        }

        private List<object> GerarMapa()
        {
            var mapa = new List<object>();
            for (int i = 1; i <= ESTAGIO_MAX; i++)
            {
                var ini = CatalogoDeEstagio.ObterInimigo(i);
                mapa.Add(new
                {
                    numero    = i,
                    nome      = ini.Nome,
                    categoria = ini.Categoria,
                    nd        = ini.NivelDeDesafio,
                    atual     = i == _estagioAtual,
                    passado   = i < _estagioAtual,
                    imagem    = ImagemInimigo(ini.Nome)
                });
            }
            return mapa;
        }

        private static string FalaDoInimigo(string nome) => nome switch
        {
            var n when n.Contains("Ogro")     => "VOCÊ... PEQUENOOO! OGRO VAI ESMAGAR!",
            var n when n.Contains("Árvore")   => "*As raízes rangem e as folhas sussurram em uma língua antiga...*",
            var n when n.Contains("Manticora")=> "Carne fresca... Finalmente algo digno de ser devorado!",
            var n when n.Contains("Lich")     => "Audacioso mortal... Você ousou chegar até mim?\nQue seus ossos sirvam à minha coleção por toda a eternidade.",
            var n when n.Contains("Lobo")     => "*Um uivo gélido ressoa pela floresta corrupta...*",
            var n when n.Contains("Zumbi")    => "*Gemidos e passos arrastados se aproximam nas trevas...*",
            var n when n.Contains("Esqueleto")=> "*O tilintar de ossos e metal enferrujado ecoa pelo corredor...*",
            var n when n.Contains("Orc")      => "Sua cabeça vai decorar minha lança!",
            var n when n.Contains("Bugbear")  => "*Um rugido gutural vibra pelas paredes de pedra...*",
            var n when n.Contains("Arbusto")  => "*Galhos finos se movem sem vento, convergindo em sua direção...*",
            _                                 => "Não passarás!"
        };

        private static string ImagemInimigo(string nome) => nome switch
        {
            var n when n.Contains("Arbusto") || n.Contains("Twig") => "assets/enemies/twig_blight.png",
            var n when n.Contains("Lobo")                          => "assets/enemies/lobo_corrompido.png",
            var n when n.Contains("Esqueleto")                     => "assets/enemies/esqueleto_aventureiro.png",
            var n when n.Contains("Zumbi")                         => "assets/enemies/zumbi_putrefato.png",
            var n when n.Contains("Orc")                           => "assets/enemies/orc_sangue.png",
            var n when n.Contains("Bugbear")                       => "assets/enemies/bugbear_menor.png",
            var n when n.Contains("Ogro")                          => "assets/enemies/ogro_esmagador.png",
            var n when n.Contains("Árvore") || n.Contains("Arvore")=> "assets/enemies/arvore_maligna.png",
            var n when n.Contains("Manticora")                     => "assets/enemies/manticora_faminta.png",
            var n when n.Contains("Lich")                          => "assets/enemies/lich_desperto.png",
            _                                                      => "assets/enemies/twig_blight.png"
        };
    }
}
