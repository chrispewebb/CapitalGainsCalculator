using System;
using System.IO;

namespace CapitalGainsCalculator
{
    public class CommsecCSVAssetRegistryParser : IAssetRegistryParser
    {
        public bool TryParseFromFile(string path, out AssetRegistry assetRegistry)
        {
            AssetRegistry newRegistry = new AssetRegistry();

            string[] lines = File.ReadAllLines(path);

            newRegistry.AssetEvents.Clear();

            foreach (string line in lines)
            {
                string[] lineSplit = line.Split(',');

                // Skip the header, and invalid lines
                if (lineSplit.Length != 11)
                {
                    continue;
                }

                // Commsec CSV is in the format
                // Code,Company,Date,Type,Quantity,Unit Price ($),Trade Value ($),Brokerage+GST ($),GST ($),Contract Note,Total Value ($)

                AssetEvent assetEvent = new AssetEvent();
                assetEvent.code = lineSplit[0].Replace("\"", string.Empty);
                assetEvent.date = DateTime.Parse(lineSplit[2]);

                string buyString = lineSplit[3];
                assetEvent.assetEventType = buyString.ToLower().Contains("buy") ? AssetEventType.Buy : AssetEventType.Sell;

                // Abs here because for sell events, commsec lists these as negative quantity. To maintain consistency with other platforms we will remove this
                assetEvent.quantity = (ulong)Math.Abs(long.Parse(lineSplit[4].Replace("\"", string.Empty)));

                assetEvent.pricePerUnitDollars = decimal.Parse(lineSplit[5].Replace("\"", string.Empty));

                assetEvent.value = decimal.Parse(lineSplit[6].Replace("\"", string.Empty));
                assetEvent.brokerageFee = decimal.Parse(lineSplit[7].Replace("\"", string.Empty));
                assetEvent.totalValue = assetEvent.value + assetEvent.brokerageFee;

                assetEvent.pricePerUnitDollarsPlusBrokerage = assetEvent.totalValue / assetEvent.quantity;

                newRegistry.AssetEvents.Add(assetEvent);
            }

            // Sort events by date. Commsec sorts alphabetically by code
            newRegistry.AssetEvents.Sort((event0, event1) => event0.date.CompareTo(event1.date));

            assetRegistry = newRegistry;
            return true;
        }
    }
}