using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;
using System.Linq;
using VSModLauncher.Items;

namespace VSModLauncher.Listeners
{
    public class DeathListener
    {
        private ICoreServerAPI sapi;
        private PrisonController prsn;
        public void OnPlayerDeath(IServerPlayer byplayer, DamageSource damagesource)
        {
            sapi.World.Logger.Notification("[SHACKLEGEAR] Event Fired.");
            if (damagesource?.SourceEntity is EntityPlayer)
            {
                sapi.World.Logger.Notification("[SHACKLEGEAR] Was EntityPlayer.");
                IPlayer killer = sapi.World.PlayerByUid(((EntityPlayer)damagesource.SourceEntity).PlayerUID);
                killer.Entity.WalkInventory(slot =>
                {
                    if (slot?.Itemstack?.Item is ItemShackleGear && (slot?.Itemstack.Attributes.GetString("pearled_uid") == null))
                    {
                        prsn.ImprisonPlayer(byplayer, (IServerPlayer)killer, slot);
                        sapi.World.Logger.Notification("[SHACKLEGEAR] Gear Found.");
                    }
                    return false;
                });
            }
        }

        public DeathListener(ICoreServerAPI _api, PrisonController _prsn)
        {
            this.sapi = _api;
            this.prsn = _prsn;
        }
    }
}