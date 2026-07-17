using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Enums
{
    // Vat category - not rate. Rate per category comes from IVatRateProvider and can 
    // be change without changing data
    public enum VatCategory
    {
        Services = 0,       // 13%
        Goods = 1           // 24%
    }
}
