using System;
using System.Collections.Generic;
using System.Text;

namespace Finalitika10.Models
{
    public class CurrencyItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string EngName { get; set; }
        public int Nominal { get; set; }
    }

    public class CurrencyRate
    {
        public string Id { get; set; }
        public string CharCode { get; set; }
        public string Name { get; set; }
        public int Nominal { get; set; }
        public double Value { get; set; }

        public double UnitRate { get; set; }

        public string DisplayRate => $"{UnitRate:N2} ₽";
    }
}
