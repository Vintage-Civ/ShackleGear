using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;
using VSModLauncher.Items;
using VSModLauncher.Listeners;

namespace VSModLauncher {
    public class main : ModSystem {
        public override void Start(ICoreAPI api) {
            base.Start(api);
            //start both client and server side
            api.RegisterItemClass("shackleitem", typeof(shackleItem));
        }

        public override void StartServerSide(ICoreServerAPI api) {
            base.StartServerSide(api);
            //Only start serverside
            PrisonController prsn = new PrisonController(api);
            DeathListener death = new DeathListener(api, prsn);
            LogoutListener logout = new LogoutListener(prsn, api);
            DespawnListener despawn = new DespawnListener(prsn, api);
            CommandCreator cc = new CommandCreator(prsn, api);
            
            //Listen for players deaths and logouts
            api.Event.PlayerDeath += death.OnPlayerDeath;
            api.Event.PlayerDisconnect += logout.EventOnPlayerDisconnect;
            api.Event.OnEntityDespawn += despawn.EventOnOnEntityDespawn;
            
            //lets get the commands set up
            cc.CreateCommandObjects();
            cc.RegisterCommandObjects(api);
        }
    }
}