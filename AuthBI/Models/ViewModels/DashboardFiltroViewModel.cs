namespace AuthBI.Models.ViewModels
{
    public class DashboardFiltroViewModel
    {
        public List<string> ContaAnalitica { get; set; } = new();
        public List<string> Nivel0 { get; set; } = new();
        public List<string> Nivel1 { get; set; } = new();
        public List<string> Nivel2 { get; set; } = new();
        public List<string> Nivel3 { get; set; } = new();
    }
}
