using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace VSModLauncher.Datasource
{
    public class TrackData
    {
        public ItemStack Item;
        public ItemSlot Slot;

        public IServerPlayer Prisoner;
        public IServerPlayer LastHolder;
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
            this.Item = slot.Itemstack;
            this.Prisoner = prisoner;
            this.LastHolder = lastHeldBy;
        }
    }
}