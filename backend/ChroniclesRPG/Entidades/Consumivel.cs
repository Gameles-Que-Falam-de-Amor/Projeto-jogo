namespace ChroniclesRPG.Entidades
{
    public class Consumivel : Item
    {
        public int Cura { get; set; }

        public Consumivel(string nome, string descricao, int valor, int cura) 
            : base(nome, descricao, valor)
        {
            Cura = cura;
        }

        // Aqui a classe recebe quem vai beber a poção para poder curar
        public void Usar(FichaPersonagem alvo)
        {
            alvo.HpAtual += Cura;
            
            // Garante que a cura não ultrapasse o HP Máximo
            if (alvo.HpAtual > alvo.HpMaximo)
            {
                alvo.HpAtual = alvo.HpMaximo;
            }
        }
    }
}