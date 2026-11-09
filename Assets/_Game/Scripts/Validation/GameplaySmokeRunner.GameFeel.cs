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
        Vector2 LeisureScreen(RectTransform scene,Vector2 uv)=>RectTransformUtility.WorldToScreenPoint(null,scene.TransformPoint(scene.rect.min+Vector2.Scale(uv,scene.rect.size)));
        IEnumerator GameFeelJourney()
        {
            musicVirtualFocus=true;
            if(Environment.GetCommandLineArgs().Contains("--game-feel-more")){yield return GameFeelMore();yield break;}
            if(Environment.GetCommandLineArgs().Contains("--game-feel-star"))
            {
                yield return CrossEnter("gyeongju");Time.timeScale=4;yield return ChapterGo("G18");yield return ChapterChoice("accept:gq03_offer");yield return CloseEvent();yield return StoryBattle("G18",false);Time.timeScale=1;
                Check(game.Battles.Enemy.mechanic=="star_cycle"&&results.Any(r=>r.Contains("Actual star-symbol input opens")),"Correct G18 run actually exercised the star shield and its real symbol controls");
                Check(game.Events.Resolved(game.Content.Event("gyeongju","enemy_star_core")),"Current combat presentation preserves actual star-core encounter resolution");KeepSave("GF-star-earned");yield break;
            }
            if(Environment.GetCommandLineArgs().Contains("--game-feel-boss")){yield return CrossEnter("seoul");yield return Phase5EarnedBoss();KeepSave("GF-boss-earned");yield break;}
            if(Environment.GetCommandLineArgs().Contains("--game-feel-fresh"))
            {
                // The existing core goes through actual fresh Mai/H3/paper/bot UI.
                yield return CasualRuntimeJourney();Time.timeScale=5;yield return ChapterGo("H7");Time.timeScale=1;
                yield return ChapterChoice("restore");yield return CloseEvent();
                Check(game.Quests.IsCompleted("HQ01")&&game.IsCityUnlocked("hue")&&game.IsCityUnlocked("jeonju"),"Fresh vertical slice earns original station/world gates without a restored relic");
                KeepSave("GF-HoiAn-earned-H7");
            }
            yield return CrossEnter("hoi_an");Time.timeScale=5;yield return ChapterGo("H16");Time.timeScale=1;yield return CloseEvent();
            var entry=LeisurePresentation.Load().entries.First(e=>e.nodeId=="H16");
            foreach(var size in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
            {
                yield return PilotResolution(size.x,size.y);yield return LeisureFullOpen(entry);
                var activity=FullView;var saved=JsonUtility.ToJson(game.Save);var scene=GameObject.Find("LeisureScene").GetComponent<RectTransform>();
                var hit=GameObject.Find("LeisureGesture").GetComponent<RectTransform>();var target=new Vector2(.88f,.40f);
                Check(hit.rect.width>=64&&hit.GetComponent<Button>()==null,"Existing lotus has a generous direct gesture surface without a duplicate button callback");
                yield return Snapshot("GF-lotus-ready-"+size.x);
                yield return SLDrag(SLPoint(hit),LeisureScreen(scene,new Vector2(.2f,.8f)),true);
                Check(!activity.Playing&&JsonUtility.ToJson(game.Save)==saved,"Invalid river drop gives retry feedback without starting or changing progression");
                yield return SLDrag(SLPoint(hit),LeisureScreen(scene,target),true);
                Check(activity.Playing&&Vector2.Distance(activity.ReleaseDestination,target)<.015f,"Actual touch drag sets the chosen water destination and releases the lotus");
                yield return new WaitForSecondsRealtime(.75f);yield return Snapshot("GF-lotus-drifting-"+size.x);
                yield return Until(()=>!activity.Playing,"direct lotus motion completes",7);
                var actual=scene.InverseTransformPoint(activity.ActivityTool.position);var uv=new Vector2((actual.x-scene.rect.xMin)/scene.rect.width,(actual.y-scene.rect.yMin)/scene.rect.height);
                Check(Vector2.Distance(uv,target)<.02f,"The visible lotus actually reaches the player-selected destination");
                yield return Snapshot("GF-lotus-selected-arrival-"+size.x);
                Check(JsonUtility.ToJson(game.Save)==saved,"A player-chosen lotus path awards no items, quest flags, coins or relic progress");
                yield return CloseEvent();yield return LeisureFullOpen(entry);activity=FullView;scene=GameObject.Find("LeisureScene").GetComponent<RectTransform>();hit=GameObject.Find("LeisureGesture").GetComponent<RectTransform>();
                var second=new Vector2(.67f,.27f);yield return SLDrag(SLPoint(hit),LeisureScreen(scene,second),false);
                Check(activity.Playing&&Vector2.Distance(activity.ReleaseDestination,second)<.015f&&Vector2.Distance(second,target)>.2f,"Mouse chooses a genuinely different lotus trajectory");
                yield return new WaitForSecondsRealtime(.25f);yield return CloseEvent();
                Check(FullView==null&&Map.View.InputEnabled&&JsonUtility.ToJson(game.Save)==saved,"Closing during the directed animation disposes its input/driver and returns to the same save");
            }
            if(Environment.GetCommandLineArgs().Contains("--game-feel-fresh"))yield return GameFeelCombat();
            KeepSave("GF-complete");
        }
        IEnumerator GameFeelMore()
        {
            foreach(var resolution in new[]{new Vector2Int(540,960),new Vector2Int(720,1280)})
            {
                yield return PilotResolution(resolution.x,resolution.y);
                foreach(var node in new[]{"D9","G10","G11"})
                {
                    var entry=LeisurePresentation.Load().entries.First(e=>e.nodeId==node);yield return CrossEnter(entry.cityId);Time.timeScale=5;yield return ChapterGo(node);Time.timeScale=1;yield return CloseEvent();yield return LeisureFullOpen(entry);
                    var saved=JsonUtility.ToJson(game.Save);var activity=FullView;var scene=GameObject.Find("LeisureScene").GetComponent<RectTransform>();var hit=GameObject.Find("LeisureGesture").GetComponent<RectTransform>();
                    if(node=="D9")
                    {
                        var at=new Vector2(.55f,.78f);yield return SLDrag(SLPoint(hit),LeisureScreen(scene,at),true);
                        Check(activity.Playing&&Vector2.Distance(activity.KiteAnchor,at)<.015f,"Touch drag actively steers the real existing kite in its safe sky region");
                        var thread=GameObject.Find("KiteString").GetComponent<LeisureThreadGraphic>();Check(Vector2.Distance(thread.End,thread.rectTransform.InverseTransformPoint(activity.ActivityTool.position))<.01f,"Steered kite string still ends at the real moving kite");
                        yield return Snapshot("GF-kite-controlled-"+resolution.x);
                    }
                    else if(node=="G10")
                    {
                        var fixedFrame=GameObject.Find("BellSupport").transform;var place=fixedFrame.position;var start=SLPoint(hit);
                        yield return SLDrag(start,start+Vector2.right*20,false);
                        Check(activity.Playing&&activity.BellStrength<.9f,"Short mouse pull chooses a gentle bell swing");yield return new WaitForSecondsRealtime(.35f);yield return Snapshot("GF-bell-gentle-"+resolution.x);
                        Check(fixedFrame.position==place&&fixedFrame.localRotation==Quaternion.identity,"Bell support stays fixed while the existing mallet and Lumi react");
                        yield return CloseEvent();yield return LeisureFullOpen(entry);activity=FullView;hit=GameObject.Find("LeisureGesture").GetComponent<RectTransform>();start=SLPoint(hit);
                        yield return SLDrag(start,start+Vector2.right*150,true);Check(activity.Playing&&activity.BellStrength>1.1f,"Long touch pull chooses a visibly wider bell swing");yield return new WaitForSecondsRealtime(.35f);yield return Snapshot("GF-bell-wide-"+resolution.x);
                    }
                    else
                    {
                        yield return SLDrag(SLPoint(hit),LeisureScreen(scene,new Vector2(.15f,.85f)),true);
                        Check(!activity.Playing&&JsonUtility.ToJson(game.Save)==saved,"Wrong wish-card drop offers a free retry, no reward/progression changes");
                        var guide=GameObject.Find("LeisureDropTarget").GetComponent<RectTransform>();yield return SLDrag(SLPoint(hit),SLPoint(guide),true);
                        Check(activity.Playing&&!guide.gameObject.activeSelf,"Actual wish-card drag accepts the real rack target and hands off to the existing animation");yield return Snapshot("GF-wish-attached-"+resolution.x);
                    }
                    yield return Until(()=>!activity.Playing,"direct cultural activity completes "+node,7);
                    Check(JsonUtility.ToJson(game.Save)==saved&&FindButton("LeisureAction").GetComponentInChildren<Text>().text=="Chơi lại","Direct cultural action ends with replay and no currency/items/quest/relic/save authority: "+node);
                    yield return Click("LeisureAction",true);yield return new WaitForSecondsRealtime(.2f);yield return CloseEvent();
                    Check(FullView==null&&Map.View.InputEnabled,"Closing a replay removes the gesture and returns input to the map: "+node);
                }
            }
            KeepSave("GF-cultural-actions-earned");
        }
        IEnumerator GameFeelCombat()
        {
            yield return PilotResolution(540,960);Time.timeScale=5;yield return ChapterGo("H12");Time.timeScale=1;yield return ChapterChoice("battle");
            var battle=game.Battles;var normal=Math.Max(1,9-battle.EnemyDefense);var light=Math.Max(1,14-battle.EnemyDefense/2);var half=Math.Max(1,(battle.EnemyAttack+1)/2);
            Check(FindButton("BattleAttack").GetComponentInChildren<Text>().text.Contains(normal+" sát thương")&&FindButton("BattleLight").GetComponentInChildren<Text>().text.Contains(light+" sát thương"),"Action labels match the actual current enemy defense, rather than invented balance numbers");
            Check(FindButton("BattleDefend").GetComponentInChildren<Text>().text.Contains(half+" sát thương")&&GameObject.Find("BattleTurn").GetComponent<Text>().text.Contains("Đáp trả:"),"Counterattack and reduced incoming damage are visible before choosing");
            foreach(var name in new[]{"BattleAttack","BattleDefend","BattleLight","BattleHeal","BattleRun"})P6Bounds(name);
            yield return Snapshot("GF-combat-decisions");
            var hp=battle.PlayerHP;var enemy=battle.EnemyHP;var turn=battle.TurnNumber;var energy=battle.Energy;
            yield return Click("BattleDefend",true);
            Check(battle.TurnNumber==turn+1&&hp-battle.PlayerHP==half&&battle.EnemyHP==enemy&&battle.Energy==Math.Min(3,energy+1),"One accepted defense causes exactly the original one turn, half damage and capped energy recovery");
            Check(GameObject.Find("BattlePlayerResponse")!=null&&!GameObject.Find("BattlePlayerResponse").GetComponent<Text>().raycastTarget,"Defense feedback is visible and never intercepts input");yield return Snapshot("GF-combat-defense");
            var questItems=game.Save.items.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();
            while(battle.Active)yield return Click("BattleDefend",true);
            Check(battle.Outcome=="loss"&&game.Save.items.SequenceEqual(questItems)&&game.Save.rewardGrantIds.SequenceEqual(ledger),"Actual loss preserves quest items and has no encounter reward");
            yield return Snapshot("GF-combat-loss");yield return Click("BattleRetry",true);Check(battle.Active,"Loss offers a real retry of the same unresolved crab");
            turn=battle.TurnNumber;enemy=battle.EnemyHP;yield return Click("BattleAttack",true);
            Check(battle.TurnNumber==turn+1&&enemy-battle.EnemyHP==normal,"Attack feedback leaves original damage and turn semantics unchanged");yield return Snapshot("GF-combat-hit");
            NodeMarkerView.SetMotionMode(MarkerMotionMode.Off);yield return Click("BattleDefend",true);yield return new WaitForSecondsRealtime(.1f);
            Check(GameObject.Find("BattlePlayerResponse")!=null,"Motion-off mode retains readable action feedback");NodeMarkerView.SetMotionMode(MarkerMotionMode.Full);
            while(battle.Active)yield return Click(battle.Energy>0?"BattleLight":"BattleAttack",true);
            Check(battle.Outcome=="win","Player decisions still reach the original actual encounter victory");yield return Snapshot("GF-combat-win");yield return Click("BattleClose",true);
            Check(Map.View.InputEnabled&&!game.Battles.Active,"Battle presentation returns cleanly to the map");
        }
    }
}
#endif
