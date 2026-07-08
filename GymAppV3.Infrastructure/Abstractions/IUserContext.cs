using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Infrastructure.Abstractions
{
    public interface IUserContext
    {
        string? UserId { get; }
    }
}
