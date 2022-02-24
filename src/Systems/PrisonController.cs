using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using ShackleGear.Datasource;
using System;

namespace ShackleGear.Controllers
{
    public class PrisonController
    {
        private ICoreServerAPI sapi;

        public PrisonController(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public BlockPos GetShacklePos(string uid)
        {
            var tracker = sapi.ModLoader.GetModSystem<ShackleGearTracker>();
            var dat = tracker.GetTrackData(uid);
            return dat?.LastPos;
        }

        public bool FreePlayer(string uid, ItemSlot slot, bool destroy = true, BlockPos brokenAt = null)
        {
            if (sapi == null) return false;
#if DEBUG
            sapi.Server.Logger.Debug(string.Format("[SHACKLE-GEAR] Free Function Fired, Call Stack: {0}", Environment.StackTrace));
#endif

            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (serverPlayer != null)
            {
                sapi.Permissions.SetRole(serverPlayer, "suplayer");
                ITreeAttribute attribs = slot?.Itemstack?.Attributes;
                var vec = GetSpawnFromAttributes(attribs);

                if (attribs != null) serverPlayer.SetSpawnPosition(new PlayerSpawnPos() { x = (int)vec.X, y = (int)vec.Y, z = (int)vec.Z });

                serverPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "You've been freed!", EnumChatType.Notification);
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

                sapi.ModLoader.GetModSystem<ShackleGearTracker>().TryRemoveItemFromTrack(serverPlayer);
                sapi.Event.UnregisterGameTickListener(sapi.ModLoader.GetModSystem<ModSystemShackleGear>().TrackerIDs[uid]);

                return true;
            }
            return false;
        }

        public void MovePlayer(string uid, BlockPos pos)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (pos != null && serverPlayer != null)
            {
                serverPlayer.Entity.TeleportTo(pos.X, pos.Y, pos.Z);
            }
        }

        public void MoveToCell(string uid)
        {
            MovePlayer(uid, GetCellSpawn(uid));
        }

        public void SetCellSpawn(string uid, BlockPos pos)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            if (pos != null && serverPlayer != null)
            {
                serverPlayer.Entity.WatchedAttributes.SetVec3i("shackled_cell", pos.ToVec3i());
            }
        }

        public BlockPos GetCellSpawn(string uid)
        {
            IServerPlayer serverPlayer = sapi.World.PlayerByUid(uid) as IServerPlayer;
            
            return serverPlayer?.Entity?.WatchedAttributes?.GetVec3i("shackled_cell")?.AsBlockPos;
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
            long ms = DateTime.UtcNow.Ticks;

            var tracker = sapi.ModLoader.GetModSystem<ShackleGearTracker>();
            
            if (tracker.IsShackled(prisoner)) return false;

            attribs.SetString("pearled_uid", prisoner.PlayerUID);
            attribs.SetString("pearled_name", prisoner.PlayerName);
            attribs.SetLong("pearled_timestamp", ms);

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