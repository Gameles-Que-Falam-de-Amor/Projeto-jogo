using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Itens;
using ChroniclesRPG.Funções;
using ChroniclesRPG.Entidades.Habilidades;

namespace ChroniclesRPG.Entidades.Classes
{
    public class Guerreiro : IClasseRPG{
        // ==========================================
        // 1. CONTRATOS DA INTERFACE
        // ==========================================
        public string NomeDaClasse => "Guerreiro";
        public int VidaInicial => 10;
        public string DadoDeVida => "1d10";
        public List<TipoArmadura> ProficienciasArmadura => new List<TipoArmadura>{ 
            TipoArmadura.Leve, TipoArmadura.Media, TipoArmadura.Pesada 
        };
        public List<TipoArma> ProficienciasArmas => new List<TipoArma>{ 
            TipoArma.LaminasCurtas, TipoArma.LaminasLongas, TipoArma.LaminasPesadas,
            TipoArma.Machados, TipoArma.Impacto, TipoArma.Hastes,
            TipoArma.Arcos, TipoArma.Bestas, TipoArma.Arremesso
        };

        // ==========================================
        // 2. APLICAÇÃO DOS ATRIBUTOS
        // ==========================================

        public int CalcularVida(){
            return Dados.Rolar(DadoDeVida);
        }

        public void AplicarBonusIniciais(FichaPersonagem ficha){
            
            // Como a ficha começa zerada, a classe Guerreiro vai ditar 
            // a "distribuição padrão" (Standard Array) ideal para ele.
            
            // Atributos Principais (Foco em combate corpo-a-corpo e sobrevivência)
            ficha.Forca = 16;        // Modificador +3
            ficha.Constituicao = 15; // Modificador +2 (Garante +2 de HP extra por nível)
            ficha.Destreza = 13;     // Modificador +1 (Ajuda um pouco na CA e Iniciativa)
            
            // Atributos Secundários (O Guerreiro padrão não foca em magias)
            ficha.Sabedoria = 12;    // Modificador +1
            ficha.Inteligencia = 10; // Modificador 0
            ficha.Carisma = 8;       // Modificador -1
        }

        // ==========================================
        // 3. HABILIDADES DE NÍVEL
        // ==========================================
        public void AplicarHabilidadesDeNivel(FichaPersonagem ficha, int nivel){
            switch (nivel){
                case 1:
                    // Nível 1: Ataque Básico + Retomar o Fôlego + Estilo de Luta
                    ficha.HabilidadesConhecidas.Add(new AtaqueBasico());
                    ficha.HabilidadesConhecidas.Add(new RetomarFolego());
                    // Estilo de Luta padrão — futuramente pode ser escolhido pelo jogador
                    //ficha.HabilidadesConhecidas.Add(new EstiloDeLuta(EstiloLuta.Duelismo));
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Ataque Básico, Retomar o Fôlego e Estilo de Luta (Duelismo)!");
                    break;

                case 2:
                    // Nível 2: Surto de Ação
                    ficha.HabilidadesConhecidas.Add(new SurtoDeAcao());
                    ficha.UsosSurtoDeAcao = 1; // 1 uso a partir do nível 2
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Surto de Ação!");
                    break;

                case 3:
                    // Nível 3: Arquétipo Marcial — Campeão ganha Crítico Aprimorado
                    ficha.HabilidadesConhecidas.Add(new ArquetipoMarcial());
                    //ficha.MargemCritico = 19; // Crítico agora ocorre com 19 ou 20
                    Console.WriteLine($"  {ficha.Nome} escolheu o arquétipo Campeão e aprendeu: Crítico Aprimorado!");
                    break;

                case 4:
                    // Nível 4: Aumento de Atributo (Feat/Aumento de Atributo)
                    ficha.Forca += 2; 
                    Console.WriteLine($"  A Força de {ficha.Nome} aumentou para {ficha.Forca}!");
                    break;

                case 5:
                    // Nível 5: Ataque Extra
                    ficha.HabilidadesConhecidas.Add(new AtaqueExtra());
                    ficha.NumeroDeAtaques = 2; // Passa a realizar 2 ataques por ação de ataque
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Ataque Extra!");
                    break;

                case 6:
                    // Nível 6: Aumento de Atributo (Feat/Aumento de Atributo)
                    ficha.Forca += 2; 
                    Console.WriteLine($"  A Força de {ficha.Nome} aumentou para {ficha.Forca}!");
                    break;
            }
        }
    }
}