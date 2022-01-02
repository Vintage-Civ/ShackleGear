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

        public bool FreePlayer(string uid, ItemSlot slot, bool destroy = true)
        {
#if DEBUG
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] Free Function Fired");
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
                    if (destroy) slot.TakeOutWhole();
                    slot.MarkDirty();
                }

                sapi.ModLoader.GetModSystem<ShackleGearTracker>().RemoveItemFromTrack(serverPlayer);
                sapi.Event.UnregisterGameTickListener(sapi.ModLoader.GetModSystem<ModSystemShackleGear>().TrackerIDs[uid]);

                return true;
            }
            return false;
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
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] Imprison Function Fired");
#endif
            if (attribs == null) return false;
            var ms = DateTime.UtcNow.Ticks / 10000000.0;

            attribs.SetString("pearled_uid", prisoner.PlayerUID);
            attribs.SetString("pearled_name", prisoner.PlayerName);
            attribs.SetDouble("pearled_timestamp", ms);

            SetSpawnInAttributes(attribs, prisoner);
            var vec = prisoner.Entity.ServerPos.XYZ;
            prisoner.SetSpawnPosition(new PlayerSpawnPos() { x = vec.XInt, y = vec.YInt, z = vec.ZInt});

            if (!sapi.ModLoader.GetModSystem<ShackleGearTracker>().RemoveItemFromTrack(prisoner))
            {
                sapi.ModLoader.GetModSystem<ShackleGearTracker>().AddItemToTrack(new TrackData(GenSlotReference(slot), killer.Entity.ServerPos.AsBlockPos, prisoner.PlayerUID, killer.PlayerUID, killer.PlayerUID));
            }

            sapi.ModLoader.GetModSystem<ModSystemShackleGear>().RegisterPearlUpdate(prisoner);
            prisoner.SendMessage(GlobalConstants.GeneralChatGroup, "You've been shackled!", EnumChatType.Notification);

            slot.MarkDirty();
            return true;
        }

        public SlotReference GenSlotReference(ItemSlot slot)
        {
            return new SlotReference(slot.Inventory.GetSlotId(slot), slot.Inventory.InventoryID);
        }

    }
}