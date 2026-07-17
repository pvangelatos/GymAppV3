using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Common
{
    public class VatCalculator
    {
        public static (decimal Net, decimal Vat) Split(decimal gross, decimal rate)
        {
            var net = Math.Round(gross / (1 + rate), 2, MidpointRounding.AwayFromZero);
            return (net, gross - net);
        }
    }
}
