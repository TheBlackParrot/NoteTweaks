/*using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;

namespace NoteTweaks.UI
{
    [ViewDefinition("NoteTweaks.UI.BSML.Empty.bsml")]
    [HotReload(RelativePathToLayout = "BSML.Empty.bsml")]
    internal class NotePreviewViewController : BSMLAutomaticViewController
    {
        public NotePreviewViewController()
        {
            if (GameObject.Find("PreviewNote"))
            {
                DestroyImmediate(GameObject.Find("PreviewNote"));
            }
            
            GameObject originalObject = GameObject.Find("NormalGameNote");
            if (originalObject)
            {
                GameObject test = Instantiate(originalObject);
                test.name = "PreviewNote";
                test.SetActive(true);
            }
        }
    }
}*/