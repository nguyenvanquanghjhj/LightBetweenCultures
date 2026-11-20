"""Apply the recorded path migration to maintained code/config and Markdown links.

Historical backup/QA manifests are never rewritten. Each changed file has old/new SHA evidence.
"""
import hashlib
import json
import os
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
WS = ROOT.parent
OUT = ROOT / 'Docs/Development/Storage/WorkspaceRestructure-20261113'


def read(name):
    return json.loads((OUT / name).read_text(encoding='utf-8-sig'))


def main():
    if (OUT / 'path-migration.json').exists():
        raise SystemExit('Completed path rewrite is protected')
    moves = read('move-result.json') + read('asset-moves.json')
    pairs = sorted([(r['old'], r['new']) for r in moves], key=lambda p: len(p[0]), reverse=True)
    def resolve(key):
        for old, new in pairs:
            if key == old or key.startswith(old + '/'):
                return new + key[len(old):]
        return key
    def original(key):
        for old, new in sorted(pairs, key=lambda p: len(p[1]), reverse=True):
            if key == new or key.startswith(new + '/'):
                return old + key[len(new):]
        return key
    project_pairs = [(old[len(ROOT.name)+1:], os.path.relpath(WS / new, ROOT).replace('\\','/'))
                     for old, new in pairs if old.startswith(ROOT.name + '/')]
    changes = []
    candidates = list((ROOT / 'Assets/_Game/Scripts').rglob('*.cs'))
    candidates += [p for p in (ROOT / 'Tools').rglob('*') if p.suffix in ('.py','.ps1','.csproj','.md','.json','.js','.html')
                   and 'Sources' not in p.relative_to(ROOT / 'Tools').parts and '/Maintenance/' not in p.as_posix()]
    candidates += list((ROOT / 'Docs').rglob('*.md')) + [ROOT / 'README.md', ROOT / 'AGENTS.md']
    # These are current authoring/identity inputs, not frozen before/after evidence.
    for rel in ('Docs/Art/asset-moves.json','Docs/Art/NPC_CANONICAL_REGISTRY.json','Docs/Art/ChapterSpriteSlices.json',
                'Docs/Art/CrossCitySpriteSlices.json','Docs/Art/DaLatArtDerivatives.json','Docs/Art/HueJeonjuArtDerivatives.json',
                'Docs/Development/Story/FullStory-20261017/native-slices.json','Docs/Development/Gameplay/CheckInArt-20261018/source-inventory.json',
                'Docs/QA/MapClarity-20261019/import-selection.json','Docs/QA/Phase6-VisualAndroid/crop-outline-plan.json','Docs/QA/Phase7/bot-alpha-plan.json'):
        if (ROOT / rel).exists():
            candidates.append(ROOT / rel)
    for path in dict.fromkeys(candidates):
        if '/Storage/' in path.as_posix():
            continue
        before = path.read_bytes()
        text = before.decode('utf-8-sig')
        if path.suffix == '.md':
            current_key = path.relative_to(WS).as_posix()
            old_file = WS / original(current_key)
            def link(match):
                target = match.group(1)
                plain = target.strip('<>')
                if re.match(r'^[a-z]+://|^#|^mailto:', plain):
                    return match.group(0)
                base, sep, anchor = plain.partition('#')
                old_target = Path(base) if re.match(r'^[A-Za-z]:',base) else old_file.parent / base
                try:
                    key = old_target.resolve().relative_to(WS.resolve()).as_posix()
                except ValueError:
                    return match.group(0)
                new_target = WS / resolve(key)
                rewritten = os.path.relpath(new_target, path.parent).replace('\\','/') + (sep + anchor if sep else '')
                return '](' + ('<' + rewritten + '>' if ' ' in rewritten else rewritten) + ')'
            text = re.sub(r'\]\(([^)]+)\)', link, text)
        else:
            # Prefix substitution is longest-first and applied once, avoiding cascading replacements.
            # Bare folder words can also be identifiers (for example Rebuild).
            substitutions = {old: new for old, new in project_pairs if old != 'build'}
            substitutions.update({'References/ImportSources': 'Tools/Import/Sources', 'References/MarkerDesign': 'Tools/Import/Sources/MarkerDesign'})
            # Folder-wide references are not all represented by per-child move rows.
            for old, new in [('Docs/Storage/','Docs/Development/Storage/'),('ReviewPacks/','../_LBC_DELIVERABLES/Handoffs/ReviewPacks/'),
                             ('Submission/','../_LBC_DELIVERABLES/CompetitionSubmission/')]:
                substitutions[old] = new
            pattern = re.compile('|'.join(re.escape(k) for k in sorted(substitutions, key=len, reverse=True)))
            text = pattern.sub(lambda m: substitutions[m[0]], text)
            if path.suffix == '.py':
                # Resolve project from the actual script location rather than its old folder depth.
                expression = 'next(p for p in Path(__file__).resolve().parents if (p/"ProjectSettings").is_dir())'
                text = re.sub(r'Path\(__file__\)\.resolve\(\)\.parents\[\d+\]', expression, text)
                text = text.replace('ROOT=HERE.parents[1]', 'ROOT=' + expression).replace('ROOT = HERE.parents[1]', 'ROOT = ' + expression)
            if path.suffix == '.cs':
                text = text.replace('Assets/_Game/Resources/AppearanceReviewData.asset','Assets/_Game/Resources/Catalogs/AppearanceReviewData.asset')
                for key in ('AdventureUIArt','DesignKitArt','SoftAudioClips','SoftMusicClips','AppearanceReviewData'):
                    text = re.sub(r'(Resources\.Load(?:<[^>]+>)?\(")' + key + r'("\))', r'\1Catalogs/' + key + r'\2', text)
                text = text.replace('Resources.Load<TextAsset>("HoiAnData")','Resources.Load<TextAsset>("Content/HoiAnData")')
            text = text.replace('../LightBetweenCultures-ArtArchive-20261113/', '../_LBC_ARCHIVE/HistoricalArt/SourceArchive-20261113/')
        if path.suffix == '.csproj':
            text = text.replace('..\\..\\Assets', '..\\..\\..\\Assets')
        after = text.encode('utf-8')
        if after != before:
            path.write_bytes(after)
            changes.append(dict(path=path.relative_to(ROOT).as_posix(), beforeSHA256=hashlib.sha256(before).hexdigest(), afterSHA256=hashlib.sha256(after).hexdigest()))
    # Gameplay values stay identical except the reviewed art lookup identifiers.
    for rel in ('Assets/_Game/Resources/Content/LeisureActivities.json','Assets/_Game/Resources/Content/CatRescue.json'):
        path=ROOT/rel; before=path.read_bytes();text=before.decode('utf-8-sig')
        text=text.replace('resource:Art/Leisure/','leisure:').replace('Art/Leisure/','')
        path.write_text(text,encoding='utf-8')
        changes.append(dict(path=rel,beforeSHA256=hashlib.sha256(before).hexdigest(),afterSHA256=hashlib.sha256(path.read_bytes()).hexdigest()))
    (OUT / 'path-migration.json').write_text(json.dumps(changes,indent=2)+'\n',encoding='utf-8')
    print('Code/config/document paths updated:',len(changes))


if __name__ == '__main__':
    main()
