using NoteTweaks.Patches;
using Zenject;

namespace NoteTweaks.Installers
{
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
           // Container.BindInterfacesTo<CutSoundPatch>().AsSingle();
        }
    }
}