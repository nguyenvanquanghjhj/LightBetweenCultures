"""Bounded metadata inventory for owner-authorized C:/D: storage cleanup. No deletion."""
from pathlib import Path
import collections, datetime, json, os, shutil, sys
ROOT=Path('D:/GAMEKKK/LightBetweenCultures')
OUT=ROOT/'Docs/Storage/DeepCleanup-20261020'
OUT.mkdir(parents=True,exist_ok=True)
def walk(root):
    stack=[str(root)]
    while stack:
        here=stack.pop()
        try:
            with os.scandir(here) as scan:
                for item in scan:
                    try:
                        st=item.stat(follow_symlinks=False)
                        if item.is_symlink() or st.st_file_attributes&0x400:continue
                        if item.is_dir(follow_symlinks=False):stack.append(item.path)
                        elif item.is_file(follow_symlinks=False):yield Path(item.path),st
                    except OSError:pass
        except OSError as e:errors.append(dict(path=here,error=str(e)))
def measure(root):
    root=Path(root);groups=collections.defaultdict(lambda:[0,0]);largest=[];total=count=0
    if not root.is_dir():return dict(path=str(root),exists=False)
    for p,st in walk(root):
        total+=st.st_size;count+=1
        rel=p.relative_to(root);parts=rel.parts
        for depth in range(1,min(3,len(parts)-1)+1):
            entry=groups[str(Path(*parts[:depth]))];entry[0]+=st.st_size;entry[1]+=1
        if len(parts)==1:
            entry=groups[p.name];entry[0]+=st.st_size;entry[1]+=1
        if st.st_size>=100*1024**2:largest.append(dict(path=str(p),bytes=st.st_size,mtime=st.st_mtime))
    return dict(path=str(root),exists=True,bytes=total,files=count,groups=[dict(path=k,bytes=v[0],files=v[1]) for k,v in sorted(groups.items(),key=lambda x:-x[1][0])],largeFiles=sorted(largest,key=lambda x:-x['bytes'])[:25])
def drives():
    return {d:dict(zip(['total','used','free'],shutil.disk_usage(d+':/'))) for d in ['C','D']}
errors=[]
if __name__=='__main__':
    user=Path(os.environ['USERPROFILE']);local=Path(os.environ['LOCALAPPDATA']);roam=Path(os.environ['APPDATA'])
    roots=[ROOT,ROOT.parent/'Backups',ROOT.parent/'Deliveries',local/'Temp',local/'Unity',local/'UnityHub',roam/'UnityHub',roam/'Unity/Asset Store-5.x',user/'.gradle',local/'CrashDumps',local/'Android','C:/Program Files/Unity/Hub/Editor/6000.6.5f1']
    before=drives();data=[]
    for root in roots:
        value=measure(root);data.append(value)
        print(str(root),round(value.get('bytes',0)/1024**3,3),'GiB',value.get('files',0),'files',flush=True)
    inventory=dict(createdUTC=datetime.datetime.now(datetime.timezone.utc).isoformat(),drivesBefore=before,roots=data,errors=errors)
    (OUT/'inventory.json').write_text(json.dumps(inventory,indent=2)+'\n',encoding='utf-8')
    print('Inventory saved:',OUT/'inventory.json')
