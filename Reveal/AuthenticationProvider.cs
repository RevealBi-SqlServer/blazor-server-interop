using DashboardViewer.Models;
using Microsoft.Extensions.Options;
using Reveal.Sdk;
using Reveal.Sdk.Data;
using Reveal.Sdk.Data.Microsoft.SqlServer;

namespace RevealSdk.Server.Reveal
{
    /// <summary>
    /// The `AuthenticationProvider` class implements the `IRVAuthenticationProvider` interface 
    /// to provide custom authentication for Reveal BI data sources. The purpose of this provider 
    /// is to supply credentials that Reveal BI uses to authenticate to databases, enabling secure 
    /// access to data sources in Reveal BI dashboards.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>Purpose:</strong>
    /// The `AuthenticationProvider` retrieves credentials for a specified data source, using configuration 
    /// settings, secret management, or other means to supply credentials securely. This is essential for 
    /// connecting to external data sources (like SQL Server) and is invoked by setting the authentication 
    /// provider in the application's dependency injection (DI) container.
    /// </para>
    /// <para>
    /// <strong>Authentication Setup:</strong>
    /// This provider must be registered in the DI container in `Program.cs` with the following call:
    /// `.AddAuthenticationProvider<AuthenticationProvider>()`, allowing Reveal BI to automatically use 
    /// this authentication provider when a data source requires credentials.
    /// </para>
    /// <para>
    /// <strong>Key Components and Methods:</strong>
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// `_sqlSettings`: An instance of `SqlServerSettings`, injected via constructor dependency injection. This 
    /// provides access to app settings, secrets, and configuration values for retrieving database credentials 
    /// (such as from a secure vault or environment variables).
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// `ResolveCredentialsAsync(IRVUserContext userContext, RVDashboardDataSource dataSource)`:
    /// This method asynchronously resolves credentials for a specific data source. It accepts a `userContext` 
    /// parameter (for user-specific credentials) and `dataSource`, which represents the requested data source.
    /// It returns an `IRVDataSourceCredential`, used to authenticate Reveal BI to that data source.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <strong>Usage Notes:</strong>
    /// The method checks if the data source is of type `RVSqlServerDataSource` and provides SQL Server 
    /// credentials in the form of `RVUsernamePasswordDataSourceCredential`, using username and password 
    /// values retrieved from `_sqlSettings`. The credentials in this demo are currently stored in UserSecrets, 
    /// in production ensure they are securely stored and retrieved (e.g., from app secrets or a key vault).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register the AuthenticationProvider in Program.cs
    /// builder.Services.AddAuthenticationProvider<AuthenticationProvider>();
    /// </code>
    /// </example>
    /// <seealso cref="https://help.revealbi.io/web/authentication/"/>
    /// <seealso cref="https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-8.0&tabs=windows"/>
    public class AuthenticationProvider : IRVAuthenticationProvider
    {
        private readonly SqlServerSettings _sqlSettings;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationProvider"/> class.
        /// </summary>
        /// <param name="options">The options containing SQL Server settings.</param>
        public AuthenticationProvider(IOptions<SqlServerSettings> options)
        {
            _sqlSettings = options.Value;
        }

        /// <summary>
        /// Asynchronously resolves credentials for a specific data source.
        /// </summary>
        /// <param name="userContext">The user context for user-specific credentials.</param>
        /// <param name="dataSource">The data source for which credentials are being resolved.</param>
        /// <returns>An <see cref="IRVDataSourceCredential"/> used to authenticate Reveal BI to the data source.</returns>
        public Task<IRVDataSourceCredential> ResolveCredentialsAsync(IRVUserContext userContext,
            RVDashboardDataSource dataSource)
        {
            IRVDataSourceCredential userCredential = new RVIntegratedAuthenticationCredential();

            if (dataSource is RVSqlServerDataSource)
            {
                userCredential = new RVUsernamePasswordDataSourceCredential(
                    _sqlSettings.DatabaseUser,
                    _sqlSettings.DatabasePassword);
            }
            return Task.FromResult(userCredential);
        }
    }
}

