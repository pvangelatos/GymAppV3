using GymAppV3.Core.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Abstractions
{
    public interface IVatRateProvider
    {
        decimal GetVatRate(VatCategory category);
    }
}
