"""Bounded snapshot/protection for the live owner UX correction; no game-state edits."""
import hashlib,json,re,shutil
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Docs/QA/OwnerConfusion-20261020'
FILES=['Assets/_Game/Scripts/Core/QuestManager.Guidance.cs','Assets/_Game/Scripts/Maps/NodeMarkerResolver.cs','Assets/_Game/Scripts/Maps/NodeMarkerView.cs','Assets/_Game/Scripts/Maps/CityMap.cs','Assets/_Game/Scripts/Maps/CityMap.Guidance.cs','Assets/_Game/Scripts/UI/GameplayPanels.Relics.cs','Assets/_Game/Scripts/UI/GameplayPanels.cs','Assets/_Game/Scripts/UI/PlayerCopy.cs','Assets/_Game/Scripts/Editor/AdventureUISetup.cs','Assets/_Game/Resources/AdventureUIArt.asset','Assets/_Game/Resources/Content/AdventurePresentation.json','Assets/_Game/Resources/Content/GameplayContent.json']
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
if __name__=='__main__':
    OUT.mkdir(parents=True,exist_ok=True)
    if not (OUT/'before.json').exists():
        hashes={}
        for f in FILES:
            p=ROOT/f;dest=OUT/'before-source'/f;dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest);hashes[f]=sha(p)
        for f in ['Assets/_Game/Resources/Content/CityNavigation.json','Assets/_Game/Resources/Content/CulturalRelics.json','Assets/_Game/Scripts/Maps/CityRouteGeometry.cs','Assets/_Game/Scripts/Core/SaveSystem.cs']:
            if (ROOT/f).exists():hashes[f]=sha(ROOT/f)
        for p in (ROOT/'Assets/_Game/Art').rglob('*.png'):
            if re.search(r'(Overview|City_.*_A|Map.*_A)',p.name,re.I):hashes[str(p.relative_to(ROOT)).replace('\\','/')]=sha(p)
        (OUT/'before.json').write_text(json.dumps(hashes,indent=2),encoding='utf-8')
        print('Snapshot saved:',len(hashes),'files')
    else:print('Existing baseline retained')
