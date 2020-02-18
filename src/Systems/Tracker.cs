using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using ShackleGear.Controllers;
using System.Linq;
using Vintagestory.API.Util;
using Newtonsoft.Json;

namespace ShackleGear.Datasource
{
    public class ShackleGearTracker : ModSystem
    {
        public List<FullTrackData> FullTracked { get; set; } = new List<FullTrackData>();
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
            FullTracked.Add(new FullTrackData(item, sapi));

            SaveTrackToDB();
        }

        public FullTrackData GetTrackData(string prisoneruid)
        {
            foreach (var val in FullTracked)
            {
                if (val.trackData?.PrisonerUID == prisoneruid)
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
            FullTrackData fulltrackeditem = null;

            foreach (var val in Tracked)
            {
                if (val.PrisonerUID == prisoner.PlayerUID)
                {
                    trackeditem = val;
                    found = true;
                    break;
                }
            }
            if (found) Tracked.Remove(trackeditem);
            found = false;

            foreach (var val in FullTracked)
            {
                if (val.trackData?.PrisonerUID == null || val.trackData?.PrisonerUID == prisoner.PlayerUID)
                {
                    fulltrackeditem = val;
                    found = true;
                    break;
                }
            }
            if (found) FullTracked.Remove(fulltrackeditem);

            SaveTrackToDB();
            return found;
        }

        public void LoadTrackFromDB()
        {
            byte[] data = sapi.WorldManager.SaveGame.GetData("shacklegear_trackdata");
            if (data != null)
            {
                Tracked = JsonUtil.FromBytes<List<TrackData>>(data);
                foreach (var val in Tracked)
                {
                    FullTracked.Add(new FullTrackData(val, sapi));
                }
            }
            else SaveTrackToDB();
        }

        public void SaveTrackToDB()
        {
            sapi.WorldManager.SaveGame.StoreData("shacklegear_trackdata", JsonUtil.ToBytes(Tracked));
        }
    }
}