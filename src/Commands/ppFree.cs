using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;

namespace VSModLauncher.Commands {
    public class ppFree
    {
        private PrisonController psrn = null;

        public ppFree(PrisonController _psrn)
        {
            this.psrn = _psrn;
        }
        
        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            if (player.InventoryManager.ActiveHotbarSlot.Itemstack.GetName() == "Shackle-Gear")
            {
                psrn.FreePlayer(player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("pearled_uid"),
                    player.InventoryManager.ActiveHotbarSlot);
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "You've freed a pearl",
                    EnumChatType.Notification);
            }
        }
    }
}