using UnityEngine;

namespace NoteTweaks.Utils
{
    // https://github.com/TheBlackParrot/NoteTweaks/issues/4
    public static class Easing
    {
        public static float OutCirc(float t)
        {
            return (float)System.Math.Sqrt(1 - (t - 1) * (t - 1));
        }

        public static float OutExpo(float t)
        {
            return !Mathf.Approximately(t, 1) ? 1f - (float)System.Math.Pow(2, -10 * t) : 1f;
        }

        public static float InOutCubic(float t)
        {
            return t >= 0.5f ? (float)(1 - System.Math.Pow(-2 * t + 2, 3) / 2) : 4f * t * t * t;
        }
    }
}