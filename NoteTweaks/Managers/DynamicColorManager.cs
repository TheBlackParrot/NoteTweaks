using NoteTweaks.Configuration;
using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class RainbowGradient
    {
        private static PluginConfig Config => PluginConfig.Instance;
        private static float Time => UnityEngine.Time.time;
        public static Color Color => Color.HSVToRGB((Time / Config.RainbowBombTimeScale * 3.0f) % 1.0f, Config.RainbowBombSaturation, Config.RainbowBombValue) * (1.0f + Config.BombColorBoost);
    }
}