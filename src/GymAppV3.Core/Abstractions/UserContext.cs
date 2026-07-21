
namespace GymAppV3.Core.Abstractions
{
    // Temporary implementation — returns null until authentication is wired up.
    // Once JWT/Identity is in place, this will read the user id from the request.
    public class UserContext : IUserContext
    {
        public string? UserId => null;
    }
}
