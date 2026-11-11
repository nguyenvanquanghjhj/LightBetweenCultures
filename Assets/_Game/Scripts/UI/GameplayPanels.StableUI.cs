using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    // Visual-only refresh. Existing callbacks, battle rules and save data are unchanged.
    public sealed partial class GameplayPanels
    {
        int liveBattleNode = -1;
        bool liveBattleActive, liveBattleShielded, liveBattleRepair;
        int liveDialogueNode = -1;
        string liveDialogueOptions;

        private static Text TextAt(Transform root, string path)
        {
            var child = root == null ? null : root.Find(path);
            return child == null ? null : child.GetComponent<Text>();
        }
        private static void SetTextAt(Transform root, string path, string value)
        {
            var label = TextAt(root, path);
            if (label != null && label.text != value) label.text = value;
        }
        private static void UpdateButton(Transform root, string name, string label, bool enabled = true)
        {
            var node = root == null ? null : root.Find(name);
            if (node == null) return;
            var button = node.GetComponent<Button>();
            if (button != null) button.interactable = enabled;
            var caption = node.GetComponentInChildren<Text>();
            if (caption != null) caption.text = label;
        }
        private static void RefreshHealth(Transform root,string key,int amount,int maximum)
        {
            var fill = root.Find(key+"Bar/Fill") as RectTransform;
            if (fill != null) fill.anchorMax = new Vector2(Mathf.Clamp01((float)amount/Math.Max(1, maximum)),1f);
            SetTextAt(root,key+"Bar/"+key,amount+" / "+maximum+" HP");
        }
        private void RecordBattlePanel(int node)
        {
            var battle=game.Battles;
            liveBattleNode=node;
            liveBattleActive=battle.Active;
            liveBattleShielded=battle.Shielded;
            liveBattleRepair=battle.CanRepair;
        }
        // Called before Sheet(). Routine turns do NOT tear down and rebuild the UI.
        private bool TryRefreshBattleInPlace(int node)
        {
            var battle=game.Battles;
            var sheet=overlay==null?null:overlay.Find("BattlePanel");
            if(sheet==null||node!=liveBattleNode||battle.Active!=liveBattleActive||battle.Shielded!=liveBattleShielded||battle.CanRepair!=liveBattleRepair)
                return false;
            RefreshHealth(sheet,"PlayerHP",battle.PlayerHP,100);
            RefreshHealth(sheet,"EnemyHP",battle.EnemyHP,battle.EnemyMaxHP);
            var turn=battle.Active?"Lượt "+battle.TurnNumber+" · Lumi hành động":battle.Outcome=="win"?"CHIẾN THẮNG":battle.Outcome=="loss"?"LUMI ĐÃ HỒI SỨC · CÓ THỂ THỬ LẠI":"ĐÃ RÚT LUI";
            var intent=battle.Active?"Lượt "+battle.TurnNumber+" · Ánh sáng "+battle.Energy+"/3\nĐáp trả: "+BattleCounterName()+" · "+battle.EnemyAttack+" HP":turn+"\nNăng lượng ánh sáng: "+battle.Energy+"/3";
            SetTextAt(sheet,"BattleTurn",intent);
            SetTextAt(sheet,"BattleLog",battle.Log);
            if(battle.Active)
            {
                if(!string.IsNullOrEmpty(battle.Enemy?.mechanic)&&!battle.Shielded)
                    SetTextAt(sheet,"BattleMechanic",battle.Enemy.mechanic=="two_phase"?"PHA "+battle.Phase+" / 2 · "+(battle.Phase==2?"Lõi đã lộ":"Phá lớp khiên"):"Khiên mở · Nhịp "+(battle.ShieldBeat%3+1)+" / 3");
                UpdateButton(sheet,"BattleAttack","Tấn công\n"+(battle.Shielded?0:Math.Max(1,9-battle.EnemyDefense))+" sát thương");
                UpdateButton(sheet,"BattleDefend","Phòng thủ · +"+(battle.Energy<3?1:0)+"\nChịu "+Math.Max(1,(battle.EnemyAttack+1)/2)+" sát thương");
                UpdateButton(sheet,"BattleLight","Tia sáng · −1\n"+(battle.Shielded?0:Math.Max(1,14-battle.EnemyDefense/2))+" sát thương",battle.Energy>0);
                var heal=Math.Min(100-battle.PlayerHP,Math.Max(1,game.Content.Item("healing_tea")?.healHP??35));
                UpdateButton(sheet,"BattleHeal","Uống trà × "+game.Inventory.Count("healing_tea")+" · +"+heal+" HP\n1 ly · 1 lượt",battle.PlayerHP<100&&game.Inventory.Has("healing_tea"));
                if(battle.CanRepair)UpdateButton(sheet,"BattleRepair","Sửa mạch "+battle.RepairProgress+" / 3");
            }
            return true;
        }
        private void RefreshPuzzleInteraction(RectTransform sheet)
        {
            var puzzle=game.Puzzles;
            var def=puzzle.Definition;
            if(def==null||sheet==null)return;
            SetTextAt(sheet,"PuzzleLog",puzzle.Log);
            if(def.mode=="fold")
            {
                var paper=sheet.Find("HanjiPaper") as RectTransform;
                if(paper!=null)
                {paper.localEulerAngles=new Vector3(0,0,puzzle.Rotations*90);paper.localScale=new Vector3(puzzle.Folds>0?.55f:1f,1f,1f);}
            }
            if(def.mode=="rings")
            {
                for(var i=0;i<3;i++)
                {
                    var wheel=sheet.Find("RingVisual_"+i) as RectTransform;
                    if(wheel!=null)wheel.localEulerAngles=new Vector3(0,0,puzzle.Rings[i]*120);
                    if(def.options!=null&&puzzle.Rings[i]>=0&&puzzle.Rings[i]<def.options.Length)
                        UpdateButton(sheet,"LanternRing_"+i,def.options[puzzle.Rings[i]]);
                }
            }
        }
        private void RefreshShopRowsInPlace()
        {
            if(shopContent==null||shopSheet==null)return;
            if(shopCoins!=null)shopCoins.text=game.Save.coins+" Xu Hành Trình";
            foreach(var entry in JourneyShop.Catalog)
            {
                var row=shopContent.Find("ShopRow_"+entry.id);
                if(row==null)continue; // Filtered-out rows are not recreated.
                SetTextAt(row,"ShopPrice_"+entry.id,entry.price+" xu"+(entry.Permission?" · một lần":" · ×1 · Trong túi: "+game.Inventory.Count(entry.itemId)));
                var reason=game.Shop.Unavailable(entry);
                UpdateButton(row,"ShopBuy_"+entry.id,string.IsNullOrEmpty(reason)?"Mua":reason,string.IsNullOrEmpty(reason));
            }
        }
        private void RememberDialoguePanel(int node, EventView view)
        {
            liveDialogueNode=node;
            liveDialogueOptions=string.Join("",view.options.Select(o=>o.id).ToArray());
        }
        // Reuse the existing speech panel when its actions and speaker remain unchanged.
        // Changes to the action set intentionally fall back to a complete redraw.
        private bool TryRefreshDialogueInPlace(int node,EventView view,EventDefinition definition,string notice)
        {
            var sheet=overlay==null?null:overlay.Find("NodeEventPanel");
            if(sheet==null||receipt!=null||node!=liveDialogueNode||
                liveDialogueOptions!=string.Join("",view.options.Select(o=>o.id).ToArray()))return false;
            var speaker=string.IsNullOrEmpty(view.speaker)?view.title:view.speaker;
            if(TextAt(sheet,"EventSpeaker")?.text!=speaker)return false;
            var text=PlayerCopy.DialogueText(view.text,definition);
            var feedback=PlayerCopy.Display(notice);
            if(string.IsNullOrWhiteSpace(feedback)||text.Contains(feedback)||feedback==text)feedback="";
            else if(feedback.Length>100){if(!feedback.Contains(text))text+="\n\n"+feedback;else text=feedback;feedback="";}
            var noticeText=TextAt(sheet,"EventNotice");
            if(!string.IsNullOrEmpty(feedback)&&(noticeText==null||noticeText.rectTransform.rect.height<20))return false;
            // A changed option requiring more vertical space must NOT overflow the hitbox.
            foreach(var option in view.options)
            {
                var button=sheet.Find("EventActionsViewport/EventActionsContent/EventOption_"+option.id);
                if(button==null)return false;
                var caption=PlayerCopy.ActionLabel(option.id,option.label,definition);
                if(!option.enabled)caption+="\n"+PlayerCopy.Unavailable(game,node,option,definition);
                var rect=button as RectTransform;
                if(rect==null||TextHeight(caption,rect.rect.width-20,17)+12>rect.rect.height)return false;
            }
            SetTextAt(sheet,"EventText",text);
            if(noticeText!=null)noticeText.text=feedback;
            foreach(var option in view.options)
            {
                var caption=PlayerCopy.ActionLabel(option.id,option.label,definition);
                if(!option.enabled)caption+="\n"+PlayerCopy.Unavailable(game,node,option,definition);
                UpdateButton(sheet.Find("EventActionsViewport/EventActionsContent"),"EventOption_"+option.id,caption,option.enabled);
            }
            var scroll=sheet.Find("DialogueTextViewport")?.GetComponent<ScrollRect>();
            if(scroll!=null)scroll.verticalNormalizedPosition=1f;
            return true;
        }
    }
}
