using Reveal.Sdk;

namespace RevealSdk.Server.Reveal
{
    /// <summary>
    /// The `DashboardProvider` class implements the `IRVDashboardProvider` interface 
    /// to customize where Reveal BI loads and saves dashboards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong>
    /// The primary function of `DashboardProvider` is to customize 
    /// where dashboards are saved and loaded. It uses the `userContext` information to 
    /// allow conditional loading and saving, based on specific users or other contextual 
    /// properties. For example, it might load dashboards from a user-specific folder or 
    /// store dashboards in a different repository, like a database.
    /// </para>
    /// <para>
    /// <strong>Provider Setup:</strong>
    /// To use this custom dashboard provider, it must be registered in the DI container 
    /// in `Program.cs` by calling `.AddDashboardProvider<DashboardProvider>()`. If not 
    /// registered, Reveal defaults to loading and saving dashboards from the standard 
    /// `/dashboards` folder.
    /// </para>
    /// <para>
    /// <strong>Key Methods:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `GetDashboardAsync(IRVUserContext userContext, string dashboardId)`:
    /// This asynchronous method retrieves a dashboard by ID from a specified folder, 
    /// here set to `Dashboards` under the current working directory. The `userContext` 
    /// parameter can be used to conditionally load dashboards based on the requesting user.
    /// The method constructs the file path for the requested dashboard ID and loads it from 
    /// `Dashboards`.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `SaveDashboardAsync(IRVUserContext userContext, string dashboardId, Dashboard dashboard)`:
    /// This asynchronous method saves the specified `dashboard` to a file path under the 
    /// `Dashboards` folder with the given dashboard ID. The `userContext` parameter 
    /// can be used to customize the save location based on the user's context. The `SaveToFileAsync` 
    /// method of `Dashboard` is used to persist the dashboard to the specified path.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage Notes:</strong>
    /// This example code saves and loads dashboards only from the `Dashboards` folder. 
    /// In production, this could be extended to dynamically determine save locations 
    /// or integrate with a database for centralized dashboard storage.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register the DashboardProvider in Program.cs
    /// builder.Services.AddDashboardProvider<DashboardProvider>();
    /// </code>
    /// </example>
    /// <seealso cref="https://help.revealbi.io/web/saving-dashboards/#example-implementing-save-with-irvdashboardprovider"/>
    public class DashboardProvider : IRVDashboardProvider
    {
        /// <summary>
        /// Retrieves a dashboard by ID from a specified folder.
        /// </summary>
        /// <param name="userContext">The user context for user-specific loading.</param>
        /// <param name="dashboardId">The ID of the dashboard to retrieve.</param>
        /// <returns>A <see cref="Dashboard"/> object representing the loaded dashboard.</returns>
        public Task<Dashboard> GetDashboardAsync(IRVUserContext userContext, string dashboardId)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, $"Dashboards/{dashboardId}.rdash");
            var dashboard = new Dashboard(filePath);
            return Task.FromResult(dashboard);
        }

        /// <summary>
        /// Saves the specified dashboard to a file path under the `Dashboards` folder.
        /// </summary>
        /// <param name="userContext">The user context for user-specific saving.</param>
        /// <param name="dashboardId">The ID of the dashboard to save.</param>
        /// <param name="dashboard">The <see cref="Dashboard"/> object to save.</param>
        /// <returns>A task representing the asynchronous save operation.</returns>
        public async Task SaveDashboardAsync(IRVUserContext userContext, string dashboardId, Dashboard dashboard)
        {
            var filePath = Path.Combine(Environment.CurrentDirectory, $"Dashboards/{dashboardId}.rdash");
            await dashboard.SaveToFileAsync(filePath);
        }
    }
}
