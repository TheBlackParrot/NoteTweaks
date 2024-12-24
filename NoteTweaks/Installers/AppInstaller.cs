using NoteTweaks.Configuration;
using NoteTweaks.Patches;
using Zenject;

namespace NoteTweaks.Installers
{
    internal class AppInstaller : Installer
    {
        private readonly PluginConfig _config;

        private AppInstaller(PluginConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
        }
    }
}