"""Read-only APK/package/signature/alignment verification, preserving old APKs."""
import hashlib, json, re, subprocess, sys, zipfile
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Validation/Catventure-20261107'
ENGINE=Path('C:/Program Files/Unity/Hub/Editor/6000.6.5f1/Editor/Data/PlaybackEngines/AndroidPlayer')
TOOLS=ENGINE/'SDK/build-tools/36.0.0'
APK=ROOT/'Builds/Android/Catventure-20261107.apk'
OLD=ROOT/'Builds/Android/LightBetweenCultures-RC1-20261024.apk'
EXPECTED_CODE=20
EXPECTED_VERSION='1.1-catventure-20261107'
if '--labels-layout-fix' in sys.argv:
    OUT=ROOT/'Validation/Catventure-LabelsLayout-20261107'
    APK=ROOT/'Builds/Android/Catventure-Fix-20261107.apk'
    OLD=ROOT/'Builds/Android/Catventure-20261107.apk'
    EXPECTED_CODE=21
    EXPECTED_VERSION='1.1.1-catventure-20261107'
OUT.mkdir(parents=True,exist_ok=True)
def run(args,name):
    r=subprocess.run([str(a) for a in args],cwd=ROOT,capture_output=True,text=True,encoding='utf-8',errors='replace')
    (OUT/name).write_text(r.stdout+r.stderr,encoding='utf-8')
    assert r.returncode==0,(name,r.returncode,(r.stdout+r.stderr)[-1600:])
    return r.stdout+r.stderr
def verify():
    badging=run([TOOLS/'aapt.exe','dump','badging',APK],'apk-badging.txt')
    old_badging=run([TOOLS/'aapt.exe','dump','badging',OLD],'original-apk-badging.txt')
    def package(s):return re.search(r"package: name='([^']+)' versionCode='([^']+)' versionName='([^']+)'",s).groups()
    name,code,version=package(badging);old_name,old_code,_=package(old_badging)
    assert name==old_name and int(code)==EXPECTED_CODE and int(code)>int(old_code) and version==EXPECTED_VERSION
    signing=[ENGINE/'OpenJDK/bin/java.exe','-jar',TOOLS/'lib/apksigner.jar','verify','--verbose','--print-certs']
    signature=run(signing+[APK],'apk-signature.txt');previous=run(signing+[OLD],'original-apk-signature.txt')
    def cert(s):return re.search(r'Signer #1 certificate SHA-256 digest: (\S+)',s).group(1)
    assert cert(signature)==cert(previous),'Signing identity changed'
    run([TOOLS/'zipalign.exe','-c','-P','16','4',APK],'apk-alignment.txt')
    with zipfile.ZipFile(APK) as z:
        assert z.testzip() is None
        metadata=z.read('assets/bin/Data/Managed/Metadata/global-metadata.dat')
        for token in [b'CatHanbokGraphic',b'CatMiniMapGraphic',b'PickHanbokPalette',b'paletteChosen']:
            assert token in metadata,('Latest production type missing from APK',token)
    result=dict(package=name,versionCode=int(code),versionName=version,bytes=APK.stat().st_size,sha256=hashlib.sha256(APK.read_bytes()).hexdigest(),signatureVerified=True,signingReference=str(OLD),samePackageAndSignerAsReference=True,alignment16K=True,zipCRC=True,latestHanbokAndMiniMapTypesPresent=True,device='NOT_TESTED_NO_DEVICE_CONNECTED')
    (OUT/'android-verification.json').write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    (OUT/'android-verification.txt').write_text('PASS: package/version, signature, same signer as reference APK, 16KB zip alignment, complete ZIP CRC.\nAndroid DEVICE NOT_TESTED: no ADB device connected.\n'+json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print('APK verification PASS:',name,version,'code',code,'; same signing identity; device NOT_TESTED')
if __name__=='__main__':verify()
