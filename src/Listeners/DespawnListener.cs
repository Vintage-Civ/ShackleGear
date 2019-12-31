using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;

namespace VSModLauncher.Listeners
{
    public class DespawnListener
    {
        private PrisonController prsn = null;
        private ICoreServerAPI sapi = null;
        
        public void EventOnOnEntityDespawn(Entity entity, EntityDespawnReason reason)
        {
            //sapi.Server.Logger.Debug("[SHACKLE-GEAR] DESPAWN EVENT FIRED\n");
        }


        public DespawnListener(PrisonController _prsn, ICoreServerAPI _sapi)
        {
            this.prsn = _prsn;
            this.sapi = _sapi;
        }
    }
}