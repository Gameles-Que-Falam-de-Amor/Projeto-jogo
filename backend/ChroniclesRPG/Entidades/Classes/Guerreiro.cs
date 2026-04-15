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
            switch (nivel)
            {
                case 1:
                    // No nível 1, ele aprende a bater e a se curar
                    ficha.HabilidadesConhecidas.Add(new AtaqueBasico());
                    ficha.HabilidadesConhecidas.Add(new RetomarFolego());
                    break;

                case 2:
                    // No nível 2, ele aprenderia o Surto de Ação (Action Surge)
                    // ficha.HabilidadesConhecidas.Add(new SurtoDeAcao());
                    Console.WriteLine($"{ficha.Nome} aprendeu uma nova habilidade de nível 2!");
                    break;

                case 5:
                    // No nível 5, ele ganharia Ataque Extra
                    // ficha.HabilidadesConhecidas.Add(new AtaqueExtra());
                    break;
            }
        }
    }
}