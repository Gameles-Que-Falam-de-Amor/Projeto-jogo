using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Itens;

namespace ChroniclesRPG.Entidades.Classes
{
    /// <summary>
    /// Implementação mínima de IClasseRPG usada para criar FichaPersonagem de proxy
    /// (espelho de FichaInimigo). Não é uma classe jogável — apenas permite instanciar
    /// uma FichaPersonagem sem lógica de progressão.
    /// </summary>
    public class ClasseVazia : IClasseRPG
    {
        public string NomeDaClasse => "Inimigo";
        public string DadoDeVida  => "1d1";
        public int    VidaInicial => 0;

        public List<TipoArmadura> ProficienciasArmadura => new();
        public List<TipoArma>     ProficienciasArmas     => new();

        public void AplicarBonusIniciais(FichaPersonagem ficha) { /* sem bônus */ }
        public int  CalcularVida() => 0;
        public void AplicarHabilidadesDeNivel(FichaPersonagem ficha, int nivel) { /* sem habilidades */ }
    }
}
