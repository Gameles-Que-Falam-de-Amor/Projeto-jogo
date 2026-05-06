using ChroniclesRPG.Entidades.Habilidades;
using ChroniclesRPG.Entidades.Itens;
using ChroniclesRPG.Funções;

namespace ChroniclesRPG.Entidades.Classes
{
    public class Paladino : IClasseRPG{

        // ==========================================
        // 1. CONTRATOS DA INTERFACE
        // =========================================='
        public string NomeDaClasse => "Paladino";
        public int VidaInicial => 10;
        public string DadoDeVida => "1d10";
        public List<TipoArmadura> ProficienciasArmadura => new List<TipoArmadura>{ 
            TipoArmadura.Leve, TipoArmadura.Media, TipoArmadura.Pesada 
        };
        public List<TipoArma> ProficienciasArmas => new List<TipoArma>{ 
            TipoArma.LaminasCurtas, TipoArma.LaminasLongas, TipoArma.LaminasPesadas,
            TipoArma.Machados, TipoArma.Impacto, TipoArma.Hastes, TipoArma.Arremesso 
        };

        // ==========================================
        // 2. APLICAÇÃO DOS ATRIBUTOS
        // ==========================================

        public int CalcularVida(){
            return Dados.Rolar(DadoDeVida);
        }

        public void AplicarBonusIniciais(FichaPersonagem ficha){

            ficha.Forca = 16;        // Modificador +3
            ficha.Constituicao = 15; // Modificador +2
            ficha.Carisma = 14;      // Modificador +2
            ficha.Sabedoria = 12;    // Modificador +1
            ficha.Destreza = 10;     // Modificador 0
            ficha.Inteligencia = 8;  // Modificador -1
            ficha.ReservaCuraPelasMaos = ficha.Nivel * 5;
        }

        // ==========================================
        // 3. HABILIDADES DE NÍVEL
        // ==========================================

        public void AplicarHabilidadesDeNivel(FichaPersonagem ficha, int nivel){

            switch (nivel){
                case 1:
                    ficha.HabilidadesConhecidas.Add(new AtaqueBasico());
                    ficha.HabilidadesConhecidas.Add(new CuraPelasMaos());
                    ficha.ReservaCuraPelasMaos = ficha.Nivel * 5;
                    break;
                case 2:
                    //ficha.ReceberSlotsDeMagia(1, 2);
                    break;
                case 3:
                   // ficha.ReceberSlotsDeMagia(1, 1);
                    break;
                case 4:
                   // ficha.Forca += 2;
                    break;
                case 5:
                    //ficha.ReceberSlotsDeMagia(1, 1);
                    //ficha.ReceberSlotsDeMagia(2, 2);
                    //ficha.TemAtaqueExtra = true;
                    break;
            }
        }
    }
}