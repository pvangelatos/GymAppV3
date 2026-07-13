using System;
using System.Collections.Generic;
using System.Text;

namespace GymAppV3.Core.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message) 
        {
        }
    }
}
