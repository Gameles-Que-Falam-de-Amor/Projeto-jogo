using ChroniclesRPG.Funções;
using ChroniclesRPG.Entidades.Itens;

namespace ChroniclesRPG.Entidades.Habilidades{
    public class AtaqueBasico : Habilidade{
        public AtaqueBasico() 
            : base("Ataque Básico", "Realiza um ataque com a arma equipada.", TipoAcao.AcaoPrincipal, TipoHabilidade.AtaqueFisico) 
        { }

        public override int Executar(FichaPersonagem usuario, FichaPersonagem? alvo = null){

            // --- Pré-condições ---
            if (usuario.ArmaEquipada == null){
                Console.WriteLine($"  {usuario.Nome} não tem uma arma equipada para atacar!");
                return 0;
            }
            if (alvo == null){
                Console.WriteLine($"  Ataque Básico requer um alvo!");
                return 0;
            }

            int danoTotalTurno = 0;

            for (int i = 1; i <= usuario.NumeroDeAtaques; i++){
                if (usuario.NumeroDeAtaques > 1){
                    Console.WriteLine($"\n  --- Ataque {i} de {usuario.NumeroDeAtaques} ---");
                }

                // ==========================================
                // PASSO 1: ESCOLHER O ATRIBUTO DE ATAQUE
                // ==========================================
                bool ehArmaDeAlcance = usuario.ArmaEquipada.Tipo == TipoArma.Arcos 
                                    || usuario.ArmaEquipada.Tipo == TipoArma.Bestas
                                    || usuario.ArmaEquipada.Tipo == TipoArma.Arremesso;

                int modificadorDeAtaque;

                if (ehArmaDeAlcance){
                    modificadorDeAtaque = usuario.ModificadorDestreza;
                } else if (usuario.ArmaEquipada.UsaDestreza){
                    modificadorDeAtaque = Math.Max(usuario.ModificadorForca, usuario.ModificadorDestreza);
                } else {
                    modificadorDeAtaque = usuario.ModificadorForca;
                }

                // ==========================================
                // PASSO 2: ROLAR O D20 DE ACERTO
                // ==========================================
                int rolagem = Dados.RolarD20();
                int totalDeAtaque = rolagem + modificadorDeAtaque;

                string nomeAtributo = ehArmaDeAlcance || usuario.ArmaEquipada.UsaDestreza && usuario.ModificadorDestreza > usuario.ModificadorForca 
                    ? "DEX" : "FOR";

                Console.WriteLine($"  {usuario.Nome} ataca {alvo.Nome} com {usuario.ArmaEquipada.Nome}!");
                Console.WriteLine($"    Rolagem de acerto: 1d20({rolagem}) + {nomeAtributo}({modificadorDeAtaque:+#;-#;+0}) = {totalDeAtaque} vs CA {alvo.ClasseArmadura}");

                // ==========================================
                // PASSO 3: COMPARAR COM A CA DO ALVO
                // ==========================================
                if (rolagem >= usuario.MargemCritico){
                    int dano1 = Dados.Rolar(usuario.ArmaEquipada.DadoDeDano);
                    int dano2 = Dados.Rolar(usuario.ArmaEquipada.DadoDeDano);
                    int danoTotal = dano1 + dano2 + modificadorDeAtaque;

                    alvo.HpAtual -= danoTotal;
                    Console.WriteLine($"    ACERTO CRÍTICO! Dano: {dano1}+{dano2}+{modificadorDeAtaque} = {danoTotal} ({usuario.ArmaEquipada.TipoDano})");
                    Console.WriteLine($"    HP de {alvo.Nome}: {alvo.HpAtual}/{alvo.HpMaximo}");
                    danoTotalTurno += danoTotal;
                }
                else if (rolagem == 1){
                    Console.WriteLine($"    ERRO CRÍTICO! O ataque falha completamente.");
                }
                else if (totalDeAtaque >= alvo.ClasseArmadura){
                    int dano = Dados.Rolar(usuario.ArmaEquipada.DadoDeDano) + modificadorDeAtaque;
                    dano = Math.Max(1, dano);

                    alvo.HpAtual -= dano;
                    Console.WriteLine($"    ACERTOU! Dano: {dano} ({usuario.ArmaEquipada.TipoDano})");
                    Console.WriteLine($"    HP de {alvo.Nome}: {alvo.HpAtual}/{alvo.HpMaximo}");
                    danoTotalTurno += dano;
                } else {
                    Console.WriteLine($"    ERROU! O ataque não penetrou a defesa do alvo.");
                }

                // Interrompe os ataques se o alvo já foi derrotado
                if (alvo.HpAtual <= 0){
                    Console.WriteLine($"\n  O {alvo.Nome} foi derrotado!");
                    break;
                }
            }

            return danoTotalTurno;
        }
    }
}