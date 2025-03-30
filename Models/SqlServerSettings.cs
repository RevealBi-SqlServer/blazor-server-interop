namespace DashboardViewer.Models
{
    /// <summary>
    /// Represents the settings required to connect to a SQL Server database.
    /// </summary>
    /// <remarks>
    /// This class is used to store the database connection settings such as user, password, host, database name, and port.
    /// These properties are using the DataSourceProvider and AuthenticationProvider to set the connection string details
    /// </remarks>
    public class SqlServerSettings
    {
        /// <summary>
        /// Gets or sets the database user.
        /// </summary>
        public string DatabaseUser { get; set; }

        /// <summary>
        /// Gets or sets the database password.
        /// </summary>
        public string DatabasePassword { get; set; }

        /// <summary>
        /// Gets or sets the database host.
        /// </summary>
        public string DatabaseHost { get; set; }

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string Database { get; set; }

        /// <summary>
        /// Gets or sets the database port.
        /// </summary>
        public string DatabasePort { get; set; }
    }
}
