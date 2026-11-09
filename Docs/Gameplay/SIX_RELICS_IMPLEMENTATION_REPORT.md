# Six Relics — trạng thái triển khai Phase1–3 ·18/10/2026

**Chỉ pilot Hội An có gameplay di vật hoàn chỉnh được targeted Windows QA; chưa hoàn thành Six Relics/toàn game.**

- [Pilot: chức năng, ảnh, QA, giới hạn](CULTURAL_ARTIFACTS_HOIAN_PILOT_REPORT.md)
- [Kiến trúc/persistence/registry](CULTURAL_ARTIFACTS_INTEGRATION_REPORT.md)
- [Art manifest + Seoul ART_PENDING](../Art/SIX_RELICS_ART_MANIFEST.md)
- [4 NPC technical integration, chưa canonical](../Art/NPC_FOUR_NEW_ART_INTEGRATION_20261017.md)
- [Review ảnh thực tế](RelicPilot-20261018/RuntimeReview.html)
- [File diff/manifest](RelicPilot-20261018/changed-files.json)

| Phase | Trạng thái |
|---|---|
|0 approval | D01–D08 và H2/persistence đã ghi vào Decisions/24-matrix. Hội An nghi thức đợi H7; pickup không là reflection proof. |
|1 NPC |4 native Single bindings thực, correct speaker/portrait runtime PASS;9 canonical+12 unconfirmed cũ giữ. Art appearance chưa duyệt. |
|2 art/data |6 full +21 matched fragments=27 native sprites,3 Seoul icons pending;6 definitions/24 IDs một registry. Chưa claim/trial năm city khác. |
|3 Hội An | H0/H4/H10/H2 gestures/proof/claims, assembly+beam, H7 ritual gate, full-city overlay, H6 alignment/album, old-save recovery và actual process restart PASS. Visual acceptance pending. |
|4–7 | NOT_STARTED, giữ phạm vi chờ cho phép. D07 policy đã approved nhưng ending runtime chưa sửa. |

Không thêm node/quest/canonical puzzle/NPC identity. Save version2 giữ nguyên; không ghi state mới cho global ending. Main map/boss không đọc relic state. [Checkpoint index](MASTER_EXECUTION_CHECKPOINT.md) ghi base và bước dừng chính xác.
