#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        [Serializable] class MapReturnSample
        {public string status;public float firstReturnSeconds,reentryReturnSeconds,maxFrameMs,p95FrameMs;public int mapLoads,eyeCount;public bool busy,inputEnabled,moving;public string[] loaded;}
        IEnumerator MapReturnJourney()
        {
            var sample=new MapReturnSample();var loaded=new List<string>();
            void Loaded(Scene scene,LoadSceneMode mode){loaded.Add(scene.name);if(scene.name=="CityMap")sample.mapLoads++;}
            SceneManager.sceneLoaded+=Loaded;
            try
            {
                Check(game.Save.accepted.Count==0&&game.Save.completed.Count==0,"P0 starts a fresh isolated save");
                yield return CrossEnter("hoi_an");yield return Travel(NodeIndex("H2"),true);yield return Click("EventOption_accept",true);
                yield return CloseEvent();yield return Travel(NodeIndex("H3"),true);yield return Click("EventOption_scene",true);yield return Scene("HoiAnDemo");
                var player=FindAnyObjectByType<HoiAnPlayerController>();var interaction=player.GetComponent<InteractionSystem>();
                var eye=FindAnyObjectByType<HoiAnItem>();
                Check(eye!=null&&eye.gameObject.activeInHierarchy,"P0 physical lamp eye is present");
                yield return Pointer(interaction.ItemTapRect.center,true);
                yield return Until(()=>!player.Walking&&interaction.CanInteract,"P0 item tap approaches through existing pathfinding",18);
                yield return Click("Collect",true);
                Check(game.Inventory.Count(game.Data.quest.item)==1,"P0 real pickup grants the lamp eye exactly once");
                var ledger=game.Save.rewardGrantIds.ToArray();var at=Time.realtimeSinceStartup;
                yield return Click("Return",true);yield return Scene("CityMap");
                yield return Until(()=>Map!=null&&Map.View!=null&&!game.Scenes.Busy,"P0 return map ready",20);
                sample.firstReturnSeconds=Time.realtimeSinceStartup-at;
                var frames=new List<float>();for(var i=0;i<120;i++){yield return null;frames.Add(Time.unscaledDeltaTime*1000);}
                frames.Sort();sample.maxFrameMs=frames.Last();sample.p95FrameMs=frames[(int)(frames.Count*.95f)];
                sample.busy=game.Scenes.Busy;sample.inputEnabled=Map.View.InputEnabled;sample.moving=Map.Moving;
                Check(!game.Scenes.Busy&&Map.View.InputEnabled&&!Map.Moving,"P0 returned map is not scene-busy or input-locked");
                yield return Travel(NodeIndex("H2"),true);
                Check(game.Save.node==NodeIndex("H2")&&Map.DialogueOpen,"P0 NPC/node responds after returning from H3");
                yield return CloseEvent();yield return Travel(NodeIndex("H3"),true);yield return Click("EventOption_scene",true);yield return Scene("HoiAnDemo");
                eye=FindAnyObjectByType<HoiAnItem>(FindObjectsInactive.Include);
                Check(eye!=null&&!eye.gameObject.activeSelf&&FindButton("Collect")==null,"P0 reentry hides the owned eye and pickup control");
                at=Time.realtimeSinceStartup;yield return Click("Return",true);yield return Scene("CityMap");
                yield return Until(()=>Map!=null&&Map.View!=null&&!game.Scenes.Busy,"P0 second return map ready",20);
                sample.reentryReturnSeconds=Time.realtimeSinceStartup-at;
                sample.eyeCount=game.Inventory.Count(game.Data.quest.item);
                Check(sample.eyeCount==1&&game.Save.rewardGrantIds.SequenceEqual(ledger),"P0 reentry/return neither loses nor duplicates the eye/rewards");
                Check(sample.mapLoads==3,"P0 exactly three requested map loads, no scene transition loop");
                sample.status="NOT_REPRODUCED";
                yield return Snapshot("P0-return-map");KeepSave("P0-earned-eye");
            }
            finally
            {
                SceneManager.sceneLoaded-=Loaded;sample.loaded=loaded.ToArray();
                File.WriteAllText(Path.Combine(output,"map-return-metrics.json"),JsonUtility.ToJson(sample,true));
            }
        }
    }
}
#endif
