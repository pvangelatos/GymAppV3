using FluentAssertions;
using GymAppV3.Core.Commands;
using GymAppV3.Core.Exceptions;
using GymAppV3.Infrastructure.Services;

namespace GymAppV3.Tests;

public class ClassCategoryServiceTests : TestBase
{
    private ClassCategoryService CreateSut() => new(Context);

    [Fact]
    public async Task CreateAsync_DuplicateName_ThrowsBusinessRule()
    {
        var sut = CreateSut();
        await sut.CreateAsync(new CreateClassCategoryCommand("Pilates"));

        var act = () => sut.CreateAsync(new CreateClassCategoryCommand("Pilates"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task UpdateAsync_SameNameOnSameCategory_Succeeds()
    {
        var sut = CreateSut();
        var created = await sut.CreateAsync(new CreateClassCategoryCommand("Pilates"));

        var act = () => sut.UpdateAsync(created.Id, new UpdateClassCategoryCommand("Pilates"));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task UpdateAsync_NameOfAnotherCategory_ThrowsBusinessRule()
    {
        var sut = CreateSut();
        await sut.CreateAsync(new CreateClassCategoryCommand("Pilates"));
        var yoga = await sut.CreateAsync(new CreateClassCategoryCommand("Yoga"));

        var act = () => sut.UpdateAsync(yoga.Id, new UpdateClassCategoryCommand("Pilates"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
