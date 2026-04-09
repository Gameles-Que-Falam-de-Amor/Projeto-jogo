namespace ChroniclesRPG.Entidades
{
    // Armadura "é um" Item
    public class Armadura : Item 
    {
        public int BonusDefesa { get; set; }

        // O ": base()" repassa o nome, desc e valor para o construtor do Pai
        public Armadura(string nome, string descricao, int valor, int bonusDefesa) 
            : base(nome, descricao, valor)
        {
            BonusDefesa = bonusDefesa;
        }
    }
}