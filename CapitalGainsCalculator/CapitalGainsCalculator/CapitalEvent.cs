using System;
using System.Collections.Generic;

namespace CapitalGainsCalculator
{
    public class CapitalEventContribution
    {
        public ulong quantity;
        public decimal pricePerUnitDollars;
        public decimal cost;
        public decimal brokerageFee;
        public DateTime date;
    }

    public class CapitalEvent
    {
        public string code;
        public ulong quantity;
        public decimal pricePerUnitDollars;
        public decimal totalValue;
        public decimal totalCost;
        public decimal totalGain;
        public DateTime date;

        public ulong unitsUnaccountedFor;

        public List<CapitalEventContribution> contributions = new List<CapitalEventContribution>();
    }
}