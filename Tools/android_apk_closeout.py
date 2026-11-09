"""Verify the produced artifact and frozen source; write a concise APK handoff."""
from pathlib import Path
import hashlib,json,zipfile,re,datetime

R=Path(__file__).resolve().parents[1];Q=R/'Docs/QA/AndroidApk-20261019'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def write(p,d):p.write_text(json.dumps(d,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')

apk=R/'Builds/Android/LightBetweenCultures-Test.apk'
result=read(Q/'build-result.json'); verification=read(Q/'apk-tool-verification.json')
assert result['result']=='Succeeded' and apk.is_file()
assert read(Q/'editor-exit.json')['exitCode']==0
assert all(verification[k]==0 for k in ['aaptExit','permissionsExit','signatureExit','alignmentExit'])
with zipfile.ZipFile(apk) as z:
    assert z.testzip() is None,'APK CRC corruption'
    names=z.namelist(); assert 'AndroidManifest.xml' in names and 'classes.dex' in names
    native=[n for n in names if n.startswith('lib/') and n.endswith('.so')]
    abi=sorted({n.split('/')[1] for n in native})
    assert abi==['arm64-v8a'],abi
    assert 'lib/arm64-v8a/libil2cpp.so' in names and 'lib/arm64-v8a/libunity.so' in names
badging=(Q/'apk-badging.txt').read_text(encoding='utf-8-sig')
package=re.search(r"package: name='([^']+)'",badging)[1]
minSDK=re.search(r"sdkVersion:'([^']+)'",badging)[1]
targetSDK=re.search(r"targetSdkVersion:'([^']+)'",badging)[1]
assert package==result['identifier'] and minSDK==str(result['minSDK'])
elf=read(Q/'native-elf-check.json')
assert len(elf)==len(native) and all(e['machine']==183 and e['atLeast16KiB'] for e in elf)
baseline=read(Q/'baseline.json');changed=[]
for e in baseline['entries']:
    p=R/e['path'];assert p.is_file(),'Missing protected file: '+str(p)
    if sha(p)!=e['sha256']:changed.append({'path':e['path'],'before':e['sha256'],'after':sha(p)})
write(Q/'protected-source-verification.json',{'checked':len(baseline['entries']),'changed':changed,'noDeletes':True})
assert not changed,'Unexpected source/settings delta: review before handoff'
permissions=(Q/'apk-permissions.txt').read_text(encoding='utf-8-sig')
permissionNames=re.findall(r"uses-permission(?:-sdk-\d+)?: name='([^']+)'",permissions)
report={'result':'PASS','apk':str(apk),'bytes':apk.stat().st_size,'MiB':round(apk.stat().st_size/1048576,2),'sha256':sha(apk),'package':package,'minSDK':minSDK,'targetSDK':targetSDK,'ABI':abi,'backend':result['backend'],'graphics':result['graphics'],'CRC':'PASS','aapt':'PASS','signature':'PASS','zipalign16KiB':'PASS','ELF64_ARM64_loadAlignment16KiB':'PASS','nativeLibraries':native,'permissions':permissionNames,'device':'ANDROID_DEVICE_NOT_TESTED','protectedSourceFiles':len(baseline['entries']),'protectedFilesChanged':0,'verifiedUTC':datetime.datetime.now(datetime.timezone.utc).isoformat()}
write(Q/'verified-apk.json',report)
text=f'''# Android APK build — 2026-10-19

**ANDROID BUILD: PASS. ANDROID_DEVICE_NOT_TESTED.**

- APK: `{apk}`
- Size: {report['bytes']:,} bytes ({report['MiB']} MiB).
- SHA256: `{report['sha256']}`.
- Unity {result['unity']}, ARM64 only, IL2CPP, Portrait, Vulkan/OpenGLES3.
- Package identifier: `{package}` (existing Android default; not renamed).
- Actual APK minSDK {minSDK}, targetSDK {targetSDK}. Project targetSDK remains Automatic.
- Bundled SDK build-tools36.0.0, NDKr27c/27.2.12479018, OpenJDK17.0.18.
- Existing debug test signing (`useCustomKeystore=false`); no production signing changes. BuildOptions.None, not a Development Player.

## Verified

Unity BuildPipeline Succeeded; Editor exit0; APK exists; ZIP CRC test PASS; Android `aapt dump badging` and permissions PASS; `apksigner verify --verbose --print-certs` PASS; `zipalign -c -P16 4` PASS. Archive contains ARM64 `libil2cpp.so` and `libunity.so`. These establish artifact/build validity; they do not prove phone gameplay or graphics compatibility on every device.

Build duration {result['buildSeconds']:.1f}seconds; errors{result['errors']}, warnings{result['warnings']}. Existing compiler/analyzer/stripped-shader and SDK metadata warnings remain in the raw log; no zero-warning claim. All eight native libraries are ELF64/AArch64 (machine183), with every PT_LOAD alignment16384; `native-elf-check.json` retains details. Unity's reported total output bytes include additional native/symbol/build outputs; the APK size above is measured from the actual signed file.

{len(baseline['entries'])} scoped production/source/assets/settings files match pre-build SHA256; zero deleted or modified. This includes current bot portraits and DrawDialogue, routes, gameplay, save, art/audio bindings. No Windows build or Phase7 replay. Existing scenes retained, including SampleScene. No cache/Library/backup cleanup.

Manifest permissions (existing package contributions, no project permissions added): {', '.join(permissionNames) or 'none reported by aapt'}.

## Execution and tooling

First launch stopped during compilation of the new Editor-only wrapper: counter fields used uint where this Unity API returns int (CS0266). Corrected those fields to int, then a controlled retry reached the actual APK build. It was a tooling error introduced here, not a production-game blocker. No Unity/package upgrade or gameplay correction.

New tooling: `Assets/_Game/Scripts/Editor/AndroidApkBuild.cs` (+meta), `Tools/verify_android_apk.ps1`, `Tools/android_apk_closeout.py`. The Editor entry point only calls BuildPipeline with existing scenes/settings and restores the prior AAB toggle. Output and result/config/exit records are in this folder; build logs in `Logs/android-apk-20261019-build01.log` and `build02.log`. No private signing key is copied into reports/checkpoint.

## Device testing

ADB device enumeration returned no connected phone. Do not wait for hardware and do not substitute Windows results for Android. Main menu, touch/node movement, zoom/pan, NPC/bot dialogue alpha, leisure, battle/audio and save/relaunch on an actual Android phone are all **NOT_TESTED**. Runtime frame rate, texture quality, audio and device-specific Vulkan/GLES behavior await the quick owner test.

## Install and quick test

Copy the APK to an ARM64 Android8.0+ phone. Open it in Files and allow installation from that app when Android asks; install and launch LightBetweenCultures. With USB debugging, `adb install -r "{apk}"` is an alternative. If an existing installation has a different signing certificate, do not uninstall or erase saves blindly; retain its data and report the signature mismatch first.

Test main menu → HộiAn node movement → zoom/pan → dialogue (check bot portrait edges) → leisure → battle/music → save → close/relaunch/load. Android may show a warning for a locally signed test APK; this is not a GooglePlay release package.

Incremental checkpoint requires the latest Phase7 base and retains only new tools/docs/logs, not another full project or APK copy. **STOP after handoff.**
'''
(R/'Docs/QA/ANDROID_APK_BUILD_20261019.md').write_text(text,encoding='utf-8')
print(json.dumps(report,ensure_ascii=False,indent=2))
