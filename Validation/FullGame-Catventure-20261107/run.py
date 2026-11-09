"""Read-only production QA orchestration. Native Players use isolated copies."""
from pathlib import Path
from datetime import datetime, timezone
import argparse, hashlib, json, os, shutil, subprocess, time

OUT=Path(__file__).resolve().parent
ROOT=OUT.parents[1]
EXE=ROOT/'Builds/Windows-Catventure-Fix-20261107/Catventure.exe'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(name,data): (OUT/name).write_text(json.dumps(data,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
def sources():
    return {p.relative_to(ROOT).as_posix():sha(p) for top in ['Assets','Packages','ProjectSettings'] for p in (ROOT/top).rglob('*') if p.is_file()}

def baseline():
    assert not (OUT/'source-before.json').exists()
    write('source-before.json',sources())
    owner=Path(os.environ['USERPROFILE'])/'AppData/LocalLow/DefaultCompany/LightBetweenCultures/hoi-an-v1.json'
    write('identity.json',dict(startedUTC=datetime.now(timezone.utc).isoformat(),player=str(EXE),assemblySHA256=sha(EXE.parent/'Catventure_Data/Managed/Assembly-CSharp.dll'),ownerSave=str(owner),ownerSaveSHA256=sha(owner) if owner.exists() else None))
    source=ROOT/'Tools/qa_content_20261025.py'
    text=source.read_text(encoding='utf-8').replace("O=R/'Validation/FullGame-20261025'", "O=R/'Validation/FullGame-Catventure-20261107'")
    exec(compile(text,str(source),'exec'),{'__file__':str(source)})
    result=subprocess.run(['C:/Users/admin/AppData/Local/Programs/Python/Python312/python.exe','-X','utf8',str(ROOT/'Tools/validate_catventure.py')],capture_output=True,text=True,encoding='utf-8')
    (OUT/'cat-content-audit.txt').write_text(result.stdout+result.stderr,encoding='utf-8');assert result.returncode==0

def run(name,args,seed=None,extras=None,limit=900):
    target=OUT/name;target.mkdir(exist_ok=False)
    if seed:shutil.copy2(seed,target/'gameplay-save.json')
    for filename,source in (extras or {}).items():shutil.copy2(source,target/filename)
    cmd=[str(EXE),'--gameplay-smoke','--gameplay-output='+str(target),'-screen-fullscreen','0','-logFile',str(target/'player.log'),*args]
    start=time.time();print('START '+name,flush=True)
    p=subprocess.Popen(cmd,cwd=ROOT,creationflags=subprocess.CREATE_NO_WINDOW)
    try:code=p.wait(timeout=limit)
    except subprocess.TimeoutExpired:p.kill();p.wait();code='TIMEOUT'
    candidates=[p for p in target.glob('*results.txt') if p.stat().st_mtime>=start-1]
    result=max(candidates,key=lambda p:p.stat().st_mtime) if candidates else None
    body=result.read_text(encoding='utf-8-sig') if result else ''
    status='PASS' if code==0 and body.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL' if body else 'BLOCKED'
    row=dict(name=name,status=status,exit=code,seconds=round(time.time()-start,2),assertions=sum(s.startswith('PASS:') for s in body.splitlines()),arguments=args,seed=str(seed) if seed else None,result=str(result) if result else None,assemblySHA256=sha(EXE.parent/'Catventure_Data/Managed/Assembly-CSharp.dll'),failureTail=body[-3500:] if status!='PASS' else None)
    (target/'receipt.json').write_text(json.dumps(row,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print('DONE '+name+' '+status+' checks='+str(row['assertions']),flush=True)
    return status

def campaigns():
    for route in ['VN_FIRST','KOREA_FIRST']:
        name='campaign-'+route.lower()
        if run(name,['--pc-full-gameplay','--phase4-e2e='+route])=='PASS':
            folder=OUT/name
            run(name+'-restart',['--pc-gameplay-restart'],folder/'gameplay-save.json',{'pc-final-state.json':folder/'pc-final-state.json'})
    run('all22-puzzles-earned',['--story-leisure-e2e','--phase4-e2e=VN_FIRST'])

def focus():
    earned=ROOT/'Validation/PcGameplayFix-20261024/campaign/pc-final-state.json'
    old=ROOT/'Validation/ThreeNpc-20261024/legacy-source.json'
    run('three-npc',['--three-npc-qa'],extras={'legacy-source.json':old})
    if (OUT/'three-npc/earned-after.json').exists():
        run('three-npc-restart',['--three-npc-qa','--three-npc-restart'],OUT/'three-npc/gameplay-save.json',{'earned-after.json':OUT/'three-npc/earned-after.json'})
    jobs=[
      ('node-panels',['--node-panel-qa'],earned),
      ('dialogue',['--dialogue-release-qa','--node-panel-qa','--buttons-partial-save='+str(ROOT/'Validation/ReleaseCandidate-20261024/shop-partial-hoian.json'),'--buttons-world-save='+str(ROOT/'Validation/ReleaseCandidate-20261024/focused/RC-earned.json')],earned),
      ('final-ux',['--final-ux-qa'],earned),
      ('hue-stamps',['--hue-snap-qa'],earned),
      ('save-recovery',['--release-candidate-qa'],earned),
      ('leisure12',['--casual-runtime','--casual-suppress-captures'],earned),
      ('map-return',['--map-return-qa'],None),
      ('shop-toolbar',['--shop-toolbar-qa'],earned),
      ('audio-input',['--soft-sfx-input'],earned),
      ('credits',['--credits-preview'],earned),
    ]
    for name,args,seed in jobs:run(name,args,seed)
    seed=OUT/'leisure12/Casual-earned-optional-lane.json'
    if not seed.exists():seed=OUT/'leisure12/gameplay-save.json'
    run('leisure-visuals',['--leisure-visuals'],seed)
    run('ending-gates-interruption',['--phase5-validation'],earned)

def extended():
    earned=ROOT/'Validation/PcGameplayFix-20261024/campaign/pc-final-state.json'
    run('all-npc-approaches',['--npc-standing-qa'],earned)
    run('all133-historical-save',['--full-story-postgame'],ROOT/'Validation/FullStory-20261017/Fresh04/full-story-final.json',limit=1200)
    if run('all-quests-story',['--full-story','--with-cross-city'],limit=1800)=='PASS':
        seed=OUT/'all-quests-story/gameplay-save.json'
        run('all-quests-restart',['--full-story-restart'],seed)
        run('all133-postgame',['--full-story-postgame'],seed,limit=1200)

def integrity():
    before=json.loads((OUT/'source-before.json').read_text());after=sources()
    identity=json.loads((OUT/'identity.json').read_text());owner=Path(identity['ownerSave'])
    data=dict(sourceFiles=len(before),productionSourceUnchanged=before==after,changedPaths=[k for k in before.keys()|after.keys() if before.get(k)!=after.get(k)],ownerSaveUnchanged=identity['ownerSaveSHA256']==(sha(owner) if owner.exists() else None))
    write('integrity.json',data);print(json.dumps(data));return data

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('mode',choices=['baseline','campaigns','focus','extended','integrity'])
    globals()[parser.parse_args().mode]()
