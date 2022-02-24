using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using ShackleGear.Datasource;
using ShackleGear.Items;
using Vintagestory.API.Datastructures;

namespace ShackleGear.EntityBehaviors
{
    class EntityBehaviorGearFinder : EntityBehavior
    {
        BlockPos Pos { get => entity?.Pos.AsBlockPos; }
        ShackleGearTracker Tracker { get => entity?.Api.ModLoader.GetModSystem<ShackleGearTracker>(); }
        long id;

        public EntityBehaviorGearFinder(Entity entity) : base(entity)
        {
        }

        public override string PropertyName() => "gearfinder";

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);
            if (!entity.World.Side.IsServer()) return;

            id = entity.World.RegisterGameTickListener(dt =>
            {
                (entity as EntityPlayer).WalkInventory(slot =>
                {
                    if (!(slot is ItemSlotCreative) && slot.Itemstack?.Item is ItemShackleGear)
                    {
                        ((ItemShackleGear)slot.Itemstack.Item).UpdateFuelState(entity.World, slot);
                        string uid = slot.Itemstack.Attributes.GetString("pearled_uid");
                        if (uid != null)
                        {
                            FullTrackData data = Tracker?.GetTrackData(uid);
                            if (!data.IsNull && Pos != null)
                            {
                                data.SetLocation(Pos);
                                data.SlotReference.InventoryID = slot.Inventory.InventoryID;
                                data.SlotReference.SlotID = slot.Inventory.GetSlotId(slot);
                                data.LastHolderUID = ((EntityPlayer)entity).PlayerUID;
                            }
                        }   
                    }
                    return true;
                });
                Tracker?.SaveTrackToDB();
            }, 500);
        }

        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            base.OnEntityDespawn(despawn);
            entity.World.UnregisterGameTickListener(id);
        }
    }
}
