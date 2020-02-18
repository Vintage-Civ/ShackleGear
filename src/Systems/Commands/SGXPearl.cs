using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace ShackleGear.Commands
{
    public class SGXPearl
    {
        private ICoreServerAPI sapi;

        public SGXPearl(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }
        
        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {

        }
    }
}