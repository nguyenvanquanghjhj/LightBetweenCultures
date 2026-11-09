import pathlib,json,re,hashlib,shutil
from PIL import Image
ROOT=pathlib.Path(__file__).resolve().parents[1]
DOC=ROOT/'Docs/RenderQuality-20261017'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def audit():
    rows=[]
    for p in (ROOT/'Assets/_Game/Art').rglob('*.png'):
        meta=pathlib.Path(str(p)+'.meta')
        if not meta.exists():continue
        t=meta.read_text(encoding='utf-8-sig');w,h=Image.open(p).size
        def field(n):
            m=re.search(r'^\s*'+n+r': ([^\r\n]+)',t,re.M);return m.group(1) if m else None
        rows.append(dict(path=p.relative_to(ROOT).as_posix(),width=w,height=h,**{n:field(n) for n in ['guid','textureType','spriteMode','maxTextureSize','filterMode','enableMipMap','nPOTScale','spritePixelsToUnits','spriteMeshType','alphaIsTransparency']},platforms=[dict(re.findall(r'^\s*(\w+): ([^\r\n]+)',s,re.M)) for s in t.split('  - serializedVersion: 4')[1:] if 'buildTarget:' in s]))
    return rows
if __name__=='__main__':
    import sys
    DOC.mkdir(parents=True,exist_ok=True)
    if '--before' in sys.argv:
        assert not (DOC/'before-hashes.json').exists()
        files={p.relative_to(ROOT).as_posix():sha(p) for d in ['Assets','Packages','ProjectSettings'] for p in (ROOT/d).rglob('*') if p.is_file()}
        (DOC/'before-hashes.json').write_text(json.dumps(files,indent=2))
        # Recoverable pre-change source, small: no caches/build/validation.
        for p in (ROOT/'Assets/_Game/Scripts').rglob('*.cs'):
            dest=DOC/'BeforeFiles'/p.relative_to(ROOT);dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest)
        for p in (ROOT/'Assets/_Game/Art').rglob('*.meta'):
            dest=DOC/'BeforeFiles'/p.relative_to(ROOT);dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest)
    rows=audit();name='imports-before.json' if '--before' in sys.argv else 'imports-after.json'
    (DOC/name).write_text(json.dumps(rows,indent=2))
    for r in rows:
        if '_SourceSheets' not in r['path'] and (int(r['maxTextureSize'])<max(r['width'],r['height']) or any(p.get('textureCompression')!='0' and p.get('overridden')=='1' for p in r['platforms'])):
            print(r['path'],r['width'],r['height'],'max',r['maxTextureSize'],'filter',r['filterMode'],'mips',r['enableMipMap'],[(p.get('buildTarget'),p.get('overridden'),p.get('textureCompression'),p.get('maxTextureSize')) for p in r['platforms']])
