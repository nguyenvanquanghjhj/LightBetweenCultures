"""Run production rescue/save domain code in a documented .NET adapter host.
No NuGet, Unity Editor, owner save, native scene or rendering dependency.
"""
import json, os, subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
SDK=Path('C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/DotNetSdk')
OUT=ROOT/'Validation/Catventure-20261107/domain'
OUT.mkdir(parents=True,exist_ok=True)
pack=next((SDK/'packs/Microsoft.NETCore.App.Ref').iterdir())
refs=sorted((pack/'ref/net8.0').glob('*.dll'))
sources=[ROOT/'Assets/_Game/Scripts/Content/CatRescue.cs',ROOT/'Assets/_Game/Scripts/Core/SaveSystem.cs',ROOT/'Tools/CatRescueTests/Adapters.cs',ROOT/'Tools/CatRescueTests/Program.cs']
args=['-nologo','-target:exe','-out:"'+str(OUT/'CatRescueTests.dll')+'"']+['-r:"'+str(p)+'"' for p in refs]+['"'+str(p)+'"' for p in sources]
rsp=OUT/'tests.rsp';rsp.write_text('\n'.join(args),encoding='utf-8-sig')
env=dict(os.environ,DOTNET_CLI_HOME=str(OUT/'dotnet-home'),DOTNET_CLI_TELEMETRY_OPTOUT='1',DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1')
def run(args,log):
    result=subprocess.run(args,cwd=ROOT,capture_output=True,text=True,encoding='utf-8',env=env)
    (OUT/log).write_text(result.stdout+result.stderr,encoding='utf-8');print(result.stdout+result.stderr,end='')
    if result.returncode:raise SystemExit(result.returncode)
run([str(SDK/'dotnet.exe'),str(SDK/'sdk/8.0.318/Roslyn/bincore/csc.dll'),'@'+str(rsp)],'compile.log')
(OUT/'CatRescueTests.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net8.0','framework':{'name':'Microsoft.NETCore.App','version':'8.0.0'}}}),encoding='utf-8')
run([str(SDK/'dotnet.exe'),str(OUT/'CatRescueTests.dll'),str(ROOT)],'run.log')
run([str(SDK/'dotnet.exe'),str(OUT/'CatRescueTests.dll'),str(ROOT),'--restart'],'restart.log')
