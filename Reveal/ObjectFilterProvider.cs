using DashboardViewer.Models;
using Microsoft.Extensions.Options;
using Reveal.Sdk;
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;

namespace RevealSdk.Server.Reveal
{

    /// <summary>
    /// The `ObjectFilterProvider` class implements the `IRVObjectFilter` interface  
    /// to filter which data sources and data source items (such as tables, views, or 
    /// stored procedures) are accessible to users within the Reveal BI environment. 
    /// It allows you to control the display and access of these database objects 
    /// based on user-specific criteria, such as user roles.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong>
    /// The `ObjectFilterProvider` serves as an optional filter mechanism, letting you 
    /// restrict access to certain databases or specific database objects in Reveal BI. 
    /// This is useful for enforcing security and data access policies, especially in 
    /// multi-tenant applications or scenarios with role-based data access.
    /// </para>
    /// <para>
    /// <strong>Provider Setup:</strong>
    /// This provider must be registered in the DI container in `Program.cs` by calling 
    /// `.AddObjectFilter<ObjectFilterProvider>()`. If not registered, Reveal will not 
    /// apply any custom filtering logic to data source or data source items.
    /// </para>
    /// <para>
    /// <strong>Key Methods:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `Filter(IRVUserContext userContext, RVDashboardDataSource dataSource)`:
    /// This method checks if a specific data source is allowed for the user based on 
    /// a predefined list of allowed databases. It can leverage `userContext` to restrict 
    /// data access by user or tenant, ensuring only authorized data sources are accessible.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `Filter(IRVUserContext userContext, RVDataSourceItem dataSourceItem)`:
    /// This method filters individual database objects, such as tables and stored procedures, 
    /// based on user roles. In this example, users with a "User" role are restricted to 
    /// viewing only the `All Orders` and `All Invoices` tables, while users with an 
    /// "Admin" role have unrestricted access. Role information is retrieved from `userContext`.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Configuration and Security:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `_authSettings`: An instance of `AuthorizationSettings`, injected via constructor dependency injection. 
    /// It provides access to app settings and secrets, which can include a list of authorized databases 
    /// or other sensitive information. 
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `allowedItems`: A hardcoded list of allowed database items, restricting access based on the 
    /// user role. In production, consider dynamically retrieving this list from a configuration file 
    /// or database.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage Notes:</strong>
    /// Both `Filter` methods are optional and can be omitted if unrestricted access to all databases 
    /// and database items is acceptable. You may adapt these methods to implement more complex filtering rules, 
    /// such as time-based access restrictions or department-based filtering.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register the ObjectFilterProvider in Program.cs
    /// builder.Services.AddObjectFilter<ObjectFilterProvider>();
    /// </code>
    /// </example>
    /// <seealso cref="https://help.revealbi.io/web/user-context/#using-the-user-context-in-the-objectfilterprovider"/>
    /// <seealso cref="https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows"/>
    public class ObjectFilterProvider : IRVObjectFilter
    {
        private readonly AuthorizationSettings _authSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFilterProvider"/> class.
        /// </summary>
        /// <param name="options">The options containing authorization settings.</param>
        public ObjectFilterProvider(IOptions<AuthorizationSettings> options)
        {
            _authSettings = options.Value;
        }

        /// <summary>
        /// Checks if a specific data source is allowed for the user based on a predefined list of allowed databases.
        /// </summary>
        /// <param name="userContext">The user context for user-specific filtering.</param>
        /// <param name="dataSource">The data source to check.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean result indicating whether the data source is allowed.</returns>
        public Task<bool> Filter(IRVUserContext userContext, RVDashboardDataSource dataSource)
        {
            // Optional: You could apply broader filters here if needed
            return Task.FromResult(true);
        }

        /// <summary>
        /// Filters individual database objects, such as tables and stored procedures, based on user roles.
        /// </summary>
        /// <param name="userContext">The user context for user-specific filtering.</param>
        /// <param name="dataSourceItem">The data source item to check.</param>
        /// <returns>A task representing the asynchronous operation, with a boolean result indicating whether the data source item is allowed.</returns>
        public Task<bool> Filter(IRVUserContext userContext, RVDataSourceItem dataSourceItem)
        {
            if (userContext?.Properties != null && dataSourceItem is RVSqlServerDataSourceItem dataSQLItem)
            {
                if (userContext.Properties.TryGetValue("UserId", out var userIdObj))
                {
                    var userId = userIdObj?.ToString();

                    // Determine role from AdminUserIds
                    var isAdmin = !string.IsNullOrWhiteSpace(userId) && _authSettings.AdminUserIds.Contains(userId);

                    // Set allowed items based on role
                    var allowedItems = isAdmin ? new HashSet<string>(_authSettings.AllowedTablesAdmin) : new HashSet<string>(_authSettings.AllowedTablesUser);

                    if ((dataSQLItem.Table != null && !allowedItems.Contains(dataSQLItem.Table)) ||
                        (dataSQLItem.Procedure != null && !allowedItems.Contains(dataSQLItem.Procedure)))
                    {
                        return Task.FromResult(false);
                    }
                }
            }

            return Task.FromResult(true);
        }
    }
}

