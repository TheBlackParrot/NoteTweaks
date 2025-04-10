﻿using IPA.Loader;
using NoteTweaks.Configuration;
using NoteTweaks.UI;
using Zenject;

namespace NoteTweaks.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.Bind<SettingsViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<NotePreviewViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<ExtraPanelViewController>().FromNewComponentAsViewController().AsSingle();
            Container.Bind<UI.SettingsFlowCoordinator>().FromNewComponentOnNewGameObject().AsSingle();
            Container.BindInterfacesAndSelfTo<ModSettingsViewController>().AsSingle();
            Container.BindInterfacesTo<MenuButtonManager>().AsSingle();
            
            if (PluginManager.GetPlugin("Heck") != null)
            {
                Container.BindInterfacesAndSelfTo<NoteTweaksSettableSettings>().AsSingle().NonLazy();
            }
        }
    }
}