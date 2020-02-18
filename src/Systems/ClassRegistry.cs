using Vintagestory.API.Common;
using Vintagestory.API.Server;
using ShackleGear.Commands;
using ShackleGear.Items;
using ShackleGear.BlockEntityBehaviors;
using ShackleGear.EntityBehaviors;

namespace ShackleGear
{
    public partial class ModSystemShackleGear
    {
        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterItemClass("shackleitem", typeof(ItemShackleGear));
            api.RegisterBlockEntityBehaviorClass("gearfinder", typeof(BEBehaviorGearFinder));
            api.RegisterEntityBehaviorClass("gearfinder", typeof(EntityBehaviorGearFinder));
        }

        public void RegisterServerCommands(ICoreServerAPI api)
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