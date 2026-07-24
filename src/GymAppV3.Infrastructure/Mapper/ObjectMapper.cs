using GymAppV3.Core.DTOs;
using Models = GymAppV3.Core.Models;
using System.Linq.Expressions;

namespace GymAppV3.Infrastructure;

/// <summary>
/// Central place for all Entity -> DTO projections.
/// Each entity has its own nested static class exposing:
///   - ToDto:         Expression<Func<Entity, Dto>> -> for projecting over IQueryable (one SQL SELECT)
///   - ToDtoCompiled: Func<Entity, Dto>              -> for in-memory mapping (e.g. right after an insert)
/// Usage: _context.ClassRooms.Select(ObjectMapper.ClassRoom.ToDto).ToListAsync();
/// </summary>
public static class ObjectMapper
{
    // Mapper for the ClassRoom entity to ClassRoomDto.
    public static class ClassRoom
    {
        public static readonly Expression<Func<Models.ClassRoom, ClassRoomDto>> ToDto =
            r => new ClassRoomDto(r.Id, r.ClassRoomName, r.Capacity, r.GymBuildingId);

        public static readonly Func<Models.ClassRoom, ClassRoomDto> ToDtoCompiled = ToDto.Compile();
    }

    // Mapper for the ClassCategory entity to ClassCategoryDto.
    public static class ClassCategory
    {
        // VERIFY the fields/order against your own ClassCategoryDto
        public static readonly Expression<Func<Models.ClassCategory, ClassCategoryDto>> ToDto =
            c => new ClassCategoryDto(c.Id, c.Name);

        public static readonly Func<Models.ClassCategory, ClassCategoryDto> ToDtoCompiled = ToDto.Compile();
    }

    // Mapper for the GymBuilding entity to GymBuildingDto, including Address (owned type).
    public static class GymBuilding
    {
        // Address is an owned type -> built inline, stays a single SELECT (no second query)
        public static readonly Expression<Func<Models.GymBuilding, GymBuildingDto>> ToDto =
            b => new GymBuildingDto(
                b.Id,
                b.Name,
                b.Description,
                new AddressDto(b.Address.Street, b.Address.City, b.Address.State, b.Address.ZipCode, b.Address.Country),
                b.Phone,
                b.Email);

        public static readonly Func<Models.GymBuilding, GymBuildingDto> ToDtoCompiled = ToDto.Compile();
    }

    // Mapper for the MembershipPackage entity to MembershipPackageDto, including navigations.
    public static class MembershipPackage
    {
        // VERIFY the fields/order against your own MembershipPackageDto
        public static readonly Expression<Func<Models.MembershipPackage, MembershipPackageDto>> ToDto =
            p => new MembershipPackageDto(
                p.Id,
                p.Name,
                p.Price,
                p.DurationInDays,
                p.SessionsIncluded,
                p.ClassCategoryId,
                p.ClassCategory.Name);

        public static readonly Func<Models.MembershipPackage, MembershipPackageDto> ToDtoCompiled = ToDto.Compile();
    }

    // Mapper for the Membership entity to MembershipDto, including navigations and enum conversion.
    public static class Membership
    {
        // Status: enum -> string inside the expression tree (EF Core translates it to SQL)
        public static readonly Expression<Func<Models.Membership, MembershipDto>> ToDto =
            m => new MembershipDto(
                m.Id,
                m.MemberId,
                m.MembershipPackageId,
                m.MembershipPackage.Name,
                m.PricePaid,
                m.StartDate,
                m.EndDate,
                m.RemainingSessions,
                m.Status.ToString());

        public static readonly Func<Models.Membership, MembershipDto> ToDtoCompiled = ToDto.Compile();
    }
    
    // Mapper for the Payment entity to PaymentDto, including computed fields.
    public static class Payment
    {
        public static readonly Expression<Func<Models.Payment, PaymentDto>> ToDto =
            p => new PaymentDto(
                p.Id,
                p.MemberId,
                p.MembershipId,
                p.Amount,
                p.NetAmount,
                p.Amount - p.NetAmount,   // VatAmount: computed inside the projection, NOT a column
                p.VatRate,
                p.Method.ToString(),      // enum -> string
                p.Status.ToString(),
                p.PaidAt);

        public static readonly Func<Models.Payment, PaymentDto> ToDtoCompiled = ToDto.Compile();
    }

    // Mapper for the Booking entity to BookingDto, including navigations.
    public static class Booking
    {
        // Status: enum -> string inside the expression tree
        public static readonly Expression<Func<Models.Booking, BookingDto>> ToDto =
            b => new BookingDto(
                b.Id,
                b.MemberId,
                b.ClassSessionId,
                b.ClassSession.Title,
                b.ClassSession.StartsAt,
                b.Status.ToString(),
                b.BookedAt,
                b.CancelledAt);

        public static readonly Func<Models.Booking, BookingDto> ToDtoCompiled = ToDto.Compile();
    }
    
    // Mapper for the ClassSession entity to ClassSessionDto, including navigations.
    public static class ClassSession
    {
        // The heavy one: 3 navigations (Trainer, ClassRoom, ClassCategory).
        // Stays a single SELECT with JOINs as long as it's called over IQueryable before ToListAsync().
        public static readonly Expression<Func<Models.ClassSession, ClassSessionDto>> ToDto =
            s => new ClassSessionDto(
                s.Id,
            s.Title,
            s.ClassCategoryId,
            s.ClassCategory.Name,
            s.StartsAt,
            s.DurationInMinutes,
            s.Capacity,
            s.AvailableSeats,
            s.TrainerId,
            s.Trainer.Firstname + " " + s.Trainer.Lastname,
            s.ClassRoomId,
            s.ClassRoom.ClassRoomName);

        public static readonly Func<Models.ClassSession, ClassSessionDto> ToDtoCompiled = ToDto.Compile();
    }

    // Mapper for the Member entity to MemberDto, including Address (owned type) and medical info.
    public static class Member
    {
        // Address is a required owned type - build the DTO inline, stays a single SELECT (no second query)
        // MedicalNotes doesn't get here - it's managed separately in the ToMedicalDto from the admin.
        public static readonly Expression<Func<Models.Member, MemberDto>> ToDto =
            m => new MemberDto(
                m.Id,
                m.UserId,
                m.Firstname,
                m.Lastname,
                m.Email,
                m.Phone,
                new AddressDto(m.Address.Street, m.Address.City, m.Address.State, m.Address.ZipCode, m.Address.Country),
                m.BirthDate,
                m.HasMedicalConditions);

        public static readonly Func<Models.Member, MemberDto> ToDtoCompiled = ToDto.Compile();

        // Admin-only projection for sensitive medical data.
        public static readonly Expression<Func<Models.Member, MemberMedicalDto>> ToMedicalDto =
            m => new MemberMedicalDto(m.Id, m.HasMedicalConditions, m.MedicalNotes);

        public static readonly Func<Models.Member, MemberMedicalDto> ToMedicalDtoCompiled = ToMedicalDto.Compile();
    }

    // Mapper for the Trainer entity to TrainerDto, including specialties.
    public static class Trainer
    {
        public static readonly Expression<Func<Models.Trainer, TrainerDto>> ToDto =
            t => new TrainerDto(
                t.Id,
                t.UserId,
                t.Firstname,
                t.Lastname,
                t.Email,
                t.Phone,
                t.Bio,
                t.Specialties
                    .Select(s => new SpecialtyDto(s.ClassCategoryId, s.ClassCategory.Name))
                    .ToList());

        public static readonly Func<Models.Trainer, TrainerDto> ToDtoCompiled = ToDto.Compile();
    }

}