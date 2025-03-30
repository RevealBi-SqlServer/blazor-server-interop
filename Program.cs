using DashboardViewer.RevealServer;
using IgniteUI.Blazor.Controls;
using Reveal.Sdk;
using Reveal.Sdk.Data;
using RevealSdk.Server.Reveal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using DashboardViewer.Models;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
    builder.Services.Configure<SqlServerSettings>(
            builder.Configuration.GetSection("SqlServerSettings"));
    builder.Services.Configure<AuthorizationSettings>(
            builder.Configuration.GetSection("AuthorizationSettings"));
}

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers().AddReveal(builder =>
{
    builder
        //.AddSettings(settings =>
        //{
        //    settings.License = "Your Reveal License Key";
        //})
        .AddAuthenticationProvider<AuthenticationProvider>()
        .AddDataSourceProvider<DataSourceProvider>()
        .AddUserContextProvider<UserContextProvider>()
        .AddObjectFilter<ObjectFilterProvider>()
        .DataSources.RegisterMicrosoftSqlServer();
});

builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IRevealServerService, RevealServerService>();
RegisterIgniteUI(builder.Services);

void RegisterIgniteUI(IServiceCollection services)
{
    services.AddIgniteUIBlazor(
        typeof(IgbListModule),
        typeof(IgbAvatarModule)
    );
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.MapGet("dashboards/{name}/isduplicate", (string name, IRevealServerService revealServerService) =>
{
    return revealServerService.IsDuplicateDashboard(name);
});

app.MapGet("dashboards", () =>
{
    var filePath = Path.Combine(Environment.CurrentDirectory, "Dashboards");
    var files = Directory.GetFiles(filePath);
    return files.Select(x => Path.GetFileNameWithoutExtension(x));
});

app.MapGet("dashboards/{name}/thumbnail", async (string name) =>
{
    var path = Path.Combine(Environment.CurrentDirectory, "Dashboards", name + ".rdash");
    if (File.Exists(path))
    {
        var dashboard = new Dashboard(path);
        var info = await dashboard.GetInfoAsync(Path.GetFileNameWithoutExtension(path));
        return TypedResults.Ok(info);
    }
    else
    {
        return Results.NotFound();
    }
});

// Required for Reveal
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
      name: "default",
      pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
