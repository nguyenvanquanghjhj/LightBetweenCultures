LIGHT BETWEEN CULTURES — CHECK-IN LANDMARK IMAGES
Approved source: 18-card collage cropped into 18 individual PNG files.
Status: IMAGE_CONCEPT_APPROVED_BY_USER; not automatically integrated into Unity.

CONTENTS
- images/: 18 cropped PNG files
- manifest.csv: filename mapping to cityId/nodeId/title

RECOMMENDED UNITY IMPORT
1. Unzip this package.
2. Copy the folder `images` into a safe art folder, for example:
   Assets/_Game/Art/CheckInLandmarks/20261018_ApprovedSet/
3. In Unity, select all imported PNG files and set:
   - Texture Type: Sprite (2D and UI)
   - Sprite Mode: Single
   - Mesh Type: Full Rect
   - sRGB: On
   - Max Size: 1024 (or 2048 if your UI shows them large)
   - Compression: High Quality
   - Filter Mode: Bilinear
   - Wrap Mode: Clamp
4. Keep the existing gameplay/check-in logic unchanged.
5. Map each image to the corresponding check-in entry in your Photo Journal / Collection UI using `nodeId` and/or the existing checkInDefinition.

SUGGESTED FILE MAPPING EXAMPLES
- hoi_an / H6  -> 04_hoi_an_H6_cau_co_anh_den.png
- hue / U6     -> 08_hue_U6_cua_ngo_mon_cach_dieu.png
- seoul / S18  -> 18_seoul_S18_role_anh_den_hanji.png

NOTES
- These crops preserve the approved postcard/card framing from the collage.
- Images were lightly sharpened and upscaled for clearer use.
- No gameplay, quest, relic, or gate data is modified by this package.
