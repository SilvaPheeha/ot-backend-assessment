using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OT.Assessment.Consumer.CreateWagers;
using OT.Assessment.Data;


var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory,
});

builder.Services.Configure<CreateWagersMqSettings>(builder.Configuration.GetSection("CreateWagersMq"));
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString")));
builder.Services.AddSingleton<CreateWagersRepository>();
builder.Services.AddSingleton<CreateWagersConsumer>();


using var host = builder.Build();

await host.StartAsync();

var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
var consumer = host.Services.GetRequiredService<CreateWagersConsumer>();

await consumer.StartAsync(lifetime.ApplicationStopping);

await host.WaitForShutdownAsync();
