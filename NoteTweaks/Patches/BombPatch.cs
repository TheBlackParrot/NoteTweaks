using HarmonyLib;
using NoteTweaks.Managers;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch(typeof(BombNoteController), "Init")]
    internal class BombPatch
    {
        private static readonly int Color0 = Shader.PropertyToID("_SimpleColor");
        
        [HarmonyPriority(int.MaxValue)]
        // ReSharper disable once InconsistentNaming
        internal static void Postfix(BombNoteController __instance)
        {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks.AutoDisable)
            {
                return;
            }
            
            float colorScale = 1.0f + Plugin.Config.BombColorBoost;
            Color bombColor = Plugin.Config.BombColor * colorScale;
            
            // i'll figure this out one day :clueless:
            /*Type objectColorizeType = AccessTools.TypeByName("Chroma.Colorizer.BombColorizerManager");
            if (objectColorizeType == null)
            {
                if (__instance.transform.GetChild(0).TryGetComponent(out Renderer bombRenderer))
                {
                    bombRenderer.sharedMaterial = Materials.BombMaterial;
                    bombRenderer.sharedMaterial.SetColor(Color0, bombColor);
                }

                if (__instance.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
                {
                    materialPropertyBlockController.materialPropertyBlock.SetColor(Color0, bombColor);
                    materialPropertyBlockController.ApplyChanges();   
                }
            }
            else
            {
                MethodInfo colorizeMethod = AccessTools.Method(objectColorizeType, "Colorize");
                if (colorizeMethod != null)
                {
                    object colorizerObject = AccessTools.CreateInstance(objectColorizeType);
                    colorizeMethod.Invoke(colorizerObject, new object[] { __instance, bombColor } );
                }
            }*/
            
            if (__instance.transform.GetChild(0).TryGetComponent(out Renderer bombRenderer))
            {
                bombRenderer.sharedMaterial = Materials.BombMaterial;
                bombRenderer.sharedMaterial.SetColor(Color0, bombColor);
            }

            if (__instance.gameObject.TryGetComponent(out MaterialPropertyBlockController materialPropertyBlockController))
            {
                materialPropertyBlockController.materialPropertyBlock.SetColor(Color0, bombColor);
                materialPropertyBlockController.ApplyChanges();   
            }

            if (!NotePhysicalTweaks.IsAllowedToScaleNotes)
            {
                return;
            }

            Vector3 scale = Vector3.one * Plugin.Config.BombScale;
            
            __instance.transform.localScale = scale;
            __instance._cuttableBySaber.GetComponent<SphereCollider>().radius = 0.18f * (1.0f / Plugin.Config.BombScale);
        }
    }
}