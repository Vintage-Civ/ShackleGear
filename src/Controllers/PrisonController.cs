using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSModLauncher.Controllers {
    public class PrisonController {
        private ICoreServerAPI sapi;
        public bool FreePlayer(string uid, ItemSlot shackegear_slot) {
            
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] Free Function Fired");
            foreach (var serverPlayer in sapi.Server.Players) {
                if (serverPlayer.PlayerUID == uid) {
                    //do the things required to free a player.
                    sapi.Permissions.SetRole(serverPlayer, "suplayer");
                    serverPlayer.SendMessage(
                        GlobalConstants.GeneralChatGroup,
                        "You've been freed!",
                        EnumChatType.Notification);

                    shackegear_slot.Itemstack.Item.Durability = 0;
                    shackegear_slot.MarkDirty();
                    
                    return true;
                }
            }
            return false;
        }

        public void ImprisonPlayer(IServerPlayer player, ItemSlot shacklegear_slot) {
            //imprison some player
            sapi.Server.Logger.Debug("[SHACKLE-GEAR] Imprison Function Fired");
            sapi.Permissions.SetRole(player, "suvisitor");
            shacklegear_slot.Itemstack.Attributes.SetString("pearled_uid", player.PlayerUID);
            shacklegear_slot.Itemstack.Attributes.SetString("pearled_name", player.PlayerName);
            
            player.SendMessage(
                GlobalConstants.GeneralChatGroup,
                "You've been pearled!",
                EnumChatType.Notification);
            
            shacklegear_slot.MarkDirty();
        }

        public PrisonController(ICoreServerAPI _api) {
            sapi = _api;
        }
    }
}