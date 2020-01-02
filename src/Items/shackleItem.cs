using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using System.Linq;
using VSModLauncher.Controllers;

namespace VSModLauncher.Items
{
    public class ItemShackleGear : Item
    {
        private ICoreServerAPI sapi;
        private float fuelMult;
        public PrisonController Prsn { get => sapi?.ModLoader.GetModSystem<ModSystemShackleGear>().Prsn; }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            sapi = api as ICoreServerAPI;
            fuelMult = Attributes["fuelmult"].AsFloat(1);
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
            handling = EnumHandHandling.PreventDefault;
            ITreeAttribute attribs = slot?.Itemstack?.Attributes;


            if (sapi != null && attribs != null)
            {
                if (byEntity.Controls.Sneak && attribs.GetString("pearled_uid") != null)
                {
                    Prsn.FreePlayer(attribs.GetString("pearled_uid"), slot);
                    ClearAttributes(attribs);
                }
                else
                {
                    slot.Inventory.Any(s =>
                    {
                        if (s?.Itemstack?.Collectible?.CombustibleProps != null)
                        {
                            if (s.Itemstack.Collectible.CombustibleProps.BurnTemperature < 1000) return false;
                            float currentfuel = attribs.GetFloat("pearled_fuel");
                            attribs.SetFloat("pearled_fuel", currentfuel + (s.Itemstack.Collectible.CombustibleProps.BurnDuration * fuelMult));
                            s.TakeOut(1);
                            s.MarkDirty();
                            slot.MarkDirty();
                            return true;
                        }
                        return false;
                    });
                }
            }
            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        public override void InGuiIdle(IWorldAccessor world, ItemStack stack)
        {
            GuiTransform.Rotation.Y = GameMath.Mod(world.ElapsedMilliseconds / 50f, 360);
        }

        public override void OnHeldIdle(ItemSlot slot, EntityAgent byEntity)
        {
            if (byEntity.World is IClientWorldAccessor)
            {
                FpHandTransform.Rotation.Y = -GameMath.Mod(byEntity.World.ElapsedMilliseconds / 50f, 360);
                TpHandTransform.Rotation.Y = -GameMath.Mod(byEntity.World.ElapsedMilliseconds / 50f, 360);
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
            ITreeAttribute attribs = inSlot?.Itemstack?.Attributes;
            string imprisonedName = attribs?.GetString("pearled_name") ?? "Nobody";
            string imprisonedUID = attribs?.GetString("pearled_uid") ?? "Empty";
            float fueledFor = (float)Math.Round(attribs?.GetFloat("pearled_fuel", 0) ?? 0.0f, 3);

            dsc.AppendLine("Shackled: " + imprisonedName).AppendLine("UID: " + imprisonedUID).AppendLine("ShackledTime: " + fueledFor + " Seconds");
            
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        public override TransitionState[] UpdateAndGetTransitionStates(IWorldAccessor world, ItemSlot inSlot)
        {
            if (world.Side.IsServer())
            {
                ITreeAttribute attribs = inSlot?.Itemstack?.Attributes;
                if (attribs?.GetString("pearled_uid") != null)
                {
                    if (attribs.GetDouble("pearled_timestamp", -1.0) != -1.0)
                    {
                        float dt = (float)(world.Calendar.TotalHours - attribs.GetFloat("pearled_timestamp")) / 120;
                        float fuel = attribs.GetFloat("pearled_fuel", 0.0f);
                        if (fuel < 0f)
                        {
                            Prsn.FreePlayer(attribs.GetString("pearled_uid"), inSlot);
                        }
                        else
                        {
                            attribs.SetFloat("pearled_fuel", fuel - dt);
                        }
                    }
                }
            }
            return base.UpdateAndGetTransitionStates(world, inSlot);
        }

        public void ClearAttributes(ITreeAttribute attribs)
        {
            foreach (var val in attribs)
            {
                attribs.RemoveAttribute(val.Key);
            }
        }
    }
}