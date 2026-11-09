"""Compile actual Unity sources with installed compiler/references, without invoking Editor.
This is a compile check, never a claim of a Player build or runtime acceptance.
"""
import json, subprocess
from pathlib import Path

ROOT=Path(__file__).resolve().parents[1]
OUTPUT=ROOT/'Validation/Catventure-20261107/compile'
OUTPUT.mkdir(parents=True,exist_ok=True)
DOTNET=Path('C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/DotNetSdk/dotnet.exe')
CSC=DOTNET.parent/'sdk/8.0.318/Roslyn/bincore/csc.dll'
def compile_profile(profile,dag,editor=False):
    source=ROOT/'Library/Bee/artifacts'/dag/('Assembly-CSharp-Editor.rsp' if editor else 'Assembly-CSharp.rsp')
    assert source.exists(),source
    lines=[]
    for line in source.read_text(encoding='utf-8-sig').splitlines():
        if line.strip('"').endswith('.cs'):continue
        if line.startswith(('-out:','-refout:','-analyzer:','/analyzer:','/additionalfile:','-additionalfile:')):continue
        if editor and ('/Assembly-CSharp.dll' in line or '/Assembly-CSharp.ref.dll' in line):
            line='-r:"'+str(OUTPUT/'editor-runtime.dll')+'"'
        lines.append(line)
    for p in (ROOT/'Assets/_Game/Scripts').rglob('*.cs'):
        if ('Editor' in p.relative_to(ROOT/'Assets/_Game/Scripts').parts)==editor:lines.append('"'+p.relative_to(ROOT).as_posix()+'"')
    lines+=['-out:"'+str(OUTPUT/(profile+'.dll'))+'"','-nologo']
    rsp=OUTPUT/(profile+'.rsp');rsp.write_text('\n'.join(lines),encoding='utf-8-sig')
    result=subprocess.run([str(DOTNET),str(CSC),'@'+str(rsp)],cwd=ROOT,capture_output=True,text=True,encoding='utf-8')
    (OUTPUT/(profile+'.log')).write_text(result.stdout+result.stderr,encoding='utf-8')
    print(profile,'PASS' if result.returncode==0 else 'FAIL',flush=True)
    if result.returncode:print((result.stdout+result.stderr)[-6500:])
    return dict(profile=profile,exitCode=result.returncode,kind='OFFLINE_COMPILE_ONLY_NOT_PLAYER_BUILD')
if __name__=='__main__':
    results=[compile_profile('editor-runtime','1300b0aE.dag'),compile_profile('windows-development','1900b0aPDev.dag'),compile_profile('android-development','1300b0aPDev.dag'),compile_profile('windows-release','1900b0aP.dag'),compile_profile('android-release','1300b0aP.dag')]
    if results[0]['exitCode']==0:results.append(compile_profile('editor-tools','1300b0aE.dag',True))
    (OUTPUT/'results.json').write_text(json.dumps(results,indent=2),encoding='utf-8')
    raise SystemExit(any(r['exitCode'] for r in results))
