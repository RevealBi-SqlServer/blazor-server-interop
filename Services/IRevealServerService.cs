using DashboardViewer.Models;

namespace DashboardViewer.RevealServer
{
    public interface IRevealServerService
    {
        Task<List<DashboardNames>> GetDashboardNamesList();
        bool DeleteDashboard(string DashboardFilename);
        bool IsDuplicateDashboard(string DashboardFilename);
    }
}
