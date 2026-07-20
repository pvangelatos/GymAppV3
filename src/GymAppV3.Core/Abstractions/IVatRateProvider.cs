using GymAppV3.Core.Enums;


namespace GymAppV3.Core.Abstractions
{
    public interface IVatRateProvider
    {
        decimal GetVatRate(VatCategory category);
    }
}
