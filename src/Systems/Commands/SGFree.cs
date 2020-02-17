using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;
using VSModLauncher.Items;

namespace VSModLauncher.Commands {
    public class SGFree
    {
        public ICoreServerAPI sapi;

        public SGFree(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        private PrisonController prsn { get => sapi.ModLoader.GetModSystem<ModSystemShackleGear>().Prsn;  }
        
        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            if (player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item is ItemShackleGear)
            {
                prsn.FreePlayer(player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("pearled_uid"), player.InventoryManager.ActiveHotbarSlot);
                player.SendMessage(GlobalConstants.GeneralChatGroup, "You've freed a pearl", EnumChatType.Notification);
            }
        }
    }
}