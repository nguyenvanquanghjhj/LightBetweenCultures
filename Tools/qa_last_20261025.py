"""Final complete dialogue invocation and earned campaign continuation."""
from pathlib import Path
import json, shutil, subprocess, time, sys
from qa_full_20261025 import sha, sources
R=Path(__file__).resolve().parents[1];O=R/'Validation/FullGame-20261025';exe=R/'Builds/Windows-ThreeNpcQA-20261025/LightBetweenCultures.exe'
earned=R/'Validation/PcGameplayFix-20261024/campaign/pc-final-state.json'
jobs=[('dialogue-complete-fixtures',['--dialogue-release-qa','--node-panel-qa','--buttons-partial-save='+str(R/'Validation/ReleaseCandidate-20261024/shop-partial-hoian.json'),'--buttons-world-save='+str(R/'Validation/ReleaseCandidate-20261024/focused/RC-earned.json')],earned),
      ('campaign-vietnam-earned-continuation',['--pc-full-gameplay','--pc-resume-opening','--phase4-e2e=VN_FIRST'],O/'jeonju-isolated/gameplay-save.json')]
rows=[]
ending_only='--ending-only' in sys.argv
if ending_only:
    jobs=[('ending-earned-tail',['--phase5-tail'],O/'campaign-vietnam-earned-continuation/gameplay-save.json')]
for name,args,seed in jobs:
    target=O/name;target.mkdir(exist_ok=False);shutil.copy2(seed,target/'gameplay-save.json')
    if name=='campaign-earned-restart':shutil.copy2(O/'campaign-vietnam-earned-continuation/pc-final-state.json',target/'pc-final-state.json')
    if 'continuation' in name:shutil.copy2(O/'campaign-vietnam/campaign-vietnam-results.txt',target/'opening-new-game-results.txt')
    start=time.time();print('START '+name,flush=True)
    p=subprocess.Popen([str(exe),'--gameplay-smoke','--gameplay-output='+str(target),'-logFile',str(target/'player.log'),*args],cwd=R,creationflags=subprocess.CREATE_NO_WINDOW)
    try:code=p.wait(timeout=900)
    except subprocess.TimeoutExpired:p.kill();p.wait();code='TIMEOUT'
    result=target/'smoke-results.txt';body=result.read_text(encoding='utf-8-sig') if result.exists() else ''
    status='PASS' if code==0 and body.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL' if body else 'BLOCKED'
    rows.append({'name':name,'status':status,'exit':code,'assertions':sum(x.startswith('PASS:') for x in body.splitlines()),'seconds':round(time.time()-start,2),'seed':str(seed),'arguments':args,'result':str(result),'failureTail':body[-2500:] if status!='PASS' else None})
    (O/('ending-runs.json' if ending_only else 'last-runs.json')).write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8');print('DONE '+name+' '+status,flush=True)
    if name=='ending-earned-tail' and status=='PASS':
        jobs.append(('ending-earned-restart',['--phase5-restart=completed'],target/'P5-ending-completed.json'))
    if name=='campaign-vietnam-earned-continuation' and status=='PASS':
        jobs.append(('campaign-earned-restart',['--pc-gameplay-restart'],target/'gameplay-save.json'))
before=json.loads((O/'source-before.json').read_text());after=sources();identity=json.loads((O/'identity.json').read_text());owner=Path(identity['ownerSave'])
(O/'last-integrity.json').write_text(json.dumps({'sourceFiles':len(before),'sourceUnchanged':before==after,'changedPaths':[k for k in before.keys()|after.keys() if before.get(k)!=after.get(k)],'ownerSaveBefore':identity['ownerSaveBefore'],'ownerSaveAfter':sha(owner),'ownerSaveUnchanged':sha(owner)==identity['ownerSaveBefore']},indent=2),encoding='utf-8')
