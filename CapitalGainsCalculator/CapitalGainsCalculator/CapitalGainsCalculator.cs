using System;

namespace CapitalGainsCalculator
{
    internal class CapitalGainsCalculator
    {
        private static AssetRegistry s_AssetRegistry;

        private static void Main(string[] args)
        {
            string line;

            while (true)
            {
                line = Console.ReadLine();

                if (!HandleCommand(line))
                {
                    break;
                }
            }
        }

        // Handles command line commands. Returns false when the program should exist.
        private static bool HandleCommand(string command)
        {
            string[] commandSplit = command.Split(' ');

            if (commandSplit == null || commandSplit.Length == 0)
            {
                return true;
            }

            string commandLower = commandSplit[0].ToLower();

            if (commandLower == "exit" || commandLower == "e")
            {
                return false;
            }

            if (commandLower == "load" || commandLower == "l")
            {
                CommandLoad(commandSplit);
                return true;
            }

            if (commandLower == "printassetevents" || commandLower == "pae")
            {
                PrintAssetEventsCommand();
                return true;
            }

            if (commandLower == "printparcels" || commandLower == "pp")
            {
                PrintParcelsCommand();
                return true;
            }

            if (commandLower == "printcapitalevents" || commandLower == "pce")
            {
                PrintCapitalEventsCommand();
                return true;
            }

            if (commandLower == "clear" || commandLower == "c")
            {
                ClearCommand();
                return true;
            }

            Debug.Log("Invalid command: " + commandLower, ConsoleColor.Red);

            return true;
        }

        private static void CommandLoad(string[] commandArgs)
        {
            if (commandArgs.Length < 2)
            {
                Debug.Log("Load command must be supplied with a path to load CSV data from.", ConsoleColor.Red);
                return;
            }

            string loadPath = commandArgs[1];

            if (string.IsNullOrWhiteSpace(loadPath))
            {
                Debug.Log("Invalid path.", ConsoleColor.Red);
            }

            try
            {
                CommsecCSVAssetRegistryParser parser = new CommsecCSVAssetRegistryParser();
                bool success = parser.TryParseFromFile(loadPath, out s_AssetRegistry);

                if (success)
                {
                    Debug.Log("Successfully loaded asset registry. Events: " + s_AssetRegistry.AssetEvents.Count);
                }
                else
                {
                    Debug.Log("Failed to load asset registry.", ConsoleColor.Red);
                }
            }
            catch (Exception e)
            {
                Debug.Log("Failed to load file: " + loadPath + "  " + e.Message, ConsoleColor.Red);
            }

            s_AssetRegistry.CalculateCapitalGainEvents();
        }

        private static void PrintAssetEventsCommand()
        {
            if (s_AssetRegistry == null)
            {
                Debug.Log("No Asset Registry is loaded. Use \"load [path]\" command to load one", ConsoleColor.Red);
                return;
            }

            if (s_AssetRegistry.AssetEvents.Count == 0)
            {
                Debug.Log("Asset Registry has no events.", ConsoleColor.Red);
            }

            s_AssetRegistry.PrintAssetEvents();
        }

        private static void PrintParcelsCommand()
        {
            if (s_AssetRegistry == null)
            {
                Debug.Log("No Asset Registry is loaded. Use \"load [path]\" command to load one", ConsoleColor.Red);
                return;
            }

            if (s_AssetRegistry.AssetEvents.Count == 0)
            {
                Debug.Log("Asset Registry has no events.", ConsoleColor.Red);
            }

            s_AssetRegistry.PrintParcels();
        }

        private static void PrintCapitalEventsCommand()
        {
            if (s_AssetRegistry == null)
            {
                Debug.Log("No Asset Registry is loaded. Use \"load [path]\" command to load one", ConsoleColor.Red);
                return;
            }

            if (s_AssetRegistry.AssetEvents.Count == 0)
            {
                Debug.Log("Asset Registry has no events.", ConsoleColor.Red);
            }

            s_AssetRegistry.PrintCapitalEvents();
        }

        private static void ClearCommand()
        {
            s_AssetRegistry = null;
            Debug.Log("Asset Registry Cleared");
        }
    }
}