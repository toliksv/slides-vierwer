using RestEase;
using SlidesViewer.Host.Contracts;
using SlidesViewer.Host.Contracts.Api;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Register HttpClient factory
builder.Services.AddHttpClient();

// Configure global JSON serialization options
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

// RestEase client for SlidesViewer.Host API
builder.Services.AddScoped<ISlidesApi>(sp =>
{
    var baseUrl = builder.Configuration["SlidesViewerApi:BaseUrl"] ?? "http://localhost:5041/";
    var httpClient = new HttpClient { BaseAddress = new Uri(baseUrl) };
    return RestClient.For<ISlidesApi>(httpClient);
});

var app = builder.Build();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
