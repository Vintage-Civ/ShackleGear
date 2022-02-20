using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using ShackleGear.Datasource;
using ShackleGear.Items;
using Vintagestory.API.Datastructures;
using HarmonyLib;
using Vintagestory.GameContent;

namespace ShackleGear.BlockEntityBehaviors
{
    [HarmonyPatch(typeof(BlockEntityGenericTypedContainer), "Initialize")]
    class PatchContainers
    {
        public static void Postfix(BlockEntityGenericTypedContainer __instance, ICoreAPI api)
        {
            if (!api.Side.IsServer()) return;

            BlockPos Pos = __instance.Pos;
            ShackleGearTracker Tracker = __instance.Api.ModLoader.GetModSystem<ShackleGearTracker>();

            __instance.RegisterGameTickListener(dt =>
            {
                __instance?.Inventory.All(slot =>
                {
                    if (slot.Itemstack?.Item is ItemShackleGear)
                    {
                        ((ItemShackleGear)slot.Itemstack.Item).UpdateFuelState(api.World, slot);
                        string uid = slot.Itemstack.Attributes.GetString("pearled_uid");
                        if (uid != null)
                        {
                            FullTrackData data = Tracker?.GetTrackData(uid);
                            if (data != null)
                            {
                                data.trackData.SetLocation(Pos);
                                data.trackData.SlotReference.InventoryID = slot.Inventory.InventoryID;
                                data.trackData.SlotReference.SlotID = slot.Inventory.GetSlotId(slot);
                            }
                        }
                    }
                    return true;
                });
                Tracker?.SaveTrackToDB();
            }, 500);
        }
    }
}
