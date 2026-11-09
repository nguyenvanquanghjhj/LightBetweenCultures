"""Current Player rerun. All gameplay saves are private QA copies."""
from pathlib import Path
import datetime, hashlib, json, os, shutil, subprocess, time

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Validation/FullGame-20261025'
EXE = ROOT / 'Builds/Windows-ThreeNpcQA-20261025/LightBetweenCultures.exe'

def sha(p):
    return hashlib.sha256(p.read_bytes()).hexdigest()

def write(name, obj):
    (OUT/name).write_text(json.dumps(obj, ensure_ascii=False, indent=2), encoding='utf-8')

def sources():
    return {p.relative_to(ROOT).as_posix(): sha(p) for base in ['Assets','Packages','ProjectSettings']
            for p in (ROOT/base).rglob('*') if p.is_file()}

def main():
    OUT.mkdir(exist_ok=False)
    baseline = sources()
    write('source-before.json', baseline)
    owner = Path(os.environ['USERPROFILE'])/'AppData/LocalLow/DefaultCompany/LightBetweenCultures/hoi-an-v1.json'
    owner_before = sha(owner) if owner.exists() else None
    write('identity.json', {'player':str(EXE), 'assemblySHA256':sha(EXE.parent/'LightBetweenCultures_Data/Managed/Assembly-CSharp.dll'),
          'ownerSave':str(owner),'ownerSaveBefore':owner_before,'startedUTC':datetime.datetime.now(datetime.timezone.utc).isoformat()})
    rows=[]
    def run(name, args, seed=None, folder=None, extras=None, limit=600):
        target=OUT/(folder or name)
        target.mkdir(exist_ok=True)
        if seed: shutil.copy2(seed,target/'gameplay-save.json')
        for dest, src in (extras or {}).items(): shutil.copy2(src,target/dest)
        result=target/'smoke-results.txt'
        if result.exists(): shutil.copy2(result,target/(name+'-previous-results.txt'))
        cmd=[str(EXE),'--gameplay-smoke','--gameplay-output='+str(target),'-logFile',str(target/(name+'-player.log')),*args]
        start=time.time()
        print('START '+name,flush=True)
        p=subprocess.Popen(cmd,cwd=ROOT,creationflags=subprocess.CREATE_NO_WINDOW)
        try: code=p.wait(timeout=limit)
        except subprocess.TimeoutExpired:
            p.kill(); p.wait(); code='TIMEOUT'
        fresh=result.exists() and result.stat().st_mtime>=start-1
        body=result.read_text(encoding='utf-8-sig') if fresh else ''
        status='PASS' if code==0 and body.rstrip().endswith('ALL CHECKS PASSED') else 'FAIL' if fresh else 'BLOCKED'
        if fresh: shutil.copy2(result,target/(name+'-results.txt'))
        row={'name':name,'status':status,'exit':code,'seconds':round(time.time()-start,2),
             'assertions':sum(s.startswith('PASS:') for s in body.splitlines()),'arguments':args,
             'seed':str(seed) if seed else None,'result':str(target/(name+'-results.txt')) if fresh else None,
             'failureTail':body[-2500:] if status!='PASS' else None}
        rows.append(row);write('runs.json',rows)
        print('DONE '+name+' '+status+' assertions='+str(row['assertions']),flush=True)
        return status

    old=ROOT/'Validation/ThreeNpc-20261024'
    earned=ROOT/'Validation/PcGameplayFix-20261024/campaign/pc-final-state.json'
    run('three-npc',['--three-npc-qa'],extras={'legacy-source.json':old/'legacy-source.json'})
    if (OUT/'three-npc/earned-after.json').exists():
        run('three-npc-restart',['--three-npc-qa','--three-npc-restart'],folder='three-npc')
    run('campaign-vietnam',['--pc-full-gameplay','--phase4-e2e=VN_FIRST'],limit=900)
    if (OUT/'campaign-vietnam/pc-final-state.json').exists():
        run('campaign-restart',['--pc-gameplay-restart'],folder='campaign-vietnam')
    run('campaign-korea',['--pc-full-gameplay','--phase4-e2e=KOREA_FIRST'],limit=900)
    run('node-panels',['--node-panel-qa'],earned)
    run('dialogue',['--dialogue-release-qa','--node-panel-qa'],earned)
    run('final-ux',['--final-ux-qa'],earned)
    run('hue-stamps',['--hue-snap-qa'],earned)
    run('save-recovery',['--release-candidate-qa'],earned)
    run('leisure',['--leisure-visuals'],earned)
    run('credits',['--credits-preview'],earned)
    after=sources()
    changes=[k for k in baseline.keys()|after.keys() if baseline.get(k)!=after.get(k)]
    write('integrity.json',{'sourceUnchanged':not changes,'changedPaths':changes,'sourceFiles':len(baseline),
          'ownerSaveBefore':owner_before,'ownerSaveAfter':sha(owner) if owner.exists() else None,
          'ownerSaveUnchanged':owner_before==(sha(owner) if owner.exists() else None)})
    print('FINISHED '+str(len(rows))+' runs',flush=True)

if __name__=='__main__': main()
