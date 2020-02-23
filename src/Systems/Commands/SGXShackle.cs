using ShackleGear.Controllers;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using System.Linq;
using ShackleGear.Items;
using System;

namespace ShackleGear.Commands
{
    public class SGXShackle
    {
        PrisonController Prison;

        public SGXShackle(PrisonController prison)
        {
            Prison = prison;
        }

        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {
            int i = 0;
            try
            {
                string playername = args.PopWord();
                if (playername != null)
                {
                    i = 28;
                    IServerPlayer prisoner = null;
                    ItemSlot slot = player.InventoryManager?.ActiveHotbarSlot;

                    foreach (var val in player.Entity.World.AllPlayers)
                    {
                        i = 34;
                        if (val.PlayerName == playername)
                        {
                            i = 37;
                            prisoner = val as IServerPlayer;
                            break;
                        }
                    }
                    if (prisoner?.PlayerUID != null)
                    {
                        i = 44;
                        if (slot?.Itemstack?.Item is ItemShackleGear)
                        {
                            i = 47;
                            if (Prison.TryImprisonPlayer(prisoner, player, slot))
                            {
                                i = 50;
                                player.SendMessage(GlobalConstants.GeneralChatGroup, "Player " + prisoner.PlayerName + " Shackled.", EnumChatType.OwnMessage);
                            }
                            else
                            {
                                player.SendMessage(GlobalConstants.GeneralChatGroup, "Not holding a ShackleGear.", EnumChatType.OwnMessage);
                            }
                        }
                    }
                    else
                    {
                        player.SendMessage(GlobalConstants.GeneralChatGroup, "Player \"" + playername + "\" does not exist.", EnumChatType.OwnMessage);
                    }
                }
                else
                {
                    player.SendMessage(GlobalConstants.GeneralChatGroup, "Please provide a valid player name.", EnumChatType.OwnMessage);
                }
            }
            catch (Exception ex)
            {
                player.Entity.World.Logger.Debug("[ShackleGear] Exception thrown after: " + i);
                player.Entity.World.Logger.Debug("[ShackleGear] Ex: " + ex);
            }

        }
    }
}