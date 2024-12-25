using NoteTweaks.Configuration;
using Zenject;

namespace NoteTweaks.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
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