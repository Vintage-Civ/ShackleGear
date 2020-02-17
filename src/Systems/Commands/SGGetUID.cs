using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands
{
    public class SGGetUID
    {
        private ICoreServerAPI sapi;

        public SGGetUID(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public void Handler(IServerPlayer byPlayer, int groupid, CmdArgs args) {
            foreach (var player in sapi.Server.Players)
            {
                string playername = args.PopWord();
                if (player.PlayerName == playername)
                {
                    byPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "PlayerUID: " + player.PlayerUID, EnumChatType.Notification);
                    return;
                }
            }
        }
    }
}