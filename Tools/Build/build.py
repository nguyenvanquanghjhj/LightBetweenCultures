"""Build current Unity source into a new workspace QA directory. Never overwrite deliveries."""
import argparse
import os
from pathlib import Path
import subprocess
from datetime import datetime

ROOT = Path(__file__).resolve().parents[2]


def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('platform',choices=('windows','android'))
    parser.add_argument('--editor',default=os.environ.get('UNITY_EDITOR','C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Unity.exe'))
    parser.add_argument('--run-id',default=datetime.now().strftime('%Y%m%d-%H%M%S'))
    args=parser.parse_args()
    if Path(args.run_id).name!=args.run_id or args.run_id in ('.','..'):
        parser.error('run-id must be a single directory name')
    workspace=ROOT.parent
    directory=workspace/'_LBC_WORK/QA/LightBetweenCultures'/('Build-'+args.run_id)/args.platform
    log=workspace/'_LBC_WORK/Logs'/('Build-'+args.run_id+'-'+args.platform+'.log')
    if directory.exists() or log.exists():
        raise SystemExit('Existing output/evidence is protected; choose a new run-id')
    directory.parent.mkdir(parents=True,exist_ok=True);log.parent.mkdir(parents=True,exist_ok=True)
    command=[args.editor,'-batchmode','-nographics','-projectPath',str(ROOT),'-logFile',str(log)]
    if args.platform=='windows':
        command+=['-buildTarget','Win64','-executeMethod','LightBetweenCultures.Editor.AssetCleanupAudit.BuildValidationPlayer',
                  '--workspace-player-output='+str(directory/'LightBetweenCultures.exe'),'-quit']
    else:
        command+=['-buildTarget','Android','-executeMethod','LightBetweenCultures.Editor.AndroidApkBuild.Build',
                  '--qa-apk-output='+str(directory/'LightBetweenCultures.apk'),'--qa-report-folder='+str(directory.parent/'android-report')]
    result=subprocess.run(command,cwd=ROOT)
    print('Unity exit:',result.returncode,'Log:',log)
    raise SystemExit(result.returncode)


if __name__=='__main__':
    main()
