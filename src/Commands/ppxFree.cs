using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands
{
    public class ppxFree
    {
        private ICoreServerAPI sapi = null;
        
        public ppxFree(ICoreServerAPI _sapi)
        {
            this.sapi = _sapi;
        }
        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            foreach (var _s_player in sapi.Server.Players)
            {
                if (_s_player.PlayerUID == args[0])
                {
                    sapi.Permissions.SetRole(_s_player, "suplayer");
                    if (_s_player.ConnectionState != EnumClientState.Offline)
                    {
                        _s_player.SendMessage(
                            GlobalConstants.GeneralChatGroup,
                            "You've been freed!",
                            EnumChatType.Notification);
                    }
                    
                    return;
                }
            }
            
            player.SendMessage(GlobalConstants.GeneralChatGroup,
                "Shacklegear Error: Player with given ID not found",
                EnumChatType.Notification);
        }
    }
}