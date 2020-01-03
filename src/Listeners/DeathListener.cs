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
#if DEBUG
                sapi.Server.Logger.Debug(
                    "[SHACKLE-GEAR] DEATH EVENT FIRED BY " +
                    damagesource.SourceEntity.GetName());
#endif
                if (damagesource.SourceEntity is EntityPlayer)
                {
                    IPlayer killer = sapi.World.PlayerByUid(((EntityPlayer)damagesource.SourceEntity).PlayerUID);

                    IInventory killer_hotbar = killer.InventoryManager.GetHotbarInventory();
                    if (killer_hotbar.Any(s =>
                    {
                        if (s?.Itemstack?.Item is ItemShackleGear && (s?.Itemstack.Attributes.GetString("pearled_uid") == null))
                        {
                            prsn.ImprisonPlayer(byplayer, (IServerPlayer)killer, s);
#if DEBUG
                            sapi.Server.Logger.Debug("[SHACKLE-GEAR] Person pearled using Item: " + s.Itemstack.GetName() + "And Attr was " + s.Itemstack.Attributes.GetString("pearled_uid"));
#endif
                            return true;
                        }
                        return false;
                    }))
#if DEBUG
                        sapi.Server.Logger.Debug("[SHACKLE-GEAR] No Empty pearl found")
#endif
#pragma warning disable CS0642
                        ;
#pragma warning restore CS0642
                }
#if DEBUG
                else sapi.Server.Logger.Debug("[SHACKLE-GEAR] DEATH EVENT WAS NOT PLAYER");
#endif
            }
#if DEBUG
            else sapi.Server.Logger.Debug("[SHACKLE-GEAR] Playerkill but without player source");
#endif
        }

        public DeathListener(ICoreServerAPI _api, PrisonController _prsn)
        {
            this.sapi = _api;
            this.prsn = _prsn;
        }
    }
}