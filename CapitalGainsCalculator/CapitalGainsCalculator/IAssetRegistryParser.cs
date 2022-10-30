namespace CapitalGainsCalculator
{
    public interface IAssetRegistryParser
    {
        bool TryParseFromFile(string path, out AssetRegistry assetRegistry);
    }
}