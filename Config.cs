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

                Logger.doLogLang(DefaultMessage: $"Configuration has been loaded successfully!", Hydra.Config.DebugLevel.Info, Base.CurrentHydraLanguage, AutoRegister._name,
                                 PortugueseMessage: $"A configuração foi carregada com sucesso!",
                                 SpanishMessage: $"¡La configuración se ha cargado correctamente!");
            }
            catch (Exception ex)
            {
                AutoRegister.PConfig = new Config();
                Logger.doLogLang(DefaultMessage: $"There was a critical error loading the Hydra configuration file, using default configuration. => {ex}!", Hydra.Config.DebugLevel.Error, Base.CurrentHydraLanguage, AutoRegister._name,
                                 PortugueseMessage: $"Ocorreu um erro ao carregar o arquivo de configuração, usando configurações padrões. => {ex}",
                                 SpanishMessage: $"Se produjo un error crítico al cargar el archivo de configuración de Hydra, utilizando la configuración predeterminada. => {ex}");
                Return = false;
            }
            return Return;
        }
    }
}
