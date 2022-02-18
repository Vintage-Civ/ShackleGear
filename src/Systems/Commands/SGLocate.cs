using ShackleGear.Controllers;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace ShackleGear.Commands
{
    public class SGLocate
    {
        private PrisonController Prison;

        public SGLocate(PrisonController prison)
        {
            Prison = prison;
        }

        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {
            var shacklePos = Prison.GetShacklePos(player.PlayerUID);
            if (shacklePos != null)
            {
                var defaultSpawn = player.Entity.World.DefaultSpawnPosition;
                var localPos = shacklePos.SubCopy(defaultSpawn.AsBlockPos);

                player.SendMessage(GlobalConstants.GeneralChatGroup, string.Format("Your shackle last pinged its location as X: {0}, Y: {1}, Z: {2}.", localPos.X, shacklePos.Y, localPos.Z), EnumChatType.OwnMessage);
            }
            else player.SendMessage(GlobalConstants.GeneralChatGroup, "You are not shackled.", EnumChatType.OwnMessage);
        }
    }
}