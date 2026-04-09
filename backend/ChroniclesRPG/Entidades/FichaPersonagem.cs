namespace ChroniclesRPG.Entidades
{
    public class FichaPersonagem
    {
        // ==========================================
        // INFORMAÇÕES BÁSICAS
        // ==========================================
        public string Nome { get; set; }
        public IClasseRPG Classe { get; private set; }
        public int Nivel { get; set; } = 1;
        public int XP { get; set; } = 0;
        public int Ouro { get; set; } = 0;
        public string Raca { get; set; }
        public string Lore { get; set; }

        // ==========================================
        // ATRIBUTOS BASE (Página em branco)
        // ==========================================
        public int Forca { get; set; }
        public int Destreza { get; set; }
        public int Constituicao { get; set; }
        public int Inteligencia { get; set; }
        public int Sabedoria { get; set; }
        public int Carisma { get; set; }

        // ==========================================
        // MODIFICADORES 
        // ==========================================
        public int ModificadorForca => (Forca - 10) / 2;
        public int ModificadorDestreza => (Destreza - 10) / 2;
        public int ModificadorConstituicao => (Constituicao - 10) / 2;
        public int ModificadorInteligencia => (Inteligencia - 10) / 2;
        public int ModificadorSabedoria => (Sabedoria - 10) / 2;
        public int ModificadorCarisma => (Carisma - 10) / 2;

        // ==========================================
        // STATUS DE COMBATE 
        // ==========================================
        public int Iniciativa => ModificadorDestreza; 
        public int ClasseArmadura { get; set; }
        public int HpMaximo { get; set; }
        public int HpAtual { get; set; }

        // ==========================================
        // ITENS 
        // ==========================================
        public Armadura ArmaduraVestida { get; set; }
        public Arma ArmaEquipada { get; set; }
        public List<Item> Inventario { get; set; } = new List<Item>();

        // ==========================================
        // CONSTRUTOR
        // ==========================================
        public FichaPersonagem(string nome, IClasseRPG classeEscolhida)
        {
            Nome = nome;
            Classe = classeEscolhida;
            
            // 1. A ficha está "zerada". O método abaixo será o responsável 
            // por preencher todos os atributos da ficha (Força, Destreza, etc.)
            Classe.AplicarBonusIniciais(this);

            // 2. O HP só é calculado APÓS a classe preencher a Constituição
            HpMaximo = Classe.DadoDeVida + ModificadorConstituicao;
            HpAtual = HpMaximo;
        }
    }
}