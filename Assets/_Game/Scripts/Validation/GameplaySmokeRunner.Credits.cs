#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public sealed partial class GameplaySmokeRunner
    {
        IEnumerator CreditsPreview()
        {
            Check(game.Events.CanonicalEndingCompleted,"Credits presentation preview uses the previously earned ending save; not a new playthrough");
            var coins=game.Save.coins;var items=game.Save.items.ToArray();var ledger=game.Save.rewardGrantIds.ToArray();var completed=game.Save.completed.ToArray();
            Screen.SetResolution(540,1080,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
            yield return CrossEnter("seoul");yield return ChapterGo("S21");yield return ChapterChoice("ending");
            for(var i=0;i<6;i++)yield return Click("EndingNext",true);
            yield return Until(()=>GameObject.Find("CreditsPanel")!=null,"credits page",8);
            foreach(var size in new[]{new Vector2Int(540,1080),new Vector2Int(1280,720)})
            {
                Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);Canvas.ForceUpdateCanvases();
                var sheet=GameObject.Find("CreditsPanel").transform;
                var body=GameObject.Find("EndingText").GetComponent<Text>();var viewport=body.GetComponentInParent<ScrollRect>();
                var pixels=body.fontSize*body.GetComponentInParent<Canvas>().scaleFactor;
                File.AppendAllText(Path.Combine(output,"credits-layout.txt"),size.x+"x"+size.y+" | fontPixels="+pixels+" | textHeight="+body.preferredHeight+" | viewportHeight="+viewport.viewport.rect.height+"\n");
                Check(!body.resizeTextForBestFit&&pixels>=23,"Credits keep readable type without automatic shrinking at "+size.x+"x"+size.y);
                Check(body.text.Contains("Ý tưởng và phát triển: Nhóm phát triển Light Between Cultures")&&body.text.Contains("Cảm ơn những người đã đồng hành cùng Lumi")&&body.text.Contains("những nhịp cầu văn hóa vẫn còn mãi."),"Credits display the approved copy");
                Check(!body.text.Contains("bộ tài nguyên")&&!body.text.Contains("Cốt truyện, thiết kế và mỹ thuật"),"Credits omit the unsupported blanket art/story attribution");
                Check(body.preferredHeight<=viewport.viewport.rect.height+1,"Approved credits fit the visible body at "+size.x+"x"+size.y);
                var title=sheet.Find("RelicTitle").GetComponent<Text>();
                Check(title.text.Replace("\n"," ")=="LIGHT BETWEEN CULTURES"&&title.preferredHeight<=title.rectTransform.rect.height+1,"Complete game title fits without clipping");
                Check(!sheet.Find("ContextHelp").gameObject.activeSelf&&!sheet.Find("RelicExit").gameObject.activeSelf&&FindButton("EndingSkip")==null,"Credits have one clear continuation action");
                var corners=new Vector3[4];((RectTransform)FindButton("EndingNext").transform).GetWorldCorners(corners);
                Check(corners.All(p=>p.x>=0&&p.y>=0&&p.x<=Screen.width+1&&p.y<=Screen.height+1),"Credits continuation stays on screen");
                yield return Snapshot("Credits-"+size.x+"x"+size.y);
            }
            yield return Click("EndingNext",true);
            Check(game.Save.coins==coins&&game.Save.items.SequenceEqual(items)&&game.Save.rewardGrantIds.SequenceEqual(ledger)&&game.Save.completed.SequenceEqual(completed),"Previewing/closing credits preserves inventory, currency, quests and receipts");
        }
    }
}
#endif
