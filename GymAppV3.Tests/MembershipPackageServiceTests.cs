using FluentAssertions;
using GymAppV3.Application.Exceptions;
using GymAppV3.Core.DTOs.MembershipPackage;
using GymAppV3.Infrastructure.Services;
using Xunit;

namespace GymAppV3.Tests;

public class MembershipPackageServiceTests : TestBase
{
    private MembershipPackageService CreateSut() => new(Context);

    [Fact]
    public async Task CreateAsync_persists_and_returns_the_package()
    {
        var sut = CreateSut();
        var request = new CreateMembershipPackageRequest("Basic", 49.99m, 30, 8);

        var result = await sut.CreateAsync(request);

        // Returned DTO reflects the input, with a generated id.
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Basic");
        result.Price.Should().Be(49.99m);

        // And it is actually in the database.
        var all = await sut.GetAllAsync();
        all.Should().ContainSingle(p => p.Id == result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_returns_null_when_not_found()
    {
        var sut = CreateSut();

        var result = await sut.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_returns_the_package_when_it_exists()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(new CreateMembershipPackageRequest("Premium", 89m, 30, 12));

        var result = await sut.GetByIdAsync(created.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Premium");
    }

    [Fact]
    public async Task UpdateAsync_changes_the_stored_values()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(new CreateMembershipPackageRequest("Basic", 49m, 30, 8));

        await sut.UpdateAsync(created.Id, new UpdateMembershipPackageRequest("Basic Plus", 59m, 45, 10));

        var updated = await sut.GetByIdAsync(created.Id);
        updated!.Name.Should().Be("Basic Plus");
        updated.Price.Should().Be(59m);
        updated.DurationInDays.Should().Be(45);
    }

    [Fact]
    public async Task UpdateAsync_throws_NotFound_for_missing_package()
    {
        var sut = CreateSut();

        var act = () => sut.UpdateAsync(
            Guid.NewGuid(),
            new UpdateMembershipPackageRequest("X", 1m, 1, 1));

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_soft_deletes_so_the_row_disappears_from_queries()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(new CreateMembershipPackageRequest("Basic", 49m, 30, 8));

        await sut.DeleteAsync(created.Id);

        // The global query filter hides the soft-deleted row.
        var result = await sut.GetByIdAsync(created.Id);
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_throws_NotFound_for_missing_package()
    {
        var sut = CreateSut();

        var act = () => sut.DeleteAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
