using Newtonsoft.Json;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using VSModLauncher.Items;

namespace VSModLauncher.Datasource
{
    [JsonObject(MemberSerialization.OptIn, ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class TrackData
    {
        ICoreServerAPI api;

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public SlotReference SlotReference;

        public ItemSlot Slot
        {
            get
            {
                bool wasunloaded = TryLoadChunk();
                ItemSlot slot = LastHolder?.InventoryManager?.Inventories?[SlotReference.InventoryID]?[SlotReference.SlotID];
                if (wasunloaded) api.Event.RegisterCallback(dt => TryUnloadChunk(), 500);
                return slot;
            }
        }

        public bool TryLoadChunk()
        {
            if (lastChunkPos == null) return false;

            bool unloaded = api.WorldManager.GetChunk(lastChunkPos.X, lastChunkPos.Y, lastChunkPos.Z) == null;
            if (unloaded)
            {
                api.WorldManager.LoadChunkColumnFast(lastChunkPos.X, lastChunkPos.Z);
                api.World.Logger.Debug("[SHACKLE-GEAR] CHUNK LOADED " + lastChunkPos);
            }
            return unloaded;
        }

        public bool TryUnloadChunk()
        {
            if (lastChunkPos == null) return false;

            bool unloaded = api.WorldManager.GetChunk(lastChunkPos.X, lastChunkPos.Y, lastChunkPos.Z) == null;
            if (!unloaded)
            {
                api.World.Logger.Debug("[SHACKLE-GEAR] CHUNK UNLOADED: " + lastChunkPos);
                api.WorldManager.UnloadChunkColumn(lastChunkPos.X, lastChunkPos.Z);
            }
            return unloaded;
        }

        public ItemStack ItemStack { get => Slot?.Itemstack; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string PrisonerUID;

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string LastHolderUID;

        public IServerPlayer Prisoner { get => (IServerPlayer)api.World.PlayerByUid(PrisonerUID); }
        public IServerPlayer LastHolder { get => (IServerPlayer)api.World.PlayerByUid(LastHolderUID); }


        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public BlockPos lastPos = new BlockPos();

        public Vec3i lastChunkPos { get => new Vec3i(lastPos.X / chunksize, lastPos.Y / chunksize, lastPos.Z / chunksize) ?? null; }

        private int chunksize { get => api.World.BlockAccessor.ChunkSize;  }
        
        public void SetLocation(int x, int y, int z)
        {
            lastPos.X = x; lastPos.Y = y; lastPos.Z = z;
        }

        public void SetLocation(BlockPos pos)
        {
            SetLocation(pos.X, pos.Y, pos.Z);
        }

        public TrackData(ItemSlot slot, IServerPlayer prisoner, IServerPlayer lastHeldBy)
        {
            SlotReference = new SlotReference(slot.Inventory.GetSlotId(slot), slot.Inventory.InventoryID);

            api = lastHeldBy.Entity.World.Api as ICoreServerAPI;
            PrisonerUID = prisoner.PlayerUID;
            LastHolderUID = lastHeldBy.PlayerUID;
        }
    }

    public class SlotReference
    {
        public SlotReference(int slotID, string inventoryID)
        {
            SlotID = slotID;
            InventoryID = inventoryID;
        }

        public int SlotID { get; set; }
        public string InventoryID { get; set; }
    }
}
