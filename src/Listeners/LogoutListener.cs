using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;

namespace VSModLauncher.Listeners {
    public class LogoutListener
    {
        private PrisonController prsn = null;
        private ICoreServerAPI sapi = null;
        public void EventOnPlayerDisconnect(IServerPlayer byplayer) {
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] LOGOUT EVENT FIRED\n");
            //should just iterate through the entire players invetory and drop every shackle item.
            foreach (var inventory in byplayer.InventoryManager.Inventories)
            {
                foreach (var slot in inventory.Value)
                {
                    //needs fixing, i shouldnt use the item name yikes
                    if (slot.Itemstack.GetName() == "Shackle-Gear" && 
                        slot.Itemstack.Attributes.GetString("pearled_uid") != null)
                    {
                        byplayer.Entity.World.Logger.Debug("[SHACKLE-GEAR] IDENTIFIED ITEM ON LOGOUT");
                        bool Succeeded = prsn.FreePlayer(slot.Itemstack.Attributes.GetString("pearled_uid"), slot);

                        if (Succeeded)
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