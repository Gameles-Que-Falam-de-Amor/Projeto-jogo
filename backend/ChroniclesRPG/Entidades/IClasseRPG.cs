namespace ChroniclesRPG.Entidades
{
    // O "I" no começo é uma convenção do C# para Interfaces
    public interface IClasseRPG
    {
        string NomeDaClasse { get; }
        int DadoDeVida { get; }
        List<TipoArmadura> ProficienciasArmadura { get; }
        // Contrato: Toda classe terá que dizer como altera a ficha no nível 1
        void AplicarBonusIniciais(FichaPersonagem ficha);
    }
}