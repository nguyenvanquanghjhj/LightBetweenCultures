# Four new NPC technical integration ? 2026-10-18

User approved import/binding only. **ART_IMPORTED_PENDING_APPROVAL**, not CANONICAL/VISUAL_APPROVED. All21 previous NPC references retained. G9/G14/S2/S11 gameplay, quest and speaker text unchanged.

| NPC | Source | GUID / localID | Sprite/portrait binding | Identity review |
|---|---|---|---|---|
| npc_engineer | Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Engineer.png | 4f74a41c66ef3364ca044bc04afa2bcb / 21300000 | NPC_Seoul_Engineer (Sprite Single) | Engineer no glasses / Keeper older than brief; user review pending |
| npc_curator | Assets/_Game/Art/Characters/NPC/Seoul/NPC_Seoul_Curator.png | 6b7b12ad613bda14d969e359a2f08d59 / 21300000 | NPC_Seoul_Curator (Sprite Single) | Curator/Inscription faces close: NEEDS_USER_VISUAL_REVIEW |
| npc_keeper | Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Keeper.png | 880215f55ae56f2418894f7baed7f6cb / 21300000 | NPC_Gyeongju_Keeper (Sprite Single) | Engineer no glasses / Keeper older than brief; user review pending |
| npc_inscription | Assets/_Game/Art/Characters/NPC/Gyeongju/NPC_Gyeongju_Inscription.png | 399ff7e49a026ed42b9cb627929f5b35 / 21300000 | NPC_Gyeongju_Inscription (Sprite Single) | Curator/Inscription faces close: NEEDS_USER_VISUAL_REVIEW |

PNG1086?1448 RGBAalpha0?255, original bytes kept, full head/hands/feet. Sprite Single100PPU, max2048, Bilinear, no mipmaps/Crunch/compression. No borrowed canonical face. Curator and Inscription are separate dedicated PNG/GUIDs; that alone does not certify distinct visual identity. Runtime screenshots/tests will be appended after targeted Player verification.

## Runtime QA — PASS kỹ thuật, chưa duyệt ngoại hình

NPC01:57 assertions PASS trong earned postgame copy; bốn target captures dưới đây đều là Windows Player mới ngày18/10/2026. Hai utility WorldMap captures của helper cũng được giữ trong manifest, không dùng làm portrait approval. Binding và DrawDialogue không đổi giữa build03 dùng cho NPC01 và final build04; final build chỉ sửa relic partial/legacy proof/UI trial.

| Role / node | Ảnh runtime mới |
|---|---|
| Curator S2 | [runtime](../../Validation/RelicPilot-20261018/NPC01/NPC-S2-npc_curator.png) |
| Engineer S11 | [runtime](../../Validation/RelicPilot-20261018/NPC01/NPC-S11-npc_engineer.png) |
| Keeper G9 | [runtime](../../Validation/RelicPilot-20261018/NPC01/NPC-G9-npc_keeper.png) |
| Inscription G14 | [runtime](../../Validation/RelicPilot-20261018/NPC01/NPC-G14-npc_inscription.png) |

Đã mở xem cả bốn ảnh: nhân vật riêng đúng binding/speaker theo data hiện tại, đủ đầu/tay/chân, giữ tỷ lệ, không nền trắng, không che nút. Keeper/Inscription vẫn dùng speaker/location text hiện hành, không tự viết lại nhân vật/cốt truyện. Curator–Inscription có nét gần nhau; **NEEDS_USER_VISUAL_REVIEW**, không tự kết luận identity conflict hoặc approved vì khác GUID. Giữ9 canonical và12 unconfirmed khác byte-equivalent trong registry identity records. Không tạo map actor mới.
