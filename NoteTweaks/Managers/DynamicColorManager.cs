using UnityEngine;

namespace NoteTweaks.Managers
{
    internal abstract class RainbowGradient
    {
        private static float Time => UnityEngine.Time.time;
        public static Color Color => Color.HSVToRGB((Time / Plugin.Config.RainbowBombTimeScale * 3.0f) % 1.0f, Plugin.Config.RainbowBombSaturation, Plugin.Config.RainbowBombValue) * (1.0f + Plugin.Config.BombColorBoost);
    }
}