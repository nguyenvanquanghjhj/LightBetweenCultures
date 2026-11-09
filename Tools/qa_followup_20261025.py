"""Correct fixture invocations and isolate observed failures; no game edits."""
from pathlib import Path
import json, shutil, subprocess, time
from qa_full_20261025 import sha, sources
R=Path(__file__).resolve().parents[1]; O=R/'Validation/FullGame-20261025'
deadline=time.time()+1200
while True:
    receipt=O/'supplement-runs.json'
    if receipt.exists() and len(json.loads(receipt.read_text(encoding='utf-8')))==4: break
    if time.time()>deadline: raise RuntimeError('Supplement suite did not finish; no concurrent Player launched')
    time.sleep(2)
exe=R/'Builds/Windows-ThreeNpcQA-20261025/LightBetweenCultures.exe'
earned=R/'Validation/PcGameplayFix-20261024/campaign/pc-final-state.json'
partial=R/'Validation/ReleaseCandidate-20261024/shop-partial-hoian.json'
lane=O/'leisure12/Casual-earned-optional-lane.json'
if not lane.exists(): lane=O/'leisure12/gameplay-save.json'
jobs=[('dialogue-correct-fixture',['--dialogue-release-qa','--node-panel-qa','--buttons-partial-save='+str(partial)],earned),
      ('leisure-correct-fixture',['--leisure-visuals'],lane),
      ('jeonju-isolated',['--phase4-city=jeonju','--phase4-restore-only'],O/'campaign-vietnam/gameplay-save.json'),
      ('campaign-korea-core',['--phase5-e2e','--phase4-e2e=KOREA_FIRST'],None)]
rows=[]
def run(name,args,source,folder=None):
    target=O/(folder or name);target.mkdir(exist_ok=True)
    if source:shutil.copy2(source,target/'gameplay-save.json')
    start=time.time();print('START '+name,flush=True)
    p=subprocess.Popen([str(exe),'--gameplay-smoke','--gameplay-output='+str(target),'-logFile',str(target/(name+'-player.log')),*args],cwd=R,creationflags=subprocess.CREATE_NO_WINDOW)
    try:code=p.wait(timeout=900)
    except subprocess.TimeoutExpired:p.kill();p.wait();code='TIMEOUT'
    result=target/'smoke-results.txt';body=result.read_text(encoding='utf-8-sig') if result.exists() and result.stat().st_mtime>=start-1 else ''
    status='PASS' if code==0 and body.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL' if body else 'BLOCKED'
    if body:shutil.copy2(result,target/(name+'-results.txt'))
    rows.append({'name':name,'status':status,'exit':code,'assertions':sum(x.startswith('PASS:') for x in body.splitlines()),'seconds':round(time.time()-start,2),'seed':str(source) if source else None,'arguments':args,'result':str(target/(name+'-results.txt')),'failureTail':body[-2500:] if status!='PASS' else None})
    (O/'followup-runs.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8');print('DONE '+name+' '+status,flush=True)
    return status
for name,args,source in jobs:run(name,args,source)
if (O/'campaign-korea-core/P5-ending-completed.json').exists():
    run('core-ending-restart',['--phase5-restart=completed'],O/'campaign-korea-core/P5-ending-completed.json')
before=json.loads((O/'source-before.json').read_text());after=sources()
identity=json.loads((O/'identity.json').read_text());owner=Path(identity['ownerSave'])
(O/'final-integrity.json').write_text(json.dumps({'sourceFiles':len(before),'sourceUnchanged':before==after,'changedPaths':[k for k in before.keys()|after.keys() if before.get(k)!=after.get(k)],'ownerSaveBefore':identity['ownerSaveBefore'],'ownerSaveAfter':sha(owner),'ownerSaveUnchanged':sha(owner)==identity['ownerSaveBefore']},indent=2),encoding='utf-8')
