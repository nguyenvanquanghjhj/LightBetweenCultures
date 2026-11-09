from pathlib import Path
import re,json,hashlib
R=Path(__file__).resolve().parents[2];O=Path(__file__).parent
p=R/'Assets/_Game/Resources/AdventureUIArt.asset';s=p.read_text()
# UI units; only reviewed actor offsets. No node/route/interaction values.
offsets={'D2':(28,32),'D3':(-12,30),'D4':(2,0),'D10':(46,18),'D13':(22,-5),'J5':(22,-8),'G3':(52,25),'G16':(50,-15),'G21':(34,-15),'S12':(-24,40)}
changes=[]
for node,(x,y) in offsets.items():
 pattern=r'(    nodeId: '+node+r'\n    artId: (npc_\w+)\n    size: [^\n]+\n    offset: )\{x: ([-\d.]+), y: ([-\d.]+)\}'
 m=re.search(pattern,s);assert m,node
 old=[float(m[3]),float(m[4])];changes.append(dict(node=node,npc=m[2],before=old,after=[x,y],delta=[x-old[0],y-old[1]]))
 s=re.sub(pattern,lambda m:m[1]+f'{{x: {x}, y: {y}}}',s,count=1)
p.write_text(s,encoding='utf-8')
(O/'changes.json').write_text(json.dumps(changes,indent=2))
