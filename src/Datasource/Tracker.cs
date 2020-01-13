using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;
using System.Linq;
using Vintagestory.API.Util;
using Newtonsoft.Json;

namespace VSModLauncher.Datasource
{
    public class ShackleGearTracker : ModSystem
    {
        public List<TrackData> Tracked { get; set; } = new List<TrackData>();

        ICoreServerAPI sapi;

        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;

        public override void StartServerSide(ICoreServerAPI api)
        {
            sapi = api;
            LoadTrackFromDB();
            api.Event.ServerRunPhase(EnumServerRunPhase.Shutdown, () => SaveTrackToDB());
        }

        public void AddItemToTrack(TrackData item)
        {
            Tracked.Add(item);
            SaveTrackToDB();
        }

        public TrackData GetTrackData(string prisoneruid)
        {
            foreach (var val in Tracked)
            {
                if (val.Prisoner.PlayerUID == prisoneruid)
                {
                    return val;
                }
            }
            return null;
        }

        public bool RemoveItemFromTrack(IServerPlayer prisoner)
        {
            bool removed_element = false;
            foreach (var tracked_item in Tracked)
            {
                if (tracked_item.Prisoner.PlayerUID == prisoner.PlayerUID)
                {
                    Tracked.Remove(tracked_item);
                    removed_element = true;
                }
            }
            SaveTrackToDB();
            return removed_element;
        }

        public void LoadTrackFromDB()
        {
            byte[] data = sapi.WorldManager.SaveGame.GetData("shacklegear_trackdata");
            if (data != null)
            {
                Tracked = JsonUtil.FromBytes<List<TrackData>>(data);
            }
            else SaveTrackToDB();
        }

        public void SaveTrackToDB()
        {
            sapi.WorldManager.SaveGame.StoreData("shacklegear_trackdata", JsonUtil.ToBytes(Tracked));
        }


        public ShackleGearTracker()
        {
            Tracked = new List<TrackData>();
        }
    }
}