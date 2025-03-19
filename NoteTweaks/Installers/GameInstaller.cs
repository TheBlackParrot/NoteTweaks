﻿using NoteTweaks.Patches;
using Zenject;

namespace NoteTweaks.Installers
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class GameInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<BeatEffectSpawnerPatch>().AsSingle();
        }
    }
}