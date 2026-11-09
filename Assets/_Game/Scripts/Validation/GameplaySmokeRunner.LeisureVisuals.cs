#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator LeisureVisualJourney()
        {
            musicVirtualFocus=true;
            var entries=LeisurePresentation.Load().entries.Where(e=>e.enabled).ToArray();
            if(Environment.GetCommandLineArgs().Contains("--leisure-visuals-fresh"))
            {
                yield return CrossEnter("hoi_an");
                foreach(var m in Map.NodeMarkers)
                    if(m.LeisureBadge!=null)Check(!m.LeisureBadge.gameObject.activeInHierarchy,"Fresh HIDDEN/PREVIEW activity has no visible leisure badge: "+m.NodeId);
                yield return Snapshot("LV-fresh-no-hidden-leak");yield break;
            }
            foreach(var city in entries.Select(e=>e.cityId).Distinct())
            {
                yield return CrossEnter(city);yield return CloseEvent();
                var local=entries.Where(e=>e.cityId==city).ToArray();
                foreach(var m in Map.NodeMarkers)
                {
                    var expected=local.Any(e=>e.nodeId==m.NodeId);
                    Check((m.LeisureBadge!=null)==expected,"Only real enabled activity nodes receive a leisure badge: "+m.NodeId);
                    if(!expected)continue;
                    Check(m.LeisureBadge.sprite==DesignKitArt.Load().secret&&!m.LeisureBadge.raycastTarget,"Existing star art reused without a touch blocker: "+m.NodeId);
                    Check(m.LeisureBadge.gameObject.activeInHierarchy==(m.gameObject.activeInHierarchy&&m.Discovery==MapDiscoveryState.REVEALED),"Leisure badge obeys real discovery: "+m.NodeId);
                    Map.View.ZoomBy(1/Map.View.Zoom);yield return null;
                    var small=m.LeisureBadge.rectTransform.lossyScale.x;
                    Map.View.ZoomBy(1.8f);yield return null;
                    Check(Mathf.Abs(small-m.LeisureBadge.rectTransform.lossyScale.x)<.01f,"Leisure badge stays readable through zoom: "+m.NodeId);
                }
                var first=local[0];Time.timeScale=5;yield return ChapterGo(first.nodeId);Time.timeScale=1;yield return CloseEvent();
                Map.View.ZoomBy(1.8f/Map.View.Zoom);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));
                yield return Snapshot("LV-markers-"+city);
                yield return LeisureFullOpen(first);Check(FullView!=null||FullPilot!=null,"Touch opens existing node activity with leisure badge present: "+first.nodeId);yield return CloseEvent();
            }
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
            {
                yield return PilotResolution(size.x,size.y);
                foreach(var node in new[]{"D9","G10"})
                {
                    var entry=entries.First(e=>e.nodeId==node);yield return CrossEnter(entry.cityId);Time.timeScale=5;yield return ChapterGo(node);Time.timeScale=1;yield return CloseEvent();yield return LeisureFullOpen(entry);
                    var saved=JsonUtility.ToJson(game.Save);var activity=FullView;var hit=GameObject.Find("LeisureGesture").GetComponent<RectTransform>();
                    if(node=="D9")
                    {
                        var scene=GameObject.Find("LeisureScene").GetComponent<RectTransform>();
                        yield return SLDrag(SLPoint(hit),LeisureScreen(scene,new Vector2(.66f,.73f)),true);
                        Check(activity.Playing,"Existing touch drag still steers the unclipped kite");
                    }
                    else
                    {
                        var support=GameObject.Find("BellSupport").transform;var at=support.position;
                        yield return SLDrag(SLPoint(hit),SLPoint(hit)+Vector2.right*100,false);yield return new WaitForSecondsRealtime(.25f);
                        Check(activity.Playing&&support.position==at&&support.localRotation==Quaternion.identity,"Existing mouse pull moves the complete mallet while support stays fixed");
                    }
                    yield return new WaitForSecondsRealtime(.35f);yield return Snapshot("LV-"+node+"-"+size.x+"-after");
                    yield return CloseEvent();Check(FullView==null&&Map.View.InputEnabled&&JsonUtility.ToJson(game.Save)==saved,"Mid-animation close removes activity without progression/reward/save mutation: "+node);
                    yield return LeisureFullOpen(entry);yield return Click("LeisureAction",true);yield return Until(()=>!FullPlaying,"activity completes "+node,7);
                    Check(JsonUtility.ToJson(game.Save)==saved,"Completion preserves full save: "+node);yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.15f);Check(FullPlaying,"Replay still starts: "+node);yield return CloseEvent();
                }
            }
            KeepSave("LV-earned-final");
        }
    }
}
#endif
