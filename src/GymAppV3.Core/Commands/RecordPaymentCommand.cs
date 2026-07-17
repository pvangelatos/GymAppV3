using GymAppV3.Core.Enums;
using GymAppV3.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Commands
{
    public record RecordPaymentCommand(
        // Input for recording a payment. Amount is the gross figure (VAT included).
        // VatRate is captured at payment time (e.g. 0.24) so the record is self-contained.
        Guid MemberId,
        Guid? MembershipId,         // optional - a payment may not tie to a specific membership
        decimal Amount,             // gross (VAT included)
        PaymentMethod Method);

}
