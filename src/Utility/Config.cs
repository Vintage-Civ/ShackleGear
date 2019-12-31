namespace VSModLauncher.Utility {
    public class Config {
        //TODO ADD CONFIGURING TO THE PLUGIN WHEN CORE FUNCTIONALITY EXISTS
        
        public static Config Load() {
            //check if there a config file, if not, create default, if there is, load it
            return new Config();
        }

        public void SaveDefault() {
            //save a default config
        }
    }
}