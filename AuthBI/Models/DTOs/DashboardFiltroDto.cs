namespace AuthBI.Models.DTOs
{
    public class DashboardFiltroDto
    {
        public List<string> ContaAnalitica { get; set; } = new();
        public List<string> Nivel0 { get; set; } = new();
        public List<string> Nivel1 { get; set; } = new();
        public List<string> Nivel2 { get; set; } = new();
        public List<string> Nivel3 { get; set; } = new();
    }

}
