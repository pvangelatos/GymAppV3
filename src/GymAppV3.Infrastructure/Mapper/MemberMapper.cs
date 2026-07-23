using GymAppV3.Core.DTOs;
using GymAppV3.Core.Models;

namespace GymAppV3.Infrastructure.Mapper;

/// <summary>
/// Mapper for Member entity to DTOs with authorization-aware logic.
/// </summary>
public static class MemberMapper
{
    /// <summary>
    /// Maps Member entity to basic MemberDto (without medical notes).
    /// </summary>
    public static MemberDto ToDto(this Member member)
    {
        return new MemberDto(
            Id: member.Id,
            UserId: member.UserId,
            Firstname: member.Firstname,
            Lastname: member.Lastname,
            Email: member.Email,
            Phone: member.Phone,
            Address: member.Address.ToDto(),
            BirthDate: member.BirthDate,
            HasMedicalConditions: member.HasMedicalConditions);
    }

    /// <summary>
    /// Maps Member entity to detailed MemberDetailDto (including medical notes).
    /// Use this only when the requesting user is authorized to view medical data.
    /// </summary>
    public static MemberDetailDto ToDetailDto(this Member member)
    {
        return new MemberDetailDto(
            Id: member.Id,
            UserId: member.UserId,
            Firstname: member.Firstname,
            Lastname: member.Lastname,
            Email: member.Email,
            Phone: member.Phone,
            Address: member.Address.ToDto(),
            BirthDate: member.BirthDate,
            HasMedicalConditions: member.HasMedicalConditions,
            MedicalNotes: member.MedicalNotes);
    }

    /// <summary>
    /// Maps AddressDto to Address entity.
    /// </summary>
    public static Address ToEntity(this AddressDto dto)
    {
        return new Address
        {
            Street = dto.Street,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode,
            Country = dto.Country
        };
    }

    /// <summary>
    /// Maps Address entity to AddressDto.
    /// </summary>
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto(
            Street: address.Street,
            City: address.City,
            State: address.State,
            ZipCode: address.ZipCode,
            Country: address.Country);
    }
}
