using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Icon packaging only. Deterministic fit/padding; no artwork generation or builds.
    public static class AndroidAppIconSetup
    {
        const string Folder="Assets/_Game/Art/UI/AppIcon";
        const string QA="Docs/QA/AndroidIcon-20261019";
        const int Size=512;
        const float RegularFit=.84f,AdaptiveFit=.53f;
        static readonly Color Backdrop=new Color32(43,49,79,255);
        [Serializable] sealed class Slot {public string kind;public int width,height;public string[] labels,paths;}
        [Serializable] sealed class Record {public string unity;public int sourceWidth,sourceHeight;public float regularFit,adaptiveFit;public bool apkBuilt;public Slot[] slots;}

        static Texture2D Fit(Texture2D source,float fraction,bool opaque)
        {
            var result=new Texture2D(Size,Size,TextureFormat.RGBA32,false,false);
            var colors=new Color[Size*Size];var scale=Size*fraction/Mathf.Max(source.width,source.height);
            var width=source.width*scale;var height=source.height*scale;
            for(int y=0;y<Size;y++)for(int x=0;x<Size;x++)
            {
                var u=((x+.5f)-(Size-width)*.5f)/width;var v=((y+.5f)-(Size-height)*.5f)/height;
                var c=opaque?Backdrop:Color.clear;
                if(u>=0&&u<=1&&v>=0&&v<=1)c=source.GetPixelBilinear(u,v);
                if(opaque)c.a=1;colors[y*Size+x]=c;
            }
            result.SetPixels(colors);result.Apply(false);return result;
        }
        static Texture2D Background()
        {
            var t=new Texture2D(Size,Size,TextureFormat.RGBA32,false,false);t.SetPixels(Enumerable.Repeat(Backdrop,Size*Size).ToArray());t.Apply(false);return t;
        }
        static Texture2D Write(string name,Texture2D t)
        {
            var path=Folder+"/"+name+".png";var data=t.EncodeToPNG();
            if(File.Exists(path)&&!File.ReadAllBytes(path).SequenceEqual(data))throw new IOException("Refuse overwrite of different icon art: "+path);
            if(!File.Exists(path))File.WriteAllBytes(path,data);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);ConfigureImport(path);
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
        static void ConfigureImport(string path)
        {
            var t=(TextureImporter)AssetImporter.GetAtPath(path);t.textureType=TextureImporterType.Default;t.npotScale=TextureImporterNPOTScale.None;
            t.mipmapEnabled=false;t.alphaIsTransparency=true;t.textureCompression=TextureImporterCompression.Uncompressed;t.maxTextureSize=1024;
            t.filterMode=FilterMode.Bilinear;t.wrapMode=TextureWrapMode.Clamp;t.SaveAndReimport();
        }
        static void Preview(Texture2D source,bool adaptive,string mask,string filename)
        {
            const int n=256;var t=new Texture2D(n,n,TextureFormat.RGBA32,false,false);var pixels=new Color[n*n];
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                float u=(x+.5f)/n,v=(y+.5f)/n,dx=Mathf.Abs((u-.5f)*2),dy=Mathf.Abs((v-.5f)*2);
                var inside=mask=="circle"?dx*dx+dy*dy<=1:mask=="squircle"?Mathf.Pow(dx,4)+Mathf.Pow(dy,4)<=1:true;
                var color=Backdrop;
                if(adaptive){var c=source.GetPixelBilinear(.5f+(u-.5f)*72/108f,.5f+(v-.5f)*72/108f);color=Color.Lerp(Backdrop,c,c.a);color.a=1;}
                else color=source.GetPixelBilinear(u,v);
                if(!inside)color=Color.clear;pixels[y*n+x]=color;
            }
            t.SetPixels(pixels);t.Apply(false);File.WriteAllBytes(QA+"/"+filename,t.EncodeToPNG());UnityEngine.Object.DestroyImmediate(t);
        }
        public static void Configure()
        {
            Directory.CreateDirectory(QA);var sourcePath=Folder+"/LBC_AppIcon_Original.png";
            AssetDatabase.ImportAsset(sourcePath,ImportAssetOptions.ForceSynchronousImport);ConfigureImport(sourcePath);
            var source=new Texture2D(2,2,TextureFormat.RGBA32,false,false);if(!source.LoadImage(File.ReadAllBytes(sourcePath)))throw new InvalidDataException("PNG decode failed");
            if(source.width!=941||source.height!=942)throw new InvalidDataException("Unexpected owner artwork dimensions");
            var regular=Fit(source,RegularFit,true);var foreground=Fit(source,AdaptiveFit,false);var background=Background();
            var regAsset=Write("LBC_Android_LegacyRound",regular);var fgAsset=Write("LBC_Android_AdaptiveForeground",foreground);var bgAsset=Write("LBC_Android_AdaptiveBackground",background);
            var target=NamedBuildTarget.Android;var rows=new List<Slot>();int changedKinds=0;bool adaptiveConfigured=false;
            // The current Android provider exposes only Adaptive at minSDK26.
            // Use the genuine default application-icon slot for the ordinary icon;
            // do not invent unsupported legacy/round platform slot kinds.
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown,new[]{regAsset},IconKind.Any);
            var defaults=PlayerSettings.GetIcons(NamedBuildTarget.Unknown,IconKind.Any);
            if(defaults.Length==0||defaults[0]!=regAsset)throw new InvalidOperationException("Default app icon did not persist");
            rows.Add(new Slot{kind="Default Application Icon",width=Size,height=Size,labels=Array.Empty<string>(),paths=defaults.Select(AssetDatabase.GetAssetPath).ToArray()});
            foreach(var kind in PlayerSettings.GetSupportedIconKinds(target))
            {
                var name=kind.ToString();var adaptive=name.IndexOf("Adaptive",StringComparison.OrdinalIgnoreCase)>=0;
                if(name.IndexOf("Monochrome",StringComparison.OrdinalIgnoreCase)>=0)continue;
                if(!adaptive&&name.IndexOf("Legacy",StringComparison.OrdinalIgnoreCase)<0&&name.IndexOf("Round",StringComparison.OrdinalIgnoreCase)<0)continue;
                var labels=typeof(PlatformIconKind).GetProperty("customLayerLabels",BindingFlags.Instance|BindingFlags.NonPublic)?.GetValue(kind) as string[];
                Debug.Log("ANDROID ICON KIND: "+name+" labels="+string.Join(",",labels??Array.Empty<string>()));
                var icons=PlayerSettings.GetPlatformIcons(target,kind);if(icons.Length==0)throw new InvalidOperationException("No icon slots for "+name);
                foreach(var icon in icons)
                {
                    if(adaptive)
                    {
                        if(labels==null||labels.Length<2)throw new InvalidOperationException("Adaptive layer labels unavailable; do not guess layer ordering");
                        var textures=new Texture2D[icon.minLayerCount];
                        for(int i=0;i<textures.Length;i++)
                        {
                            if(labels[i].IndexOf("Foreground",StringComparison.OrdinalIgnoreCase)>=0)textures[i]=fgAsset;
                            else if(labels[i].IndexOf("Background",StringComparison.OrdinalIgnoreCase)>=0)textures[i]=bgAsset;
                            else throw new InvalidOperationException("Unexpected required adaptive layer "+labels[i]);
                        }
                        icon.SetTextures(textures);
                    }
                    else icon.SetTextures(regAsset);
                }
                PlayerSettings.SetPlatformIcons(target,kind,icons);changedKinds++;adaptiveConfigured|=adaptive;
                foreach(var icon in PlayerSettings.GetPlatformIcons(target,kind))
                {
                    var textures=icon.GetTextures();if(textures.Length<icon.minLayerCount||textures.Any(t=>t==null))throw new InvalidOperationException("Missing icon layer "+name);
                    rows.Add(new Slot{kind=name,width=icon.width,height=icon.height,labels=adaptive?labels:Array.Empty<string>(),paths=textures.Select(AssetDatabase.GetAssetPath).ToArray()});
                }
            }
            if(changedKinds<1||!adaptiveConfigured)throw new InvalidOperationException("Android adaptive slots were not configured");
            AssetDatabase.SaveAssets();
            Preview(regular,false,"square","Legacy-square.png");Preview(regular,false,"circle","Legacy-round.png");
            Preview(foreground,true,"circle","Adaptive-circle.png");Preview(foreground,true,"squircle","Adaptive-squircle.png");
            File.WriteAllText(QA+"/icon-bindings.json",JsonUtility.ToJson(new Record{unity=Application.unityVersion,sourceWidth=source.width,sourceHeight=source.height,regularFit=RegularFit,adaptiveFit=AdaptiveFit,apkBuilt=false,slots=rows.ToArray()},true));
            Debug.Log("ANDROID ICON CONFIGURED: "+rows.Count+" populated slots; no APK build invoked.");
        }
    }
}
