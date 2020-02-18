using Newtonsoft.Json;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using ShackleGear.Items;

namespace ShackleGear.Datasource
{
    public class FullTrackData
    {
        public FullTrackData(TrackData trackData, ICoreServerAPI api)
        {
            this.trackData = trackData;
            this.api = api;
        }

        public TrackData trackData;
        private ICoreServerAPI api;

        public ItemStack ItemStack { get => Slot?.Itemstack; }
        public IServerPlayer Prisoner { get => (IServerPlayer)api.World.PlayerByUid(trackData.PrisonerUID); }
        public IServerPlayer LastHolder { get => (IServerPlayer)api.World.PlayerByUid(trackData.LastHolderUID); }
        public Vec3i LastChunkPos { get => new Vec3i(trackData.LastPos.X / Chunksize, trackData.LastPos.Y / Chunksize, trackData.LastPos.Z / Chunksize) ?? null; }
        private int Chunksize { get => api.World.BlockAccessor.ChunkSize; }
        public bool IsChunkLoaded { get; private set; }

        public ItemSlot Slot
        {
            get
            {
                ItemSlot slot = null;

                bool wasunloaded = TryLoadChunk();
                if (LastHolder?.InventoryManager?.Inventories != null && LastHolder.InventoryManager.Inventories.ContainsKey(trackData.SlotReference.InventoryID))
                {
                    slot = LastHolder.InventoryManager.Inventories[trackData.SlotReference.InventoryID][trackData.SlotReference.SlotID];
                }
                else
                {
                    slot = (api.World.BlockAccessor.GetBlockEntity(trackData.LastPos) as IBlockEntityContainer)?.Inventory?[trackData.SlotReference.SlotID];
                }
                
                if (wasunloaded) api.Event.RegisterCallback(dt => TryUnloadChunk(), 500);
                return slot;
            }
        }

        public bool TryLoadChunk()
        {
            if (LastChunkPos == null) return false;

            IsChunkLoaded = api.WorldManager.GetChunk(LastChunkPos.X, LastChunkPos.Y, LastChunkPos.Z) != null;
            if (!IsChunkLoaded)
            {
                api.WorldManager.LoadChunkColumnFast(LastChunkPos.X, LastChunkPos.Z, new ChunkLoadOptions() { KeepLoaded = true });
#if DEBUG
                api.World.Logger.Debug("[SHACKLE-GEAR] CHUNK LOADING: " + LastChunkPos);
#endif
            }
            return !IsChunkLoaded;
        }

        public bool TryUnloadChunk()
        {
            if (LastChunkPos == null) return false;

            IsChunkLoaded = api.WorldManager.GetChunk(LastChunkPos.X, LastChunkPos.Y, LastChunkPos.Z) != null;
            if (IsChunkLoaded)
            {
                api.WorldManager.UnloadChunkColumn(LastChunkPos.X, LastChunkPos.Z);
#if DEBUG
                api.World.Logger.Debug("[SHACKLE-GEAR] CHUNK UNLOADING: " + LastChunkPos);
#endif
                IsChunkLoaded = false;
            }
            return !IsChunkLoaded;
        }
    }

    [JsonObject(MemberSerialization.OptIn, ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class TrackData
    {
        public TrackData(SlotReference slotReference, BlockPos lastPos, string prisonerUID, string lastHolderUID)
        {
            SlotReference = slotReference;
            LastPos = lastPos;
            PrisonerUID = prisonerUID;
            LastHolderUID = lastHolderUID;
        }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public SlotReference SlotReference { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public BlockPos LastPos { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string PrisonerUID { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string LastHolderUID { get; set; }
        
        public void SetLocation(int x, int y, int z)
        {
            LastPos.X = x; LastPos.Y = y; LastPos.Z = z;
        }

        public void SetLocation(BlockPos pos)
        {
            SetLocation(pos.X, pos.Y, pos.Z);
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
