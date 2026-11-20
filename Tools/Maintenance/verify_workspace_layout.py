"""Validate all ten workspace gates from real files and the relocation inventory."""
import json
from pathlib import Path
from workspace_model import PROJECT, WORKSPACE, EVIDENCE, load, sha, resolve, check, write_report, active_path_violations


def main():
    checks=[]
    actual={p.name for p in WORKSPACE.iterdir()}
    expected={PROJECT.name,'_LBC_ARCHIVE','_LBC_DELIVERABLES','_LBC_WORK'}
    checks.append(check('WS-001',sorted(actual^expected),'workspace root must contain exactly the four official groups'))
    checks.append(check('WS-002',[p.name for p in WORKSPACE.iterdir() if p.name not in expected and ('backup' in p.name.lower() or 'checkpoint' in p.name.lower())]))
    checks.append(check('WS-003',[p.name for p in WORKSPACE.iterdir() if p.name not in expected and ('patch' in p.name.lower() or p.name.startswith('LBC_'))]))
    checks.append(check('WS-004',[p.name for p in WORKSPACE.iterdir() if p.is_file()]))
    archive=WORKSPACE/'_LBC_ARCHIVE/HistoricalArt'
    checks.append(check('WS-005',([] if archive.is_dir() else ['HistoricalArt missing'])+[p.as_posix() for p in PROJECT.iterdir() if 'ArtArchive' in p.name]))
    original=load(EVIDENCE/'inventory.json')
    preserved=dict(deliveries=[],backups=[])
    verified=dict(deliveries=0,backups=0)
    for row in original['files']:
        if not row.get('moveGroup'):continue
        target=resolve(row['path'])
        key=target.relative_to(WORKSPACE).as_posix()
        group='deliveries' if key.startswith('_LBC_DELIVERABLES/') else 'backups' if key.startswith(('_LBC_ARCHIVE/Backups/','_LBC_ARCHIVE/Checkpoints/')) else None
        if group:
            if not target.is_file() or sha(target)!=row['sha256']:preserved[group].append(key)
            else:verified[group]+=1
    checks.append(check('WS-006',preserved['deliveries'],'SHA-256 verified delivery files: '+str(verified['deliveries'])))
    manifest=EVIDENCE/'backup-preservation.json'
    checks.append(check('WS-007',preserved['backups']+([] if manifest.exists() else ['backup relocation manifest missing']),
                        'SHA-256 verified backup files: '+str(verified['backups'])+'; existing manifest bytes preserved, not a restore rehearsal'))
    checks.append(check('WS-008',active_path_violations(),'literal active file reads and maintained Python asset inputs; dynamic Unity bindings have a separate native gate'))
    checks.append(check('WS-009',[p.name for p in WORKSPACE.iterdir() if p.name not in expected and ('archive' in p.name.lower() or 'old' in p.name.lower())]))
    top=original.get('topLevel',[])
    checks.append(check('WS-010',([] if original.get('files') and original.get('folders') and top else ['recursive original inventory missing'])+
                        [r['old'] for r in load(EVIDENCE/'move-result.json') if (WORKSPACE/r['old']).exists()],
                        'original recursive inventory includes hidden files; moves recorded individually'))
    return write_report('workspace-validation.json',checks)


if __name__=='__main__':raise SystemExit(main())
