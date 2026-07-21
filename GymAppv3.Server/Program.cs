using GymAppv3.Server.Endpoints;
using GymAppv3.Server.Configuration;  

var builder = WebApplication.CreateBuilder(args);

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.ConfigureApplication();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapGymBuildingEndpoints();

app.Run();
