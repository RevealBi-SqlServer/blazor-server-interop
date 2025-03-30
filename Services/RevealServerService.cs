using DashboardViewer.RevealServer;
using Reveal.Sdk.Dom;
using Reveal.Sdk;
using DashboardViewer.Models;

public class RevealServerService : IRevealServerService
{
    private readonly string _dashboardsFolderPath;

    public RevealServerService()
    {
        _dashboardsFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "Dashboards");
    }

    public async Task<List<DashboardNames>> GetDashboardNamesList()
    {
        var dashboardNames = new List<DashboardNames>();

        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Dashboards");
        var files = Directory.GetFiles(folderPath, "*.rdash"); 

        var tasks = files.Select(async file =>
        {
            try
            {
                var dashboard = new Dashboard(file);
                var thumbnailInfo = await dashboard.GetInfoAsync(Path.GetFileNameWithoutExtension(file).ToString());
                var info = await dashboard.GetInfoAsync(Path.GetFileNameWithoutExtension(file));

                return new DashboardNames
                {
                    DashboardFilename = Path.GetFileNameWithoutExtension(file),
                    DashboardTitle = RdashDocument.Load(file).Title,
                    ThumbnailInfo = info.Info
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Reading FileData {file}: {ex.Message}");
                return null;
            }
        });

        var results = await Task.WhenAll(tasks);
        dashboardNames = results.Where(result => result != null).ToList();

        return dashboardNames;
    }

    public bool DeleteDashboard(string dashboardFilename)
    {
        string filePath = Path.Combine(_dashboardsFolderPath, $"{dashboardFilename}.rdash");
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Deleting File {filePath}: {ex.Message}");
                return false;
            }
        }
        else
        {
            Console.WriteLine($"File {filePath} does not exist.");
            return false;
        }
    }

    public bool IsDuplicateDashboard(string name)
    {
        var filePath = Path.Combine(_dashboardsFolderPath, $"{name}.rdash");
        return File.Exists(filePath);
    }
}
