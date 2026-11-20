"""Bounded presentation patch, requires before-source snapshots. No route edits."""
from pathlib import Path
import json,re
R=Path(__file__).resolve().parents[1]
def edit(path,changes):
 p=R/path;s=p.read_text(encoding='utf-8-sig')
 for old,new in changes:
  assert old in s,(path,old[:90]);s=s.replace(old,new)
 p.write_text(s,encoding='utf-8')
def main():
 edit('Assets/_Game/Scripts/UI/UIController.cs',[
  ('Message.text = value;','Message.text = PlayerCopy.Display(value);'),('text.font = Font; text.text = value;','text.font = Font; text.text = PlayerCopy.Display(value);')])
 edit('Assets/_Game/Scripts/UI/GameplayPanels.cs',[
  ('var text=view.text??"";var feedbackText=notice??"";','var text=PlayerCopy.Display(view.text);var feedbackText=PlayerCopy.Display(notice);'),
  ('var optionHeights=new float[count];float optionsHeight=0;\n            for(var i=0;i<count;i++){optionHeights[i]=Mathf.Max(52,TextHeight(view.options[optionPage*3+i].label,width*.80f-36,17)+20);optionsHeight+=optionHeights[i]+8;}',
   'var optionHeights=new float[count];var captions=new string[count];float optionsHeight=0;\n            for(var i=0;i<count;i++){var option=view.options[optionPage*3+i];captions[i]=PlayerCopy.ActionLabel(option.id,option.label,definition);if(!option.enabled)captions[i]+="\\n"+PlayerCopy.Unavailable(game,node,option,definition);optionHeights[i]=Mathf.Max(52,TextHeight(captions[i],width*.80f-36,17)+20);optionsHeight+=optionHeights[i]+8;}'),
  ('Place(nameplate,20,14,width-84,34)','Place(nameplate,116,14,width-140,34)'),
  ('var skip=MobileUI.IconButton(ui,sheet,"DialogueSkipEffect","",Vector2.zero,Vector2.zero,()=>{foreach(var p in overlay.GetComponentsInChildren<PortraitPresence>())p.Skip();},AdventureSymbol.Arrow);Place((RectTransform)skip.transform,width-60,8,52,52);',
   'var closeX=MobileUI.Button(ui,sheet,"DialogueCloseX","Đóng",Vector2.zero,Vector2.zero,Close,illustrated:true);Place((RectTransform)closeX.transform,16,8,94,52);MobileUI.Cue(closeX,AdventureSymbol.Close);'),
  ('Place(nameplate,20,14,width-164,34);','Place(nameplate,116,14,width-252,34);'),
  ('(optionPage+1)+" / "+pages+" ›"','"Thêm · "+(optionPage+1)+"/"+pages'),
  ('width-140,8,76,52','width-126,8,110,52'),
  ('"EventOption_"+option.id,option.label','"EventOption_"+option.id,captions[i]'),
  ('MobileUI.Cue(button,option.id=="checkin"||option.id=="checkin_view"?AdventureSymbol.Camera:i==0?AdventureSymbol.Check:AdventureSymbol.Chat);','MobileUI.Cue(button,PlayerCopy.ActionIcon(option.id));'),
  ('var hasNext=lumiSpeaking||definition?.type=="npc"||!string.IsNullOrEmpty(definition?.npcId)||definition?.actor=="Lumi";','var hasNext=lumiSpeaking;'),
  ('count>0?"Nhận sau / Trở lại":"Trở lại hành trình"','"Trở về bản đồ"'),
  ('"DialogueNext","Tiếp theo"','"DialogueNext",definition?.actor=="Cô Mai"?"Nghe Cô Mai":"Xem lựa chọn"'),
  ('"Giữ lại / Nói chuyện tiếp"','"Hủy · Giữ vật phẩm"'),
  ('"Đã nhận HQ01 · Nhật ký đã cập nhật."','"Đã nhận nhiệm vụ · Xem việc cần làm trong nhật ký."')])
 edit('Assets/_Game/Scripts/Core/QuestManager.Guidance.cs',[
  ('int guidanceKey=int.MinValue;string[] guidance;','int guidanceKey=int.MinValue;string[] guidance;int guidanceTarget=-1;\n        public int GuidanceTarget {get {Guidance();return guidanceTarget;}}'),
  ('if(target<0)return new[]{','guidanceTarget=target;\n            if(target<0)return new[]{'),
  ('"Chọn Tương tác ở địa điểm này. Vật phẩm và tiến độ cần thiết được ghi trong nhật ký."','"Chạm vào địa điểm được chỉ dẫn để Lumi đi tới, rồi chọn hành động trong khung trò chuyện. Nhật ký lưu lại việc còn thiếu."')])
 edit('Assets/_Game/Scripts/UI/GameplayPanels.Guidance.cs',[
  ('"Gợi ý theo tiến độ hiện tại · Không mở đường hay bỏ qua nhiệm vụ"','"Đọc lời nhắc hoặc tìm địa điểm trên bản đồ"'),
  ('if(level<2)MobileUI.Button','var target=game.Quests.GuidanceTarget;\n            if(target>=0)MobileUI.Button(ui,sheet,"GuidanceLocate","Xem địa điểm trên bản đồ",new Vector2(.12f,.32f),new Vector2(.88f,.40f),()=>{Close();Object.FindAnyObjectByType<CityMap>()?.FocusObjective();},true,true);\n            if(level<2)MobileUI.Button'),
  ('new Vector2(.09f,.36f),new Vector2(.91f,.73f)','new Vector2(.09f,.45f),new Vector2(.91f,.73f)')])
 edit('Assets/_Game/Scripts/Maps/CityMap.Guidance.cs',[
  ('"ObjectiveHint","Gợi ý"','"ObjectiveHint","Việc cần làm"'),('AdventureSymbol.Puzzle','AdventureSymbol.Journal'),
  ('new Vector2(70,143)','new Vector2(126,143)'),
  ('        void CreateGuidance()','        public void FocusObjective()\n        {\n            var target=game.Quests.GuidanceTarget;if(target<0||!game.Discovery.IsVisible(target))return;\n            SelectedNode=target;tooltipName.text=game.Data.nodes[target].name;tooltipState.text="Điểm đến của nhiệm vụ · Chạm để đi tới";\n            View.ZoomBy(Mathf.Max(1,1.6f/View.Zoom));View.Focus(CityRouteGeometry.Arrival(game.Data.nodes[target]));\n            paths.Highlight(game.Data.FindPath(game.Save.node,target,game.Save));\n        }\n        void CreateGuidance()')])
 edit('Assets/_Game/Scripts/Maps/CityMap.cs',[
  ('city.name.ToUpperInvariant()+" · NHẬT KÝ KHÁM PHÁ"','"Toàn cảnh · Kéo và chụm hai ngón để xem gần"'),
  ('new Vector2(.10f,.88f),new Vector2(.90f,.95f),20,false','new Vector2(.07f,.04f),new Vector2(.93f,.09f),15,false'),
  ('Mathf.Clamp(viewport.rect.height/map.rect.height,1,1.75f)','Mathf.Clamp(viewport.rect.height/map.rect.height,1,MapPanZoom.MaxZoom)'),
  ('            Refresh();\n        }\n\n        private void CompactHeader()',
   '            Refresh();\n            if(PlayerCopy.AutoGuide(game)){dialogue=true;Refresh();panels.ShowPlayerGuide();}\n        }\n\n        private void CompactHeader()'),
  ('"",new Vector2(1,0),new Vector2(1,0),clicks[i],symbols[i]','new[]{"Thu nhỏ","Phóng to","Toàn cảnh","Lumi"}[i],new Vector2(1,0),new Vector2(1,0),clicks[i],symbols[i]'),
  ('NodeMarkerView.SetMotionMode((MarkerMotionMode)(((int)NodeMarkerView.MotionMode+1)%3));\n                motionButton.transform.Find("Label").GetComponent<Text>().text=MotionCaption();','if(dialogue||Moving)return;dialogue=true;Refresh();panels.ShowMapMotion();'),
  ('private static string MotionCaption()=>NodeMarkerView.MotionMode==MarkerMotionMode.Off?"Tắt":NodeMarkerView.MotionMode==MarkerMotionMode.Reduced?"Nhẹ":"Nhịp";','private static string MotionCaption()=>"Hiệu ứng";'),
  ('var leisure="\\n\\nTRẢI NGHIỆM THƯ GIÃN · "+game.Save.leisureCompleted.Distinct().Count()+" / 12\\nCác hoạt động này hoàn toàn tùy chọn, không mở nhiệm vụ hay di vật.";',
   'var leisure="\\n\\nNHỮNG KHOẢNG NGHỈ\\nXích đu và thả hoa đăng là hoạt động tùy chọn. Bạn có thể chơi lại; chúng không trao vật phẩm hay hoàn tất nhiệm vụ.";'),
  ('game.Quests.Journal+leisure','"VIỆC CẦN LÀM\\n"+game.Quests.Guidance()+"\\n\\nNHIỆM VỤ\\n"+game.Quests.Journal+"\\n\\nĐỊA ĐIỂM ĐÃ GHÉ\\n"+string.Join(" · ",game.Save.visited.Where(i=>i>=0&&i<game.Data.nodes.Length).Select(i=>game.Data.nodes[i].name).Take(8))+leisure'),
  ('new Vector2(.52f,.04f),new Vector2(.94f,.13f)','new Vector2(.68f,.04f),new Vector2(.95f,.13f)'),
  ('new Vector2(.06f,.04f),new Vector2(.49f,.13f)','new Vector2(.04f,.04f),new Vector2(.35f,.13f)'),
  ('MobileUI.Cue(collection,AdventureSymbol.Check);','MobileUI.Cue(collection,AdventureSymbol.Check);\n                MobileUI.Button(ui,panel,"JournalGuide","Cách chơi",new Vector2(.37f,.04f),new Vector2(.66f,.13f),()=>{panel.gameObject.SetActive(false);Destroy(panel.gameObject);panels.ShowPlayerGuide();},illustrated:true);'),
  ('tooltipName.text = data.id+" · "+data.name;','tooltipName.text = data.name;')])
 edit('Assets/_Game/Scripts/Maps/MainMenu.cs',[
  ('new Vector2(.12f,.34f),new Vector2(.88f,.76f)','new Vector2(.12f,.43f),new Vector2(.88f,.78f)'),
  ('"Start","Bắt đầu"','"Start",game.Save.accepted.Count==0?"Bắt đầu hành trình":"Tiếp tục hành trình"'),
  ('            if(SaveSystem.RecoveryBlocked)','            ui.Button(ui.Root,"MainGuide","Tôi là Lumi · Xem cách chơi",new Vector2(.15f,.33f),new Vector2(.85f,.41f),()=>new GameplayPanels(ui,game,DesignKitArt.Load(),null,null).ShowPlayerGuide());\n            if(SaveSystem.RecoveryBlocked)')])
 # Real copy changes only: no ID, answer, condition, quantity or reward mutation.
 p=R/'Assets/_Game/Resources/Content/GameplayContent.json';j=json.loads(p.read_text(encoding='utf-8-sig'))
 legacy=json.loads((R/'Assets/_Game/Resources/HoiAnData.json').read_text(encoding='utf-8-sig'));names={n['id']:n['name'] for n in legacy['nodes']}
 player_fields={'text','offer','reminder','thanks','description','preview','missingText','completedText','confirmationText','alternateText','alternateCompletedText','battleCompletedText','clue'}
 changes=[]
 def clean(record,where):
  for k,v in record.items():
   if k in player_fields and isinstance(v,str):
    n=re.sub(r'\bH\d{1,2}\b',lambda m:names.get(m[0],m[0]),v)
    if n!=v:changes.append({'record':where,'field':k,'before':v,'after':n});record[k]=n
 for kind in ('events','quests','puzzles'):
  for e in j[kind]:
   if e.get('cityId')=='hoi_an':clean(e,kind+':'+e['id'])
 for e in j['items']:
  if re.search(r'\bH\d{1,2}\b',e.get('description','')):clean(e,'items:'+e['id'])
 q=next(q for q in j['quests'] if q['id']=='HQ01');old=q['reminder'];q['reminder']='Hãy tìm Mắt Đèn trong Hẻm mái ngói và mang về cho cô. Sau đó, ghé Chợ ven sông tìm manh mối cho Xưởng ghép giấy. Ông Phúc có thể giúp cháu tìm cách qua bot mà không giao chiến.'
 changes.append({'record':'quests:HQ01','field':'reminder','before':old,'after':q['reminder']})
 p.write_text(json.dumps(j,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 (R/'Docs/QA/PlayerExperience-20261019/copy-changes.json').write_text(json.dumps(changes,ensure_ascii=False,indent=2),encoding='utf-8')
 print('Applied bounded UI/copy patch; changed text records',len(changes))
if __name__=='__main__':main()
