#18 ảnh check-in — crop và sharp nhẹ

Nguồn duy nhất: `_CheckIn_Import_20261018/images`, giữ nguyên PNG và chân thẻ gốc. Không resize/upscale, không AI. Unsharp Mask radius0.55 / percent25 / threshold3 chỉ trên vùng phong cảnh; giữ nguyên pixel phần chữ sau crop. Crop top chỉ loại dải trắng, không cắt cảnh. Crop vùng chữ ở UI bằng UV, không xóa chân bưu thiếp khỏi PNG derived. H6 chỉ thay art gallery; ảnh căn khớp Map A/hotspot/biểu tượng giữ nguyên.

| Filename gốc | Node | Crop L/R/T px | Trước → sau | Runtime path | Binding |
|---|---|---|---|---|---|
| 01_hoi_an_H2_tiem_den_co_mai.png | H2 | 0/0/0 | 450×726 → 450×726 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/01_hoi_an_H2_tiem_den_co_mai.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 02_hoi_an_H3_hem_mai_ngoi.png | H3 | 30/8/0 | 450×726 → 412×726 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/02_hoi_an_H3_hem_mai_ngoi.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 03_hoi_an_H4_cho_ven_song.png | H4 | 0/4/0 | 450×726 → 446×726 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/03_hoi_an_H4_cho_ven_song.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 04_hoi_an_H6_cau_co_anh_den.png | H6 | 0/0/0 | 450×726 → 450×726 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/04_hoi_an_H6_cau_co_anh_den.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 05_hoi_an_H7_tram_anh_sang_hoi_an.png | H7 | 9/0/0 | 450×726 → 441×726 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/05_hoi_an_H7_tram_anh_sang_hoi_an.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 06_hoi_an_H17_ben_do_pho_co.png | H17 | 21/0/0 | 450×726 → 429×726 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/06_hoi_an_H17_ben_do_pho_co.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 07_hue_U4_dau_cau_vao_thanh.png | U4 | 0/6/16 | 450×722 → 444×706 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/07_hue_U4_dau_cau_vao_thanh.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 08_hue_U6_cua_ngo_mon_cach_dieu.png | U6 | 0/0/16 | 450×722 → 450×706 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/08_hue_U6_cua_ngo_mon_cach_dieu.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 09_da_lat_D0_ben_ho_xuan_huong.png | D0 | 10/0/16 | 450×722 → 440×706 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/09_da_lat_D0_ben_ho_xuan_huong.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 10_da_lat_D14_dai_dan_sang_nui.png | D14 | 8/0/15 | 450×722 → 442×707 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/10_da_lat_D14_dai_dan_sang_nui.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 11_jeonju_J8_cau_da_tay.png | J8 | 4/0/16 | 450×722 → 446×706 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/11_jeonju_J8_cau_da_tay.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 12_jeonju_J11_dai_ngam_mai_ngoi.png | J11 | 5/0/16 | 450×722 → 445×706 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/12_jeonju_J11_dai_ngam_mai_ngoi.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png | G4 | 0/0/7 | 450×756 → 450×749 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/13_gyeongju_G4_dai_cheomseongdae_duoi_troi_sao.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 14_gyeongju_G14_kho_ban_khac_di_tich.png | G14 | 11/0/6 | 450×756 → 439×750 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/14_gyeongju_G14_kho_ban_khac_di_tich.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 15_gyeongju_G17_dai_long_den_bo_tay.png | G17 | 10/8/6 | 450×756 → 432×750 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/15_gyeongju_G17_dai_long_den_bo_tay.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 16_seoul_S2_cong_thanh_xua_ben_song_han.png | S2 | 0/2/6 | 450×756 → 448×750 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/16_seoul_S2_cong_thanh_xua_ben_song_han.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 17_seoul_S10_chan_thap_n_seoul.png | S10 | 1/3/6 | 450×756 → 446×750 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/17_seoul_S10_chan_thap_n_seoul.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
| 18_seoul_S18_role_anh_den_hanji.png | S18 | 0/0/6 | 450×756 → 450×750 | Assets/_Game/Art/CheckInLandmarks/20261018_RuntimeDerived/18_seoul_S18_role_anh_den_hanji.png | RUNTIME_ART_BOUND; WINDOWS_RUNTIME_PASS; NEEDS_VISUAL_REVIEW |
