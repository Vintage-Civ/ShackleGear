using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using ShackleGear.Items;

namespace ShackleGear.Commands
{
    public class SGDebug
    {
        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {
            if (player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item is ItemShackleGear)
            {
                player.SendMessage(
                    GlobalConstants.GeneralChatGroup,
                    "Current pearled uid = " +
                    player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("pearled_uid") +
                    "\nCurrently pearled playername = " +
                    player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("pearled_name"),
                    EnumChatType.Notification);
            }
        }
    }
}