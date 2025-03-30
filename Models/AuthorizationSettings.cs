namespace DashboardViewer.Models
{
    /// <summary>
    /// Represents the authorization settings for the dashboard viewer.
    /// </summary>
    public class AuthorizationSettings
    {
        /// <summary>
        /// Gets or sets the list of table names that administrators are allowed to access.
        /// Use this in the ObjectFilter code, or in the DataSourceItemProvider
        /// to determine what tables the user is allowed to access based on the 
        /// UserContextProvider.
        /// </summary>
        public List<string> AllowedTablesAdmin { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of table names that regular users are allowed to access.
        /// Use this in the ObjectFilter code, or in the DataSourceItemProvider
        /// to determine what tables the user is allowed to access based on the 
        /// UserContextProvider.
        /// </summary>
        public List<string> AllowedTablesUser { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of user IDs that have administrative privileges.
        /// This is used in the ObjectFilter code to determine if the user is an admin.
        /// </summary>
        public List<string> AdminUserIds { get; set; } = new();
    }
}
