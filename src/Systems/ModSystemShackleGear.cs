using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using VSModLauncher.BlockEntityBehaviors;
using VSModLauncher.Controllers;
using VSModLauncher.Datasource;
using VSModLauncher.Items;

namespace VSModLauncher
{
    public class ModSystemShackleGear : ModSystem
    {
        public PrisonController Prsn { get; private set; }
        ICoreAPI Api;
        ICoreServerAPI sapi;
        Dictionary<string, long> TrackerIDs = new Dictionary<string, long>();
        ShackleGearTracker Tracker { get => Api.ModLoader.GetModSystem<ShackleGearTracker>(); }

        public override void Start(ICoreAPI api)
        {
            Api = api;
            api.RegisterItemClass("shackleitem", typeof(ItemShackleGear));
            api.RegisterBlockEntityBehaviorClass("gearfinder", typeof(BEBehaviorGearFinder));
            api.RegisterEntityBehaviorClass("gearfinder", typeof(EntityBehaviorGearFinder));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            Prsn = new PrisonController(api);

            api.Event.PlayerDeath += OnPlayerDeath;
            api.Event.PlayerDisconnect += EventOnPlayerDisconnect;
            api.Event.OnEntityDespawn += EventOnOnEntityDespawn;

            api.Event.PlayerJoin += player =>
            {
                RegisterPearlSearch(player);
            };
        }

        public void RegisterPearlSearch(IServerPlayer player)
        {
            TrackData data = Tracker.GetTrackData(player.PlayerUID);
            BlockPos pos = data?.lastPos;
            if (pos != null)
            {
                sapi.Permissions.SetRole(player, "suvisitor");

                TrackerIDs[player.PlayerUID] = sapi.Event.RegisterGameTickListener(dt =>
                {
                    bool wasunloaded = false;

                    if (data != null)
                    {
                        wasunloaded = data.TryLoadChunk();
                        (data.ItemStack.Item as ItemShackleGear)?.UpdateFuelState(sapi.World, data.Slot);
                    }
                    else sapi.Event.UnregisterGameTickListener(TrackerIDs[player.PlayerUID]);

                    if (wasunloaded) data.TryUnloadChunk();
                }, 500);
            }
        }

        public void OnPlayerDeath(IServerPlayer byplayer, DamageSource damagesource)
        {
            sapi.World.Logger.Notification("[SHACKLEGEAR] Event Fired.");
            if (damagesource?.SourceEntity is EntityPlayer)
            {
                sapi.World.Logger.Notification("[SHACKLEGEAR] Was EntityPlayer.");
                IPlayer killer = sapi.World.PlayerByUid(((EntityPlayer)damagesource.SourceEntity).PlayerUID);
                killer.Entity.WalkInventory(slot =>
                {
                    if (slot?.Itemstack?.Item is ItemShackleGear && (slot?.Itemstack.Attributes.GetString("pearled_uid") == null))
                    {
                        Prsn.ImprisonPlayer(byplayer, (IServerPlayer)killer, slot);
                        sapi.World.Logger.Notification("[SHACKLEGEAR] Gear Found.");
                        return false;
                    }
                    return true;
                });
            }
        }

        public void EventOnOnEntityDespawn(Entity entity, EntityDespawnReason reason)
        {
            
        }

        public void EventOnPlayerDisconnect(IServerPlayer byplayer)
        {
            if (TrackerIDs.ContainsKey(byplayer.PlayerUID)) sapi.Event.UnregisterGameTickListener(TrackerIDs[byplayer.PlayerUID]);
#if DEBUG
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] LOGOUT EVENT FIRED\n");
#endif
            foreach (var inventory in byplayer.InventoryManager.Inventories)
            {
                foreach (var slot in inventory.Value)
                {
                    if (slot is ItemSlotCreative) continue;

                    if (slot?.Itemstack?.Item is ItemShackleGear && slot.Itemstack.Attributes.GetString("pearled_uid") != null)
                    {
#if DEBUG
                        byplayer.Entity.World.Logger.Debug("[SHACKLE-GEAR] IDENTIFIED ITEM ON LOGOUT");
#endif
                        ItemStack stack = slot.TakeOutWhole();
                        sapi.World.SpawnItemEntity(stack, byplayer.Entity.ServerPos.XYZ);
                    }
                }
            }
        }

    }
}