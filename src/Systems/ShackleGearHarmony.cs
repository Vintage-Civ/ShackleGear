using Vintagestory.API.Common;
using HarmonyLib;
using Vintagestory.API.Client;

namespace ShackleGear
{
    internal class ShackleGearHarmony : ModSystem
    {
        private const string patchCode = "Novocain.ModSystem.ShackleGearHarmony";
        public string sidedPatchCode;

        public Harmony harmonyInstance;

        public override void Start(ICoreAPI api)
        {
            if ((api as ICoreClientAPI)?.IsSinglePlayer ?? false) return;

            sidedPatchCode = string.Format("{0}.{1}", patchCode, api.Side);
            harmonyInstance = new Harmony(sidedPatchCode);
            harmonyInstance.PatchAll();
        }

        public override void Dispose()
        {
            harmonyInstance?.UnpatchAll(sidedPatchCode);
        }
    }
}