using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using ShackleGear.Controllers;
using ShackleGear.Items;

namespace ShackleGear.Commands {
    public class SGFree
    {
        public ICoreServerAPI sapi;

        public SGFree(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        private PrisonController prsn { get => sapi.ModLoader.GetModSystem<ModSystemShackleGear>().Prison;  }
        
        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            if (player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item is ItemShackleGear)
            {
                prsn.FreePlayer(player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("pearled_uid"), player.InventoryManager.ActiveHotbarSlot);
                player.SendMessage(GlobalConstants.GeneralChatGroup, "You've freed a pearl", EnumChatType.Notification);
            }
        }
    }
}