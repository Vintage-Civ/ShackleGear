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
using Vintagestory.API.Config;
using HarmonyLib;
using ShackleGear.Utility;

namespace ShackleGear
{
    public partial class ModSystemShackleGear : ModSystem
    {
        public PrisonController Prison { get; private set; }
        ICoreAPI api;
        ICoreServerAPI sapi;
        public Dictionary<string, long> TrackerIDs = new();
        ShackleGearTracker Tracker { get => api.ModLoader.GetModSystem<ShackleGearTracker>(); }
        Type dummyPlayerType;
        internal ShackleGearServerConfig shackleServerConfig;

        public override void Start(ICoreAPI api)
        {
            this.api = api;
            RegisterClasses(api);
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;

            Prison = new PrisonController(sapi);
            RegisterServerCommands(sapi);

            dummyPlayerType = AccessTools.TypeByName("dummyplayer.src.EntityClonePlayer");

            api.Event.OnEntityDeath += OnEntityDeath;
            api.Event.PlayerDeath += OnPlayerDeath;
            api.Event.PlayerDisconnect += EventOnPlayerDisconnect;
            api.Event.OnEntityDespawn += EventOnOnEntityDespawn;

            api.Event.PlayerJoin += RegisterPearlUpdate;

            shackleServerConfig = new ShackleGearServerConfig(api);
            shackleServerConfig.Load();
        }

        private void OnEntityDeath(Entity entity, DamageSource damageSource)
        {
            if (entity is EntityPlayer entityPlayer)
            {
                OnPlayerDeath((IServerPlayer)entityPlayer.Player, damageSource);
            }

            if (dummyPlayerType != null)
            {
                if (entity.GetType() == dummyPlayerType)
                {
                    var player = (IServerPlayer)sapi.World.PlayerByUid(entity.GetField<string>("sourceEntityUID"));
                    OnPlayerDeath(player, damageSource);
                }
            }
        }

        public void RegisterPearlUpdate(IServerPlayer player)
        {
            FullTrackData data = Tracker.GetTrackData(player.PlayerUID);
            BlockPos pos = data?.LastPos;

            if (pos != null && player?.PlayerUID != null)
            {
                sapi.Permissions.SetRole(player, shackleServerConfig.ShackledGroup);
                string uid = player.PlayerUID;

                TrackerIDs[uid] = sapi.Event.RegisterGameTickListener(dt =>
                {
                    try
                    {
                        data.LoadMyChunk();
                        if (data.IsChunkForceLoaded)
                        {
                            if (data.ItemStack?.Item is ItemShackleGear)
                            {
                                ((ItemShackleGear)data.ItemStack.Item).UpdateFuelState(sapi.World, data.Slot);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        data.MarkUnloadable();
                        sapi.Event.UnregisterGameTickListener(TrackerIDs[uid]);
                    }

                }, 500);
            }
        }

        public void OnPlayerDeath(IServerPlayer byplayer, DamageSource damagesource)
        {
            if (damagesource?.SourceEntity is EntityPlayer)
            {
                IPlayer killer = sapi.World.PlayerByUid(((EntityPlayer)damagesource.SourceEntity).PlayerUID);
                killer.Entity.WalkInventory(slot =>
                {
                    if (slot?.Itemstack?.Item is ItemShackleGear && (slot?.Itemstack.Attributes.GetString("shackled_uid") == null))
                    {
                        Prison.TryImprisonPlayer(byplayer, (IServerPlayer)killer, slot);
                        return false;
                    }
                    return true;
                });
            }
        }

        public void EventOnOnEntityDespawn(Entity entity, EntityDespawnData data)
        {
            
        }


        public void EventOnPlayerDisconnect(IServerPlayer byplayer)
        {
            if (TrackerIDs.ContainsKey(byplayer.PlayerUID)) sapi.Event.UnregisterGameTickListener(TrackerIDs[byplayer.PlayerUID]);

            foreach (var inventory in byplayer.InventoryManager.Inventories)
            {
                string name = inventory.Value.ClassName;
                if (name == "chest" || name == GlobalConstants.creativeInvClassName || name == GlobalConstants.groundInvClassName || name == GlobalConstants.creativeInvClassName) continue;
                foreach (var slot in inventory.Value)
                {
                    if (slot is ItemSlotCreative) continue;

                    if (slot?.Itemstack?.Item is ItemShackleGear && slot.Itemstack.Attributes.GetString("shackled_uid") != null)
                    {
                        ItemStack stack = slot.TakeOutWhole();
                        sapi.World.SpawnItemEntity(stack, byplayer.Entity.ServerPos.XYZ);
                    }
                }
            }
        }

    }
}