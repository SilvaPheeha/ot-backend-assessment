using Cortex.Mediator.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using OT.Assessment.App.Feature.Wager.CreateWagers;
using OT.Assessment.App.Feature.Wager.CreateWagers.Interface;
using OT.Assessment.App.Feature.Wager.GetLatestWagers;
using OT.Assessment.App.Feature.Wager.GetTopSpanders;
using OT.Assessment.Data;

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.AddOpenApi();
builder.Services.AddCortexMediator(
    handlerAssemblyMarkerTypes: [typeof(Program)],
    configure: options =>
    {
        options.AddDefaultBehaviors();
    }
);

builder.Services.AddDbContextFactory<AppDbContext>(options =>
     options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnectionString")));

builder.Services.AddSingleton<ICreateWagersPublisher, CreateWagersPublisher>();
builder.Services.AddMemoryCache();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "OT Assessment API",
        Description = "OT Assessment API",
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "OT Assessment API V1");
    c.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();
app.MapCreateWagersEndpoint();
app.MapGetLatestWagersEndpoint();
app.MapGetTopSpandersEndpoint();

app.Run();