using System.Reflection;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using NoteTweaks.Configuration;
using NoteTweaks.Installers;
using SiraUtil.Zenject;
using IPALogger = IPA.Logging.Logger;

namespace NoteTweaks
{
    [Plugin(RuntimeOptions.DynamicInit)]
    [NoEnableDisable]
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Plugin
    {
        internal static IPALogger Log { get; private set; }
        internal static PluginConfig Config;
        private static Harmony _harmony;

        [Init]
        public Plugin(IPALogger logger, Config config, Zenjector zenjector)
        {
            Log = logger;
            Config = config.Generated<PluginConfig>();
            
            zenjector.Install<AppInstaller>(Location.App, Config);
            zenjector.Install<MenuInstaller>(Location.Menu);
            zenjector.Install<GameInstaller>(Location.Player);
        }

        [OnEnable]
        public void OnEnable()
        {
            _harmony = new Harmony("TheBlackParrot.NoteTweaks");
            _harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        [OnDisable]
        public void OnDisable()
        {
            _harmony.UnpatchSelf();
        }
    }
}