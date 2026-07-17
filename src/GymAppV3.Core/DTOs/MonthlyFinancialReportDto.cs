using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs
{
    // Aggregated financials for a single month.Totals are split into gross, net, and
    // VAT so the figures can go straight into a VAT return.
    public record MonthlyFinancialReportDto(
        int Year,
        int Month,
        int PaymentCount,
        decimal TotalGross,        // sum of all gross amounts
        decimal TotalNet,          // sum of all net amounts
        decimal TotalVat);         // sum of all VAT amounts
}
