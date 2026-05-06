using ChroniclesRPG.Entidades;
using ChroniclesRPG.Entidades.Itens;
using ChroniclesRPG.Funções;
using ChroniclesRPG.Entidades.Habilidades;

namespace ChroniclesRPG.Entidades.Classes{
    public class Mago : IClasseRPG{
        // ==========================================
        // 1. CONTRATOS DA INTERFACE
        // ==========================================
        public string NomeDaClasse => "Mago";
        public int VidaInicial => 6;
        public string DadoDeVida => "1d6";
        public List<TipoArmadura> ProficienciasArmadura => new List<TipoArmadura>{ 
            TipoArmadura.Leve 
        };
        public List<TipoArma> ProficienciasArmas => new List<TipoArma>{ 
            TipoArma.LaminasCurtas,  // Adagas
            TipoArma.Arremesso,      // Dardos e Fundas
            TipoArma.Bestas,         // Bestas Leves
            TipoArma.Hastes,         // Bordões
            TipoArma.Cajado          // Cajado Arcano
        };

        // ==========================================
        // 2. APLICAÇÃO DOS ATRIBUTOS
        // ==========================================

        public int CalcularVida(){
            return Dados.Rolar(DadoDeVida);
        }

        public void AplicarBonusIniciais(FichaPersonagem ficha){
            
            // Atributos Principais (Foco em Inteligência e Sobrevivência)
            ficha.Inteligencia = 16; // Modificador +3 (Principal para magias)
            ficha.Destreza = 15;     // Modificador +2 (Ajuda na CA e Iniciativa, crucial para o Mago)
            
            // Atributos Secundários
            ficha.Constituicao = 14; // Modificador +2 (Um pouco mais robusto que o padrão 13)
            ficha.Sabedoria = 12;    // Modificador +1
            ficha.Carisma = 10;      // Modificador 0
            ficha.Forca = 8;         // Modificador -1 (O Mago é frágil físicamente)
        }

        // ==========================================
        // 3. HABILIDADES DE NÍVEL
        // ==========================================
        public void AplicarHabilidadesDeNivel(FichaPersonagem ficha, int nivel){
            switch (nivel){
                case 1:
                    // Nível 1: 3 truques iniciais + 2 slots de nível 1
                    ficha.HabilidadesConhecidas.Add(new AtaqueBasico());
                    ficha.HabilidadesConhecidas.Add(new AtaqueCerteiro());
                    ficha.HabilidadesConhecidas.Add(new ProtecaoContraLaminas());
                    ficha.HabilidadesConhecidas.Add(new RaioDeGelo());
                    ficha.ReceberSlotsDeMagia(1, 2);
                    Console.WriteLine($"  {ficha.Nome} aprendeu os truques iniciais e recebeu 2 slots de magia de 1º círculo!");
                    break;

                case 2:
                    // Nível 2: Tradição Arcana (Escola de Abjuração) + slot extra nível 1
                    ficha.HabilidadesConhecidas.Add(new ProtecaoArcana());
                    ficha.ReceberSlotsDeMagia(1, 1); // Total: 3
                    Console.WriteLine($"  {ficha.Nome} escolheu a Escola de Abjuração e aprendeu: Proteção Arcana!");
                    break;

                case 3:
                    // Nível 3: slot extra nível 1 + 2 slots nível 2
                    ficha.ReceberSlotsDeMagia(1, 1); // Total: 4
                    ficha.ReceberSlotsDeMagia(2, 2);
                    Console.WriteLine($"  {ficha.Nome} desbloqueou slots de 2º círculo! (4x Nível 1 | 2x Nível 2)");
                    break;

                case 4:
                    // Nível 4: Incremento de Atributo + slot extra nível 2
                    ficha.Inteligencia += 2;
                    ficha.ReceberSlotsDeMagia(2, 1); // Total: 3
                    Console.WriteLine($"  A Inteligência de {ficha.Nome} aumentou para {ficha.Inteligencia}! (4x Nível 1 | 3x Nível 2)");
                    break;

                case 5:
                    // Nível 5: 2 slots de 3º círculo (truques de nível 0 escalam automaticamente pelo Nivel)
                    ficha.ReceberSlotsDeMagia(3, 2);
                    Console.WriteLine($"  {ficha.Nome} domina a magia de 3º círculo! (4x Nível 1 | 3x Nível 2 | 2x Nível 3)");
                    Console.WriteLine($"  Raio de Gelo e Raio de Fogo agora causam dano dobrado!");
                    break;

                case 6:
                    // Nível 6: Proteção Projetada + Raio de Fogo (4º truque) + slot extra nível 3
                    ficha.HabilidadesConhecidas.Add(new ProtecaoProjetada());
                    ficha.HabilidadesConhecidas.Add(new RaioDeFogo());
                    ficha.ReceberSlotsDeMagia(3, 1); // Total: 3
                    Console.WriteLine($"  {ficha.Nome} aprendeu: Proteção Projetada e o truque Raio de Fogo!");
                    break;
            }
        }
    }
}