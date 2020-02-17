using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Commands
{
    public class SGXFree
    {
        private ICoreServerAPI sapi;
        
        public SGXFree(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }
        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {
        }
    }
}