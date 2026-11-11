using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public static class SeoulRelicArtRepair
    {
        const string ArtPath = "Assets/_Game/Resources/AdventureUIArt.asset";
        const string RelicDataPath = "Assets/_Game/Resources/Content/CulturalRelics.json";
        const string GameplayDataPath = "Assets/_Game/Resources/Content/GameplayContent.json";
        [MenuItem("Tools/Light Between Cultures/Repair Seoul Relic Art")]
        public static void Apply()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            var art = AssetDatabase.LoadAssetAtPath<AdventureUIArt>(ArtPath);
            if (art == null) throw new InvalidDataException("AdventureUIArt resource is missing.");

            BindFragment(art, "fragment_seoul_heritage_data", "Assets/_Game/Art/Items/Collectibles/Seoul/Fragment_HeritageData.png");
            BindFragment(art, "fragment_seoul_jiwon_sync", "Assets/_Game/Art/Items/Collectibles/Seoul/Fragment_JiwonSync.png");
            BindFragment(art, "fragment_seoul_stellar_ring", "Assets/_Game/Art/Items/Collectibles/Seoul/Fragment_StellarRing.png");

            var telescope = Array.Find(art.items, entry => entry.id == "stargazer_lens");
            var starlightLens = art.Relic("relic_gyeongju_star_lens");
            if (telescope == null || starlightLens == null)
                throw new InvalidDataException("The telescope item or Gyeongju star-lens sprite is missing.");
            telescope.sprite = starlightLens;
            telescope.source = AssetDatabase.GetAssetPath(starlightLens);
            telescope.status = "NATIVE_CROP; Gyeongju starlight lens quest art";
            EditorUtility.SetDirty(art);

            ReplaceObjectField(RelicDataPath, "fragment_seoul_heritage_data", "artStatus", "ART_PENDING", "NATIVE_CROP");
            ReplaceObjectField(RelicDataPath, "fragment_seoul_jiwon_sync", "artStatus", "ART_PENDING", "NATIVE_CROP");
            ReplaceObjectField(RelicDataPath, "fragment_seoul_stellar_ring", "artStatus", "ART_PENDING", "NATIVE_CROP");
            ReplaceObjectField(GameplayDataPath, "stargazer_lens", "iconId", "glasses", "stargazer_lens");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Seoul relic art repaired: three cropped source-sheet fragments bound; telescope item now uses Gyeongju starlight-lens art.");
            EditorApplication.Exit(0);
        }

        static void BindFragment(AdventureUIArt art, string id, string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) throw new InvalidDataException("Missing fragment texture: " + path);
            if (importer.textureType != TextureImporterType.Sprite || importer.spriteImportMode != SpriteImportMode.Single || !importer.alphaIsTransparency || importer.mipmapEnabled)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) throw new InvalidDataException("Unity failed to import fragment sprite: " + path);

            var entries = (art.fragments ?? Array.Empty<AdventureImage>()).ToList();
            var entry = entries.FirstOrDefault(item => item.id == id);
            if (entry == null)
            {
                entry = new AdventureImage { id = id };
                entries.Add(entry);
            }
            entry.sprite = sprite;
            entry.portrait = sprite;
            entry.source = path;
            entry.status = "NATIVE_CROP; extracted from Seoul relay source sheet";
            entry.categoryIcon = false;
            art.fragments = entries.ToArray();
            EditorUtility.SetDirty(art);
        }

        static void ReplaceObjectField(string path, string id, string field, string oldValue, string newValue)
        {
            var text = File.ReadAllText(path);
            var pattern = "(\\{\\s*\"id\"\\s*:\\s*\"" + Regex.Escape(id) + "\"[^}]*\"" + Regex.Escape(field) + "\"\\s*:\\s*\")" + Regex.Escape(oldValue) + "(\")";
            var updated = Regex.Replace(text, pattern, "$1" + newValue + "$2", RegexOptions.Singleline);
            if (updated == text && !Regex.IsMatch(text, pattern.Replace(Regex.Escape(oldValue), Regex.Escape(newValue)), RegexOptions.Singleline))
                throw new InvalidDataException("Could not update " + field + " for " + id + " in " + path);
            if (updated != text) File.WriteAllText(path, updated);
        }
    }
}
