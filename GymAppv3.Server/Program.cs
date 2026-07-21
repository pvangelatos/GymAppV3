using GymAppv3.Server.Configuration;
using GymAppv3.Server.Endpoints.ClassCategory;
using GymAppv3.Server.Endpoints.ClassRoom;
using GymAppv3.Server.Endpoints.GymBuilding;
using GymAppv3.Server.Endpoints.MembershipPackage;

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
app.MapClassCategoryEndpoints();
app.MapClassRoomEndpoints();
app.MapMembershipPackageEndpoints();

app.Run();
