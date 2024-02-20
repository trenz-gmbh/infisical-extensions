using Example;
using TRENZ.Extensions.Infisical;

var builder = Host.CreateApplicationBuilder(args);

builder.AddInfisicalConfiguration();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();
