import subprocess,pathlib,json,time,os,sys,hashlib
ROOT=pathlib.Path(__file__).resolve().parent
ADB=r'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
SERIAL='007514870e238617';PACKAGE='com.DefaultCompany.LightBetweenCultures'
SAVE='/sdcard/Android/data/'+PACKAGE+'/files/hoi-an-v1.json'
os.environ['ANDROID_USER_HOME']=r'C:\Users\admin\.android'
os.environ['ANDROID_SDK_HOME']=r'C:\Users\admin'
records=[]
def adb(*args,timeout=15,required=True):
    start=time.monotonic();r=subprocess.run([ADB,'-s',SERIAL,*args],capture_output=True,timeout=timeout)
    records.append({'command':list(args),'seconds':round(time.monotonic()-start,3),'exitCode':r.returncode,'output':r.stdout.decode('utf-8',errors='replace') if args[:2]!=('exec-out','screencap') else '<image>','stderr':r.stderr.decode('utf-8',errors='replace')})
    if required and r.returncode:raise RuntimeError(str(args)+': '+r.stderr.decode(errors='replace'))
    return r
def shot(name):
    data=adb('exec-out','screencap','-p',timeout=20).stdout
    assert data.startswith(b'\x89PNG');(ROOT/(name+'.png')).write_bytes(data)
def read_save(name):
    r=adb('exec-out','cat',SAVE,required=False)
    if r.returncode or b'No such file or directory' in r.stdout:return None
    data=json.loads(r.stdout);(ROOT/(name+'.json')).write_bytes(r.stdout);return data
def status():
    r=adb('shell','dumpsys','package',PACKAGE).stdout.decode(errors='replace')
    (ROOT/'android-installed-package.txt').write_text(r,encoding='utf-8')
    return [x.strip() for x in r.splitlines() if 'versionName=' in x or 'versionCode=' in x or 'User 0:' in x or 'User 10:' in x]
try:
    mode=sys.argv[1]
    if mode=='install':
        adb('get-state');before=read_save('android-pre-update-save')
        adb('install','--user','0','-r',str(ROOT.parents[1]/'Builds/Android/LightBetweenCultures-RC2-20261024.apk'),timeout=100)
        print('\n'.join(status()),flush=True)
        adb('shell','input','keyevent','224')
        adb('shell','am','start','--user','0','-W','-n',PACKAGE+'/com.unity3d.player.UnityPlayerGameActivity',timeout=25)
        time.sleep(4);shot('android-RC2-menu')
        (ROOT/'android-install-result.json').write_text(json.dumps({'result':'INSTALLED_AND_LAUNCHED','existingPrimarySave':before is not None,'profile10Save':'NOT_ACCESSED_OR_MODIFIED','captures':['android-RC2-menu.png']},indent=2),encoding='utf-8')
    elif mode=='checkpoint':
        read_save('android-safe-state');shot('android-RC2-checkpoint')
    elif mode=='restart':
        adb('shell','input','keyevent','3');time.sleep(1)
        before=read_save('android-before-cold-restart');assert before is not None,'No committed save to verify'
        adb('shell','am','force-stop','--user','0',PACKAGE)
        adb('shell','am','start','--user','0','-W','-n',PACKAGE+'/com.unity3d.player.UnityPlayerGameActivity',timeout=25)
        time.sleep(3);after=read_save('android-after-cold-restart');shot('android-RC2-restarted-menu')
        assert after==before,'Committed save differs after cold restart'
        print('COLD_RESTART_EXACT_SAVE_PASS',flush=True)
        (ROOT/'android-save-result.json').write_text(json.dumps({'result':'PASS','scope':'primary profile test save only','beforeSHA256':hashlib.sha256((ROOT/'android-before-cold-restart.json').read_bytes()).hexdigest(),'afterSHA256':hashlib.sha256((ROOT/'android-after-cold-restart.json').read_bytes()).hexdigest(),'city':after.get('cityId'),'node':after.get('node'),'coins':after.get('coins'),'quests':after.get('accepted'),'completed':after.get('completed')},indent=2),encoding='utf-8')
finally:
    (ROOT/('android-'+sys.argv[1]+'-commands.json')).write_text(json.dumps(records,indent=2),encoding='utf-8')
