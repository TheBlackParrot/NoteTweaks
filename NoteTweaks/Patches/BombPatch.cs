using HarmonyLib;
using NoteTweaks.Managers;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch(typeof(BombNoteController), "Init")]
    [HarmonyAfter("aeroluna.Chroma")]
    internal class BombPatch
    {
        private static readonly int Color0 = Shader.PropertyToID("_SimpleColor");
        
        internal static void Postfix(BombNoteController __instance)
        {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks._autoDisable)
            {
                return;
            }
            
            float colorScale = 1.0f + Plugin.Config.BombColorBoost;
            Color bombColor = Plugin.Config.BombColor * colorScale;
            
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