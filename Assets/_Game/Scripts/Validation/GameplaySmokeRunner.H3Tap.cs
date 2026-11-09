#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using LightBetweenCultures.HoiAn;
using UnityEngine;
using UnityEngine.UI;
namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator H3TapJourney()
        {
            musicVirtualFocus=true;
            var restart=Environment.GetCommandLineArgs().Contains("--h3-tap-restart");
            if(restart)
            {
                Check(game.Inventory.Has(game.Data.quest.item)&&!game.Quests.Completed,"Restart begins with the actually picked-up eye, before tutorial return");
                var saved=JsonUtility.ToJson(game.Save);yield return CrossEnter("hoi_an");
                yield return Travel(NodeIndex("H3"),true);yield return Click("EventOption_scene",true);yield return Scene("HoiAnDemo");
                var item=FindAnyObjectByType<HoiAnItem>(FindObjectsInactive.Include);
                Check(item!=null&&!item.gameObject.activeSelf&&FindButton("Collect")==null,"Restart hides owned Mắt Đèn and pickup control");
                saved=JsonUtility.ToJson(game.Save); // Entering H3 legitimately updates previousNode before this interaction.
                FindAnyObjectByType<InteractionSystem>().Interact();
                Check(JsonUtility.ToJson(game.Save)==saved,"Repeated interaction after restart changes no item or reward state");
                yield return Snapshot("H3-restart-no-duplicate");yield return Click("Return",true);yield return Scene("CityMap");
                yield return Travel(NodeIndex("H2"),true);yield return Click("EventOption_tutorial_return",true);
                Check(game.Quests.Completed&&!game.Quests.IsCompleted("HQ01")&&!game.IsCityUnlocked("hue"),"Existing tutorial return still grants only local completion, preserving station/world gate");
                KeepSave("H3-tap-returned");yield break;
            }
            Check(game.Save.accepted.Count==0&&!game.Inventory.Has(game.Data.quest.item),"H3 pickup UX starts fresh, without injected quest or items");
            yield return CrossEnter("hoi_an");yield return Travel(NodeIndex("H2"),true);yield return Click("EventOption_accept",true);
            yield return CloseEvent();yield return Travel(NodeIndex("H3"),true);yield return ExploreH3();
            Check(!game.Quests.Completed&&!game.Quests.IsCompleted("HQ01")&&!game.IsCityUnlocked("hue"),"Tap approach and pickup do not complete tutorial/main quest or world unlock");
            KeepSave("H3-tap-earned");
        }
        IEnumerator H3TapChecks(HoiAnPlayerController player,HoiAnItem item)
        {
            var mouseTap=Environment.GetCommandLineArgs().Contains("--h3-tap-mouse");
            var interaction=player.GetComponent<InteractionSystem>();var target=(Vector2)item.transform.position;
            var stand=new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right}.Select(d=>target+d*2.2f)
                .First(p=>HoiAnPlayerController.Walkable(p)&&HoiAnPlayerController.FindPath(player.Position,p)!=null&&
                    Physics2D.Linecast(p,target,HoiAnPlayerController.ObstacleMask).collider==null);
            Check(player.MoveTo(stand),"Manual ground path can still reach the area where Mắt Đèn is visible");
            yield return Until(()=>!player.Walking,"manual approach to visible item");yield return null;
            Check(!interaction.CanInteract&&FindButton("Collect")==null,"Pickup button is hidden while outside generous interaction range");
            var tap=interaction.ItemTapRect.center;
            Check(tap.x>0&&tap.x<Screen.width&&tap.y>0&&tap.y<Screen.height,"Actual item is on screen before the pointer test");
            Check(interaction.ItemTapRect.width>=48&&interaction.ItemTapRect.height>=48,"Mắt Đèn has a forgiving screen-space touch target");
            var before=JsonUtility.ToJson(game.Save);var start=player.Position;
            var block=new GameObject("QA_ONLY_H3_BlockedPickup");block.layer=8;block.transform.position=target;
            block.AddComponent<BoxCollider2D>().size=new Vector2(3.2f,3.2f);Physics2D.SyncTransforms();
            yield return Pointer(tap,!mouseTap);yield return new WaitForSecondsRealtime(.25f);
            Check(!player.Walking&&!interaction.Approaching&&Vector2.Distance(start,player.Position)<.03f,"No valid arrival path leaves Lumi stationary; no wall traversal");
            Check(FindAnyObjectByType<UIController>().Message.text.Contains("Chưa có lối đi"),"Blocked approach explains how to go around instead of silently failing");
            Check(JsonUtility.ToJson(game.Save)==before,"Blocked item tap cannot grant or alter quest state");
            yield return Snapshot("H3-tap-no-path");Destroy(block);yield return null;Physics2D.SyncTransforms();
            yield return Pointer(interaction.ItemTapRect.center,!mouseTap);yield return new WaitForSecondsRealtime(.08f);
            Check(interaction.Approaching&&player.Walking,"Tapping the actual item starts the existing collider-aware movement");
            yield return Snapshot("H3-tap-approaching");
            var deadline=Time.realtimeSinceStartup+12;var samples=0;
            while(player.Walking&&Time.realtimeSinceStartup<deadline)
            {
                Check(Physics2D.OverlapBox(player.Position,HoiAnPlayerController.Footprint*.95f,0,HoiAnPlayerController.ObstacleMask)==null,"Approach motion remains clear of walls, sample "+samples++);
                yield return new WaitForSecondsRealtime(.08f);
            }
            Check(!player.Walking&&interaction.CanInteract,"Auto approach arrives at a valid interaction position near the item");
            Check(Vector2.Distance(player.Position,target)<=InteractionSystem.PickupRange&&Vector2.Distance(player.Position,target)>.3f,"Lumi stands nearby instead of requiring the item's exact centre");
            Check(item.gameObject.activeSelf&&JsonUtility.ToJson(game.Save)==before,"Arriving only offers pickup; no automatic item/reward grant");
            yield return null;
            var button=FindButton("Collect");Check(button!=null&&button.GetComponentInChildren<Text>().text=="Nhặt Mắt Đèn","Arrival reveals the explicit pickup action");
            var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
            Check(corners[2].y-corners[0].y>=48&&corners[2].x-corners[0].x>=120,"Pickup button has a mobile-friendly actual screen hit area");
            yield return Snapshot("H3-tap-ready-to-pick");yield return Click("Collect",!mouseTap);
            var picked=JsonUtility.ToJson(game.Save);interaction.Interact();interaction.Interact();
            Check(JsonUtility.ToJson(game.Save)==picked,"Repeated Interact after pickup does not duplicate item or reward");
        }
    }
}
#endif
