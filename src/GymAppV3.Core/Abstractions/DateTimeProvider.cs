using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Abstractions
{
    // Real clock. In tests this is swapped for a fake that returns a fixed time.
    public class DateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
