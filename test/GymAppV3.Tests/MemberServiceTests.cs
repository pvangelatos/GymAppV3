using GymAppV3.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace GymAppV3.Tests;

/// <summary>
/// Basic database integration tests for Member entity.
/// Full service testing requires UserManager/authentication mocking.
/// </summary>
public class MemberServiceTests : TestBase
{
    [Fact]
    public async Task Member_CanBeCreatedInDatabase()
    {
        // Arrange
        var member = new Member
        {
            Firstname = "Test",
            Lastname = "Member",
            Email = "test@example.com",
            Address = new Address
            {
                Street = "123 Main St",
                City = "Athens",
                State = "Attica",
                ZipCode = "12345",
                Country = "Greece"
            },
            BirthDate = new DateOnly(2000, 1, 1),
            HasMedicalConditions = false
        };

        // Act
        Context.Members.Add(member);
        await Context.SaveChangesAsync();

        // Assert
        var memberInDb = await Context.Members.FirstOrDefaultAsync(m => m.Id == member.Id);
        Assert.NotNull(memberInDb);
        Assert.Equal("Test", memberInDb.Firstname);
        Assert.Equal("test@example.com", memberInDb.Email);
        Assert.NotNull(memberInDb.Address);
        Assert.Equal("Athens", memberInDb.Address.City);
    }

    [Fact]
    public async Task Member_SoftDelete_SetsIsDeletedFlag()
    {
        // Arrange
        var member = new Member
        {
            Firstname = "Delete",
            Lastname = "Test",
            Email = "delete@example.com",
            Address = new Address
            {
                Street = "456 Test St",
                City = "Thessaloniki",
                State = "Macedonia",
                ZipCode = "54321",
                Country = "Greece"
            },
            BirthDate = new DateOnly(1995, 5, 15),
            HasMedicalConditions = false
        };

        Context.Members.Add(member);
        await Context.SaveChangesAsync();

        // Act
        member.IsDeleted = true;
        await Context.SaveChangesAsync();

        // Assert
        var memberInDb = await Context.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == member.Id);
        Assert.NotNull(memberInDb);
        Assert.True(memberInDb.IsDeleted);
    }
}
