using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.BlockEntityBehaviors;
using VSModLauncher.Controllers;
using VSModLauncher.Items;
using VSModLauncher.Listeners;

namespace VSModLauncher
{
    public class ModSystemShackleGear : ModSystem
    {
        public PrisonController Prsn { get; private set; }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            //start both client and server side
            api.RegisterItemClass("shackleitem", typeof(ItemShackleGear));
            api.RegisterBlockEntityBehaviorClass("gearfinder", typeof(BEBehaviorGearFinder));
            api.RegisterEntityBehaviorClass("gearfinder", typeof(EntityBehaviorGearFinder));
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            //Only start serverside
            Prsn = new PrisonController(api);
            DeathListener death = new DeathListener(api, Prsn);
            LogoutListener logout = new LogoutListener(Prsn, api);
            DespawnListener despawn = new DespawnListener(Prsn, api);
            CommandCreator cc = new CommandCreator(Prsn, api);

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