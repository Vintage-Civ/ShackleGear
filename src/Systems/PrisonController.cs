using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using ShackleGear.Datasource;

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
                if (attribs != null) serverPlayer.SpawnPosition.SetPos(GetSpawnFromAttributes(attribs));

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
                attribs.SetDouble("pearled_x", player.SpawnPosition.X);
                attribs.SetDouble("pearled_y", player.SpawnPosition.Y);
                attribs.SetDouble("pearled_z", player.SpawnPosition.Z);
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

            attribs.SetString("pearled_uid", prisoner.PlayerUID);
            attribs.SetString("pearled_name", prisoner.PlayerName);
            attribs.SetDouble("pearled_timestamp", sapi.World.Calendar.TotalHours);

            SetSpawnInAttributes(attribs, prisoner);
            prisoner.SpawnPosition.SetPos(prisoner.Entity.ServerPos.XYZ);

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