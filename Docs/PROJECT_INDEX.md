# M?c l?c project

M?c l?c n?y t? ch?c vi?c tra c?u, kh?ng di chuy?n c?c ???ng d?n ?? ???c code/tool/manifest tham chi?u.

| C?n t?m | V? tr? | Vai tr? |
|---|---|---|
| ?i?m b?t ??u | [README](../README.md) | M? project, build ?ang gi?, tr?ng th?i hi?n t?i |
| Source runtime | [Assets/_Game/Scripts](../Assets/_Game/Scripts/) | Core, Content, Maps, UI; HoiAn l? c?nh m? ??u/H3 |
| N?i dung s?u th?nh ph? | [GameplayContent.json](../Assets/_Game/Resources/Content/GameplayContent.json) | Registry canonical, 133 node, event/quest/puzzle |
| ???ng ?i ?? duy?t | [CityNavigation.json](../Assets/_Game/Resources/Content/CityNavigation.json), [Docs/Maps](Maps/) | Graph, junction v? thuy?n; kh?ng s?a trong cleanup |
| Scene/prefab/registry Unity | [Scenes](../Assets/_Game/Scenes/), [Prefabs](../Assets/_Game/Prefabs/), [Resources](../Assets/_Game/Resources/) | Gi? nguy?n asset/GUID/meta |
| Ngo?i h?nh chu?n | [Lumi](Art/LUMI_CANONICAL_20261021.md), [NPC](Art/NPC_CANONICAL_REGISTRY.md) | Tham chi?u authoritative cho nh?n v?t |
| QA m?i nh?t | [Current status](QA/CURRENT_QA_STATUS.md), [Final UX](QA/FINAL_UX_20261023.md) | PASS/NOT_TESTED v? checklist ng??i m?i ch?i |
| B?ng ch?ng m?i nh?t | [Gallery](../Validation/FinalUX-20261023/gallery.html), [summary](../Validation/FinalUX-20261023/summary.json) | ?nh/runtime results, kh?ng ??nh ??ng automation v?i ?? vui |
| QA l?ch s? | [Docs/QA](QA/), [Validation](../Validation/) | L?u t?i ch? ?? gi? link v? b?ng ch?ng l?i/nghi?m thu |
| C?ng c? | [Tools](../Tools/) | Authoring, import, evidence, packaging; kh?ng t? x?a script ch? v? t?n Phase c? |
| Route editor | [SixCityRouteEditor](../Tools/SixCityRouteEditor/) | D?ng d? li?u v? art ph? thu?c; gi? nguy?n th? m?c |
| H? s? d? thi | [Submission](../Submission/), [Docs/Contest](Contest/) | T?i li?u b?n giao, kh?ng ph?i cache |
| Kit/?nh g?c | C?c th? m?c LBC_*, DesignKit, References, ReviewPacks ? root | Ngu?n nh?p v? provenance; gi? t?i ch?, ch?a x?c minh h?t ph? thu?c ?? di chuy?n |
| D?n dung l??ng | [Audit](Storage/SafeCleanup-20261023/AUDIT.md), [k?t qu?](Storage/SafeCleanup-20261023/RESULT.md) | K? ho?ch, recovery, danh s?ch ch?nh x?c file x?a |

## Nh?ng vi?c kh?ng c?n l?m

Kh?ng ch?y l?i to?n b? generator/Phase setup ?? m? project. Kh?ng ??i t?n h?ng lo?t asset ho?c x?a `.meta`. Kh?ng di chuy?n Tools/Docs ch? ?? ??p th? m?c: nhi?u ???ng d?n ???c hard-code trong c?c c?ng c? v? manifest.

Cache Gradle output, Windows PlayerDataCache v? objcopy symbol c? th? ???c t?o l?i b?i l?n build sau. Gi? Library import DB v? PackageCache gi?p tr?nh reimport/download kh?ng c?n thi?t. Save v? signing configuration ???c b?o to?n.
