using DashboardViewer.Models;
using Microsoft.Extensions.Options;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Reveal.Sdk;
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;
using System.Text.RegularExpressions;

namespace RevealSdk.Server.Reveal
{
    /// <summary>
    /// The `DataSourceProvider` class implements the `IRVDataSourceProvider` interface.
    /// The main purpose of `DataSourceProvider` is to dynamically set connection properties 
    /// (like host and database) and customize SQL queries or stored procedures based on 
    /// incoming requests, such as table requests from the client side. This is essential 
    /// for scenarios that involve dynamic data sources, multi-tenancy, or secure, 
    /// role-based access to data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Provider Setup:</strong>
    /// This provider must be registered in the DI container in `Program.cs` with the 
    /// following call: `.AddDataSourceProvider<DataSourceProvider>()`. Without this 
    /// registration, Reveal BI will not use this custom data source configuration.
    /// </para>
    /// <para>
    /// <strong>Key Methods:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `ChangeDataSourceAsync(IRVUserContext userContext, RVDashboardDataSource dataSource)`:
    /// This asynchronous method configures data source connection properties (e.g., 
    /// setting the SQL Server host and database) based on user context and data source type.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `ChangeDataSourceItemAsync(IRVUserContext userContext, string dashboardId, RVDataSourceItem dataSourceItem)`:
    /// This method sets or modifies SQL queries based on the data source item requested 
    /// by the client. It includes support for specific procedures (e.g., `CustOrderHist`), 
    /// ad-hoc queries (e.g., `CustomerOrders`), and role-based access. It validates 
    /// parameters and query format to prevent SQL injection and unauthorized access.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Validation and Security Helpers:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `IsValidCustomerId` and `IsValidOrderId`: Regular expressions validate that customer 
    /// and order IDs are well-formed to prevent SQL injection attacks and enforce 
    /// proper data formats.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `EscapeSqlInput`: Sanitizes SQL inputs by escaping single quotes in dynamic queries 
    /// to prevent SQL injection.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `IsSelectOnly`: A helper function that parses SQL using `TSql150Parser` and 
    /// `ReadOnlySelectVisitor` to ensure only read-only `SELECT` statements are allowed, 
    /// helping prevent malicious SQL statements in custom queries.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage Notes:</strong>
    /// Sensitive data, such as credentials and server details, should be retrieved from 
    /// secure configurations (e.g., `IConfiguration` and app secrets). In production, 
    /// queries should be strictly validated to avoid any security vulnerabilities.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register the DataSourceProvider in Program.cs
    /// builder.Services.AddDataSourceProvider<DataSourceProvider>();
    /// </code>
    /// </example>
    /// <seealso cref="https://help.revealbi.io/web/datasources/"/>
    /// <seealso cref="https://help.revealbi.io/web/adding-data-sources/ms-sql-server/"/>
    /// <seealso cref="https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows"/>
    internal class DataSourceProvider : IRVDataSourceProvider
    {
        private readonly SqlServerSettings _sqlSettings;
        private readonly AuthorizationSettings _authSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceProvider"/> class.
        /// </summary>
        /// <param name="sqlOptions">The options containing SQL Server settings.</param>
        /// <param name="authOptions">The options containing authorization settings.</param>
        public DataSourceProvider(IOptions<SqlServerSettings> sqlOptions, IOptions<AuthorizationSettings> authOptions)
        {
            _sqlSettings = sqlOptions.Value;
            _authSettings = authOptions.Value;
        }

        /// <summary>
        /// Configures data source connection properties based on user context and data source type.
        /// </summary>
        /// <param name="userContext">The user context for user-specific configuration.</param>
        /// <param name="dataSource">The data source to configure.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task<RVDashboardDataSource> ChangeDataSourceAsync(IRVUserContext userContext, RVDashboardDataSource dataSource)
        {
            if (dataSource is RVSqlServerDataSource sqlDs)
            {
                sqlDs.Host = _sqlSettings.DatabaseHost;
                sqlDs.Database = _sqlSettings.Database;
            }
            return Task.FromResult(dataSource);
        }

        /// <summary>
        /// Sets or modifies SQL queries based on the data source item requested by the client.
        /// </summary>
        /// <param name="userContext">The user context for user-specific configuration.</param>
        /// <param name="dashboardId">The ID of the dashboard.</param>
        /// <param name="dataSourceItem">The data source item to configure.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public Task<RVDataSourceItem> ChangeDataSourceItemAsync(IRVUserContext userContext, string dashboardId, RVDataSourceItem dataSourceItem)
        {
            // Every request for data passes thru changeDataSourceItem
            // You can set query properties based on the incoming requests
            // for example, you can check:
            // - dsi.id
            // - dsi.table
            // - dsi.procedure
            // - dsi.title
            // and take a specific action on the dsi as this request is processed

            if (dataSourceItem is not RVSqlServerDataSourceItem sqlDsi) return Task.FromResult(dataSourceItem);

            // Ensure data source is updated
            ChangeDataSourceAsync(userContext, sqlDsi.DataSource);

            string customerId = userContext.UserId;
            string orderId = userContext.Properties["OrderId"]?.ToString();
            bool isAdmin = userContext.Properties["Role"]?.ToString() == "Admin";

            var allowedTables = isAdmin ? _authSettings.AllowedTablesAdmin : _authSettings.AllowedTablesUser;

            switch (sqlDsi.Id)
            {
                // Example of how to use a stored procedure with a parameter
                case "CustOrderHist":
                case "CustOrdersOrders":
                    if (!IsValidCustomerId(customerId))
                        throw new ArgumentException("Invalid CustomerID format. CustomerID must be a 5-character alphanumeric string.");
                    sqlDsi.Procedure = sqlDsi.Id;
                    sqlDsi.ProcedureParameters = new Dictionary<string, object> { { "@CustomerID", customerId } };
                    break;

                // Example of how to use a stored procedure 
                case "TenMostExpensiveProducts":
                    sqlDsi.Procedure = "Ten Most Expensive Products";
                    break;

                // Example of an ad-hoc-query
                case "CustomerOrders":
                    if (!IsValidOrderId(orderId))
                        throw new ArgumentException("Invalid OrderId format. OrderId must be a 5-digit numeric value.");

                    orderId = EscapeSqlInput(orderId);
                    string customQuery = $"SELECT * FROM Orders WHERE OrderId = '{orderId}'";
                    if (!IsSelectOnly(customQuery))
                        throw new ArgumentException("Invalid SQL query.");
                    sqlDsi.CustomQuery = customQuery;
                    break;

                // Example pulling in the list of allowed tables that have the customerId column name
                // this ensures that _any_ time a request is made for customer specific data in allowed tables
                // the customerId parameter is passed
                // note that the Admin role is not restricted to a custom query, the Admin role will see all 
                // customer data with no restriction
                // the tables being checked are in the allowedtables.json
                case var table when allowedTables.Contains(sqlDsi.Table):
                    if (isAdmin && dashboardId != "Customer Orders")
                        break;

                    if (!IsValidCustomerId(customerId))
                        throw new ArgumentException("Invalid CustomerID format. CustomerID must be a 5-character alphanumeric string.");

                    customerId = EscapeSqlInput(customerId);
                    string query = $"SELECT * FROM [{sqlDsi.Table}] WHERE customerId = '{customerId}'";
                    if (!IsSelectOnly(query))
                        throw new ArgumentException("Invalid SQL query.");

                    sqlDsi.CustomQuery = query;
                    break;

                default:
                    // If you do not want to allow any other tables, throw an exception
                    // throw new ArgumentException("Invalid Table");
                    // return null;
                    break;
            }

            return Task.FromResult(dataSourceItem);
        }

        // Modify any of the code below to meet your specific needs
        // The code below is not part of the Reveal SDK, these are helpers to clean / validate parameters
        // specific to this sample code.  For example, ensuring the customerId & orderId are well formed, 
        // and ensuring that no invalid / illegal statements are passed in the header to the custom query

        private static bool IsValidCustomerId(string customerId) => Regex.IsMatch(customerId, @"^[A-Za-z0-9]{5}$");
        private static bool IsValidOrderId(string orderId) => Regex.IsMatch(orderId, @"^\d{5}$");
        private string EscapeSqlInput(string input) => input.Replace("'", "''");

        public bool IsSelectOnly(string sql)
        {
            TSql150Parser parser = new TSql150Parser(true);
            IList<ParseError> errors;
            TSqlFragment fragment;

            using (TextReader reader = new StringReader(sql))
            {
                fragment = parser.Parse(reader, out errors);
            }

            if (errors.Count > 0)
            {
                foreach (var error in errors)
                {
                    Console.WriteLine($"Error: {error.Message}");
                }
                return false;
            }

            var visitor = new ReadOnlySelectVisitor();
            fragment.Accept(visitor);
            return visitor.IsReadOnly;
        }
    }
}