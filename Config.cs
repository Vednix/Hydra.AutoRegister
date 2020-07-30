using Hydra.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TShockAPI.Hooks;
using static Hydra.Config;

namespace Hydra.AutoRegister
{
    public class Config
    {
        public bool EnableAutoRegister = true;
        public bool AnnounceNewAccount = true;
        public bool SelectRegisterGroupAccordingGender = false;
        public string DefaultGroup = "default";
        public string FemaleDefaultGroup = "default-female";
        //public string WelcomeMessageNewPlayerDefault = "Olá! Bem Vindo!!";
        //public string WelcomeMessageNewPlayerPortuguese = "Olá! Bem Vindo!!";
        //public string WelcomeMessageNewPlayerSpanish = "Olá! Bem Vindo!!";
        //public string WelcomeMessageNewPlayerEnglishIFNotDefault = "Olá! Bem Vindo!!";
        public static void OnReloadEvent(ReloadEventArgs args)
        {
            Read();
        }
        public static void OnPluginInitialize(EventArgs args)
        {
            Read();
        }
        public static bool Read()
        {
            bool Return = false;
            try
            {
                if (!Directory.Exists(Base.SavePath))
                    Directory.CreateDirectory(Base.SavePath);
                string filepath = Path.Combine(Base.SavePath, "AutoRegister.json");

                Config config = new Config();

                if (File.Exists(filepath))
                    config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(filepath));
                File.WriteAllText(filepath, JsonConvert.SerializeObject(config, Formatting.Indented));

                AutoRegister.PConfig = config;

                Logger.doLog("[Hydra.AutoRegister] Configuration has been loaded successfully!", DebugLevel.Info);
            }
            catch (Exception e)
            {
                AutoRegister.PConfig = new Config();
                Logger.doLog($"[Hydra.AutoRegister] There was an error loading the configuration file, using default configuration. => {e.Message}", DebugLevel.Critical);
                Return = false;
            }
            return Return;
        }
    }
}
