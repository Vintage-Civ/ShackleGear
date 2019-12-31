using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace VSModLauncher.Items {
    public class shackleItem : Item
    {

        private ICoreServerAPI sapi;
        
        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            if (api.Side != EnumAppSide.Server) return;
            
            sapi = api as ICoreServerAPI;
        }

        public override void OnModifiedInInventorySlot(IWorldAccessor world, ItemSlot slot, ItemStack extractedStack = null)
        {
            base.OnModifiedInInventorySlot(world, slot, extractedStack);

            if (world is IServerWorldAccessor)
            {
                world.Logger.Debug("[SHACKLE-GEAR] Shackle Item Modified");
            }
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel,
            bool firstEvent, ref EnumHandHandling handling)
        {
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
            
            if (slot.Itemstack.GetName() == "Shackle-Gear")
            {
                byEntity.World.Logger.Debug("[SHACKLE-GEAR] OBJECT IN HAND IS IDENTIFIED");
            }
            else
            {
                byEntity.World.Logger.Debug("[SHACKLE-GEAR] OBJECT FAILED");
            }
        }
    }
    
}