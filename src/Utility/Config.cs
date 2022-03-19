using Newtonsoft.Json;
using System;
using Vintagestory.API.Server;

namespace ShackleGear.Utility
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal class ShackleGearServerConfig
    {
        private ICoreServerAPI sapi;

        [JsonProperty]
        private float shackleBurnTimeMul = 60.0f;

        [JsonProperty]
        public string shackledGroup = "suvisitor";

        public ShackleGearServerConfig(ICoreServerAPI sapi)
        {
            this.sapi = sapi;
        }

        public float ShackleBurnMulRO
        {
            get => shackleBurnTimeMul;
        }

        public float ShackleBurnMul
        {
            get
            {
                Load();
                return shackleBurnTimeMul;
            }
            set
            {
                shackleBurnTimeMul = value;
                Save();
            }
        }

        public string ShackledGroup
        {
            get
            {
                Load();
                return shackledGroup;
            }
            set
            {
                shackledGroup = value;
                Save();
            }
        }

        public void Save()
        {
            sapi.StoreModConfig(this, "shacklegear/server.json");
        }

        public void Load()
        {
            try
            {
                var conf = sapi.LoadModConfig<ShackleGearServerConfig>("shacklegear/server.json") ?? new ShackleGearServerConfig(sapi);

                shackleBurnTimeMul = conf.shackleBurnTimeMul;
                shackledGroup = conf.shackledGroup;
            }
            catch (Exception ex)
            {
                sapi.Logger.Error("Malformed ModConfig file shacklegear/server.json, Exception: \n {0}", ex.StackTrace);
            }
        }
    }
}