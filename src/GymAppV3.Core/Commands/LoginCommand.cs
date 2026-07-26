namespace GymAppV3.Core.Commands;

public record LoginCommand(
    string Email,
    string Password);
