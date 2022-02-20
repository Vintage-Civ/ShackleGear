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
        public Dictionary<string, TrackData> TrackedByUID { get; set; } = new Dictionary<string, TrackData>();

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
            TrackedByUID[item.PrisonerUID] = item;
            SaveTrackToDB();
        }

        public FullTrackData GetTrackData(string prisoneruid)
        {
            TrackedByUID.TryGetValue(prisoneruid, out TrackData trackData);
            if (trackData == null) return null;

            return new FullTrackData(trackData, sapi);
        }

        public bool IsShackled(IServerPlayer player)
        {
            return TrackedByUID.ContainsKey(player.PlayerUID);
        }

        public bool TryRemoveItemFromTrack(IServerPlayer prisoner)
        {
            FullTrackData fulltrackeditem = GetTrackData(prisoner.PlayerUID);
            if (fulltrackeditem != null)
            {
                fulltrackeditem.MarkUnloadable();
                TrackedByUID.Remove(prisoner.PlayerUID);
            }

            SaveTrackToDB();
            return fulltrackeditem != null;
        }

        public void LoadTrackFromDB()
        {
            byte[] data = sapi.WorldManager.SaveGame.GetData("shacklegear_trackdata");
            if (data != null)
            {
                var Tracked = JsonUtil.FromBytes<List<TrackData>>(data);
                foreach (var val in Tracked)
                {
                    TrackedByUID[val.PrisonerUID] = val;
                }
            }
            else SaveTrackToDB();
        }

        public void SaveTrackToDB()
        {
            sapi.WorldManager.SaveGame.StoreData("shacklegear_trackdata", JsonUtil.ToBytes(TrackedByUID.Values.ToList()));
        }
    }
}