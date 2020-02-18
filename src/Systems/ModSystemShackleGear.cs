using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using ShackleGear.BlockEntityBehaviors;
using ShackleGear.Controllers;
using ShackleGear.Datasource;
using ShackleGear.Items;
using ShackleGear.EntityBehaviors;

namespace ShackleGear
{
    public partial class ModSystemShackleGear : ModSystem
    {
        public PrisonController Prison { get; private set; }
        ICoreAPI api;
        ICoreServerAPI sapi;
        Dictionary<string, long> TrackerIDs = new Dictionary<string, long>();
        ShackleGearTracker Tracker { get => api.ModLoader.GetModSystem<ShackleGearTracker>(); }

        public override void Start(ICoreAPI api)
        {
            this.api = api;
            RegisterClasses(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            RegisterServerCommands(sapi);
            Prison = new PrisonController(sapi);

            api.Event.PlayerDeath += OnPlayerDeath;
            api.Event.PlayerDisconnect += EventOnPlayerDisconnect;
            api.Event.OnEntityDespawn += EventOnOnEntityDespawn;

            api.Event.PlayerJoin += player =>
            {
                RegisterPearlUpdate(player);
            };
        }

        public void RegisterPearlUpdate(IServerPlayer player)
        {
            FullTrackData data = Tracker.GetTrackData(player.PlayerUID);
            BlockPos pos = data?.trackData?.LastPos;

            if (pos != null && player?.PlayerUID != null)
            {
                sapi.Permissions.SetRole(player, "suvisitor");
                string uid = player.PlayerUID;

                TrackerIDs[uid] = sapi.Event.RegisterGameTickListener(dt =>
                {
                    try
                    {
                        bool wasunloaded = false;

                        if (data?.ItemStack?.Item as ItemShackleGear != null)
                        {
                            wasunloaded = data.TryLoadChunk();
                            ((ItemShackleGear)data.ItemStack.Item).UpdateFuelState(sapi.World, data.Slot);
                        }
                        else sapi.Event.UnregisterGameTickListener(TrackerIDs[uid]);

                        if (wasunloaded) data.TryUnloadChunk();
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        sapi.World.Logger.Debug("[ShackleGear] Exception thrown: " + ex);
#endif
                        sapi.Event.UnregisterGameTickListener(TrackerIDs[uid]);
                    }

                }, 500);
            }
        }

        public void OnPlayerDeath(IServerPlayer byplayer, DamageSource damagesource)
        {
#if DEBUG
            sapi.World.Logger.Notification("[SHACKLEGEAR] Event Fired.");
#endif
            if (damagesource?.SourceEntity is EntityPlayer)
            {
#if DEBUG
                sapi.World.Logger.Notification("[SHACKLEGEAR] Was EntityPlayer.");
#endif
                IPlayer killer = sapi.World.PlayerByUid(((EntityPlayer)damagesource.SourceEntity).PlayerUID);
                killer.Entity.WalkInventory(slot =>
                {
                    if (slot?.Itemstack?.Item is ItemShackleGear && (slot?.Itemstack.Attributes.GetString("pearled_uid") == null))
                    {
                        Prison.ImprisonPlayer(byplayer, (IServerPlayer)killer, slot);
#if DEBUG
                        sapi.World.Logger.Notification("[SHACKLEGEAR] Gear Found.");
#endif
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