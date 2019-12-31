using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands
{
    public class ppGetUID
    {
        private ICoreServerAPI sapi = null;

        public ppGetUID(ICoreServerAPI _sapi)
        {
            this.sapi = _sapi;
        }

        public void Handler(IServerPlayer player, int groupid, CmdArgs args) {
            foreach (var _s_player in sapi.Server.Players)
            {
                if (_s_player.PlayerName == args[0])
                {
                    player.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        "PlayerUID: " + _s_player.PlayerUID,
                        EnumChatType.Notification);

                    return;
                }
            }
        }
    }
}