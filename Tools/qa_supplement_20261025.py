"""Additional sequential UI runs after the initial current-build suite exits."""
from pathlib import Path
import json, shutil, subprocess, time
R=Path(__file__).resolve().parents[1]; O=R/'Validation/FullGame-20261025'
deadline=time.time()+1800
while not (O/'integrity.json').exists():
    if time.time()>deadline: raise RuntimeError('Initial suite did not complete; no concurrent player launched')
    time.sleep(2)
exe=R/'Builds/Windows-ThreeNpcQA-20261025/LightBetweenCultures.exe'
seed=R/'Validation/PcGameplayFix-20261024/campaign/pc-final-state.json'
jobs=[('leisure12',['--casual-runtime','--casual-suppress-captures'],seed),
      ('map-return',['--map-return-qa'],None),
      ('shop-toolbar',['--shop-toolbar-qa'],seed),
      ('audio-input',['--soft-sfx-input'],seed)]
rows=[]
for name,args,source in jobs:
    target=O/name; target.mkdir(exist_ok=False)
    if source: shutil.copy2(source,target/'gameplay-save.json')
    start=time.time();print('START '+name,flush=True)
    process=subprocess.Popen([str(exe),'--gameplay-smoke','--gameplay-output='+str(target),'-logFile',str(target/'player.log'),*args],cwd=R,creationflags=subprocess.CREATE_NO_WINDOW)
    try: code=process.wait(timeout=600)
    except subprocess.TimeoutExpired: process.kill();process.wait();code='TIMEOUT'
    file=target/'smoke-results.txt'; body=file.read_text(encoding='utf-8-sig') if file.exists() else ''
    status='PASS' if code==0 and body.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL' if body else 'BLOCKED'
    rows.append({'name':name,'status':status,'exit':code,'seconds':round(time.time()-start,2),'assertions':sum(x.startswith('PASS:') for x in body.splitlines()),'result':str(file),'seed':str(source),'arguments':args,'failureTail':body[-2500:] if status!='PASS' else None})
    (O/'supplement-runs.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf-8')
    print('DONE '+name+' '+status,flush=True)
