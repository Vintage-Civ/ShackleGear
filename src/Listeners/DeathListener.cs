using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;

namespace VSModLauncher.Listeners {
    public class DeathListener {
        private ICoreServerAPI sapi;
        private PrisonController prsn;
        public void OnPlayerDeath(IServerPlayer byplayer, DamageSource damagesource) {
            if (damagesource != null && damagesource.Source == EnumDamageSource.Entity) {
                sapi.Server.Logger.Debug(
                    "[SHACKLE-GEAR] DEATH EVENT FIRED BY " + 
                    damagesource.SourceEntity.GetName());
                
                if (damagesource.SourceEntity is EntityPlayer)
                {
                    //do something when someone is killed
                    double x = damagesource.GetSourcePosition().X;
                    double y = damagesource.GetSourcePosition().Y;
                    double z = damagesource.GetSourcePosition().Z;
                
                    IPlayer killer = sapi.World.NearestPlayer(x, y, z);

                    IInventory killer_hotbar = killer.InventoryManager.GetHotbarInventory();
                
                    for (int i = 0; i > killer_hotbar.Count-1; i++) {
                        if (killer_hotbar[i].Itemstack.GetName() == "Shackle-Gear" && 
                            killer_hotbar[i].Itemstack.Attributes.GetString("pearled_uid") == null) {
                            //imprison player
                            sapi.Server.Logger.Debug(
                                "[SHACKLE-GEAR] Person pearled using Item: " + 
                                killer_hotbar[i].Itemstack.GetName() + 
                                "And Attr was " + 
                                killer_hotbar[i].Itemstack.Attributes.GetString("pearled_uid"));
                        
                            prsn.ImprisonPlayer(byplayer, killer_hotbar[i]);

                            return;
                        }
                    }

                    sapi.Server.Logger.Debug(
                        "[SHACKLE-GEAR] No Empty pearl found");
                }
                else
                {
                    sapi.Server.Logger.Debug(
                        "[SHACKLE-GEAR] DEATH EVENT WAS NOT PLAYER");
                }
            }
            else
            {
                sapi.Server.Logger.Debug(
                    "[SHACKLE-GEAR] Playerkill but without player source");
            }
        }

        public DeathListener(ICoreServerAPI _api, PrisonController _prsn) {
            this.sapi = _api;
            this.prsn = _prsn;
        }
    }
}