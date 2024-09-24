var builder = DistributedApplication.CreateBuilder(args);

var rabbitMQ = builder.AddRabbitMQ("messaging");
var redis = builder.AddRedis("redis");

// Corrected SQL Server container configuration
var sqlServer = builder
    .AddSqlServer("dotnetoauth")
    .WithImage("mssql/server", "2022-latest")
    .WithEnvironment("MSSQL_SA_PASSWORD", "guest")
    .WithDataVolume();

// Auth
var authDb =  sqlServer
    .AddDatabase("authdb");

var apiService = builder
    .AddProject<Projects.Dotnet0Auth_ApiService>("apiservice")
    .WithReference(rabbitMQ)
    .WithReference(redis)
    .WithReference(authDb);

builder.AddProject<Projects.Dotnet0Auth_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(rabbitMQ)
    .WithReference(redis)
    .WithReference(apiService);

builder.Build().Run();
