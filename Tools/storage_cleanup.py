"""Bounded storage audit; never deletes backups, source, saves, builds or caches.

Audit first; --apply deletes only the exact SHA-checked obsolete QA PNG plan.
"""
import datetime, hashlib, json, os, re, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
AREA = ROOT / 'Docs/Storage/20261017'
BACKUPS = ROOT.parent / 'Backups'

def sha(p):
    h = hashlib.sha256()
    with p.open('rb') as f:
        for chunk in iter(lambda: f.read(4*1024*1024), b''): h.update(chunk)
    return h.hexdigest()

def write(name, data):
    AREA.mkdir(parents=True, exist_ok=True)
    (AREA/name).write_text(json.dumps(data, ensure_ascii=False, indent=2), encoding='utf-8')

def load(p):
    return json.loads(p.read_text(encoding='utf-8-sig'))

def records(folder):
    return [{'path':str(p.relative_to(folder)), 'bytes':p.stat().st_size, 'sha256':sha(p)}
            for p in folder.rglob('*') if p.is_file() and not p.is_symlink()]

def audit():
    # Read every small text reference and all evidence manifests. Backup manifest
    # paths refer to the independent backup, not to working Validation files.
    texts=[]
    for base in [ROOT/'Docs',ROOT/'Tools',ROOT/'Validation']:
        for p in base.rglob('*'):
            if p.suffix.lower() in ('.json','.md','.html','.py','.csv') and 'Storage' not in p.parts:
                texts.append(p.read_text(encoding='utf-8-sig',errors='replace').replace('\\\\','/').replace('\\','/'))
    texts += [p.read_text(encoding='utf-8-sig',errors='replace') for p in ROOT.iterdir() if p.suffix in ('.md','.html')]
    references='\n'.join(texts)
    # Explicit superseded run -> retained successful replacement. Keep all logs,
    # saves, manifests and before/after references, plus diagnostic PNG samples.
    families={
        'CityMapUIPolish-20261017': {
            'AfterDiagnostics01':'Final04-Diagnostics','AfterDiagnostics02':'Final04-Diagnostics',
            'AfterJourney01':'Final04-Journey','AfterJourney02':'Final04-Journey',
            'AfterReverse02':'Final04-Reverse','BeforeCrossRetry':'Final04-Journey',
            'FinalDiagnostics':'Final04-Diagnostics','FinalJourney':'Final04-Journey','FinalReverse':'Final04-Reverse'},
        'PortraitStory-20261017': {
            'Journey02':'Journey04','Diagnostics02':'Diagnostics04','Diagnostics03':'Diagnostics04',
            'Reverse03':'Reverse04','UI03':'UI04','AfterBaTu03':'AfterBaTu04','AfterPhuc03':'AfterPhuc04'},
        'DaLatChapter-20261017': {
            'Attempt01':'AcceptanceIntegrated','Attempt02':'AcceptanceIntegrated','Attempt03':'AcceptanceIntegrated',
            'Attempt04':'AcceptanceIntegrated','Attempt05':'AcceptanceIntegrated',
            'FinalHueFirst':'FinalIntegratedHueFirst','FinalIntegrated':'AcceptanceIntegrated','FinalBattleThief':'AcceptanceBattleThief'},
        'HueJeonjuChapters-20261017': {
            'Attempt01':'HueFirst','Attempt02':'HueFirst','Attempt03Fresh':'HueFirst',
            'ValidatedBuild04HueFirst':'HueFirst','ValidatedBuild04JeonjuFirst':'JeonjuFirst'}
    }
    candidates=[]; kept=[]
    for family,pairs in families.items():
        for old,new in pairs.items():
            folder=ROOT/'Validation'/family/old; replacement=folder.parent/new
            result=replacement/'smoke-results.txt'
            if not result.exists() or not result.read_text(encoding='utf-8-sig').rstrip().endswith('ALL CHECKS PASSED'): continue
            imgs=sorted(folder.glob('*.png'))
            for p in imgs:
                rel=p.relative_to(ROOT).as_posix()
                # Relative filenames in local manifests also count as references.
                local_refs='\n'.join(q.read_text(encoding='utf-8-sig',errors='replace') for q in folder.glob('*.json'))
                if rel in references or (old+'/'+p.name) in references or p.name in local_refs or p in imgs[:2]+imgs[-2:] or re.search('fail|error|exception',p.name,re.I):
                    kept.append(rel);continue
                candidates.append(dict(path=rel,bytes=p.stat().st_size,sha256=sha(p),replacement=replacement.relative_to(ROOT).as_posix(),reason='Superseded QA capture; no document/evidence path reference; logs/saves and diagnostic samples retained'))
    write('cleanup-plan.json',dict(files=candidates,totalBytes=sum(x['bytes'] for x in candidates),retainedReferencedOrDiagnostic=kept))
    # Immutable guard: exact source, source packs, pending requests, approved art,
    # final build, every save and every backup file. Hashes recorded before cleanup.
    protected=[]
    for folder in ['Assets','Packages','ProjectSettings','References','Builds','LBC_AnhMoi_BanGiaoCodex_20261015','LBC_Gameplay_6Cities_Detailed_v1','LBC_HoiAn_StoryExpansion_v1','LightBetweenCultures_DesignKit_v1']:
        for e in records(ROOT/folder):e['path']=folder+'/'+e['path'];protected.append(e)
    for p in (ROOT/'Validation').rglob('*.json'):
        protected.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=sha(p)))
    for base in [ROOT/'Docs/Story',ROOT/'Docs/Art',ROOT/'Docs/Maps',ROOT/'Docs/Requests']:
        for p in base.rglob('*'):
            if p.is_file():protected.append(dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size,sha256=sha(p)))
    write('protected-project-hashes.json',protected)
    catalog=[]; allbackup=[]
    for b in sorted(BACKUPS.iterdir()):
        if not b.is_dir():continue
        entries=records(b)
        allbackup += [dict(path=b.name+'/'+e['path'],bytes=e['bytes'],sha256=e['sha256']) for e in entries]
        manifests=[p for p in b.iterdir() if p.is_file() and 'manifest' in p.name.lower() and p.suffix=='.json']
        metadata=[]; mismatches=[]; checked=0
        actual={e['path'].replace('\\','/'):e['sha256'] for e in entries}
        for m in manifests:
            j=load(m)
            if isinstance(j,dict):
                metadata.append({k:v for k,v in j.items() if k not in ('files','entries','sourceInputs','screenshots') and not isinstance(v,(list,dict))})
                listed=j.get('entries',j.get('files',[]))
            else:
                metadata.append({'manifest':m.name,'format':'file list'})
                listed=j if isinstance(j,list) else []
            for e in listed:
                if not isinstance(e,dict) or 'path' not in e or 'sha256' not in e:continue
                p=b/e['path']
                if not p.is_relative_to(b) or actual.get(e['path'].replace('\\','/'),'').lower()!=e['sha256'].lower():mismatches.append(e['path'])
                checked+=1
        components=sorted(x.name for x in b.iterdir() if x.is_dir())
        complete=all((b/x).is_dir() for x in ['Assets','Packages','ProjectSettings'])
        catalog.append(dict(name=b.name,createdLocal=datetime.datetime.fromtimestamp(b.stat().st_ctime).isoformat(),bytes=sum(e['bytes'] for e in entries),files=len(entries),components=components,manifestMetadata=metadata,manifestEntriesChecked=checked,manifestMismatches=mismatches,restore='FULL_SOURCE_LAYOUT' if complete else 'PARTIAL_OR_OVERLAY_REQUIRES_BASE',deleteApproved=False))
        print('Backup audited:',b.name,len(entries), 'mismatch',len(mismatches),flush=True)
    write('backup-hashes-before.json',allbackup)
    # Exact independent full copies only. Other historical source differences
    # remain UNCERTAIN, even when a newer checkpoint has equivalent gameplay.
    for row in catalog:
        mine=[e for e in allbackup if e['path'].startswith(row['name']+'/')]
        other={e['sha256'] for e in allbackup if not e['path'].startswith(row['name']+'/')}
        unique=[e for e in mine if e['sha256'] not in other]
        row['uniqueBytesAcrossBackups']=sum(e['bytes'] for e in unique)
        row['uniqueFileCountAcrossBackups']=len(unique)
        row['uniquePaths']=[e['path'].split('/',1)[1] for e in unique]
    write('backup-audit.json',catalog)
    print('PLAN',len(candidates),sum(x['bytes'] for x in candidates),flush=True)

def apply():
    plan=load(AREA/'cleanup-plan.json'); deleted=[]
    for e in plan['files']:
        p=(ROOT/e['path']).resolve(strict=True)
        assert p.is_relative_to((ROOT/'Validation').resolve()) and p.suffix=='.png'
        assert not any(x in p.parts for x in ['FullStory-20261017','TerrainRoutes-20261017','H3'])
        assert not p.is_symlink() and p.stat().st_size==e['bytes'] and sha(p)==e['sha256']
    for e in plan['files']:
        # Exact file unlink only; never recursive or computed directory deletion.
        (ROOT/e['path']).unlink();deleted.append(e)
    write('deleted-files.json',dict(files=deleted,bytes=sum(e['bytes'] for e in deleted)))
    verify()

def verify():
    for e in load(AREA/'protected-project-hashes.json'):
        assert sha(ROOT/e['path'])==e['sha256'],e['path']
    for e in load(AREA/'backup-hashes-before.json'):
        assert sha(BACKUPS/e['path'])==e['sha256'],e['path']
    print('PASS: protected source/art/config/build/saves/documents and every backup byte unchanged.')

if __name__=='__main__':
    if '--apply' in sys.argv:apply()
    elif '--verify' in sys.argv:verify()
    else:audit()
