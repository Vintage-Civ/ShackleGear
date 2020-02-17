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
            bool found = false;
            TrackData trackeditem = null;

            foreach (var val in Tracked)
            {
                if (val.Prisoner.PlayerUID == prisoner.PlayerUID)
                {
                    trackeditem = val;
                    found = true;
                    break;
                }
            }
            if (found) Tracked.Remove(trackeditem);
            SaveTrackToDB();
            return found;
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