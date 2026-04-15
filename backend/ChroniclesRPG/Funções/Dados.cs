using System;

namespace ChroniclesRPG.Funções{

    public static class Dados{

        // Instância única para evitar números repetidos em execuções muito rápidas
        private static readonly Random _gerador = new Random();

        /// <summary>
        /// Rola um dado baseado em uma string de notação (ex: "1d8", "2d6").
        /// </summary>
        public static int Rolar(string notacao){
            if (string.IsNullOrWhiteSpace(notacao)) {
                return 0;
            };

            try {
                // Converte para minúsculo e divide no 'd'
                string[] partes = notacao.ToLower().Split('d');
                
                int quantidade = int.Parse(partes[0]);
                int faces = int.Parse(partes[1]);
                
                int total = 0;
                for (int i = 0; i < quantidade; i++){
                    // Next(min, max) -> o max é exclusivo, por isso o +1
                    total += _gerador.Next(1, faces + 1);
                }
                
                return total;
            }catch (Exception){
                Console.WriteLine($"Erro ao rolar o dado: {notacao}. Verifique o formato.");
                return 0;
            }
        }

        /// <summary>
        /// Atalho para a rolagem mais importante do D&D.
        /// </summary>
        public static int RolarD20() => _gerador.Next(1, 21);
    }
}