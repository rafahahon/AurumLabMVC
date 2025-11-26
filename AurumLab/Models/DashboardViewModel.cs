

using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AurumLab.Models
{
    public class DashboardViewModel
    {
        public int TotalDispositivos { get; set; }
        public int TotalAtivos { get; set; }
        public int TotalEmManutencao { get; set; }
        public int TotalInoperantes { get; set; }

        // usuario
        public string NomeUsuario { get; set; }
        public string FotoUsuario { get; set; }

        // lista de agrupamentos 
        public List<ItemAgrupado> DispositivosPorTipo { get; set; }
        public List<LocalDispositivo> Locais { get; set; }
    }
}