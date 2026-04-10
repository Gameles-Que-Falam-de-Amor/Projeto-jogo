namespace ChroniclesRPG.Entidades
{
    public class Guerreiro : IClasseRPG
    {
        // ==========================================
        // 1. CONTRATOS DA INTERFACE
        // ==========================================
        public string NomeDaClasse => "Guerreiro";
        public int DadoDeVida => 10; // Rola 1d10 para vida, garantindo muito HP
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
        public void AplicarBonusIniciais(FichaPersonagem ficha)
        {
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
    }
}