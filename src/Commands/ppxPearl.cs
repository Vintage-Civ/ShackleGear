using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands
{
    public class ppxPearl
    {
        private ICoreServerAPI sapi = null;

        public ppxPearl(ICoreServerAPI _sapi)
        {
            this.sapi = _sapi;
        }
        
        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            foreach (var _s_player in sapi.Server.Players)
            {
                if (_s_player.PlayerUID == args[0])
                {
                    sapi.Permissions.SetRole(player, "suvisitor");
                    if (_s_player.ConnectionState != EnumClientState.Offline)
                    {
                        _s_player.SendMessage(
                            GlobalConstants.GeneralChatGroup,
                            "You've been pearled!",
                            EnumChatType.Notification);
                    }
                    //this is not finished, needs to figure out wtf the game domain is
                    ItemStack new_shacklegear_itemstack = new ItemStack(sapi.World.GetItem(new AssetLocation("")));
                    new_shacklegear_itemstack.Attributes.SetString("pearled_uid", player.PlayerUID);
                    new_shacklegear_itemstack.Attributes.SetString("pearled_name", player.PlayerName);

                    player.InventoryManager.TryGiveItemstack(new_shacklegear_itemstack);
                    
                    return;
                }
            }
            
            player.SendMessage(GlobalConstants.GeneralChatGroup,
                "Shacklegear Error: Player with given ID not found",
                EnumChatType.Notification);
        }
    }
}