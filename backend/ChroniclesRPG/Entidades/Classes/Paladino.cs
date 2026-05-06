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
                    ficha.HabilidadesConhecidas.Add(new DestruicaoDivina());
                    ficha.ReceberSlotsDeMagia(1, 2);
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Destruição Divina e Magias de Nível 1!");
                    break;
                case 3:
                    ficha.UsosCanalizarDivindade = 1;
                    ficha.HabilidadesConhecidas.Add(new OndaDeVitalidade());
                    ficha.HabilidadesConhecidas.Add(new MagiaBencao());
                    ficha.HabilidadesConhecidas.Add(new MagiaEscudoDaFe());
                    ficha.HabilidadesConhecidas.Add(new MagiaDestruicaoTrovejante());
                    ficha.ReceberSlotsDeMagia(1, 1); // Total 3
                    Console.WriteLine($"  {ficha.Nome} fez o Juramento da Proteção e aprendeu novas magias!");
                    break;
                case 4:
                    ficha.Forca += 2;
                    Console.WriteLine($"  Os atributos de {ficha.Nome} aumentaram!");
                    break;
                case 5:
                    // Nível 5: Ataque Extra e Magias de Nível 2
                    ficha.HabilidadesConhecidas.Add(new AtaqueExtra());
                    ficha.NumeroDeAtaques = 2; // Passa a realizar 2 ataques por ação de ataque
                    ficha.HabilidadesConhecidas.Add(new MagiaAjuda());
                    ficha.HabilidadesConhecidas.Add(new MagiaRestauracaoMenor());
                    ficha.ReceberSlotsDeMagia(1, 1); // Total 4
                    ficha.ReceberSlotsDeMagia(2, 2); // Total 2
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Ataque Extra e Magias de Nível 2!");
                    break;
                case 6:
                    ficha.HabilidadesConhecidas.Add(new AuraDeProtecao());
                    ficha.HabilidadesConhecidas.Add(new MagiaArmaMagica());
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Aura de Proteção e Arma Mágica!");
                    break;
            }
        }
    }
}