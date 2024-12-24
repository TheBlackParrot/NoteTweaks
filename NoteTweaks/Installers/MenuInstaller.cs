using NoteTweaks.Patches;
using NoteTweaks.UI;
using Zenject;

namespace NoteTweaks.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<SettingsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NotePreviewViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<UI.SettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
        }
    }
}