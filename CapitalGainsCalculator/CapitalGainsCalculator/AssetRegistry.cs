using System;
using System.Collections.Generic;

namespace CapitalGainsCalculator
{
    public class AssetRegistry
    {
        private Dictionary<string, Queue<AssetParcel>> m_AssetParcelPool = new Dictionary<string, Queue<AssetParcel>>();

        public List<AssetEvent> AssetEvents { get; set; } = new List<AssetEvent>(1000);

        public List<CapitalEvent> CapitalEvents { get; set; } = new List<CapitalEvent>(1000);

        public void PrintAssetEvents()
        {
            string headerText = string.Format("{0,-3}| {1,-4}| {2,-4}| {3,-9}| {4,-6}| {5,-6}| {6,-9}| {7,-6}| {8,-9}| {9,-6}",
                                              "ID",
                                              "CODE",
                                              "TYPE",
                                              "DATE",
                                              "$/UNIT",
                                              "UNITS",
                                              "VALUE",
                                              "FEE",
                                              "TOTAL",
                                              "TOTAL $/U");

            Debug.Log(headerText);

            for (int i = 0; i < AssetEvents.Count; ++i)
            {
                AssetEvent assetEvent = AssetEvents[i];

                string printText = string.Format("{0,-3}| {1,-4}| {2,-4}| {3,-9}| {4,-6}| {5,-6}| {6,-9}| {7,-6}| {8,-9}| {9,-6}",
                                                 i,
                                                 assetEvent.code,
                                                 assetEvent.assetEventType.ToString(),
                                                 assetEvent.date.ToShortDateString(),
                                                 assetEvent.pricePerUnitDollars.ToString("0.#########"),
                                                 assetEvent.quantity,
                                                 assetEvent.value.ToString("0.#########"),
                                                 assetEvent.brokerageFee.ToString("0.#########"),
                                                 assetEvent.totalValue.ToString("0.#########"),
                                                 assetEvent.pricePerUnitDollarsPlusBrokerage.ToString("0.#########"));

                Debug.Log(printText, assetEvent.assetEventType == AssetEventType.Buy ? ConsoleColor.Green : ConsoleColor.Red);
            }
        }

        public void PrintParcels()
        {
            foreach (KeyValuePair<string, Queue<AssetParcel>> pair in m_AssetParcelPool)
            {
                Debug.Log("----" + pair.Key + "----");

                foreach (AssetParcel parcel in pair.Value)
                {
                    string quantity = parcel.quantity.ToString();
                    string price = parcel.pricePerUnitDollars.ToString("0.#########");
                    string printText = string.Format("{0,-5}| {1,-6}", quantity, price);
                    Debug.Log(printText);
                }

                Debug.Log("-----------");
                Debug.Log("");
            }
        }

        public void PrintCapitalEvents()
        {
            foreach (CapitalEvent capitalEvent in CapitalEvents)
            {
                Debug.Log("----" + capitalEvent.code + "----");

                Debug.Log("Date: " + capitalEvent.date);
                Debug.Log("Quantity: " + capitalEvent.quantity);
                Debug.Log("Price Per Unit: " + capitalEvent.pricePerUnitDollars);
                Debug.Log("Cost: " + capitalEvent.totalCost.ToString("0.#########"));
                Debug.Log("Value: " + capitalEvent.totalValue.ToString("0.#########"));
                Debug.Log("Gain: " + capitalEvent.totalGain.ToString("0.#########"), capitalEvent.totalGain >= 0 ? ConsoleColor.Green : ConsoleColor.Red);
                Debug.Log("Units Unaccounted For: " + capitalEvent.unitsUnaccountedFor, capitalEvent.unitsUnaccountedFor == 0 ? ConsoleColor.White : ConsoleColor.Red);

                if (capitalEvent.contributions.Count > 0)
                {
                    Debug.Log("");
                    Debug.Log("Contributions");

                    foreach (CapitalEventContribution contribution in capitalEvent.contributions)
                    {
                        Debug.Log("");
                        Debug.Log("    Date: " + contribution.date);
                        Debug.Log("    Quantity: " + contribution.quantity);
                        Debug.Log("    Price Per Unit: " + contribution.pricePerUnitDollars.ToString("0.#########"));
                        Debug.Log("    Cost: " + contribution.cost.ToString("0.#########"));
                    }
                }

                Debug.Log("");
            }
        }

        public void CalculateCapitalGainEvents()
        {
            // Process events one by one.
            // Sell events add to an asset pool
            // Buy events remove from the pool in FIFO fashion and record the buy price vs sale price

            m_AssetParcelPool.Clear();
            CapitalEvents.Clear();

            foreach (AssetEvent assetEvent in AssetEvents)
            {
                // Add the parcel to the pool on buys
                if (assetEvent.assetEventType == AssetEventType.Buy)
                {
                    AddAssetParcel(assetEvent);
                }
                // Generate capital gain events on sells
                else
                {
                    ProcessCapitalGainEvent(assetEvent.code, assetEvent.quantity, assetEvent.pricePerUnitDollars, assetEvent.date);
                }
            }
        }

        private void AddAssetParcel(AssetEvent assetEvent)
        {
            if (!m_AssetParcelPool.TryGetValue(assetEvent.code, out Queue<AssetParcel> parcelQueue))
            {
                parcelQueue = new Queue<AssetParcel>();
                m_AssetParcelPool.Add(assetEvent.code, parcelQueue);
            }

            AssetParcel parcel = new AssetParcel
            {
                quantity = assetEvent.quantity, pricePerUnitDollars = assetEvent.pricePerUnitDollars, pricePerUnitDollarsPlusBrokerage = assetEvent.pricePerUnitDollarsPlusBrokerage, date = assetEvent.date
            };

            parcelQueue.Enqueue(parcel);
        }

        private void ProcessCapitalGainEvent(string code, ulong quantity, decimal pricePerUnitDollars, DateTime date)
        {
            CapitalEvent capitalEvent = new CapitalEvent();
            capitalEvent.quantity = quantity;
            capitalEvent.pricePerUnitDollars = pricePerUnitDollars;
            capitalEvent.totalValue = quantity * pricePerUnitDollars;
            capitalEvent.code = code;
            capitalEvent.date = date;

            if (!m_AssetParcelPool.TryGetValue(code, out Queue<AssetParcel> parcelQueue))
            {
                capitalEvent.unitsUnaccountedFor = quantity;
                quantity = 0;
            }
            else
            {
                while (quantity > 0)
                {
                    if (parcelQueue.Count > 0)
                    {
                        AssetParcel parcel = parcelQueue.Peek();

                        CapitalEventContribution contribution = new CapitalEventContribution();
                        contribution.date = parcel.date;
                        contribution.pricePerUnitDollars = parcel.pricePerUnitDollarsPlusBrokerage;

                        if (parcel.quantity > quantity)
                        {
                            contribution.quantity = quantity;
                            parcel.quantity -= quantity;
                            quantity = 0;
                        }
                        else
                        {
                            contribution.quantity = parcel.quantity;
                            quantity -= parcel.quantity;
                            parcelQueue.Dequeue();
                        }

                        contribution.cost = contribution.quantity * contribution.pricePerUnitDollars;

                        capitalEvent.totalCost += contribution.cost;
                        capitalEvent.contributions.Add(contribution);
                    }
                    else
                    {
                        // We dont have a contribution to fill this order
                        // This is likely due to things like share purchase plans or company buyouts, which add shares 
                        // in an uncountable fashion, unless manually entered.
                        // We will record this as uncounted, which will require manual fixup

                        capitalEvent.unitsUnaccountedFor = quantity;
                        quantity = 0;
                    }
                }
            }

            capitalEvent.totalGain = capitalEvent.totalValue - capitalEvent.totalCost;

            CapitalEvents.Add(capitalEvent);
        }
    }
}