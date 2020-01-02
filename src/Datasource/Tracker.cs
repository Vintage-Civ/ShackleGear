using System.Collections.Generic;
using Vintagestory.API.Server;
using VSModLauncher.Controllers;

namespace VSModLauncher.Datasource
{
    public class Tracker
    {
        private static Tracker instance = null;
        private List<TrackData> tracked;

        public static Tracker GetInstance()
        {
            return (instance == null) ? (instance = new Tracker()) : instance;
        }
        
        public void InitTracker()
        {
            
        }

        public void AddItemToTrack(TrackData item)
        {
            tracked.Add(item);
        }

        public bool RemoveItemFromTrack(IServerPlayer prisoner)
        {
            bool removed_element = false;
            foreach (var tracked_item in tracked)
            {
                if (tracked_item.Prisoner.PlayerUID == prisoner.PlayerUID)
                {
                    tracked.Remove(tracked_item);
                    removed_element = true;
                }
            }
            return removed_element;
        }

        public void SaveTrackToDB()
        {
            //Do something to save to db
        }


        public Tracker()
        {
            tracked = new List<TrackData>();
        }
    }
}