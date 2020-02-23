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

namespace ShackleGear.BlockEntityBehaviors
{
    class BEBehaviorGearFinder : BlockEntityBehavior
    {
        BlockPos Pos { get => Blockentity.Pos; }
        ShackleGearTracker Tracker { get => Api.ModLoader.GetModSystem<ShackleGearTracker>(); }

        public BEBehaviorGearFinder(BlockEntity blockentity) : base(blockentity)
        {
        }

        public override void Initialize(ICoreAPI api, JsonObject properties)
        {
            base.Initialize(api, properties);
            if (!api.Side.IsServer()) return;

            Blockentity.RegisterGameTickListener(dt =>
            {
                (Blockentity as IBlockEntityContainer)?.Inventory.All(slot =>
                {
                    if (slot.Itemstack?.Item is ItemShackleGear)
                    {
                        ((ItemShackleGear)slot.Itemstack.Item).UpdateFuelState(api.World, slot);
                        string uid = slot.Itemstack.Attributes.GetString("pearled_uid");
                        if (uid != null)
                        {
                            FullTrackData data = Tracker.GetTrackData(uid);
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
                Tracker.SaveTrackToDB();
            }, 500);
        }
    }
}
