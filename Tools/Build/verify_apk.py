"""Read-only Android SDK checks. No PowerShell execution-policy changes or device installs."""
import argparse
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--apk',required=True,type=Path)
    parser.add_argument('--output',required=True,type=Path)
    parser.add_argument('--editor',type=Path,default=Path(os.environ.get('UNITY_EDITOR','C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe')))
    args=parser.parse_args()
    if args.output.exists():raise SystemExit('Existing evidence is protected')
    apk=args.apk.resolve(strict=True)
    sdk=args.editor.parent/'Data/PlaybackEngines/AndroidPlayer'
    build_tools=max((sdk/'SDK/build-tools').iterdir(),key=lambda p:tuple(int(n) for n in re.findall(r'\d+',p.name)))
    commands={
        'badging':[str(build_tools/'aapt.exe'),'dump','badging',str(apk)],
        'signature':[str(sdk/'OpenJDK/bin/java.exe'),'-jar',str(build_tools/'lib/apksigner.jar'),'verify','--verbose','--print-certs',str(apk)],
        'alignment':[str(build_tools/'zipalign.exe'),'-c','-P','16','4',str(apk)]}
    args.output.mkdir(parents=True)
    result=dict(apk=str(apk),sha256=hashlib.sha256(apk.read_bytes()).hexdigest(),deviceInstall='NOT_TESTED',checks=[])
    for name,command in commands.items():
        try:
            run=subprocess.run(command,capture_output=True,text=True,encoding='utf-8',errors='replace')
            (args.output/(name+'.txt')).write_text(run.stdout+run.stderr,encoding='utf-8')
            result['checks'].append(dict(name=name,status='PASS' if run.returncode==0 else 'FAIL',exitCode=run.returncode))
        except OSError as error:
            result['checks'].append(dict(name=name,status='BLOCKED',error=str(error)))
    result['status']='PASS' if all(c['status']=='PASS' for c in result['checks']) else 'FAIL_OR_BLOCKED'
    (args.output/'result.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(result,indent=2))
    return 0 if result['status']=='PASS' else 1


if __name__=='__main__':raise SystemExit(main())
