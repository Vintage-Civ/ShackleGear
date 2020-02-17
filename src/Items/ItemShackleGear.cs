using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using System.Linq;
using VSModLauncher.Controllers;
using VSModLauncher.Datasource;
using System.Collections.Generic;
using Vintagestory.Client.NoObf;

namespace VSModLauncher.Items
{
    public class ItemShackleGear : Item
    {
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;
        private double fuelMult;
        private double maxSeconds;
        public PrisonController Prsn { get => api.ModLoader.GetModSystem<ModSystemShackleGear>().Prsn; }
        public ShackleGearTracker Tracker { get => api.ModLoader.GetModSystem<ShackleGearTracker>(); }
        public List<TrackData> Tracked { get => Tracker.Tracked; }

        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);

            sapi = api as ICoreServerAPI;
            capi = api as ICoreClientAPI;

            fuelMult = Attributes["fuelmult"].AsDouble(1);
            maxSeconds = Attributes["maxseconds"].AsDouble(1210000);
        }

        public override void OnModifiedInInventorySlot(IWorldAccessor world, ItemSlot slot, ItemStack extractedStack = null)
        {
            base.OnModifiedInInventorySlot(world, slot, extractedStack);
#if DEBUG
            if (world is IServerWorldAccessor)
            {
                world.Logger.Debug("[SHACKLE-GEAR] Shackle Item Modified");
            }
#endif
        }

        public override void OnGroundIdle(EntityItem entityItem)
        {
            base.OnGroundIdle(entityItem);
            ITreeAttribute attribs = entityItem.Slot?.Itemstack?.Attributes;
            if (attribs?.GetString("pearled_uid") != null)
            {
                Prsn.FreePlayer(attribs.GetString("pearled_uid"), entityItem.Slot);
                entityItem.Die();
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
                }
                else
                {
                    slot.Inventory.Any(s =>
                    {
                        if (s?.Itemstack?.Collectible?.CombustibleProps != null)
                        {
                            if (s.Itemstack.Collectible.CombustibleProps.BurnTemperature < 1000) return false;
                            double currentfuel = attribs.GetDouble("pearled_fuel");
                            if (currentfuel > maxSeconds) return true;

                            attribs.SetDouble("pearled_fuel", currentfuel + (s.Itemstack.Collectible.CombustibleProps.BurnDuration * fuelMult));
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

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            ITreeAttribute attribs = inSlot?.Itemstack?.Attributes;
            string imprisonedName = attribs?.GetString("pearled_name") ?? "Nobody";
            string imprisonedUID = attribs?.GetString("pearled_uid") ?? "Empty";
            double fueledFor = Math.Round(attribs?.GetDouble("pearled_fuel", 0) ?? 0.0f, 3);
            TimeSpan time = TimeSpan.FromSeconds(fueledFor);

            dsc.AppendLine("Shackled: " + imprisonedName).AppendLine("UID: " + imprisonedUID).AppendLine("Remaining Time: " + time.ToString(@"dd\:hh\:mm\:ss"));
            
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        public void UpdateFuelState(IWorldAccessor world, ItemSlot inSlot)
        {
            if (inSlot == null)
            {
                world.Logger.Debug("[SHACKLE-GEAR] Slot was null during update fuel state.");
                return;
            }

            if (world.Side.IsServer() && !(inSlot is ItemSlotCreative))
            {
                ITreeAttribute attribs = inSlot?.Itemstack?.Attributes;
                if (attribs?.GetString("pearled_uid") != null)
                {
                    if (attribs.GetDouble("pearled_timestamp", -1.0) != -1.0)
                    {
                        double dt = (world.Calendar.TotalHours - attribs.GetFloat("pearled_timestamp")) / 60;
                        double fuel = attribs.GetDouble("pearled_fuel", 0.0f);
                        if (fuel < 0f)
                        {
                            Prsn.FreePlayer(attribs.GetString("pearled_uid"), inSlot);
                            attribs.SetDouble("pearled_timestamp", -1.0);
                        }
                        else
                        {
                            attribs.SetDouble("pearled_fuel", fuel - dt);
                        }
                    }
                }
            }
        }
    }
}