using Newtonsoft.Json;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace VSModLauncher.Datasource
{
    [JsonObject(MemberSerialization.OptIn, ItemReferenceLoopHandling = ReferenceLoopHandling.Ignore)]
    public class TrackData
    {
        [JsonProperty]
        public ItemSlot Slot;

        public ItemStack ItemStack { get => Slot?.Itemstack; }

        [JsonProperty]
        public IServerPlayer Prisoner;

        [JsonProperty]
        public IServerPlayer LastHolder;

        [JsonProperty]
        public BlockPos lastPos = new BlockPos();
        
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
            Slot = slot;
            Prisoner = prisoner;
            LastHolder = lastHeldBy;
        }
    }
}
