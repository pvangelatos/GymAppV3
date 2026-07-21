using GymAppV3.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Tests;

// A clock frozen at a fixed instant, so time-dependent rules are deterministic.
// This is exactly why IDateTimeProvider exists — no dependency on the real clock.
public class FixedClock : IDateTimeProvider
{
    private readonly DateTimeOffset _now;
    public FixedClock(DateTimeOffset now) => _now = now;
    public DateTimeOffset UtcNow => _now;
}
