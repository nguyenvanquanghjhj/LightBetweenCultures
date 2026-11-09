"""Original deterministic synthesis; no downloaded samples or copied game audio."""
from pathlib import Path
import json, math, wave, hashlib
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'Assets/_Game/Audio/SFX'
DOC=ROOT/'Docs/QA/SoftSfx-20261018'
SPECS={
 'UI_Click':(.095,[(0,523.25,.8),(0,1046.5,.10)]),
 'UI_Back':(.17,[(0,493.88,.6),(.055,392,.5)]),
 'UI_Confirm':(.31,[(0,523.25,.5),(.075,659.25,.5),(.15,783.99,.4)]),
 'UI_Invalid':(.19,[(0,349.23,.5),(.06,329.63,.35)]),
 'Item_Pickup':(.38,[(0,659.25,.6),(.08,987.77,.4),(.16,1318.51,.18)]),
 'Relic_Place':(.25,[(0,440,.5),(.018,659.25,.32),(.04,880,.15)]),
 'Puzzle_Success':(.82,[(0,523.25,.4),(.10,659.25,.4),(.20,783.99,.4),(.31,1046.5,.25)])}

def main():
 OUT.mkdir(exist_ok=False)
 records=[];rate=44100
 for name,(duration,tones) in SPECS.items():
  t=np.arange(round(duration*rate))/rate;signal=np.zeros_like(t)
  for start,freq,gain in tones:
   u=np.maximum(t-start,0);attack=np.minimum(u/.006,1)
   envelope=attack*np.exp(-u/(.045 if name=='UI_Click' else .12))*(t>=start)
   signal+=gain*envelope*(np.sin(2*math.pi*freq*u)+.12*np.sin(2*math.pi*freq*2*u))
  signal*=np.minimum((duration-t)/.028,1).clip(0,1)
  signal*=.24/max(.001,np.abs(signal).max());signal[0]=signal[-1]=0
  pcm=np.round(signal*32767).astype('<i2');path=OUT/(name+'.wav')
  with wave.open(str(path),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(rate);f.writeframes(pcm.tobytes())
  records.append({'id':name,'path':path.relative_to(ROOT).as_posix(),'duration':duration,'sampleRate':rate,'channels':1,'peakDBFS':round(20*np.log10(np.abs(signal).max()),2),'rmsDBFS':round(20*np.log10(np.sqrt(np.mean(signal**2))),2),'firstSample':int(pcm[0]),'lastSample':int(pcm[-1]),'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'tones':tones})
 (DOC/'synthesis.json').write_text(json.dumps({'provenance':'Original sine partials/envelopes synthesized for LBC; no external samples; project may use/modify/distribute without third-party sample restrictions.','assets':records},ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
 print('Created seven original mono PCM16 WAVs',sum((ROOT/e['path']).stat().st_size for e in records),'bytes')
if __name__=='__main__':main()
