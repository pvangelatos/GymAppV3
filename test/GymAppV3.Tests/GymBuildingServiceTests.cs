using FluentAssertions;
using GymAppV3.Core.Exceptions;
using GymAppV3.Core.Models;
using GymAppV3.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Tests;

public class GymBuildingServiceTests : TestBase
{
    private GymBuildingService CreateSut() => new(Context);

    private async Task<GymBuilding> SeedBuilding()
    {
        var building = new GymBuilding
        {
            Name = "Main Gym",
            Address = new Address
            {
                Street = "Main 1",
                City = "Athens",
                State = "Attica",
                ZipCode = "10434",
                Country = "Greece"
            }
        };
        Context.GymBuildings.Add(building);
        await Context.SaveChangesAsync();
        return building;
    }

    [Fact]
    public async Task DeleteAsync_BuildingWithoutRooms_SoftDeletesAndKeepsAddress()
    {
        var building = await SeedBuilding();

        await CreateSut().DeleteAsync(building.Id);

        Context.ChangeTracker.Clear();   // read back from the store, not the tracked instance
        var deleted = await Context.GymBuildings.IgnoreQueryFilters()
            .SingleAsync(b => b.Id == building.Id);

        deleted.IsDeleted.Should().BeTrue();
        deleted.Address.City.Should().Be("Athens");
    }

    [Fact]
    public async Task DeleteAsync_BuildingWithRooms_ThrowsBusinessRule()
    {
        var building = await SeedBuilding();
        Context.ClassRooms.Add(new ClassRoom { ClassRoomName = "Room A", Capacity = 10, GymBuildingId = building.Id });
        await Context.SaveChangesAsync();

        var act = () => CreateSut().DeleteAsync(building.Id);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}