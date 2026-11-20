"""Bounded source protection and incremental delivery for H3 pickup UX."""
from pathlib import Path
import hashlib,json,shutil,sys
R=Path(__file__).resolve().parents[1];D=R/'Docs/QA/H3TapPickup-20261020'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def dump(p,x):p.write_text(json.dumps(x,ensure_ascii=False,indent=2),encoding='utf-8')
if sys.argv[1]=='prepare':
 assert not(D/'baseline.json').exists();D.mkdir(parents=True,exist_ok=True)
 files=['Assets/_Game/Scripts/HoiAn/InteractionSystem.cs','Assets/_Game/Scripts/HoiAn/HoiAnGameplay.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Editor/Phase5Setup.cs']
 for name in files:
  p=D/'before-source'/name;p.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(R/name,p)
 protected=['Assets/_Game/Scripts/HoiAn/HoiAnPlayerController.cs','Assets/_Game/Scripts/HoiAn/HoiAnItem.cs','Assets/_Game/Scripts/Core/QuestManager.cs','Assets/_Game/Scripts/Core/SaveSystem.cs','Assets/_Game/Scripts/Core/GameData.cs','Assets/_Game/Scripts/Core/CityNavigation.cs','Assets/_Game/Resources/Content/CityNavigation.json','Assets/_Game/Resources/Content/GameplayContent.json','Assets/_Game/Resources/HoiAnData.json','Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Scripts/Maps/CityMapPaths.cs','Assets/_Game/Scripts/Maps/CityRouteGeometry.cs','ProjectSettings/ProjectSettings.asset']
 protected += [str(p.relative_to(R)).replace('\\','/')for p in (R/'Assets/_Game/Scenes').glob('*.unity')]
 dump(D/'baseline.json',{'base':'D:/GAMEKKK/Backups/CheckpointCasualAdventureUI-20261020','sourceBefore':files,'protected':[{'path':p,'sha256':sha(R/p)}for p in protected]})
 print('Baseline saved; H3 physics, collection service, quests, save, maps/navigation and scenes protected')
elif sys.argv[1]=='verify':
 checks=[{'path':e['path'],'unchanged':sha(R/e['path'])==e['sha256']}for e in read(D/'baseline.json')['protected']]
 result={'pass':all(e['unchanged']for e in checks),'checks':checks};dump(D/'protection.json',result)
 print(json.dumps({'pass':result['pass'],'files':len(checks),'failures':[e['path']for e in checks if not e['unchanged']]}));assert result['pass']
