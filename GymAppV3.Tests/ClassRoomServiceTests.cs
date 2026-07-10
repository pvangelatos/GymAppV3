using FluentAssertions;
using GymAppV3.Core.DTOs;
using GymAppV3.Core.DTOs.ClassRoom;
using GymAppV3.Core.DTOs.GymBuilding;
using GymAppV3.Infrastructure.Services;
using GymAppV3.Application.Exceptions;

namespace GymAppV3.Tests;

public class ClassRoomServiceTests : TestBase
{
    private ClassRoomService CreateSut() => new(Context);
    private GymBuildingService CreateBuildingSut() => new(Context);

    private static CreateGymBuildingRequest SampleBuilding() =>
        new("Downtown", null,
            new AddressDto("Main St 1", "Athens", "Attica", "10434", "Greece"),
            null, null);

    [Fact]
    public async Task CreateAsync_succeeds_when_building_exists()
    {
        var building = await CreateBuildingSut().CreateAsync(SampleBuilding());
        var sut = CreateSut();

        var result = await sut.CreateAsync(new CreateClassRoomRequest("Room A", 8, building.Id));

        result.Id.Should().NotBeEmpty();
        result.GymBuildingId.Should().Be(building.Id);
    }

    [Fact]
    public async Task CreateAsync_throws_NotFound_when_building_does_not_exist()
    {
        var sut = CreateSut();

        var act = () => sut.CreateAsync(new CreateClassRoomRequest("Room A", 8, Guid.NewGuid()));

        // The referential check should reject a room pointing at a missing building.
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
