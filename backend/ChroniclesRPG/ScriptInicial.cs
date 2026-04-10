using ChroniclesRPG.Entidades;

namespace ChroniclesRPG
{
    public static class ScriptInicial
    {
        public static void CarregarDados()
        {
            // ==========================================
            // INSTANCIA DE ARMADURAS
            // ==========================================

            //Armaduras Leves
            Armadura acolchoada = new Armadura("Armadura Acolchoada", "Armadura leve de tecido acolchoado.", TipoArmadura.Leve, 5, 1);
            Armadura couro = new Armadura("Armadura de couro", "Armadura de couro animel", TipoArmadura.Leve, 10, 1);
            Armadura couroBatido = new Armadura("Armadura de couro batido", "Armadura de couro batido", TipoArmadura.Leve, 45, 2);

            //Armaduras Médias
            Armadura gibao = new Armadura("Gibão", "Armadura média de couro batido.", TipoArmadura.Media, 10, 3);
            Armadura camisaoDeMalha = new Armadura("Camisão de Malha", "Armadura média de malha de metal.", TipoArmadura.Media, 30, 4);
            Armadura brunea = new Armadura("Brunea", "Armadura média de metal.", TipoArmadura.Media, 50, 5);
            Armadura peitoral = new Armadura("Peitoral", "Armadura média de metal.", TipoArmadura.Media, 400, 6);
            Armadura meiaArmadura = new Armadura("Meia Armadura", "Armadura média de metal.", TipoArmadura.Media, 750, 7);

            //Armaduras Pesadas
            Armadura cotaDeAneis = new Armadura("Cota de Anéis", "Armadura pesada de anéis entrelaçados.", TipoArmadura.Pesada, 30, 6);
            Armadura cotaDeMalha = new Armadura("Cota de Malha", "Armadura pesada de anéis entrelaçados.", TipoArmadura.Pesada, 75, 8);
            Armadura cotaDeTalas = new Armadura("Cota de Talas", "Armadura pesada de anéis entrelaçados.", TipoArmadura.Pesada, 200, 9);
            Armadura armaduraCompleta = new Armadura("Armadura Completa", "Armadura pesada de anéis entrelaçados.", TipoArmadura.Pesada, 1500, 10);

            // ==========================================
            // INSTANCIA DE ARMAS
            // ==========================================

            // --- Lâminas Curtas ---
            Arma adaga = new Arma("Adaga", "Pequena e fácil de esconder.", TipoArma.LaminasCurtas,TipoDano.Perfurante, 2, "1d4", true); // Acuidade
            Arma foiceCurta = new Arma("Foice Curta", "Lâmina curva para colheita ou combate.", TipoArma.LaminasCurtas,TipoDano.Cortante, 1, "1d4");

            // --- Lâminas Longas ---
            Arma espadaCurta = new Arma("Espada Curta", "Lâmina curta e perfurante.", TipoArma.LaminasLongas,TipoDano.Perfurante, 10, "1d6", true); // Acuidade
            Arma espadaLonga = new Arma("Espada Longa", "Uma espada de lâmina reta afiada.", TipoArma.LaminasLongas,TipoDano.Cortante, 15, "1d8");
            Arma cimitarra = new Arma("Cimitarra", "Lâmina curva e ágil.", TipoArma.LaminasLongas,TipoDano.Cortante, 25, "1d6", true); // Acuidade
            Arma rapieira = new Arma("Rapieira", "Lâmina fina e elegante.", TipoArma.LaminasLongas,TipoDano.Perfurante, 25, "1d8", true); // Acuidade

            // --- Lâminas Pesadas ---
            Arma espadaGrande = new Arma("Espada Grande", "Lâmina enorme de duas mãos.", TipoArma.LaminasPesadas,TipoDano.Cortante, 50, "2d6");
            Arma alabarda = new Arma("Alabarda", "Haste longa com lâmina de machado.", TipoArma.LaminasPesadas,TipoDano.Cortante, 20, "1d10");
            Arma glaive = new Arma("Glaive", "Lâmina larga em uma haste.", TipoArma.LaminasPesadas,TipoDano.Cortante, 20, "1d10");

            // --- Machados ---
            Arma machadinha = new Arma("Machadinha", "Machado pequeno de mão.", TipoArma.Machados,TipoDano.Cortante, 5, "1d6");
            Arma machadoBatalha = new Arma("Machado de Batalha", "Machado versátil de metal.", TipoArma.Machados,TipoDano.Cortante, 10, "1d8");
            Arma machadoGrande = new Arma("Machado Grande", "Machado pesado de guerra.", TipoArma.Machados,TipoDano.Cortante, 30, "1d12");

            // --- Impacto ---
            Arma porrete = new Arma("Porrete", "Um porrete simples de madeira.", TipoArma.Impacto,TipoDano.Concussão, 1, "1d4");
            Arma maca = new Arma("Maça", "Cabo de madeira com cabeça de metal.", TipoArma.Impacto,TipoDano.Concussão, 5, "1d6");
            Arma marteloLeve = new Arma("Martelo Leve", "Martelo pequeno para arremesso.", TipoArma.Impacto,TipoDano.Concussão, 2, "1d4");
            Arma marteloGuerra = new Arma("Martelo de Guerra", "Martelo pesado para quebrar armaduras.", TipoArma.Impacto,TipoDano.Concussão, 15, "1d8");
            Arma malho = new Arma("Malho", "Martelo enorme de duas mãos.", TipoArma.Impacto,TipoDano.Concussão, 10, "2d6");

            // --- Hastes ---
            Arma bordao = new Arma("Bordão", "Um cajado de madeira resistente.", TipoArma.Hastes,TipoDano.Concussão, 1, "1d6");
            Arma clavaGrande = new Arma("Clava Grande", "Um pedaço de madeira pesado.", TipoArma.Hastes,TipoDano.Concussão, 1, "1d8");
            Arma lanca = new Arma("Lança", "Haste de madeira com ponta de metal.", TipoArma.Hastes,TipoDano.Perfurante, 1, "1d6");
            Arma tridente = new Arma("Tridente", "Lança de três pontas.", TipoArma.Hastes,TipoDano.Perfurante, 5, "1d6");

            // --- Arcos ---
            Arma arcoCurto = new Arma("Arco Curto", "Arco compacto de madeira.", TipoArma.Arcos,TipoDano.Perfurante, 25, "1d6", true);
            Arma arcoLongo = new Arma("Arco Longo", "Arco grande para longas distâncias.", TipoArma.Arcos,TipoDano.Perfurante, 50, "1d8", true);

            // --- Bestas ---
            Arma bestaLeve = new Arma("Besta Leve", "Besta mecânica simples.", TipoArma.Bestas,TipoDano.Perfurante, 25, "1d8", true);
            Arma bestaPesada = new Arma("Besta Pesada", "Besta mecânica potente.", TipoArma.Bestas,TipoDano.Perfurante, 50, "1d10", true);

            // --- Arremesso ---
            Arma dardo = new Arma("Dardo", "Pequena lâmina de arremesso.", TipoArma.Arremesso,TipoDano.Perfurante, 1, "1d4", true);
            Arma funda = new Arma("Funda", "Tira de couro para lançar pedras.", TipoArma.Arremesso,TipoDano.Concussão, 1, "1d4", true);
            Arma azagaia = new Arma("Azagaia", "Uma lança leve de arremesso.", TipoArma.Arremesso,TipoDano.Perfurante, 1, "1d6");

            // --- Cajados ---
            Arma cajadoSimples = new Arma("Cajado Simples", "Um cajado de madeira resistente.", TipoArma.Cajado,TipoDano.Concussão, 1, "1d6");
            Arma cajadoMago = new Arma("Cajado de Mago", "Um cajado encantado.", TipoArma.Cajado,TipoDano.Concussão, 1, "1d6");
            Arma bastao = new Arma("Bastão", "Um bastão de madeira resistente.", TipoArma.Cajado,TipoDano.Concussão, 1, "1d6");

            Consumivel pocaoCura = new Consumivel("Poção de Cura", "Um líquido vermelho borbulhante.", 10, 5);
        }
    }
}
