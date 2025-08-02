namespace SemanticTest.Models
{
    public class InfosProcessus
    {
        public string Nom { get; set; } = string.Empty;
        public int PID { get; set; } // identifiant unique du processus
        public string TitreFenetre { get; set; } = string.Empty;
        public long MemoireOctets { get; set; }
        public string HeureDemarrage { get; set; } = string.Empty;
    }
}
