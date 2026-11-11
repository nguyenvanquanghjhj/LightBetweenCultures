#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        static Rect CatScreenRect(RectTransform rt)
        {
            var corners=new Vector3[4];rt.GetWorldCorners(corners);
            var a=RectTransformUtility.WorldToScreenPoint(null,corners[0]);
            var b=RectTransformUtility.WorldToScreenPoint(null,corners[2]);
            return Rect.MinMaxRect(a.x,a.y,b.x,b.y);
        }
        void CatCheckMapControls()
        {
            Canvas.ForceUpdateCanvases();
            var cat=FindButton("CatRescueHUD");var audio=FindButton("AudioSettings");
            var controls=cat.transform.parent.GetComponentsInChildren<Button>().Where(b=>b.gameObject.activeInHierarchy&&b.transform.parent==cat.transform.parent).ToArray();
            foreach(var control in new[]{cat,audio})
                Check(controls.All(b=>b==control||!CatScreenRect((RectTransform)control.transform).Overlaps(CatScreenRect((RectTransform)b.transform))),control.name+" has no overlapping toolbar button: "+game.Data.cityId);
            var marker=GameObject.Find("CatOnMap");
            Check(marker!=null&&marker.GetComponent<Button>()==null&&marker.GetComponentsInChildren<Graphic>().All(g=>!g.raycastTarget),"Cat location illustration never captures node input: "+game.Data.cityId);
        }
        IEnumerator CatCheckMarkerZoom()
        {
            var original=Map.View.Zoom;
            foreach(var zoom in new[]{1f,2f,4f})
            {
                Map.View.ZoomBy(zoom/Map.View.Zoom);yield return new WaitForSecondsRealtime(.3f);
                var marker=(RectTransform)GameObject.Find("CatOnMap").transform;
                Check(Mathf.Abs(marker.localScale.x-1/Map.View.Zoom)<.001f&&Mathf.Abs(marker.anchoredPosition.x*Map.View.Zoom-60)<.1f,"Animated cat keeps its size and node clearance at zoom "+zoom);
            }
            Map.View.ZoomBy(original/Map.View.Zoom);Map.View.Focus(Map.Marker.anchorMin);yield return null;
        }
        IEnumerator CatClick(string name,bool finger=true)
        {
            yield return Until(()=>FindButton(name)!=null,"Cat button "+name,8);
            var button=FindButton(name);var scroll=button.GetComponentInParent<ScrollRect>();
            if(scroll!=null)
            {
                Canvas.ForceUpdateCanvases();var rect=(RectTransform)button.transform;
                var center=(Vector2)scroll.content.InverseTransformPoint(rect.TransformPoint(rect.rect.center));
                var range=scroll.content.rect.height-scroll.viewport.rect.height;
                scroll.verticalNormalizedPosition=range>0?1-Mathf.Clamp01((-center.y-scroll.viewport.rect.height*.5f)/range):1;
                Canvas.ForceUpdateCanvases();yield return null;yield return null;
            }
            yield return Click(name,finger);
        }
        IEnumerator CatHub()
        {
            if(FindButton("CatClose")==null){Map.OpenCatRescue();yield return null;}
        }
        IEnumerator CatTravelButton(string name,string node)
        {
            yield return CatHub();yield return CatClick(name);
            yield return Until(()=>game.Data.nodes[game.Save.node].id==node&&!Map.Moving&&FindButton("CatClose")!=null,"Cat navigation "+node,120);
        }
        IEnumerator CatDrag(string piece,string target,bool finger)
        {
            Canvas.ForceUpdateCanvases();yield return null;
            var a=(RectTransform)GameObject.Find(piece).transform;var b=(RectTransform)GameObject.Find(target).transform;
            yield return SLDrag(SLPoint(a),SLPoint(b),finger);
        }
        List<int> CatMazeRoute(CatRescueChallenge c)
        {
            var width=c.maze[0].Length;var queue=new Queue<int>();var prev=new Dictionary<int,int>{{c.start,-1}};queue.Enqueue(c.start);
            while(queue.Count>0)
            {
                var cell=queue.Dequeue();if(cell==c.exit)break;
                foreach(var next in new[]{cell-1,cell+1,cell-width,cell+width})
                {
                    if(next<0||next>=width*c.maze.Length||Math.Abs(next%width-cell%width)+Math.Abs(next/width-cell/width)!=1||c.maze[next/width][next%width]=='#'||prev.ContainsKey(next))continue;
                    prev[next]=cell;queue.Enqueue(next);
                }
            }
            var path=new List<int>();for(var cell=c.exit;cell>=0;cell=prev[cell])path.Add(cell);path.Reverse();return path;
        }
        bool CatMazeStillMounted(GameObject boardObject,Transform board,Button[] cells,Vector2[] positions)
        {
            if(boardObject==null||board==null||!ReferenceEquals(GameObject.Find("CatChallengeBoard"),boardObject)||!ReferenceEquals(boardObject.transform,board))return false;
            for(var i=0;i<cells.Length;i++)
            {
                if(cells[i]==null||!ReferenceEquals(cells[i],FindButton("CatCell"+i))||!ReferenceEquals(cells[i].transform.parent,board))return false;
                if(Vector2.Distance(((RectTransform)cells[i].transform).anchoredPosition,positions[i])>.01f)return false;
            }
            return true;
        }
        IEnumerator CatRescueJourney()
        {
            PlayerPrefs.SetInt(PlayerCopy.GuideKey,1);musicVirtualFocus=true;
            if(Environment.GetCommandLineArgs().Contains("--cat-rescue-restart"))
            {
                Check(game.Cats.RescuedCount==6,"Separate process reload keeps six rescued cats");
                var coins=game.Save.coins;
                Check(!game.Cats.Rescue(Vector2.zero)&&game.Save.coins==coins,"Restart rejects reward replay");
                Check(game.Save.rewardGrantIds.Count(id=>id.StartsWith("catrescue.reward."))==6,"Exactly six reward receipts survive process restart");
                yield break;
            }
            // Only the isolated QA save path is overwritten.
            var legacy=new SaveData{version=1,coins=7};legacy.items.Add("lamp_eye");
            File.WriteAllText(savePath,JsonUtility.ToJson(legacy).Replace("\"catRescues\":[]", "\"unknownOldField\":[]"));game.Reload();
            Check(game.Save.version==2&&game.Save.catRescues.Count==0&&game.Save.coins==7&&game.Save.items.Contains("lamp_eye"),"Legacy v1 load preserves inventory/coins and initializes rescue state");
            File.WriteAllText(savePath,JsonUtility.ToJson(new SaveData()));game.Reload();
            game.Scenes.Go("CityMap");yield return Scene("CityMap");
            yield return Click("CatRescueHUD",true);
            Check(FindButton("CatReturnMap")!=null && GameObject.Find("CatCurrentStep")!=null,"Catventure shows a clear next step and map exit");
            Check(game.Cats.Progress("hoi_an")==null,"Reading rescue introduction grants no progress");
            yield return Snapshot("CAT-HoiAn-introduction");yield return CatClick("CatStart");
            Check(game.Cats.Progress("hoi_an").started,"Real touch accepts Hoi An rescue");
            yield return Click("CatClose",true);yield return Snapshot("CAT-HoiAn-map");
            CatCheckMapControls();
            yield return CatCheckMarkerZoom();
            var originalState=JsonUtility.ToJson(game.Save);yield return Click("AudioSettings",true);
            Check(GameObject.Find("AudioSettingsPanel")!=null,"Audio settings remain accessible beside the rescue toolbar");
            yield return Click("AudioSettingsClose",true);Check(JsonUtility.ToJson(game.Save)==originalState,"Opening/closing settings preserves rescue and legacy state");
            Check(GameObject.Find("MiniMapRoads").GetComponent<CatMiniMapGraphic>()!=null,"Mini-map uses the actual open navigation graph");
            yield return CatHub();
            var modes=new HashSet<string>();var mazeCities=new HashSet<string>();
            var all=game.Cats.Catalog.chapters;
            var count=Environment.GetCommandLineArgs().Contains("--cat-rescue-pilot")?1:all.Length;
            for(var cityIndex=0;cityIndex<count;cityIndex++)
            {
                var city=all[cityIndex];
                if(cityIndex>0)
                {
                    Check(game.IsCityUnlocked(city.cityId),"Cat route unlocks next city: "+city.cityId);
                    if(FindButton("CatClose")!=null)yield return Click("CatClose",true);
                    Check(game.EnterCity(city.cityId),"Enter next rescue city through bootstrap guard: "+city.cityId);
                    game.Scenes.Go("CityMap");yield return Scene("CityMap");yield return CatHub();yield return CatClick("CatStart");
                    yield return Click("CatClose",true);CatCheckMapControls();yield return CatHub();
                }
                var cats=game.Cats;var c=cats.Current;var current=cats.Next(c);
                Check(!cats.Complete(current.id)&&!cats.Rescue(Vector2.zero),"Cannot skip challenge or rescue early: "+c.cityId);
                var before=JsonUtility.ToJson(game.Save.catRescues[cityIndex]);
                if(!cats.At(current.nodeId))Check(!cats.Match(current.id,0,0),"Remote interaction rejected: "+c.cityId);
                Check(!cats.Start()&&JsonUtility.ToJson(game.Save.catRescues[cityIndex])==before,"Repeated accept is inert: "+c.cityId);
                foreach(var supply in c.supplies)
                {
                    if(!cats.At(supply.nodeId))yield return CatTravelButton("CatSupply_"+supply.id,supply.nodeId);
                    yield return CatClick("CatSupply_"+supply.id);
                    Check(cats.Progress(c.cityId).supplies.Contains(supply.id),"Travel and touch collect supply: "+supply.id);
                    Check(!cats.Collect(supply.id),"Duplicate supply rejected: "+supply.id);
                }
                foreach(var challenge in c.challenges)
                {
                    if(!cats.At(challenge.nodeId)){yield return CatTravelButton("CatTask_"+challenge.id,challenge.nodeId);Check(FindButton("CatSubmit")!=null,"Target arrival opens actual challenge: "+challenge.id);}
                    if(c.cityId=="hue"&&challenge.mode=="maze")
                    {
                        yield return Click("CatClose",true);Map.View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[game.Save.node]));yield return null;
                        CatCheckMapControls();yield return Snapshot("CAT-Hue-map-controls");yield return CatHub();
                    }
                    if(FindButton("CatSubmit")==null)yield return CatClick("CatTask_"+challenge.id);
                    Check(!FindButton("CatSubmit").interactable,"Empty trial cannot submit: "+challenge.id);
                    if(modes.Add(challenge.mode))yield return Snapshot("CAT-mode-"+challenge.mode);
                    if(challenge.mode=="choice"||challenge.mode=="scenario")
                    {
                        yield return Click("CatChoice"+((challenge.answer+1)%challenge.options.Length),true);
                        Check(!cats.Ready(challenge),"Wrong answer explains and allows retry: "+challenge.id);
                        yield return Click("CatChoice"+challenge.answer,true);
                    }
                    else if(challenge.mode=="hidden")
                    {
                        yield return Click("CatHidden"+challenge.tokens.Length,true);
                        Check(!cats.Ready(challenge),"Hidden-object distractor cannot complete: "+challenge.id);
                        for(var i=0;i<challenge.tokens.Length;i++)yield return Click("CatHidden"+i,i%2==0);
                    }
                    else if(challenge.mode=="maze")
                    {
                        Check(FindButton("CatCell"+challenge.exit).GetComponentInChildren<Text>().text=="Mèo","Maze destination uses a generic cat label: "+c.cityId);
                        var path=CatMazeRoute(challenge);
                        Check(!cats.Walk(challenge.id,challenge.exit),"Maze cannot teleport to exit: "+challenge.id);
                        mazeCities.Add(c.cityId);
                        var mazeBoardObject=GameObject.Find("CatChallengeBoard");var mazeBoard=mazeBoardObject.transform;
                        var mazeCells=Enumerable.Range(0,challenge.maze.Length*challenge.maze[0].Length).Select(i=>FindButton("CatCell"+i)).ToArray();
                        var mazePositions=mazeCells.Select(b=>((RectTransform)b.transform).anchoredPosition).ToArray();
                        Check(mazeCells.Length==25,"Maze uses the expected 5x5 board: "+c.cityId);
                        Check(mazeCells.All(b=>b.transition==Selectable.Transition.None&&b.GetComponent<UIButtonFeedback>()?.enabled!=true),"Maze tiles do not animate or sink under touch: "+c.cityId);
                        yield return Snapshot("CAT-maze-"+c.cityId+"-before-move");
                        yield return Click("CatCell"+path[1],true);
                        Check(CatMazeStillMounted(mazeBoardObject,mazeBoard,mazeCells,mazePositions)&&FindButton("CatCell"+cats.MazePosition(challenge)).GetComponentInChildren<Text>().text=="Lumi","First step keeps the same quiet 5x5 board and moves only Lumi: "+c.cityId);
                        yield return Snapshot("CAT-maze-"+c.cityId+"-after-move");
                        yield return Click("CatMazeReset",true);
                        Check(CatMazeStillMounted(mazeBoardObject,mazeBoard,mazeCells,mazePositions)&&cats.MazePosition(challenge)==challenge.start&&!cats.Ready(challenge),"Reset clears the route without rebuilding or shifting the maze: "+c.cityId);
                        foreach(var cell in path.Skip(1))
                        {
                            yield return Click("CatCell"+cell,true);
                            Check(CatMazeStillMounted(mazeBoardObject,mazeBoard,mazeCells,mazePositions),"Maze step preserves all cell objects and positions: "+c.cityId+" cell "+cell);
                        }
                    }
                    else
                    {
                        yield return CatDrag("CatPiece0","CatTarget1",false);
                        Check(cats.State(challenge)?.marks.Count!=challenge.tokens.Length,"Wrong drop cannot finish: "+challenge.id);
                        for(var i=0;i<challenge.tokens.Length;i++)
                        {
                            yield return CatDrag("CatPiece"+i,"CatTarget"+i,i%2==1);
                            Check(cats.State(challenge)?.marks.Contains(i)==true,"Mouse/touch matching accepted: "+challenge.id+" piece "+i);
                            if(i==0&&cityIndex==0)
                            {
                                yield return Click("CatClose",true);yield return CatHub();yield return CatClick("CatTask_"+challenge.id);
                                Check(GameObject.Find("CatPiece0").GetComponent<CatDragHandle>().placed,"Close/reopen restores partial placement");
                            }
                        }
                    }
                    if(challenge.mode=="hanbok")
                    {Check(!cats.Ready(challenge),"Hanbok requires a chosen palette as well as garment matching");yield return Click("CatPalette2",true);Check(cats.State(challenge).paletteChosen&&cats.State(challenge).palette==2,"Hanbok palette is saved");}
                    Check(cats.Ready(challenge),"Interaction proof ready: "+challenge.id);
                    yield return Click("CatSubmit",true);
                    Check(cats.Progress(c.cityId).completed.Contains(challenge.id),"Explicit completion persists: "+challenge.id);
                    yield return Click("CatFactContinue",true);
                }
                if(!cats.At(c.rescueNode)){yield return CatTravelButton("CatRescueAction",c.rescueNode);Check(FindButton("CatFinalContinue")!=null,"Rescue location opens final rescue panel: "+c.cityId);}
                if(FindButton("CatFinalContinue")==null)yield return CatClick("CatRescueAction");var coins=game.Save.coins;
                yield return CatDrag("RescueCat","CatShelter",true);
                Check(cats.Rescued(c.cityId)&&game.Save.coins==coins+c.rewardCoins,"Actual rescue gesture awards exactly once: "+c.cityId);
                Check(!cats.Rescue(Vector2.zero)&&game.Save.coins==coins+c.rewardCoins,"Repeated rescue rejected: "+c.cityId);
                if(c.cityId=="hoi_an")Check(Map.CatLitLanterns>0,"Hoi An rescue lights the additive lantern layer without legacy gate proofs");
                yield return Snapshot("CAT-rescued-"+c.cityId);yield return Click("CatFinalContinue",true);
                Check(FindButton("CatPostcard").interactable,"Rescued cat, badge and postcard available: "+c.cityId);
                yield return Click("CatPostcard",true);yield return Click("CatPostcardBack",true);
                yield return Click("CatClose",true);
                if(c.cityId=="hoi_an")yield return Snapshot("CAT-map-restored-hoi_an");
                game.Reload();Check(game.Cats.Rescued(c.cityId),"Disk reload keeps completed rescue: "+c.cityId);
            }
            Check(game.Save.catRescues.Count(p=>p.rescued)==count,"All requested rescue chapters persist");
            if(!Environment.GetCommandLineArgs().Contains("--cat-rescue-pilot"))
                Check(mazeCities.SetEquals(new[]{"hue","da_lat","gyeongju","seoul"}),"All four city mazes were exercised without panel reloads");
            Check(game.Save.completed.Count==0&&game.Save.defeatedEnemies.Count==0&&game.Save.solvedPuzzles.Count==0,"Cat campaign never fabricates legacy quest, combat or puzzle completion");
            game.Persist();
        }
    }
}
#endif
