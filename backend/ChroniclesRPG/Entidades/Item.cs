namespace ChroniclesRPG.Entidades
{
    public abstract class Item
    {
        private string _nome;
        private string _descricao;
        private int _valor;

        public string Nome 
        { 
            get { return _nome; } 
            set { _nome = value; } 
        }
        
        public string Descricao 
        { 
            get { return _descricao; } 
            set { _descricao = value; } 
        }
        
        public int Valor 
        { 
            get { return _valor; } 
            set { _valor = value; } 
        }

        protected Item(string nome, string descricao, int valor)
        {
            Nome = nome;
            Descricao = descricao;
            Valor = valor;
        }
    }
}