var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMafyouHealth();

var app = builder.Build();

app.UseFrontFiles()
    .MapMafyouHealth()
    .MapFront();

app.Run();

public partial class Program;
