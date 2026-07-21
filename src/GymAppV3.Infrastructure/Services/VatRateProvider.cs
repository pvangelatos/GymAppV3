using GymAppV3.Core.Abstractions;
using GymAppV3.Core.Enums;

namespace GymAppV3.Infrastructure.Services;

public class VatRateProvider : IVatRateProvider
{
    public decimal GetVatRate(VatCategory category) => category switch
    {
        VatCategory.Services => 0.13m,  // reduced rate for gym services
        VatCategory.Goods => 0.24m,     // standard rate
        _ => 0.24m                      // safe default: standard rate
    };
}
