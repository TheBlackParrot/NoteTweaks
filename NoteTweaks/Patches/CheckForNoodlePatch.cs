using System.Linq;
using HarmonyLib;
using SongCore.Data;
using static IPA.Loader.PluginManager;
using static SongCore.Collections;

namespace NoteTweaks.Patches
{
    [HarmonyPatch]
    internal class CheckForNoodlePatch
    {
        private static bool MapHasNoodle(IDifficultyBeatmap beatmapLevel)
        {
            bool hasNoodle = false;
            
            ExtraSongData.DifficultyData diffData = RetrieveDifficultyData(beatmapLevel);
            if (diffData != null)
            {
                string[] requirements = diffData.additionalDifficultyData._requirements;
                hasNoodle = requirements.Any(x => x == "Noodle Extensions");
            }
            return EnabledPlugins.Any(x => x.Name == "NoodleExtensions") && hasNoodle;
        }
        
        [HarmonyPatch(typeof(StandardLevelDetailView), "RefreshContent")]
        private static void Postfix(StandardLevelDetailView __instance)
        {
            NotePhysicalTweaks.AutoDisable = MapHasNoodle(__instance.selectedDifficultyBeatmap) && Plugin.Config.DisableIfNoodle;
        }
    }
}