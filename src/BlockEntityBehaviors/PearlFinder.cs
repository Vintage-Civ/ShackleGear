using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using VSModLauncher.Datasource;
using VSModLauncher.Items;

namespace VSModLauncher.BlockEntityBehaviors
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
                (Blockentity as IBlockEntityContainer)?.Inventory.All(a =>
                {
                    if (a.Itemstack?.Item is ItemShackleGear)
                    {
                        string uid = a.Itemstack.Attributes.GetString("pearled_uid");
                        if (uid != null)
                        {
                            TrackData data = Tracker.GetTrackData(uid);
                            if (data != null)
                            {
                                data.SetLocation(Pos);
                                data.ItemStack = a.Itemstack;
                            }
                        }
                    }
                    return false;
                });
                Tracker.SaveTrackToDB();
            }, 500);
            
        }
    }

    class EntityBehaviorGearFinder : EntityBehavior
    {
        BlockPos Pos { get => entity.Pos.AsBlockPos; }
        ShackleGearTracker Tracker { get => entity.Api.ModLoader.GetModSystem<ShackleGearTracker>(); }
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
                (entity as EntityPlayer).WalkInventory(s =>
                {
                    if (s.Itemstack?.Item is ItemShackleGear)
                    {
                        string uid = s.Itemstack.Attributes.GetString("pearled_uid");
                        if (uid != null)
                        {
                            TrackData data = Tracker.GetTrackData(uid);
                            if (data != null)
                            {
                                data.SetLocation(Pos);
                                data.ItemStack = s.Itemstack;
                                data.LastHolder = (entity.World as IServerWorldAccessor).PlayerByUid(((EntityPlayer)entity).PlayerUID) as IServerPlayer;
                            }
                        }   
                    }
                    return false;
                });
                Tracker.SaveTrackToDB();
            }, 500);
        }

        public override void OnEntityDespawn(EntityDespawnReason despawn)
        {
            base.OnEntityDespawn(despawn);
            entity.World.UnregisterGameTickListener(id);
        }
    }
}
