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
        public int Last_x;
        public int Last_y;
        public int Last_z;
        
        public void SetLocation(int x, int y, int z)
        {
            this.Last_x = x;
            this.Last_y = y;
            this.Last_z = z;
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