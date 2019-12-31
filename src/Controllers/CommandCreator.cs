using Vintagestory.API.Server;
using VSModLauncher.Commands;

namespace VSModLauncher.Controllers
{
    public class CommandCreator
    {
        private PrisonController psrn = null;
        private ICoreServerAPI sapi = null;
        private ppDebug ppDebug = null;
        private ppFree ppFree = null;
        private ppGetUID ppGetUID = null;
        private ppLocate ppLocate = null;
        private ppWho ppWho = null;
        private ppxFree ppxFree = null;
        private ppxPearl ppxPearl = null;


        //method to create the command objects
        public void CreateCommandObjects()
        {
            this.ppDebug = new ppDebug();
            this.ppGetUID = new ppGetUID(sapi);
            this.ppFree = new ppFree(psrn);
            this.ppLocate = new ppLocate();
            this.ppWho = new ppWho();
            this.ppxFree = new ppxFree(sapi);
            this.ppxPearl = new ppxPearl(sapi);
        }

        //method to actually register them
        public void RegisterCommandObjects(ICoreServerAPI api)
        {
            api.RegisterCommand(
                "ppfree",
                "Frees a pearled player if you're holding a prisonpearl",
                "/ppfree",
                this.ppFree.Handler
            );
            
            api.RegisterCommand(
                "ppgetuid",
                "Displays thePlayerUID for a given name",
                "/ppgetuid name",
                this.ppGetUID.Handler
            );

            api.RegisterCommand(
                "pplocate",
                "Displays the location of your prisonpearl, if you're pearled",
                "/pplocate",
                this.ppLocate.Handler
            );

            api.RegisterCommand(
                "ppwho",
                "Displays the name of the player in a held pearl",
                "/ppwho",
                this.ppWho.Handler
            );

            api.RegisterCommand(
                "ppdebug",
                "Debug command for shackle-gear",
                "/ppdebug",
                this.ppDebug.Handler
            );

            api.RegisterCommand(
                "ppxfree",
                "Frees a pearled player",
                "/ppxfree",
                this.ppxFree.Handler,
                Privilege.ban
            );

            api.RegisterCommand(
                "ppxpearl",
                "Imprisons a player and gives you the pearl",
                "/ppxPearl",
                this.ppxPearl.Handler,
                Privilege.ban
            );
        }

        public CommandCreator(PrisonController _psrn, ICoreServerAPI _sapi)
        {
            this.psrn = _psrn;
            this.sapi = _sapi;
        }
    }
}