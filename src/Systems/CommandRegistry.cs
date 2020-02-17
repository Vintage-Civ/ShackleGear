using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Commands;

namespace VSModLauncher.Controllers
{
    public class CommandRegistry : ModSystem
    {
        public override void StartServerSide(ICoreServerAPI api)
        {
            api.RegisterCommand("sgfree", "Frees a shackled player if you're holding a shackle", "/sgfree", new SGFree(api).Handler);
            api.RegisterCommand("sggetuid", "Displays the PlayerUID for a given name", "/sggetuid name", new SGGetUID(api).Handler );
            api.RegisterCommand("sglocate", "Displays the location of your shackle gear, if you're shackled", "/sglocate", new SGLocate().Handler);
            api.RegisterCommand("sgwho", "Displays the name of the player in a held shackle", "/sgwho", new SGWho().Handler);
            api.RegisterCommand("sgdebug", "Debug command for shackle-gear", "/sgdebug", new SGDebug().Handler);
            api.RegisterCommand("sgxfree", "Frees a shackled player", "/sgxfree", new SGXFree(api).Handler, Privilege.ban);
            api.RegisterCommand("sgxpearl", "Imprisons a player and gives you the shackle", "/sgxPearl", new SGXPearl(api).Handler, Privilege.ban);
        }
    }
}