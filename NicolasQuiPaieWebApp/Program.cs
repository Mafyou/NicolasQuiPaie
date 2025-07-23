using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Blazored.LocalStorage;
using NicolasQuiPaieWebApp;
using NicolasQuiPaieWebApp.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuration de l'API de base
var apiBaseAddress = builder.Configuration["ApiBaseAddress"] ?? "https://localhost:7001";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseAddress) });

// Configuration de l'authentification
builder.Services.AddMsalAuthentication(options =>
{
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add("api://nicolas-qui-paie-api/access");
});

// Configuration du stockage local
builder.Services.AddBlazoredLocalStorage();

// Configuration de SignalR
builder.Services.AddScoped<ISignalRService, SignalRService>();

// Services personnalisés pour l'API
builder.Services.AddScoped<IProposalApiService, ProposalApiService>();
builder.Services.AddScoped<IVotingApiService, VotingApiService>();
builder.Services.AddScoped<ICommentApiService, CommentApiService>();
builder.Services.AddScoped<IAnalyticsApiService, AnalyticsApiService>();
builder.Services.AddScoped<IUserApiService, UserApiService>();

// Configuration des HttpClients avec authentification
builder.Services.AddHttpClient("NicolasQuiPaieAPI", client =>
{
    client.BaseAddress = new Uri(apiBaseAddress);
})
.AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

await builder.Build().RunAsync();
