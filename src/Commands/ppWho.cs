using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands {
    public class ppWho {
        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            if (player.InventoryManager.ActiveHotbarSlot.Itemstack.GetName() == "Shackle-Gear") {
                string heldplayer =
                    player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString(
                        "pearled_name");

                if (heldplayer != null) {
                    player.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        "The held player is " + heldplayer,
                        EnumChatType.Notification
                        );
                    
                } else {
                    player.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        "The pearl is empty",
                        EnumChatType.Notification
                    );
                }

            } else {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "You aren't holding a pearl",
                    EnumChatType.Notification
                );
            }
        }
    }
}