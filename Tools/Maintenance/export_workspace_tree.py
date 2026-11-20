"""Export real directory trees and a final recursive inventory; never move/delete files."""
import json
import os
from pathlib import Path
from workspace_model import PROJECT, WORKSPACE, EVIDENCE, sha

GENERATED={'.git','Library','Temp','obj','Logs','UserSettings','__pycache__','.vs'}


def tree(root,limit=None,skip_generated=False):
    lines=[root.name+'/']
    def visit(folder,prefix='',depth=0):
        children=sorted(folder.iterdir(),key=lambda p:(not p.is_dir(),p.name))
        children=[p for p in children if not p.name.endswith('.meta')]
        for i,p in enumerate(children):
            last=i==len(children)-1
            lines.append(prefix+('└── ' if last else '├── ')+p.name+('/' if p.is_dir() else ''))
            if p.is_dir():
                pad=prefix+('    ' if last else '│   ')
                if skip_generated and p.name in GENERATED:
                    lines.append(pad+'… generated/Git internals retained')
                elif limit is not None and depth+1>=limit:
                    lines.append(pad+'… see final-inventory.json')
                else:visit(p,pad,depth+1)
    visit(root)
    return '\n'.join(lines)


def main():
    files=[];folders=[]
    for current,dirs,names in os.walk(WORKSPACE):
        folder=Path(current);rel=folder.relative_to(WORKSPACE)
        folders.append(rel.as_posix())
        generated=folder.is_relative_to(PROJECT) and any(p in GENERATED for p in folder.relative_to(PROJECT).parts)
        for name in names:
            p=folder/name
            try:
                files.append(dict(path=p.relative_to(WORKSPACE).as_posix(),bytes=p.stat().st_size,
                                  sha256=None if generated else sha(p),hashReason='generated/Git internals not moved' if generated else 'SHA-256'))
            except OSError as e:
                files.append(dict(path=p.relative_to(WORKSPACE).as_posix(),error=str(e)))
    report=dict(files=files,folders=folders,fileCount=len(files),folderCount=len(folders),bytes=sum(r.get('bytes',0) for r in files),
                imageCount=sum(Path(r['path']).suffix.lower() in ('.png','.jpg','.jpeg','.webp','.psd','.svg') for r in files))
    (EVIDENCE/'final-inventory.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
    architecture=PROJECT/'Docs/Architecture'
    for name,root,limit,skip in [('WORKSPACE_TREE.md',WORKSPACE,3,True),('PROJECT_TREE.md',PROJECT,4,True),('FINAL_TREE.md',PROJECT/'Assets/_Game',None,False)]:
        (architecture/name).write_text('# Actual filesystem tree\n\n.meta omitted for readability; bounded branches and generated internals remain inventoried in final-inventory.json.\n\n```text\n'+tree(root,limit,skip)+'\n```\n',encoding='utf-8')
    print(json.dumps({k:v for k,v in report.items() if k not in ('files','folders')}))


if __name__=='__main__':main()
