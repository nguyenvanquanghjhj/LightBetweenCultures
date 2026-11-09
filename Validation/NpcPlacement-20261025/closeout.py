from pathlib import Path
import csv,json,re,hashlib,html
R=Path(__file__).resolve().parents[2];O=Path(__file__).parent
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
changes=json.loads((O/'changes.json').read_text());accepted={c['node'] for c in changes}
pending={'U2':'Chân trên mái nhà thuốc; nền đứng phù hợp cách xa, chưa có chỉnh nhỏ được xác nhận.',
 'U7':'Chân sát cây và prop ngai; chưa tìm được offset nhỏ đồng thời tránh vật trang trí và Lumi.',
 'D4':'Chân sát bồn hoa; chỉnh thử che NPC bởi Lumi, đã hoàn nguyên.',
 'D10':'Chân trên tán cây; chỉnh thử vẫn chưa đưa đủ chân ra nền, đã hoàn nguyên.',
 'G16':'Chân trên cây/mái bên đền; chỉnh thử bị hệ thống tiếp cận chặn, đã hoàn nguyên.',
 'G21':'Chân ở bụi hoa; chỉnh thử vẫn nằm trong mảng cây/bồn hoa, đã hoàn nguyên.'}
notes={'D2':'Đưa chân từ mép lan can/nước về lối đi ven hồ.', 'D3':'Đưa chân từ mép rào về lối lát đá phía trong.',
 'D13':'Hạ chân từ mép luống hoa về mép lối vườn.', 'J5':'Hạ chân từ chân tường/cửa xuống nền trước cửa.',
 'G3':'Đưa chân khỏi bụi cây sang lối cạnh hàng rào đài.', 'S12':'Đưa chân khỏi cây/bồn hoa lên lối công viên phía trên.'}
rows=list(csv.DictReader((O/'after/placement-runtime.csv').open(encoding='utf-8-sig')))
assert len(rows)==29 and len(accepted)==6
assert all(r['dialogue']==r['close']==r['resume']=='True' for r in rows if r['node'] in accepted)
before=(O/'BeforeSource/AdventureUIArt.asset').read_text();after=(R/'Assets/_Game/Resources/AdventureUIArt.asset').read_text()
normalized=after
for c in changes:
 pattern=r'(    nodeId: '+c['node']+r'\n    artId: npc_\w+\n    size: [^\n]+\n    offset: )\{[^}]+\}'
 old=re.search(pattern,before)[0];normalized=re.sub(pattern,lambda m:old,normalized,count=1)
assert normalized==before,'Unexpected binding/actor delta'
baseline=json.loads((R/'Validation/FullGame-20261025/source-before.json').read_text())
current={p.relative_to(R).as_posix():sha(p) for d in ['Assets','Packages','ProjectSettings'] for p in (R/d).rglob('*') if p.is_file()}
changed=sorted(k for k in baseline.keys()|current.keys() if baseline.get(k)!=current.get(k))
owned={'Assets/_Game/Resources/AdventureUIArt.asset','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.NpcPlacement.cs','Assets/_Game/Scripts/Validation/GameplaySmokeRunner.NpcPlacement.cs.meta'}
external=set(changed)-owned
assert all(p.startswith('Assets/_Game/Scripts/Validation/') for p in external),changed
content=json.loads((R/'Assets/_Game/Resources/Content/GameplayContent.json').read_text(encoding='utf-8-sig'))
assert sum(len(c['nodes']) for c in content['cities'])==133
integrity={'canonicalNodes':133,'onlyAcceptedActorOffsetsChanged':True,'protectedAssetsPackagesSettingsExactAgainstDailyBaseline':True,'sourceChanges':changed,'otherQaFilesChangedDuringAuditNotEditedByThisTask':sorted(external),'acceptedChanges':changes,'pendingVisualNodes':pending,'allRuntimeRows':29,'acceptedInteractionPass':6,'globalRuntimeAcceptance':'PARTIAL','qaBuild':'Windows-NpcPlacementQA-20261025 contains rejected trials; diagnostic only, not a final release','bell':'Original masks restored after trial showed detached rope pixels. Unresolved.'}
(O/'final-integrity.json').write_text(json.dumps(integrity,ensure_ascii=False,indent=2),encoding='utf-8')
lines=['# Kiểm tra vị trí NPC — 25/10/2026','',
 'Kết quả: **29 NPC tại 6 City Map; 17 vị trí đạt và giữ nguyên; 12 vị trí có lỗi hiển thị, trong đó 6 đã sửa và 6 để owner quyết định.** Không tính enemy hoặc marker thông tin là NPC. Chó D10 được tính. G16, G21 và S12 đều được kiểm tra.', '',
 'Ảnh Windows Player: 720×1280, zoom 2.4, cùng save QA hoàn thành sao chép từ `Validation/PcGameplayFix-20261024/campaign/pc-final-state.json`. Mỗi NPC được đặt fixture tại đúng node canonical để soi chân, sau đó click sprite bằng chuột; hệ thống tiếp cận hiện có tự xử lý. Fixture đặt node không được dùng làm bằng chứng đi hết campaign. Ảnh đo chân dùng mesh sprite thực tế; kết luận nền cảnh được đọc trực quan từ ảnh. Không tuyên bố kiểm thử mọi zoom/tỉ lệ cửa sổ.', '',
 '## Thay đổi được giữ lại','',
 '| NPC / node | Offset cũ → mới (UI unit) | Δx, Δy | Lý do | Chuột / hội thoại / đóng / đi tiếp |', '|---|---|---|---|---|']
for c in changes:
 lines.append(f"| {c['npc']} / {c['node']} | {c['before']} → {c['after']} | {c['delta']} | {notes[c['node']]} | PASS |")
lines+=['','Kích thước, sprite ID, ảnh nguồn, anchor node và phạm vi tương tác được giữ nguyên. Δ dương x là sang phải, Δ dương y là lên trên. Đây là offset UI đã counter-scale theo hệ thống sẵn có, không phải tọa độ canonical.', '',
 '## Chưa sửa an toàn','', '| Node | Kết luận |','|---|---|']+[f'| {n} | {s} |' for n,s in pending.items()]
lines+=['','## Kiểm kê toàn bộ','', '| City | Node | NPC | Hiển thị | Tương tác runtime |','|---|---|---|---|---|']
for r in rows:
 n=r['node'];visual='Đã sửa' if n in accepted else 'Owner quyết định' if n in pending else 'Đạt; giữ nguyên'
 interaction='PASS' if r['dialogue']==r['close']==r['resume']=='True' else 'BLOCKED tiếp cận' if r['dialogue']=='False' else 'Mở/đóng PASS; đi tiếp chưa xác nhận'
 if n=='G16':interaction='Trial bị chặn; offset đã hoàn nguyên'
 lines.append(f"| {r['city']} | {n} | {r['npc']} | {visual} | {interaction} |")
lines+=['','## Giới hạn và lỗi có sẵn','',
 '- Cả 6 offset được giữ lại đều PASS click sprite → tiếp cận → hội thoại → nút X → Lumi đi sang node lân cận. Đọc/đóng không đổi save.',
 '- G14 có nền đứng hợp lý và được giữ nguyên, nhưng tiếp cận bị chặn ngay cả khi không sửa vị trí. Đợt baseline bằng bộ test cũ cũng dừng khi chờ hội thoại G14. Không tự sửa graph.',
 '- J15 mở/đóng hội thoại được; lượt click node lân cận chưa xác nhận đi tiếp. Đây là vị trí giữ nguyên; cần owner kiểm tra riêng. Không kết luận là lỗi đường đi từ một fixture.',
 '- Log runner kết thúc `ALL CHECKS PASSED` nhưng có ba dòng `REVIEW` (J15/G14/G16); vì vậy nghiệm thu toàn bộ được ghi **PARTIAL**, không dùng dòng cuối để tuyên bố 29/29 tương tác đạt.',
 '- Tại vị trí tới node của fixture, Lumi có thể che một phần NPC. Những trường hợp còn chưa xác nhận được khoảng hở hoàn toàn không được tuyên bố PASS về mọi trạng thái tiếp cận.',
 '', '## Hoạt động đánh chuông','',
 'Đã thấy mảnh pixel cột/dây bị tách khi chày đung đưa. Mask chữ nhật cũ bao cả vùng gần cột/móc; trial chia rope mask tạo thêm đoạn dây đứng yên, nên đã hoàn nguyên toàn bộ `GameplayPanels.Leisure.cs`. Chưa giữ lại bản sửa chuông. Chuột kéo/thả, đóng panel và trở lại map vẫn PASS ở bản gốc. Cần tinh chỉnh mask được duyệt và kiểm tra nhiều pha chuyển động trước khi chốt; không đổi bitmap artwork.',
 '', '## Bằng chứng và tính toàn vẹn','',
 '- Mở `review.html` để xem trước/sau **chỉ 6 NPC được giữ sửa**; ảnh gốc nằm trong `before/Placement-<node>.png` và `after/Placement-<node>.png`.',
 '- Sáu contact sheet runtime trong `before/` ghi nhận toàn bộ 29 NPC. Ảnh trial khác là chẩn đoán, không phải bản sửa đã nghiệm thu.',
 '- `after/placement-runtime.csv`, `after/smoke-results.txt`, log Player, và hai build log được giữ nguyên, không sửa kết quả lịch sử.',
 '- `final-integrity.json`: chỉ sáu offset actor khác baseline; toàn bộ 133 node, graph Phase B, junction, ferry, gameplay, quest/reward/save/ending, artwork, ID và kích thước sprite không đổi. Ba file QA runner được thêm/sửa để chạy focused test, không thêm QA framework.',
 '- Hai lượt build Windows phục vụ audit và thử sửa; không Android, không campaign, không checkpoint lớn. Build `Windows-NpcPlacementQA-20261025` là trial chẩn đoán (còn chứa các chỉnh đã loại), **không phát hành như bản cuối**. Source cuối giữ sáu offset đã PASS và hoàn nguyên bốn trial cùng phần chuông. Không rebuild lần ba chỉ để đóng gói các hoàn nguyên.',
 '- Không xóa capture có tham chiếu trong manifest. Không ghi vào save của owner.']
if external:lines+=['','Hai file QA khác đổi trong workspace trong lúc audit, không do lượt này sửa và không hoàn nguyên: '+', '.join('`'+p+'`' for p in sorted(external))+'. Chúng không thuộc bằng chứng nghiệm thu của task này.']
(O/'REPORT.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
parts=['<!doctype html><meta charset="utf-8"><title>NPC placement review</title><style>body{font:16px system-ui;background:#eee;color:#222;max-width:1100px;margin:30px auto}section{background:white;padding:20px;margin:20px 0}.pair{display:flex;gap:16px}figure{margin:0;width:50%}img{width:100%}h2{margin-top:0}</style><h1>6 NPC: ảnh runtime trước / sau</h1><p>Windows 720×1280. Chỉ các thay đổi được giữ lại. <a href="REPORT.md">Báo cáo đủ 29 NPC</a></p>']
for c in changes:
 n=c['node'];parts.append(f'<section><h2>{html.escape(c["npc"])} — {n}</h2><p>Δ {c["delta"]} UI unit. {notes[n]} Tương tác: PASS.</p><div class="pair">'+''.join(f'<figure><figcaption>{label}</figcaption><a href="{side}/Placement-{n}.png"><img src="{side}/Placement-{n}.png"></a></figure>' for side,label in [('before','Trước'),('after','Sau')])+'</div></section>')
(O/'review.html').write_text(''.join(parts),encoding='utf-8')
(R/'Builds/Windows-NpcPlacementQA-20261025/QA_TRIAL_ONLY.txt').write_text('DIAGNOSTIC TRIAL ONLY. Contains rejected G16/D4/D10/G21 offsets and a rejected bell-mask trial. Final source restores those trials and retains six passing NPC offsets. See Validation/NpcPlacement-20261025/REPORT.md. Do not distribute this trial as a final game build.\n')
print(json.dumps({'checked':29,'unchangedCorrect':17,'incorrect':12,'fixed':6,'pending':list(pending),'changedSource':changed},ensure_ascii=True,indent=2))
