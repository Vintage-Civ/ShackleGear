using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using ShackleGear.Datasource;
using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.Server;
using Vintagestory.Common;
using Vintagestory.API.Util;

namespace ShackleGear.Controllers
{
    public class PrisonController
    {
        private ICoreServerAPI sapi;
        ShackleGearTracker tracker;
        ModSystemShackleGear shackleGear;
        public PrisonController(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
            tracker = sapi.ModLoader.GetModSystem<ShackleGearTracker>();
            shackleGear = sapi.ModLoader.GetModSystem<ModSystemShackleGear>();
        }

        public BlockPos GetShacklePos(string uid)
        {
            var dat = tracker.GetTrackData(uid);
            return dat?.LastPos;
        }

        public void FreePlayer(string uid, ItemSlot slot, bool destroy = true, BlockPos brokenAt = null)
        {
            if (sapi == null) return;
#if DEBUG
            sapi.Server.Logger.Debug(string.Format("[SHACKLE-GEAR] Free Function Fired, Call Stack: {0}", Environment.StackTrace));
#endif

            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (serverPlayer != null)
            {
                serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "You've been freed!", EnumChatType.Notification);
            }

            var playerDat = sapi.PlayerData.GetPlayerDataByUid(uid) as ServerPlayerData;
            playerDat.SetRole((sapi.World as ServerMain).Config.RolesByCode["suplayer"]);

            ITreeAttribute attribs = slot?.Itemstack?.Attributes;

            if (attribs != null)
            {
                var vec = GetSpawnFromAttributes(attribs);

                var server = sapi.World as ServerMain;
                var worldData = server.GetWorldPlayerData(uid) as ServerWorldPlayerData;

                if (worldData != null)
                {
                    worldData.SpawnPosition = new PlayerSpawnPos() { x = (int)vec.X, y = (int)vec.Y, z = (int)vec.Z };
                }
                else
                {
                    byte[] data = server.GetField<ChunkServerThread>("chunkThread").GetField<GameDatabase>("gameDatabase").GetPlayerData(uid);
                    if (data != null)
                    {
                        try
                        {
                            worldData = SerializerUtil.Deserialize<ServerWorldPlayerData>(data);
                            worldData.Init(server);

                            worldData.SpawnPosition = new PlayerSpawnPos() { x = (int)vec.X, y = (int)vec.Y, z = (int)vec.Z };

                            server.PlayerDataManager.WorldDataByUID[uid] = worldData;
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }

            if (slot != null)
            {
                if (destroy)
                {
                    if (brokenAt != null)
                    {
                        sapi.World.PlaySoundAt(new AssetLocation("sounds/block/glass"), brokenAt.X + 0.5, brokenAt.Y, brokenAt.Z + 0.5);
                        sapi.World.SpawnCubeParticles(brokenAt.ToVec3d().Add(0.5, 0, 0.5), slot.Itemstack, 1, 32);
                    }
                    slot.TakeOutWhole();
                }
                slot.MarkDirty();
            }

            tracker.TryRemoveItemFromTrack(uid);
            if (shackleGear.TrackerIDs.ContainsKey(uid))
            {
                sapi.Event.UnregisterGameTickListener(shackleGear.TrackerIDs[uid]);
            }
            ClearCellSpawn(uid);
        }

        public void MovePlayer(string uid, BlockPos pos)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (pos != null && serverPlayer != null)
            {
                serverPlayer.Entity.TeleportTo(pos.X, pos.Y, pos.Z);
            }
        }

        public void MoveToCell(Entity entity)
        {
            string uid = ((entity as EntityPlayer)?.Player as IServerPlayer)?.PlayerUID;

            MovePlayer(uid, GetCellSpawn(entity));
        }

        public void MoveToCell(string uid)
        {
            if (uid == null) return;

            MovePlayer(uid, GetCellSpawn(uid));
        }

        public void ClearCellSpawn(string uid)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (serverPlayer != null)
            {
                serverPlayer.Entity.WatchedAttributes.RemoveAttribute("shackled_cellX");
                serverPlayer.Entity.WatchedAttributes.RemoveAttribute("shackled_cellY");
                serverPlayer.Entity.WatchedAttributes.RemoveAttribute("shackled_cellZ");
            }
        }

        public void SetCellSpawn(string uid, BlockPos pos)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (pos != null && serverPlayer != null)
            {
                serverPlayer.Entity.WatchedAttributes.SetVec3i("shackled_cell", pos.ToVec3i());
            }
        }

        public BlockPos GetCellSpawn(Entity entity)
        {
            return entity?.WatchedAttributes?.GetVec3i("shackled_cell")?.AsBlockPos;
        }

        public BlockPos GetCellSpawn(string uid)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;

            return GetCellSpawn(serverPlayer?.Entity);
        }

        public void SetSpawnInAttributes(ITreeAttribute attribs, IServerPlayer player)
        {
            if (!attribs.HasAttribute("pearled_x"))
            {
                var pos = player.GetSpawnPosition(false);
                attribs.SetDouble("pearled_x", pos.X);
                attribs.SetDouble("pearled_y", pos.Y);
                attribs.SetDouble("pearled_z", pos.Z);
            }
        }

        public Vec3d GetSpawnFromAttributes(ITreeAttribute attribs)
        {
            return new Vec3d(attribs.GetDouble("pearled_x", 0), attribs.GetDouble("pearled_y", 0), attribs.GetDouble("pearled_z", 0));
        }

        public bool TryImprisonPlayer(IServerPlayer prisoner, IServerPlayer killer, ItemSlot slot)
        {
            //imprison some player

            ITreeAttribute attribs = slot?.Itemstack?.Attributes;
#if DEBUG
            sapi.Server.Logger.Debug(string.Format("[SHACKLE-GEAR] Imprison Function Fired, Call Stack: {0}", Environment.StackTrace));
#endif
            if (attribs == null) return false;
            long ticks = DateTime.UtcNow.Ticks;

            var tracker = sapi.ModLoader.GetModSystem<ShackleGearTracker>();
            
            if (tracker.IsShackled(prisoner)) return false;

            attribs.SetString("pearled_uid", prisoner.PlayerUID);
            attribs.SetString("pearled_name", prisoner.PlayerName);
            attribs.SetLong("pearled_timestamp", ticks);

            SetSpawnInAttributes(attribs, prisoner);
            var vec = prisoner.Entity.ServerPos.XYZ;
            prisoner.SetSpawnPosition(new PlayerSpawnPos() { x = vec.XInt, y = vec.YInt, z = vec.ZInt});

            if (!tracker.TryRemoveItemFromTrack(prisoner))
            {
                tracker.AddItemToTrack(new TrackData(GenSlotReference(slot), killer.Entity.ServerPos.AsBlockPos, prisoner.PlayerUID, killer.PlayerUID, killer.PlayerUID));
            }

            sapi.ModLoader.GetModSystem<ModSystemShackleGear>().RegisterPearlUpdate(prisoner);
            prisoner.SendMessage(GlobalConstants.GeneralChatGroup, "You've been shackled!", EnumChatType.Notification);

            slot.MarkDirty();
            (sapi.World as Vintagestory.Server.ServerMain).EventManager.TriggerPlayerRespawn(prisoner);
            sapi.World.PlaySoundAt(new AssetLocation("sounds/wearable/chain1"), prisoner, null, false, 8, 8);
            return true;
        }

        public SlotReference GenSlotReference(ItemSlot slot)
        {
            return new SlotReference(slot.Inventory.GetSlotId(slot), slot.Inventory.InventoryID);
        }

    }
}