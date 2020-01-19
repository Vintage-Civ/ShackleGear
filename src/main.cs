using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using VSModLauncher.BlockEntityBehaviors;
using VSModLauncher.Controllers;
using VSModLauncher.Datasource;
using VSModLauncher.Items;
using VSModLauncher.Listeners;

namespace VSModLauncher
{
    public class ModSystemShackleGear : ModSystem
    {
        public PrisonController Prsn { get; private set; }
        ICoreAPI Api;
        ShackleGearTracker Tracker { get => Api.ModLoader.GetModSystem<ShackleGearTracker>(); }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            Api = api;

            //start both client and server side
            api.RegisterItemClass("shackleitem", typeof(ItemShackleGear));
            api.RegisterBlockEntityBehaviorClass("gearfinder", typeof(BEBehaviorGearFinder));
            api.RegisterEntityBehaviorClass("gearfinder", typeof(EntityBehaviorGearFinder));
        }

        long id;
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

            api.Event.PlayerJoin += player =>
            {
                TrackData data = Tracker.GetTrackData(player.PlayerUID);
                BlockPos pos = data?.lastPos;
                if (pos != null)
                {
                    //update transition states until player is set free
                    id = api.Event.RegisterGameTickListener(dt =>
                    {
                        bool wasunloaded = data.TryLoadChunk();

                        if (data != null) (data.ItemStack.Item as ItemShackleGear)?.UpdateAndGetTransitionStates(api.World, data.Slot);
                        else api.Event.UnregisterGameTickListener(id);

                        if (wasunloaded) data.TryUnloadChunk();
                    }, 500);
                }
            };

            //lets get the commands set up
            cc.CreateCommandObjects();
            cc.RegisterCommandObjects(api);
        }
    }
}