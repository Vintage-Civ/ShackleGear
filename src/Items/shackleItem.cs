using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace VSModLauncher.Items
{
    public class ItemShackleGear : Item
    {
        private ICoreServerAPI sapi;

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
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

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
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

        public override void InGuiIdle(IWorldAccessor world, ItemStack stack)
        {
            GuiTransform.Rotation.Y = GameMath.Mod(world.ElapsedMilliseconds / 50f, 360);
        }

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            if (byEntity.World is IClientWorldAccessor)
            {
                FpHandTransform.Rotation.Y = GameMath.Mod(byEntity.World.ElapsedMilliseconds / 50f, 360);
                TpHandTransform.Rotation.Y = GameMath.Mod(byEntity.World.ElapsedMilliseconds / 50f, 360);
            }

        }

        public override void OnGroundIdle(EntityItem entityItem)
        {
            GroundTransform.Rotation.Y = GameMath.Mod(entityItem.World.ElapsedMilliseconds / 50f, 360);

            if (entityItem.World is IClientWorldAccessor)
            {
                float angle = (entityItem.World.ElapsedMilliseconds / 15f + entityItem.EntityId * 20) % 360;
                float bobbing = entityItem.Collided ? GameMath.Sin(angle * GameMath.DEG2RAD) / 15 : 0;
                Vec3d pos = entityItem.LocalPos.XYZ;
                pos.Y += 0.15f + bobbing;
            }
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc.AppendLine("Imprisoned UID: " + inSlot.Itemstack.Attributes.GetString("pearled_uid", "Empty")), world, withDebugInfo);
        }
    }
}