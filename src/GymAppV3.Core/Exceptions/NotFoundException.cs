namespace GymAppV3.Application.Exceptions;

// Thrown when an operation targets an entity that does not exist (or is soft-deleted
// and therefore invisible). The API layer maps this to a 404 response.
public class NotFoundException : Exception
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with id '{key}' was not found.")
    {
    }
}
