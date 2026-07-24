
using System.Security.Cryptography;


namespace GymAppV3.Infrastructure.Identity;

/// <summary>
/// Generates one-off passwords for staff-created accounts. The plaintext is shown
/// to the creator once and never stored.
/// </summary>
public static class TemporaryPasswordGenerator
{
    // Ambiguous characters (I/l/1, O/0) are left out so the password survives being
    // read aloud or copied by hand.
    private const string Uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Lowercase = "abcdefghjkmnopqrstuvwxyz";
    private const string Digits = "23456789";
    private const string Specials = "!@#$%";
    
    public static string Generate(int length = 10)
    {
        // One from each set up front quarantess the Identity password policy is met.
        var required = new[]
        {
            RandomFrom(Uppercase),
            RandomFrom(Lowercase),
            RandomFrom(Digits),
            RandomFrom(Specials)
        };

        var all = Uppercase + Lowercase + Digits + Specials;
        var rest = Enumerable.Range(0, Math.Max(0, length - required.Length))
            .Select(_ => RandomFrom(all));

        // Shuffle so the guaranteed characters aren't always in the same positions.
        return new string(required.Concat(rest)
            .OrderBy(_ => RandomNumberGenerator.GetInt32(int.MaxValue))
            .ToArray());
    }

    private static char RandomFrom(string set) => 
        set[RandomNumberGenerator.GetInt32(set.Length)];
}
