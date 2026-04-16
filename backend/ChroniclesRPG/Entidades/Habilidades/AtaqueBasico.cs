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

            // ==========================================
            // PASSO 1: ESCOLHER O ATRIBUTO DE ATAQUE
            // ==========================================
            // Armas com UsaDestreza (adagas, arcos, armas finesse) usam o maior entre FOR e DEX.
            // Armas normais de corpo a corpo sempre usam Força.
            // Armas de alcance (Arcos, Bestas, Arremesso) sempre usam Destreza.

            bool ehArmaDeAlcance = usuario.ArmaEquipada.Tipo == TipoArma.Arcos 
                                || usuario.ArmaEquipada.Tipo == TipoArma.Bestas
                                || usuario.ArmaEquipada.Tipo == TipoArma.Arremesso;

            int modificadorDeAtaque;

            if (ehArmaDeAlcance){
                // Armas de alcance sempre usam Destreza
                modificadorDeAtaque = usuario.ModificadorDestreza;
            } else if (usuario.ArmaEquipada.UsaDestreza){
                // Armas "finesse" (ex: adaga, rapieira): usa o MAIOR entre FOR e DEX
                modificadorDeAtaque = Math.Max(usuario.ModificadorForca, usuario.ModificadorDestreza);
            } else {
                // Armas corpo a corpo normais: usa Força
                modificadorDeAtaque = usuario.ModificadorForca;
            }

            // ==========================================
            // PASSO 2: ROLAR O D20 DE ACERTO
            // ==========================================
            // Fórmula: 1d20 + modificador de atributo
            // (Bônus de proficiência pode ser adicionado aqui futuramente)

            int rolagem = Dados.RolarD20();
            int totalDeAtaque = rolagem + modificadorDeAtaque;

            // Apenas para exibição no console
            string nomeAtributo = ehArmaDeAlcance || usuario.ArmaEquipada.UsaDestreza && usuario.ModificadorDestreza > usuario.ModificadorForca 
                ? "DEX" : "FOR";

            Console.WriteLine($"  {usuario.Nome} ataca {alvo.Nome} com {usuario.ArmaEquipada.Nome}!");
            Console.WriteLine($"    Rolagem de acerto: 1d20({rolagem}) + {nomeAtributo}({modificadorDeAtaque:+#;-#;+0}) = {totalDeAtaque} vs CA {alvo.ClasseArmadura}");
            //---------------------------

            // ==========================================
            // PASSO 3: COMPARAR COM A CA DO ALVO
            // ==========================================

            // Acerto crítico (nat 20): sempre acerta e rola o dado de dano duas vezes
            if (rolagem == 20){
                int dano1 = Dados.Rolar(usuario.ArmaEquipada.DadoDeDano);
                int dano2 = Dados.Rolar(usuario.ArmaEquipada.DadoDeDano);
                int danoTotal = dano1 + dano2 + modificadorDeAtaque;

                alvo.HpAtual -= danoTotal;
                Console.WriteLine($"    ACERTO CRÍTICO! Dano: {dano1}+{dano2}+{modificadorDeAtaque} = {danoTotal} ({usuario.ArmaEquipada.TipoDano})");
                Console.WriteLine($"    HP de {alvo.Nome}: {alvo.HpAtual}/{alvo.HpMaximo}");
                return danoTotal;
            }

            // Erro crítico (nat 1): sempre erra, independente de bônus
            if (rolagem == 1){
                Console.WriteLine($"    ERRO CRÍTICO! O ataque falha completamente.");
                return 0;
            }

            // Acerto normal: total precisa ser >= CA do alvo
            if (totalDeAtaque >= alvo.ClasseArmadura){
                int dano = Dados.Rolar(usuario.ArmaEquipada.DadoDeDano) + modificadorDeAtaque;
                // O dano mínimo é sempre 1, mesmo com modificador negativo
                dano = Math.Max(1, dano);

                alvo.HpAtual -= dano;
                Console.WriteLine($"    ACERTOU! Dano: {dano} ({usuario.ArmaEquipada.TipoDano})");
                Console.WriteLine($"    HP de {alvo.Nome}: {alvo.HpAtual}/{alvo.HpMaximo}");
                return dano;
            } else {
                Console.WriteLine($"    ERROU! O ataque não penetrou a defesa do alvo.");
                return 0;
            }
        }
    }
}