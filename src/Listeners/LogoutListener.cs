using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;
using VSModLauncher.Items;

namespace VSModLauncher.Listeners
{
    public class LogoutListener
    {
        private PrisonController prsn = null;
        private ICoreServerAPI sapi = null;
        public void EventOnPlayerDisconnect(IServerPlayer byplayer)
        {
#if DEBUG
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] LOGOUT EVENT FIRED\n");
#endif
            //should just iterate through the entire players invetory and drop every shackle item.
            foreach (var inventory in byplayer.InventoryManager.Inventories)
            {
                foreach (var slot in inventory.Value)
                {
                    if (slot?.Itemstack?.Item is ItemShackleGear && slot.Itemstack.Attributes.GetString("pearled_uid") != null)
                    {
#if DEBUG
                        byplayer.Entity.World.Logger.Debug("[SHACKLE-GEAR] IDENTIFIED ITEM ON LOGOUT");
#endif
                        if (prsn.FreePlayer(slot.Itemstack.Attributes.GetString("pearled_uid"), slot))
                        {
                            slot.TakeOutWhole();
                        }
                    }
                }
            }
        }

        public LogoutListener(PrisonController _psrn, ICoreServerAPI _sapi)
        {
            this.prsn = _psrn;
            this.sapi = _sapi;
        }
    }
}