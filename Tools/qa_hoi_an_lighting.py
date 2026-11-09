"""Compare only this task's actual Player captures; never modifies source art or saves."""
import json
from pathlib import Path
import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[1]
RUN = ROOT / 'Validation/HoiAnLighting-20261018/run01'
OUT = ROOT / 'Docs/Gameplay/HoiAnLighting-20261018'


def main():
    names = ['A-before-tutorial', 'B-mai-returned', 'C-full-city-restored']
    images = [Image.open(RUN / (name + '.png')).convert('RGB') for name in names]
    assert all(im.size == (1280, 960) for im in images)
    # The live runner records identical world-to-screen map bounds in all three frames.
    records = (RUN / 'lighting-measurements.txt').read_text().splitlines()
    for name in names:
        row = next(r for r in records if r.startswith(name + ' |'))
        assert '1280x960' in row and 'zoom=1 ' in row
        assert 'mapMin=(94.72, 78.72) mapMax=(1185.28, 896.64)' in row
    arrays = [np.asarray(im, dtype=float) for im in images]
    # Fixed full-map ROI. Discard near-black pixels for stable ratios, and avoid HUD.
    roi = (slice(65, 880), slice(96, 1184))
    luma = [a[roi] @ np.array([.2126, .7152, .0722]) for a in arrays]
    a, b, c = luma
    valid = (c > 45) & (c < 235)
    ac, bc = a[valid] / c[valid], b[valid] / c[valid]
    gain = b[valid] / np.maximum(a[valid], 1)
    metrics = {
        'method': 'sRGB luma median ratios in fixed map ROI; game objects/halos can locally differ',
        'screen': [1280, 960], 'zoom': 1,
        'A_over_C_median': float(np.median(ac)),
        'A_perceived_darker_percent': float(100 * (1 - np.median(ac))),
        'B_near_canonical_pixel_percent': float(100 * np.mean(bc >= .94)),
        'B_dark_pixel_percent': float(100 * np.mean(bc < .7)),
        'B_over_A_bright_district_median': float(np.median(gain[gain > 1.4])),
        'B_over_C_restored_district_median': float(np.median(bc[gain > 1.4])),
        'B_over_C_distant_district_median': float(np.median(bc[gain < 1.05])),
    }
    for name, area in [('world_button', (slice(895, 949), slice(15, 72))),
                       ('inventory_journal', (slice(0, 60), slice(1150, 1275)))]:
        x, y = arrays[0][area], arrays[2][area]
        stable = (x.mean(2) > 50) & (y.mean(2) > 50)
        metrics['HUD_' + name + '_brightness_ratio_median'] = float(np.median(y[stable] / np.maximum(x[stable], 1)))
        metrics['HUD_' + name + '_exact_pixel_fraction'] = float(np.mean(np.max(np.abs(x-y), axis=2) == 0))
        # Dynamic font sampling can differ between milestones; compare brightness, not byte identity.
        assert metrics['HUD_' + name + '_brightness_ratio_median'] == 1
    picked = np.asarray(Image.open(RUN / 'A-eye-picked-not-returned.png').convert('RGB'), dtype=float)[roi] @ np.array([.2126, .7152, .0722])
    h7 = np.asarray(Image.open(RUN / 'B-after-H7-before-relic.png').convert('RGB'), dtype=float)[roi] @ np.array([.2126, .7152, .0722])
    metrics['A_picked_over_A_initial_median'] = float(np.median(picked[valid] / np.maximum(a[valid], 1)))
    metrics['B_after_H7_over_B_before_H7_median'] = float(np.median(h7[valid] / np.maximum(b[valid], 1)))
    assert .56 < metrics['A_over_C_median'] < .64, metrics
    assert .38 < metrics['B_near_canonical_pixel_percent'] / 100 < .52, metrics
    assert metrics['A_picked_over_A_initial_median'] == 1
    assert metrics['B_after_H7_over_B_before_H7_median'] == 1
    OUT.mkdir(parents=True, exist_ok=True)
    (OUT / 'runtime-pixel-analysis.json').write_text(json.dumps(metrics, indent=2) + '\n', encoding='utf-8')
    font = ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf', 22)
    sheet = Image.new('RGB', (1920, 526), '#132b30')
    draw = ImageDraw.Draw(sheet)
    labels = ['A — Chưa trả Mắt Đèn', 'B — Đã trả Cô Mai; chưa phục dựng', 'C — Đèn Mắt Phố đã restored']
    for i, (im, label) in enumerate(zip(images, labels)):
        draw.text((i * 640 + 12, 9), label, fill='#ffe8ad', font=font)
        sheet.paste(im.resize((640, 480), Image.Resampling.LANCZOS), (i * 640, 46))
    sheet.save(OUT / 'ABC-runtime-comparison.png')
    extra = ['A-eye-picked-not-returned', 'B-after-H7-before-relic', 'B-wide-district', 'C-first-restoration-spread']
    html = ['<!doctype html><meta charset="utf-8"><title>Hội An — Three-stage lighting</title>',
            '<style>body{background:#10272d;color:#ffe8b6;font:16px system-ui;margin:24px}img{max-width:100%;height:auto}figure{margin:20px 0}a{color:#9be5dc}.grid{display:grid;grid-template-columns:repeat(3,1fr);gap:12px}</style>',
            '<h1>Hội An — A / B / C runtime</h1><p>Windows Player thật, cùng 1280×960, zoom 1, focus (0.5,0.5). Click ảnh để xem PNG gốc. Không mockup, không chỉnh màu ảnh runtime.</p><div class="grid">']
    for name, label in zip(names, labels):
        src = '../../../Validation/HoiAnLighting-20261018/run01/' + name + '.png'
        html.append(f'<figure><figcaption>{label}</figcaption><a href="{src}"><img src="{src}"></a></figure>')
    html.append('</div>')
    for name in extra:
        src = '../../../Validation/HoiAnLighting-20261018/run01/' + name + '.png'
        html.append(f'<figure><figcaption>{name}</figcaption><a href="{src}"><img src="{src}"></a></figure>')
    html.append('<h2>Save / process restart</h2><div class="grid">')
    for case, state in [('A', 'A'), ('A-picked', 'A'), ('B', 'B'), ('B-H7', 'B'), ('C', 'C')]:
        src = f'../../../Validation/HoiAnLighting-20261018/restart-{case}/restart-{state}.png'
        html.append(f'<figure><figcaption>Restart {case}</figcaption><a href="{src}"><img src="{src}"></a></figure>')
    html.append('</div><h2>Chùa Cầu — traversal, không mở postcard H6</h2>')
    for label in ['under-roof', 'exit']:
        src = f'../../../Validation/HoiAnLighting-20261018/restart-C/lighting-ChuaCau-{label}.png'
        html.append(f'<figure><figcaption>{label}</figcaption><a href="{src}"><img src="{src}"></a></figure>')
    (OUT / 'RuntimeReview.html').write_text('\n'.join(html), encoding='utf-8')
    print(json.dumps(metrics, indent=2))


if __name__ == '__main__':
    main()
