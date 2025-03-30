namespace DashboardViewer.Models;

/// <summary>
/// Represents the names and related information of a dashboard.
/// </summary>
/// <remarks>
/// This class is used in the Blazor component to manage and display dashboard information.
/// </remarks>
public class DashboardNames
{
    /// <summary>
    /// Gets or sets the filename of the dashboard.
    /// </summary>
    /// <example>
    /// Example usage in a Blazor component:
    /// <code>
    /// private static List&lt;DashboardNames&gt; _revealServerDashboardNames = new();
    /// 
    /// @foreach (var item in _revealServerDashboardNames)
    /// {
    ///     &lt;IgbListItem class="igb-list" style="@SetListItemCss(item.DashboardTitle)" @onclick="() => ListItemClick(item)"&gt;
    ///         &lt;IgbCardMedia style="height: 50px; width: 100px;" slot="start"&gt;
    ///             &lt;DashboardThumbnail Info="@item.ThumbnailInfo"&gt;&lt;/DashboardThumbnail&gt;
    ///         &lt;/IgbCardMedia&gt;
    ///         &lt;span slot="end" class="material-icons icon" @onclick:stopPropagation="true" @onclick="@(e => ConfirmDelete(item.DashboardFilename))"&gt;
    ///             delete
    ///         &lt;/span&gt;
    ///         &lt;div slot="title"&gt;@item.DashboardTitle&lt;/div&gt;
    ///     &lt;/IgbListItem&gt;
    /// }
    /// </code>
    /// </example>
    public string DashboardFilename { get; set; }

    /// <summary>
    /// Gets or sets the title of the dashboard.
    /// </summary>
    public string DashboardTitle { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail information of the dashboard.
    /// </summary>
    public IDictionary<string, object> ThumbnailInfo { get; set; }
}
