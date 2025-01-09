using HarmonyLib;
using NoteTweaks.Managers;
using UnityEngine;

namespace NoteTweaks.Patches
{
    [HarmonyPatch(typeof(BombNoteController), "Init")]
    internal class BombPatch
    {
        private static readonly int Color0 = Shader.PropertyToID("_SimpleColor");
        
        internal static void Postfix(BombNoteController __instance)
        {
            if (!Plugin.Config.Enabled || NotePhysicalTweaks._autoDisable)
            {
                return;
            }
            
            float scale = 1.0f + Plugin.Config.BombColorBoost;
            Color bombColor = Plugin.Config.BombColor * scale;
            
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
    }
}