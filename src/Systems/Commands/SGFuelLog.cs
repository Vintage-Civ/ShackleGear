using ShackleGear.Datasource;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace ShackleGear.Commands
{
    class SGFuelLog
    {
        ICoreServerAPI sapi;

        public SGFuelLog(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        ShackleGearTracker Tracker { get => sapi.ModLoader.GetModSystem<ShackleGearTracker>(); }

        public void Handler(IServerPlayer player, int groupid, CmdArgs args)
        {
            var data = Tracker.GetTrackData(player.PlayerUID);
            if (data != null)
            {
                string uid = data.LastFuelerUID;
                if (uid != null) player.SendMessage(GlobalConstants.GeneralChatGroup, "The player who last fueled your shackle is: " + sapi.World.PlayerByUid(uid).PlayerName + " With the UID of: " + uid, EnumChatType.OwnMessage);
            }
            else
            {
                player.SendMessage(GlobalConstants.GeneralChatGroup, "No log exists, unshackled?", EnumChatType.OwnMessage);
            }
        }
    }
}
