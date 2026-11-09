"""Real Android input driven by the QA build's read-only runtime telemetry.

No writes to saves, game APIs, quest flags, inventory or rewards. Finite stages,
one diagnostic retry for transport; captures only milestones and first failure.
"""
import argparse
import hashlib
import json
import os
from pathlib import Path
import socket
import subprocess
import time

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'Validation' / 'AndroidAutoPlay-20261024'
ADB = r'C:\Program Files\Unity\Hub\Editor\6000.6.5f1\Editor\Data\PlaybackEngines\AndroidPlayer\SDK\platform-tools\adb.exe'
SERIAL = '007514870e238617'
PACKAGE = 'com.DefaultCompany.LightBetweenCultures.androidqa'

class Blocked(RuntimeError):
    pass

class Driver:
    def __init__(self, seconds=1800):
        self.started = time.monotonic()
        self.deadline = self.started + seconds
        self.actions = 0
        self.polls = 0
        self.last = {}
        self.milestones = []
        self.tried = set()
        OUT.mkdir(exist_ok=True)
        self.events = (OUT / 'playthrough.jsonl').open('a', encoding='utf-8')

    def checktime(self):
        if time.monotonic() > self.deadline:
            raise Blocked('Overall automation timeout')

    def adb(self, *args, timeout=10):
        self.checktime()
        p = subprocess.run([ADB, '-s', SERIAL, *map(str,args)], capture_output=True,
                           encoding='utf-8', errors='replace', timeout=timeout)
        if p.returncode:
            raise Blocked((p.stdout+p.stderr)[-1200:])
        return p.stdout.strip()

    def record(self, kind, **fields):
        self.events.write(json.dumps(dict(t=round(time.monotonic()-self.started,2),type=kind,**fields),ensure_ascii=False)+'\n')
        self.events.flush()

    def state(self):
        self.checktime()
        for attempt in range(2):
            try:
                with socket.create_connection(('127.0.0.1',38777),timeout=3) as connection:
                    connection.settimeout(3)
                    data = bytearray()
                    while not data.endswith(b'\n'):
                        chunk=connection.recv(65536)
                        if not chunk:
                            break
                        data.extend(chunk)
                        if len(data)>2_000_000:
                            raise Blocked('Telemetry exceeded bound')
                s=json.loads(data)
                if s.get('error'):
                    raise Blocked('Bridge state error: '+s['error'])
                if s.get('androidUser')!=11:
                    raise Blocked('BLOCKED_SAVE_ISOLATION: bridge user is not 11')
                self.last=s
                self.polls+=1
                return s
            except (OSError,ValueError) as exc:
                if attempt:
                    raise Blocked('Bridge transport failed after one retry: '+str(exc))
                self.record('transport_retry',reason=str(exc))
                time.sleep(.5)

    def wait(self, predicate, label, seconds=15):
        end=min(self.deadline,time.monotonic()+seconds)
        while time.monotonic()<end:
            s=self.state()
            if predicate(s):
                return s
            time.sleep(.25)
        raise Blocked('Timeout: '+label)

    def available(self, s=None):
        return [c for c in (s or self.last).get('controls',[]) if c.get('enabled') and c.get('hit')]

    def control(self, identifier, s=None, require_hit=True):
        found=[c for c in (s or self.last).get('controls',[]) if c['id']==identifier and c.get('enabled') and (c.get('hit') or not require_hit)]
        if not found:
            raise Blocked('No verified input target: '+identifier)
        if len(found)>1:
            raise Blocked('Ambiguous runtime input target: '+identifier)
        return found[0]

    def input(self, args, target):
        # Android input acts on the focused window; check that window before
        # every gesture. The .androidqa package and UID guard exclude Owner.
        user=self.adb('shell','am','get-current-user')
        focus=self.adb('shell','dumpsys','window','windows')
        if user!='11' or not any('mCurrentFocus=' in line and 'u11 ' in line and PACKAGE in line for line in focus.splitlines()):
            raise Blocked('BLOCKED_SAVE_ISOLATION: QA window lost input focus')
        self.adb('shell','input',*args,timeout=10)
        self.actions+=1
        self.record('input',target=target,gesture=args,scene=self.last.get('scene'),city=self.last.get('city'),node=self.last.get('node'))
        time.sleep(.22)

    def tap(self, identifier):
        s=self.state()
        c=self.control(identifier,s)
        frame=s['frame']
        self.input(['tap',round(c['x']),round(c['y'])],identifier)
        return self.wait(lambda st:st['frame']>frame+3,'fresh frame after '+identifier,5)

    def swipe(self, source, target=None, duration=500):
        s=self.state()
        a=self.control(source,s)
        if target:
            b=self.control(target,s,require_hit=False)
            x1,y1,x2,y2=a['x'],a['y'],b['x'],b['y']
        elif source=='MicroHold':
            x1=x2=a['x'];y1=y2=a['y'];duration=1500
        else:
            x1=a['left']+3;y1=a['y'];x2=a['right']-3;y2=y1
        self.input(['swipe',round(x1),round(y1),round(x2),round(y2),duration],source+'->'+str(target))

    def capture(self, name):
        self.adb('shell','screencap','-p','/data/local/tmp/lbc-autoplay.png')
        self.adb('pull','/data/local/tmp/lbc-autoplay.png',OUT/(name+'.png'))
        (OUT/(name+'-state.json')).write_text(json.dumps(self.state(),ensure_ascii=False,indent=2),encoding='utf-8')

    def milestone(self, name):
        self.milestones.append(name)
        self.record('milestone',name=name,save=self.last.get('save'),diskHash=self.last.get('diskHash'))
        self.capture(name)
        print('MILESTONE',name,flush=True)

    @staticmethod
    def progress(s):
        save=s.get('save',{})
        keys=['accepted','completed','items','flags','solvedPuzzles','defeatedEnemies','rewardGrantIds','coins','collected']
        return hashlib.sha256(json.dumps({k:save.get(k) for k in keys},sort_keys=True).encode()).hexdigest()

    def close(self):
        for _ in range(8):
            s=self.state();ids={c['id'] for c in self.available(s)}
            name=next((n for n in ['ReceiptContinue','BattleClose','ActivityClose','PuzzleClose','DialogueCloseX','EventClose','CheckInClose','GuideClose','CloseGuide','GuidanceClose'] if n in ids),None)
            if not name:
                return s
            self.tap(name)
        raise Blocked('Overlay close limit reached')

    def goto(self, node):
        self.close()
        s=self.state()
        if s['node']==node and not s['moving']:
            return
        n=next((n for n in s['nodes'] if n['id']==node),None)
        if not n or not n['reachable'] or not n['visible']:
            raise Blocked('Node not legally reachable: '+node)
        target='node:'+node
        if not any(c['id']==target for c in self.available(s)):
            self.tap('MapOverview')
        self.tap(target)
        self.wait(lambda st:st['node']==node and not st['moving'],'travel to '+node,65)

    def choose(self, option):
        for _ in range(5):
            s=self.state();ids={c['id'] for c in self.available(s)}
            if 'EventOption_'+option in ids:
                return self.tap('EventOption_'+option)
            next_button=next((n for n in ['ReceiptContinue','DialogueNext','Interact'] if n in ids),None)
            if not next_button:
                raise Blocked('Cannot reach dialogue choice '+option)
            self.tap(next_button)
        raise Blocked('Dialogue navigation limit: '+option)

    def start(self, resume):
        self.adb('forward','tcp:38777','tcp:38777')
        s=self.wait(lambda st:bool(st.get('controls')),'initial menu',30)
        self.milestone('00-start')
        if not resume:
            if s.get('save',{}).get('accepted') or s.get('save',{}).get('completed'):
                raise Blocked('QA package already has progress; refusing implicit reset')
            self.tap('Start')
            self.wait(lambda st:st['scene']=='WorldMap','world selection',12)
            self.tap('EnterCity')
            self.wait(lambda st:st['scene']=='CityMap','Hoi An map',15)
            self.record('new_game',status='PASS',save=self.last['save'])

    def first_quest(self):
        self.goto('H2');self.choose('accept');self.close()
        self.goto('H3');self.choose('scene')
        self.wait(lambda s:s['scene']=='HoiAnDemo','H3 exploration',15)
        self.tap('world:lamp-eye-approach')
        self.wait(lambda s:not s.get('moving') and any(c['id']=='Collect' for c in self.available(s)),'physical pickup range',25)
        self.tap('Collect')
        self.wait(lambda s:'lamp_eye' in s['save']['items'] or 'hoi_an.lamp_eye_found' in s['save']['flags'],'lamp eye pickup',8)
        self.tap('Return');self.wait(lambda s:s['scene']=='CityMap','return to map',12)
        self.goto('H2');self.choose('complete')
        self.wait(lambda s:'HQ01' in s['save']['completed'],'HQ01 completion',12)
        self.close();self.milestone('01-HQ01-completed')
        self.verify_save()

    def verify_save(self):
        s=self.state()
        if s['recoveryBlocked'] or not s.get('diskSave'):
            raise Blocked('Save unavailable after quest')
        disk=json.loads(s['diskSave'])
        for key in ['cityId','completed','items','flags','coins','rewardGrantIds']:
            if disk.get(key)!=s['save'].get(key):
                raise Blocked('Disk/memory save mismatch: '+key)
        before=self.progress(s)
        self.adb('shell','am','force-stop','--user','11',PACKAGE)
        self.adb('shell','am','start','--user','11','-n',PACKAGE+'/com.unity3d.player.UnityPlayerGameActivity')
        time.sleep(3)
        after=self.wait(lambda st:bool(st.get('controls')),'save reload',20)
        if before!=self.progress(after):
            raise Blocked('Reload changed earned progress')
        self.record('save_load',status='PASS',diskHash=after['diskHash'])

    def run(self, resume=False):
        status='BLOCKED';reason=''
        try:
            self.start(resume)
            if 'HQ01' not in self.last['save']['completed']:
                self.first_quest()
            self.campaign()
            if not self.last.get('ending'):
                raise Blocked('Ending flag absent')
            status='ANDROID_FULL_PLAYTHROUGH_PASS'
        except Exception as exc:
            reason=str(exc);self.record('blocked',reason=reason)
            try:self.capture('first-failure')
            except Exception as capture_error:self.record('capture_error',error=str(capture_error))
        finally:
            try:
                processes=self.adb('shell','pidof',PACKAGE)
                for pid in processes.split():
                    (OUT/'logcat.txt').write_text(self.adb('logcat','-d','--pid='+pid,'-t','350'),encoding='utf-8')
                self.adb('shell','am','switch-user','0')
            except Exception as cleanup_error:self.record('cleanup_error',error=str(cleanup_error))
            report=dict(status=status,reason=reason,input_actions=self.actions,polls=self.polls,seconds=round(time.monotonic()-self.started,2),milestones=self.milestones,city=self.last.get('city'),node=self.last.get('node'),completed=self.last.get('save',{}).get('completed',[]),ending=self.last.get('ending',False))
            (OUT/'playthrough-result.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
            print(json.dumps(report,ensure_ascii=True),flush=True)
            self.events.close()

    def campaign(self):
        raise Blocked('Campaign awaiting verified opening quest/input support')

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--resume',action='store_true');parser.add_argument('--seconds',type=int,default=1800)
    args=parser.parse_args()
    Driver(args.seconds).run(args.resume)
