using UnityEditor;
namespace LightBetweenCultures.Editor
{
    // Explicitly applies existing actor configuration; never slices art or rebuilds scenes.
    public static class MapNpcMarkerSetup
    {
        public static void Build()
        {
            AdventureUISetup.ApplyMapActorPresentation();
            AssetDatabase.SaveAssets();
            RelicPilotSetup.Build();
        }
    }
}
