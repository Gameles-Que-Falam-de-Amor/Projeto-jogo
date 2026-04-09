namespace ChroniclesRPG.Entidades
{
    public class Arma : Item
    {
        public string DadoDeDano { get; set; } // Ex: "1d8"
        public bool UsaDestreza { get; set; } // Para adagas ou arcos

        public Arma(string nome, string descricao, int valor, string dadoDeDano, bool usaDestreza = false) 
            : base(nome, descricao, valor)
        {
            DadoDeDano = dadoDeDano;
            UsaDestreza = usaDestreza;
        }
    }
}