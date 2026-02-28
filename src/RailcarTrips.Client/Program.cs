using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using global::RailcarTrips.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Update this value to match the API URL/port when running RailcarTrips.Api.
const string apiBaseAddress = "https://localhost:5001/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

await builder.Build().RunAsync();
