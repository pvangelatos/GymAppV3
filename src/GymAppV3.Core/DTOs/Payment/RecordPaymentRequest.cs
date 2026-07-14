using GymAppV3.Core.Enums;
using GymAppV3.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.DTOs.Payment
{
    public record RecordPaymentRequest(
        // Input for recording a payment. Amount is the gross figure (VAT included).
        // VatRate is captured at payment time (e.g. 0.24) so the record is self-contained.
        Guid MemberId,
        Guid? MembershipId,         // optional - a payment may not tie to a specific membership
        decimal Amount,             // gross (VAT included)
        decimal VatRate,            // e.g 0.13 or 0.24
        PaymentMethod Method);

}
