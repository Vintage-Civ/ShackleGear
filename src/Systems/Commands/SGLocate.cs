using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands
{
    public class SGLocate
    {
        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {
            player.SendMessage(GlobalConstants.GeneralChatGroup, "NYI", EnumChatType.OwnMessage);
        }
    }
}