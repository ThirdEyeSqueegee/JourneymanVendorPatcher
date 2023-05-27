using Mutagen.Bethesda;
using Mutagen.Bethesda.Plugins;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;

namespace JourneymanVendorPatcher
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            return await SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "JourneymanVendorPatcher.esp")
                .Run(args);
        }

        private static readonly ModKey Journeyman = ModKey.FromNameAndExtension("Journeyman.esp");

        private static readonly HashSet<string> Edids = new()
        {
            "MerchantCaravanAChest",
            "MerchantCaravanBChest",
            "MerchantCaravanCChest",
            "MerchantDawnstarWindpeakInnChest",
            "MerchantDragonBridgeFourShieldsTavernChest",
            "MerchantFalkreathDeadMansDrinkChest",
            "MerchantIvarsteadVilemyrInnChest",
            "MerchantKynesgroveBraidwoodInnChest",
            "MerchantMarkarthSilverFishInnChest",
            "MerchantMorthalMoorsideInnChest",
            "MerchantNightgateInnChest",
            "MerchantOldHroldanHangedManInnChest",
            "MerchantRiftenBeeandBarbChest",
            "MerchantRiverwoodSleepingGiantChest",
            "MerchantRoriksteadFrostFruitInnChest",
            "MerchantRiverwoodTraderChest",
            "MerchantSolitudeWinkingSkeeverChest",
            "MerchantWhiterunBannerdMareChest",
            "MerchantWhiterunBelethorsGoodsChest",
            "MerchantWhiterunDrunkenHuntsmanChest",
            "MerchantWindhelmCandlehearthHallChest",
            "MerchantWinterholdFrozenHearthChest",
        };

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            if (!state.LoadOrder.ContainsKey(Journeyman))
                throw new Exception("ERROR: Journeyman.esp not found in load order");

            var travelPack = state.LinkCache.Resolve<IMiscItemGetter>("MAG_TravelPack");
            var travelPackKywd = state.LinkCache.Resolve<IKeywordGetter>("MAG_TravelPackKeyword");

            var innkeeperFormlist = state.LinkCache.Resolve<IFormListGetter>("VendorItemsInnkeeper");
            var patchedFormlist = state.PatchMod.FormLists.GetOrAddAsOverride(innkeeperFormlist);
            patchedFormlist.Items.Add(travelPackKywd);

            foreach (var merchantChest in state.LoadOrder.PriorityOrder.Container().WinningContextOverrides())
            {
                if (!Edids.Contains(merchantChest.Record.EditorID!)) continue;

                var patchedChest = state.PatchMod.Containers.GetOrAddAsOverride(merchantChest.Record);

                patchedChest.Items!.Add(new ContainerEntry
                {
                    Item = new ContainerItem
                    {
                        Item = travelPack.ToLink(),
                        Count = 3
                    }
                });

                Console.WriteLine($"Patched vendor chest: {merchantChest.Record.EditorID}");
            }
        }
    }
}
