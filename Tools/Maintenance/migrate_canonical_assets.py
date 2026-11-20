"""One-shot reviewed asset/source relocation, retaining every original .meta and byte hash."""
import hashlib
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
WORKSPACE = ROOT.parent
OUT = ROOT / 'Docs/Development/Storage/WorkspaceRestructure-20261113'


def main():
    manifest = OUT / 'asset-moves.json'
    if manifest.exists():
        raise SystemExit('Completed asset moves are protected')
    moves = []
    def move(old, new):
        source, target = ROOT / old, WORKSPACE / new
        source.resolve().relative_to(ROOT.resolve())
        target.resolve().relative_to(WORKSPACE.resolve())
        assert source.is_file() and not target.exists(), (old, new)
        expected = hashlib.sha256(source.read_bytes()).hexdigest()
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.move(str(source), str(target))
        assert hashlib.sha256(target.read_bytes()).hexdigest() == expected
        moves.append(dict(old=ROOT.name + '/' + old, new=new, bytes=target.stat().st_size, sha256=expected))
    def asset(old, new):
        move(old, new)
        move(old + '.meta', new + '.meta')
    p = ROOT.name + '/'
    for path in sorted((ROOT / 'Assets/_Game/Resources/Art').rglob('*.png')):
        old = path.relative_to(ROOT).as_posix()
        if path.name == 'RescueCats.png':
            new = 'Assets/_Game/Art/Characters/Cats/RescueCats.png'
        elif path.name == 'DaLatStation.png':
            new = 'Assets/_Game/Art/Maps/DaLat/DaLatStation.png'
        else:
            new = 'Assets/_Game/Art/Leisure/Props/' + path.name
        asset(old, p + new)
    for path in sorted((ROOT / 'Assets/_Game/Resources').glob('*.asset')):
        asset(path.relative_to(ROOT).as_posix(), p + 'Assets/_Game/Resources/Catalogs/' + path.name)
    asset('Assets/_Game/Resources/HoiAnData.json', p + 'Assets/_Game/Resources/Content/HoiAnData.json')
    for old in ('Assets/_Game/Art/Items/Source/Item_HoiAn_Chili.jpg', 'Assets/_Game/Art/Items/Source/Item_HoiAn_Shallot.jpg',
                'Assets/_Game/Art/UI/Postcards/Source/Item_Food_Postcard.jpg', 'Assets/_Game/Art/UI/AppIcon/LBC_AppIcon_Original.png'):
        asset(old, p + 'Tools/Import/Sources/Artwork/' + Path(old).name)
    # Proven unreachable current assets; the obsolete authoring entry points are retired separately.
    for old in ('Assets/_Game/Art/NPC/Seoul/NPC_Seoul_Curator.png', 'Assets/_Game/Art/Environment/HoiAn/H3/H3_Stone_A.asset',
                'Assets/_Game/Art/Environment/HoiAn/H3/H3_Stone_B.asset'):
        asset(old, '_LBC_ARCHIVE/HistoricalArt/RetiredAuthoring/' + Path(old).name)
    # Unused folder metadata is preserved outside Assets, rather than losing its GUID.
    for folder in sorted([p for p in (ROOT / 'Assets/_Game').rglob('*') if p.is_dir()], key=lambda p: len(p.parts), reverse=True):
        if any(folder.iterdir()):
            continue
        meta = Path(str(folder) + '.meta')
        if meta.exists():
            move(meta.relative_to(ROOT).as_posix(), '_LBC_ARCHIVE/HistoricalArt/RetiredFolderMetadata/' + meta.relative_to(ROOT).as_posix())
        folder.rmdir()
    manifest.write_text(json.dumps(moves, indent=2) + '\n', encoding='utf-8')
    print('Asset/source files moved, including metadata:', len(moves))


if __name__ == '__main__':
    main()
