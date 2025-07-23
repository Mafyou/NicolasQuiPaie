var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.NicolasQuiPaieAPI>("api")
    .WithHttpHealthCheck("/health");

var web = builder.AddProject<Projects.NicolasQuiPaieWeb>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(api);

builder.Build().Run();
