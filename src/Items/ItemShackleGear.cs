using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using System.Linq;
using ShackleGear.Controllers;
using ShackleGear.Datasource;
using System.Collections.Generic;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace ShackleGear.Items
{
    public class ItemShackleGear : Item
    {
        private ICoreServerAPI sapi;
        private ICoreClientAPI capi;
        private double fuelMult;
        private double maxSeconds;
        public PrisonController Prsn { get => api.ModLoader.GetModSystem<ModSystemShackleGear>().Prison; }
        public ShackleGearTracker Tracker { get => api.ModLoader.GetModSystem<ShackleGearTracker>(); }

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
                //world.Logger.Debug("[SHACKLE-GEAR] Shackle Item Modified");
            }
#endif
        }

        public override void OnGroundIdle(EntityItem entityItem)
        {
            base.OnGroundIdle(entityItem);
            ITreeAttribute attribs = entityItem.Slot?.Itemstack?.Attributes;
            if (attribs?.GetString("pearled_uid") != null)
            {
                if (entityItem.Collided)
                {
                    Prsn?.FreePlayer(attribs.GetString("pearled_uid"), entityItem.Slot, true, entityItem.Pos.AsBlockPos.UpCopy());
                    entityItem.Die();
                }
            }
        }

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.PreventDefault;
            ITreeAttribute attribs = slot?.Itemstack?.Attributes;
            var pos = blockSel?.Position;

            if (sapi != null && attribs != null)
            {
                Block selBlock = pos != null ? sapi.World.BlockAccessor.GetBlock(pos) : null;
                string uid = attribs.GetString("pearled_uid");
                double cooldown = attribs.GetDouble("shackled_cell_cooldown");

                if (selBlock is BlockBed)
                {
                    if (api.World.Calendar.TotalHours > cooldown && uid != null)
                    {
                        foreach (var invSlot in slot.Inventory)
                        {
                            ItemStack stack = invSlot?.Itemstack;
                            Item item = stack?.Item;

                            if (item is ItemRustyGear && stack.StackSize >= 2)
                            {
                                invSlot.TakeOut(2);
                                invSlot.MarkDirty();
                                slot.MarkDirty();
                                attribs.SetDouble("shackled_cell_cooldown", api.World.Calendar.TotalHours + 24);
                                attribs.SetBlockPos("shackled_cell", pos);

                                Prsn.SetCellSpawn(uid, pos);
                                Prsn.MoveToCell(uid);

                                break;
                            }
                        }
                    }
                }
                else
                {
                    if (byEntity.Controls.Sneak && uid != null)
                    {
                        Prsn.FreePlayer(uid, slot, true, pos.UpCopy());
                    }
                    else
                    {
                        double currentfuel = attribs.GetDouble("pearled_fuel");
                        foreach (var invSlot in slot.Inventory)
                        {
                            if (currentfuel > maxSeconds) break;

                            var cobj = invSlot?.Itemstack?.Collectible;

                            if (cobj?.CombustibleProps != null)
                            {
                                if (cobj.CombustibleProps.BurnTemperature < 1000) continue;

                                attribs.SetDouble("pearled_fuel", currentfuel + (cobj.CombustibleProps.BurnDuration * fuelMult));
                                invSlot.TakeOut(1);
                                invSlot.MarkDirty();
                                slot.MarkDirty();
                                if (attribs.GetString("pearled_uid") != null)
                                {
                                    Tracker.SetLastFuelerUID(attribs.GetString("pearled_uid"), (byEntity as EntityPlayer).PlayerUID);
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            ITreeAttribute attribs = inSlot?.Itemstack?.Attributes;
            string imprisonedName = attribs?.GetString("pearled_name");
            string imprisonedUID = attribs?.GetString("pearled_uid");
            double fueledFor = Math.Round(attribs?.GetDouble("pearled_fuel", 0) ?? 0.0f, 3);
            double cooldown = attribs?.GetDouble("shackled_cell_cooldown") ?? 0;
            bool inCooldown = cooldown > world.Calendar.TotalHours;
            int timeLeft = (int)Math.Round(cooldown - world.Calendar.TotalHours);

            TimeSpan time = TimeSpan.FromSeconds(fueledFor);

            dsc.AppendLine("Shackled: " + imprisonedName ?? "Nobody").AppendLine("UID: " + imprisonedUID ?? "Empty").AppendLine("Remaining Time: " + time.ToString(@"dd\:hh\:mm\:ss"));
            dsc.AppendLine(string.Format("Can {0} set cell spawn {1}", 
                imprisonedName == null || inCooldown ? "not" : "", 
                imprisonedName == null ? "nobody imprisoned!" : inCooldown ? string.Format("now, must wait {0} game hours before next cell set.", timeLeft) : "now." ));
            
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);
        }

        public void UpdateFuelState(IWorldAccessor world, ItemSlot inSlot)
        {
            if (inSlot == null)
            {
#if DEBUG
                world.Logger.Debug("[SHACKLE-GEAR] Slot was null during update fuel state.");
#endif
                return;
            }

            if (world.Side.IsServer() && !(inSlot is ItemSlotCreative))
            {
                ITreeAttribute attribs = inSlot?.Itemstack?.Attributes;
                if (attribs?.GetString("pearled_uid") != null)
                {
                    if (attribs.GetDouble("pearled_timestamp", -1.0) != -1.0)
                    {
                        long ms = DateTime.UtcNow.Ticks;
                        long dt = ms - attribs.GetLong("pearled_timestamp");
                        double fuel = attribs.GetDouble("pearled_fuel", 0.0f);
#if DEBUG
                        world.Logger.Debug(string.Format("[SHACKLE-GEAR] Fuel Left On This Tick: {0} Units", Math.Round(fuel, 3)));
                        world.Logger.Debug(string.Format("[SHACKLE-GEAR] TimeStamp: {0}", Math.Round(attribs.GetFloat("pearled_timestamp"), 3)));
                        world.Logger.Debug(string.Format("[SHACKLE-GEAR] MS: {0}", ms));
#endif
                        if (fuel < 0f)
                        {
                            Prsn.FreePlayer(attribs.GetString("pearled_uid"), inSlot);
                            attribs.SetLong("pearled_timestamp", -1);
                        }
                        else
                        {
                            attribs.SetDouble("pearled_fuel", fuel - (double)(dt / 10000000.0));
                        }
                    }
                }
                inSlot.MarkDirty();
            }
        }
    }
}