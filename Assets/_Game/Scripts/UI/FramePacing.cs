using System;
using UnityEngine;
namespace LightBetweenCultures
{
    // Display preference only; never serialized into gameplay saves.
    public static class FramePacing
    {
        static string Key=>Environment.GetCommandLineArgs().Length>1&&Array.IndexOf(Environment.GetCommandLineArgs(),"--gameplay-smoke")>=0?"LBC.QA.FrameRate":"LBC.FrameRate";
        public static int Requested=>PlayerPrefs.GetInt(Key,60)==30?30:60;
        public static void Apply(){Application.targetFrameRate=Requested;}
        public static void Toggle(){PlayerPrefs.SetInt(Key,Requested==60?30:60);PlayerPrefs.Save();Apply();}
    }
}
