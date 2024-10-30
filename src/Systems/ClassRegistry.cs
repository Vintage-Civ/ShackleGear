using Vintagestory.API.Common;
using Vintagestory.API.Server;
using ShackleGear.Commands;
using ShackleGear.Items;
using ShackleGear.BlockEntityBehaviors;
using ShackleGear.EntityBehaviors;
using Vintagestory.API.Config;

namespace ShackleGear
{
    public partial class ModSystemShackleGear
    {
        public void RegisterClasses(ICoreAPI api)
        {
            api.RegisterItemClass("shackleitem", typeof(ItemShackleGear));
            api.RegisterEntityBehaviorClass("gearfinder", typeof(EntityBehaviorGearFinder));
        }

        public void RegisterServerCommands(ICoreServerAPI api)
        {
            var parsers = api.ChatCommands.Parsers;

            api.ChatCommands
                .GetOrCreate("sg")
                .BeginSubCommand("free")
                    .WithDescription("Frees a shackled player if you're holding a shackle.")
                    .HandleWith(SGFree)
                .EndSubCommand();
                /*.BeginSubCommand("getuid")
                    .WithDescription("Displays the PlayerUID for a given name")
                    .WithArgs(parsers.OnlinePlayer)*/

            //api.RegisterCommand("sgfree", "Frees a shackled player if you're holding a shackle", "/sgfree", new SGFree(api).Handler);
            api.RegisterCommand("sggetuid", "Displays the PlayerUID for a given name", "/sggetuid name", new SGGetUID(api).Handler );
            api.RegisterCommand("sglocate", "Displays the location of your shackle gear, if you're shackled", "/sglocate", new SGLocate(Prison).Handler);
            api.RegisterCommand("sgwho", "Displays the name of the player in a held shackle", "/sgwho", new SGWho().Handler);
            api.RegisterCommand("sgdebug", "Debug command for shackle-gear", "/sgdebug", new SGDebug().Handler);
            api.RegisterCommand("sgfuellog", "Shows who last fueled your shackle-gear", "/sgfuellog", new SGFuelLog(api).Handler);
            api.RegisterCommand("sgxfree", "Frees a shackled player", "/sgxfree", new SGXFree(Prison, Tracker).Handler, Privilege.ban);
            api.RegisterCommand("sgxshackle", "Imprisons a player.", "/sgxPearl", new SGXShackle(Prison).Handler, Privilege.ban);
        }

        public TextCommandResult SGFree(TextCommandCallingArgs args)
        {
            var player = args.Caller.Player as IServerPlayer;

            if (player.InventoryManager.ActiveHotbarSlot?.Itemstack?.Item is ItemShackleGear)
            {
                Prison.FreePlayer(player.InventoryManager.ActiveHotbarSlot.Itemstack.Attributes.GetString("shackled_uid"), player.InventoryManager.ActiveHotbarSlot);
                return TextCommandResult.Success("You've freed a shackle.");
            }
            return TextCommandResult.Error("The need to shackle someone first.");
        }

        /*
        public TextCommandResult SGGetUID(TextCommandCallingArgs args)
        {
            args[0]
            foreach (var player in sapi.Server.Players)
            {
                if (player.PlayerName == playername)
                {
                    byPlayer.SendMessage(GlobalConstants.GeneralChatGroup, "PlayerUID: " + player.PlayerUID, EnumChatType.Notification);
                    return;
                }
            }
        }
        */
    }
}