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
        public bool IsChunkForceLoaded { get; private set; }

        public ItemSlot Slot
        {
            get
            {
                ItemSlot slot = null;
                if (IsChunkForceLoaded)
                {
                    if (LastHolder?.InventoryManager?.Inventories != null && LastHolder.InventoryManager.Inventories.ContainsKey(trackData.SlotReference.InventoryID))
                    {
                        slot = LastHolder.InventoryManager.Inventories[trackData.SlotReference.InventoryID][trackData.SlotReference.SlotID];
                    }
                    else
                    {
                        slot = (api.World.BlockAccessor.GetBlockEntity(trackData.LastPos) as IBlockEntityContainer)?.Inventory?[trackData.SlotReference.SlotID];
                    }
                }
                return slot;
            }
        }

        public void LoadMyChunk()
        {
            if (!IsChunkForceLoaded)
            {
                api.WorldManager.LoadChunkColumnFast(LastChunkPos.X, LastChunkPos.Z, new ChunkLoadOptions()
                {
                    KeepLoaded = true,
                    OnLoaded = () =>
                    {
                        IsChunkForceLoaded = true;
#if DEBUG
                        api.World.Logger.Debug("[ShackleGear] Chunk Column Loaded: " + LastChunkPos.X + ", " + LastChunkPos.Z);
#endif
                        var be = api.World.BlockAccessor.GetBlockEntity(trackData.LastPos);
                        be?.Initialize(api);
                    }
                });
            }
        }

        public void MarkUnloadable()
        {
            api.WorldManager.LoadChunkColumnFast(LastChunkPos.X, LastChunkPos.Z, new ChunkLoadOptions() { KeepLoaded = false });
            api.World.Logger.Debug("[ShackleGear] Chunk Column Marked Unloadable: " + LastChunkPos.X + ", " + LastChunkPos.Z);
            IsChunkForceLoaded = false;
        }
    }

    [JsonObject(MemberSerialization.OptIn, ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class TrackData
    {
        public TrackData(SlotReference slotReference, BlockPos lastPos, string prisonerUID, string lastHolderUID, string lastFuelerUID)
        {
            SlotReference = slotReference;
            LastPos = lastPos;
            PrisonerUID = prisonerUID;
            LastHolderUID = lastHolderUID;
            LastFuelerUID = lastFuelerUID;
        }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public SlotReference SlotReference { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public BlockPos LastPos { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string PrisonerUID { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string LastHolderUID { get; set; }

        [JsonProperty(ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
        public string LastFuelerUID { get; set; }

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
