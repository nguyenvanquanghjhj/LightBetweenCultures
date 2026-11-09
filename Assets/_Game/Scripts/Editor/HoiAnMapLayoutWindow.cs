using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    public sealed class HoiAnMapLayoutWindow : EditorWindow
    {
        private const string DataPath = "Assets/_Game/Resources/HoiAnData.json";
        private GameData data;
        private Texture2D background;
        private int selected, edgeIndex, dragging = -1;
        private Vector2 scroll;
        private bool changed;
        [MenuItem("Tools/Light Between Cultures/Edit Hoi An Map Layout")]
        public static void Open() => GetWindow<HoiAnMapLayoutWindow>("Hội An Layout");
        private void OnEnable() => Reload();
        private void Reload()
        {
            data = JsonUtility.FromJson<GameData>(File.ReadAllText(DataPath));
            background = AssetDatabase.LoadAssetAtPath<Texture2D>(HoiAnMapSetup.SpritePath);
            changed = false;
        }
        private void OnGUI()
        {
            if (background == null) { EditorGUILayout.HelpBox("Run Import Hoi An Illustrated Map first.",MessageType.Info); return; }
            EditorGUILayout.LabelField("Drag nodes on the image; normalized coordinates use bottom-left origin.",EditorStyles.wordWrappedLabel);
            var width = Mathf.Min(position.width-20,Mathf.Max(200,(position.height-260)*4/3));
            var rect = GUILayoutUtility.GetRect(width,width*.75f,GUILayout.ExpandWidth(false));
            GUI.DrawTexture(rect,background,ScaleMode.StretchToFill);
            Handles.BeginGUI();
            foreach(var edge in data.edges)
            {
                var points=CityRouteGeometry.Points(data,edge.from,edge.to);
                Handles.color=string.IsNullOrEmpty(edge.requirement)?Color.yellow:Color.gray;
                for(var i=1;i<points.Count;i++) Handles.DrawLine(ScreenPoint(rect,points[i-1]),ScreenPoint(rect,points[i]));
            }
            Handles.EndGUI();
            for(var i=0;i<data.nodes.Length;i++)
            {
                var point=ScreenPoint(rect,data.nodes[i].position);
                GUI.Box(new Rect(point.x-12,point.y-10,24,20),"H"+i);
                if(Event.current.type==EventType.MouseDown && Vector2.Distance(Event.current.mousePosition,point)<18)
                { selected=i; dragging=i; Event.current.Use(); }
            }
            if(Event.current.type==EventType.MouseDrag && dragging>=0)
            {
                var p=Event.current.mousePosition;
                data.nodes[dragging].position=new Vector2(Mathf.Clamp01((p.x-rect.x)/rect.width),Mathf.Clamp01(1-(p.y-rect.y)/rect.height));
                changed=true; Event.current.Use(); Repaint();
            }
            if(Event.current.type==EventType.MouseUp) dragging=-1;
            scroll=EditorGUILayout.BeginScrollView(scroll);
            selected=EditorGUILayout.IntSlider("Node",selected,0,data.nodes.Length-1);
            EditorGUI.BeginChangeCheck();
            data.nodes[selected].position=EditorGUILayout.Vector2Field("H"+selected+" "+data.nodes[selected].name,data.nodes[selected].position);
            edgeIndex=EditorGUILayout.IntSlider("Edge",edgeIndex,0,data.edges.Length-1);
            var chosen=data.edges[edgeIndex];
            EditorGUILayout.LabelField("H"+chosen.from+" → H"+chosen.to+" (graph/gates unchanged)");
            if(chosen.bends==null) chosen.bends=Array.Empty<Vector2>();
            for(var i=0;i<chosen.bends.Length;i++) chosen.bends[i]=EditorGUILayout.Vector2Field("Bend "+i,chosen.bends[i]);
            if(EditorGUI.EndChangeCheck()) changed=true;
            EditorGUILayout.BeginHorizontal();
            if(GUILayout.Button("Add bend")) { Array.Resize(ref chosen.bends,chosen.bends.Length+1); chosen.bends[chosen.bends.Length-1]=(data.nodes[chosen.from].position+data.nodes[chosen.to].position)*.5f; changed=true; }
            if(GUILayout.Button("Remove last bend")&&chosen.bends.Length>0) { Array.Resize(ref chosen.bends,chosen.bends.Length-1); changed=true; }
            if(GUILayout.Button(changed?"Save layout *":"Save layout")) Save();
            if(GUILayout.Button("Reload")) Reload();
            EditorGUILayout.EndHorizontal(); EditorGUILayout.EndScrollView();
        }
        private static Vector2 ScreenPoint(Rect rect,Vector2 p) => new Vector2(rect.x+p.x*rect.width,rect.y+(1-p.y)*rect.height);
        private void Save()
        {
            foreach(var node in data.nodes) node.position=Clamp(node.position);
            foreach(var edge in data.edges) if(edge.bends!=null) for(var i=0;i<edge.bends.Length;i++) edge.bends[i]=Clamp(edge.bends[i]);
            var folder=Path.GetFullPath("../Backups/MapLayouts"); Directory.CreateDirectory(folder);
            File.Copy(DataPath,Path.Combine(folder,"HoiAnData-"+DateTime.Now.ToString("yyyyMMdd-HHmmssfff")+".json"));
            File.WriteAllText(DataPath,JsonUtility.ToJson(data,true),new System.Text.UTF8Encoding(false));
            AssetDatabase.ImportAsset(DataPath,ImportAssetOptions.ForceUpdate); changed=false;
        }
        private static Vector2 Clamp(Vector2 p) => new Vector2(Mathf.Clamp01(p.x),Mathf.Clamp01(p.y));
    }
}
