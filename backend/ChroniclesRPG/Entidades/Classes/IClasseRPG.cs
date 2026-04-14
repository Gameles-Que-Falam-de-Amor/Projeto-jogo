using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Itens;

namespace ChroniclesRPG.Entidades.Classes
{
    // O "I" no começo é uma convenção do C# para Interfaces
    public interface IClasseRPG
    {
        string NomeDaClasse { get; }
        int DadoDeVida { get; }

        // ==========================================
        // PROFICIÊNCIAS
        // ==========================================

        // Contrato: Toda classe deve declarar com quais tipos de armadura tem proficiência
        List<TipoArmadura> ProficienciasArmadura { get; }

        // Contrato: Toda classe deve declarar com quais tipos de arma tem proficiência
        List<TipoArma> ProficienciasArmas { get; }

        // Contrato: Toda classe terá que dizer como altera a ficha no nível 1
        void AplicarBonusIniciais(FichaPersonagem ficha);
    }
}