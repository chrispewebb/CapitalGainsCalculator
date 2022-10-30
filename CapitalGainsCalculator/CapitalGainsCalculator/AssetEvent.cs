using System;

namespace CapitalGainsCalculator
{
    public class AssetEvent
    {
        public string code;
        public DateTime date;
        public AssetEventType assetEventType;
        public ulong quantity;
        public decimal pricePerUnitDollars;
    }
}