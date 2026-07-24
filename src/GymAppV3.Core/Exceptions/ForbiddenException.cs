

namespace GymAppV3.Core.Exceptions;

// Thrown when the caller is authenticated but lacks permission for the operation.
// The API layer maps this to a 403 response.
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
