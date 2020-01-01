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
            if (damagesource != null && damagesource.Source == EnumDamageSource.Entity)
            {
                sapi.Server.Logger.Debug(
                    "[SHACKLE-GEAR] DEATH EVENT FIRED BY " +
                    damagesource.SourceEntity.GetName());

                if (damagesource.SourceEntity is EntityPlayer)
                {
                    IPlayer killer = sapi.World.PlayerByUid(((EntityPlayer)damagesource.SourceEntity).PlayerUID);

                    IInventory killer_hotbar = killer.InventoryManager.GetHotbarInventory();
                    if (killer_hotbar.Any(s =>
                    {
                        if (s?.Itemstack?.Item is ItemShackleGear && (s?.Itemstack.Attributes.GetString("pearled_uid") == null))
                        {
                            prsn.ImprisonPlayer(byplayer, s);
                            sapi.Server.Logger.Debug("[SHACKLE-GEAR] Person pearled using Item: " + s.Itemstack.GetName() + "And Attr was " + s.Itemstack.Attributes.GetString("pearled_uid"));
                            return true;
                        }
                        return false;
                    })) sapi.Server.Logger.Debug("[SHACKLE-GEAR] No Empty pearl found");
                }
                else sapi.Server.Logger.Debug("[SHACKLE-GEAR] DEATH EVENT WAS NOT PLAYER");
            }
            else sapi.Server.Logger.Debug("[SHACKLE-GEAR] Playerkill but without player source");
        }

        public DeathListener(ICoreServerAPI _api, PrisonController _prsn)
        {
            this.sapi = _api;
            this.prsn = _prsn;
        }
    }
}