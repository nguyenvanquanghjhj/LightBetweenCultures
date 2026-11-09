using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    /// <summary>
    /// Visual atmosphere only. No quest, save, pathing, flood collision or reward logic.
    /// Single masked UI mesh, no textures, allocations of particles, coroutines or physics.
    /// The CityMap places this BELOW path markers, actors and screen HUD.
    /// Slide 19: Hoi An = flooded streets; Hue = heavy rain/flood;
    /// Da Lat = fog; Seoul = sudden storm; Jeonju/Gyeongju = no rain.
    /// </summary>
    public sealed class CatWeatherGraphic : MaskableGraphic
    {
        string requested = "clear";
        string displayed = "clear";
        bool isCelebrating;
        float visibility;
        float nextRefresh;

        // Kept for the existing CatRescue celebration panel.
        public bool celebration
        {
            get => isCelebrating;
            set
            {
                if (isCelebrating == value) return;
                isCelebrating = value;
                SetVerticesDirty();
            }
        }

        // Kept compatible with the existing CityMap/CatRescue field contract.
        public string weather
        {
            get => requested;
            set
            {
                var next = string.IsNullOrEmpty(value) ? "clear" : value;
                if (requested == next) return;
                requested = next;
                if (next != "clear")
                {
                    displayed = next;
                    if (visibility < .02f) visibility = 0f; // fade in on entry
                }
                SetVerticesDirty();
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            raycastTarget = false;
            nextRefresh = 0f;
        }

        void Update()
        {
            if (isCelebrating) return;
            var target = requested == "clear" ? 0f : 1f;
            visibility = Mathf.MoveTowards(visibility, target, Time.unscaledDeltaTime * .85f);
            if (visibility <= .001f && target <= 0f)
            {
                displayed = "clear";
                return; // no idle mesh rebuild for clear Jeonju/Gyeongju
            }
            // Android UI mesh updates limited to ~15 fps, desktop ~20 fps.
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + (Application.isMobilePlatform ? .066f : .05f);
            SetVerticesDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var r = rectTransform.rect;
            if (r.width < 2f || r.height < 2f) return;
            if (isCelebrating)
            {
                // Legacy visual feedback, deliberately does not become rain.
                Band(vh, r, 0f, .08f, new Color(1f, .77f, .32f, .07f));
                Band(vh, r, .92f, 1f, new Color(1f, .77f, .32f, .07f));
                return;
            }
            if (visibility <= .001f || displayed == "clear") return;
            float time = Time.unscaledTime;
            float fade = visibility;
            switch (displayed)
            {
                case "flood":
                    // Hoi An: shallow flooded old town; storm itself has passed.
                    Wash(vh, r, new Color(.13f, .30f, .40f, .028f * fade));
                    FloodWater(vh, r, time, .70f * fade);
                    Rain(vh, r, time, 15, .18f, .72f * fade);
                    break;
                case "downpour":
                    // Hue: persistent rain and rising water, with calmer intervals.
                    Wash(vh, r, new Color(.12f, .24f, .37f, .07f * fade));
                    FloodWater(vh, r, time, 1f * fade);
                    float pulse = .42f + .58f * (.5f + .5f * Mathf.Sin(time * .22f));
                    Rain(vh, r, time, 62, .34f, pulse * fade);
                    break;
                case "mist":
                    // Da Lat: layered low cloud, never distracting rain strips.
                    Fog(vh, r, time, fade);
                    break;
                case "storm":
                    // Seoul: fast slanted rain, gust arcs and VERY soft lightning.
                    Wash(vh, r, new Color(.08f, .16f, .28f, .10f * fade));
                    Rain(vh, r, time, 76, .88f, fade);
                    WindArcs(vh, r, time, fade);
                    Lightning(vh, r, time, fade);
                    break;
                // Legacy content values: neither is interpreted as a storm.
                // The slide shows festival preparation / ruins, NOT rainfall.
                case "petals":
                case "stars":
                case "wind":
                case "clear":
                default:
                    break;
            }
        }

        static float Noise(int seed)
        {
            // Stable deterministic particle offsets; no Random state changes.
            var n = Mathf.Sin(seed * 12.9898f + 78.233f) * 43758.5453f;
            return n - Mathf.Floor(n);
        }

        static Color Alpha(Color c, float factor) => new Color(c.r, c.g, c.b, Mathf.Clamp01(c.a * factor));

        static void Wash(VertexHelper vh, Rect r, Color c)
        {
            Quad(vh, new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMax), c, c);
        }

        static void Band(VertexHelper vh, Rect r, float low, float high, Color tint)
        {
            Quad(vh,
                new Vector2(r.xMin, r.yMin + r.height * low),
                new Vector2(r.xMax, r.yMin + r.height * high), tint, tint);
        }

        static void Quad(VertexHelper vh, Vector2 min, Vector2 max, Color lower, Color upper)
        {
            int i = vh.currentVertCount;
            vh.AddVert(new Vector3(min.x, min.y, 0), lower, Vector2.zero);
            vh.AddVert(new Vector3(max.x, min.y, 0), lower, Vector2.zero);
            vh.AddVert(new Vector3(max.x, max.y, 0), upper, Vector2.zero);
            vh.AddVert(new Vector3(min.x, max.y, 0), upper, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        static void SoftStroke(VertexHelper vh, Vector2 from, Vector2 to, float width, Color color)
        {
            var d = to - from;
            if (d.sqrMagnitude < .0001f) return;
            var side = new Vector2(-d.y, d.x).normalized * width * .5f;
            int i = vh.currentVertCount;
            var transparent = Alpha(color, .05f);
            // Transparent tail, delicate bright tip: not solid falling needles.
            vh.AddVert(from - side, transparent, Vector2.zero);
            vh.AddVert(from + side, transparent, Vector2.zero);
            vh.AddVert(to + side, color, Vector2.zero);
            vh.AddVert(to - side, color, Vector2.zero);
            vh.AddTriangle(i, i + 1, i + 2);
            vh.AddTriangle(i, i + 2, i + 3);
        }

        static void Rain(VertexHelper vh, Rect r, float t, int count, float wind, float strength)
        {
            if (strength < .04f) return;
            float w = r.width, h = r.height;
            for (int i = 0; i < count; i++)
            {
                int band = i % 3;
                float depth = band == 0 ? .70f : band == 1 ? .88f : 1f;
                float speed = h * (.08f + depth * .19f);
                float x = Mathf.Repeat(Noise(i * 5 + 2) + t * speed / w * wind * .28f, 1f);
                float y = Mathf.Repeat(Noise(i * 7 + 11) - t * speed / h, 1f);
                float len = h * (.009f + depth * .009f);
                float thickness = Mathf.Max(.75f, w * (.0007f + depth * .00045f));
                var p = new Vector2(r.xMin + x * w, r.yMin + y * h);
                var tail = p + new Vector2(-wind * len * 1.35f, len);
                var tint = new Color(.81f, .91f, 1f, (.04f + depth * .065f) * strength);
                SoftStroke(vh, tail, p, thickness, tint);
            }
        }

        static void FloodWater(VertexHelper vh, Rect r, float t, float visibility)
        {
            // Decorative flood-water hint only. No fictitious player collision.
            if (visibility < .02f) return;
            var water = new Color(.23f, .60f, .70f, .058f * visibility);
            Quad(vh, new Vector2(r.xMin, r.yMin),
                new Vector2(r.xMax, r.yMin + .16f * r.height), water,
                Alpha(water, .03f));
            for (int k = 0; k < 6; k++)
            {
                float cx = r.xMin + (.12f + .15f * k) * r.width;
                float cy = r.yMin + (.028f + .021f * (k % 3)) * r.height;
                float rx = r.width * (.018f + .012f * Noise(k + 2));
                float ry = Mathf.Max(1.4f, r.height * .0022f);
                float pulse = .55f + .45f * Mathf.Sin(t * 1.2f + k * 1.8f);
                for (int a = 0; a < 6; a++)
                {
                    float ang1 = Mathf.PI * (.17f + a * .11f);
                    float ang2 = Mathf.PI * (.17f + (a + 1) * .11f);
                    var p1 = new Vector2(cx + rx * Mathf.Cos(ang1), cy + ry * Mathf.Sin(ang1));
                    var p2 = new Vector2(cx + rx * Mathf.Cos(ang2), cy + ry * Mathf.Sin(ang2));
                    SoftStroke(vh, p1, p2, Mathf.Max(1f, r.width * .0009f),
                        new Color(.80f, .96f, 1f, .11f * pulse * visibility));
                }
            }
        }

        static void Fog(VertexHelper vh, Rect r, float t, float fade)
        {
            for (int k = 0; k < 4; k++)
            {
                float shift = Mathf.Sin(t * (.075f + .02f * k) + k * 2.1f) * .04f;
                float baseY = .11f + k * .19f;
                var bottom = new Color(.78f, .88f, .90f, 0f);
                var top = new Color(.78f, .88f, .90f, (.035f + .017f * k) * fade);
                var a = new Vector2(r.xMin + (shift - .06f) * r.width, r.yMin + baseY * r.height);
                var b = new Vector2(r.xMax + shift * r.width, r.yMin + (baseY + .20f) * r.height);
                Quad(vh, a, b, bottom, top);
            }
        }

        static void WindArcs(VertexHelper vh, Rect r, float t, float fade)
        {
            float wobble = Mathf.Sin(t * .66f) * .015f * r.width;
            var center = new Vector2(r.xMin + r.width * .83f + wobble,
                                     r.yMin + r.height * .69f);
            for (int ring = 0; ring < 3; ring++)
            {
                float rx = r.width * (.065f + ring * .042f);
                float ry = r.height * (.026f + ring * .013f);
                for (int j = 0; j < 9; j++)
                {
                    var a = j * .17f + t * .16f;
                    var b = a + .17f;
                    var p = center + new Vector2(Mathf.Cos(a) * rx, Mathf.Sin(a) * ry);
                    var q = center + new Vector2(Mathf.Cos(b) * rx, Mathf.Sin(b) * ry);
                    SoftStroke(vh, p, q, Mathf.Max(1.3f, r.width * .001f),
                        new Color(.80f, .92f, 1f, .065f * fade));
                }
            }
        }

        static void Lightning(VertexHelper vh, Rect r, float t, float fade)
        {
            // A faint bolt for 0.18 s every 14 s; NO full-screen white strobe.
            float phase = Mathf.Repeat(t + 2f, 14f);
            if (phase > .18f) return;
            float x = r.xMin + r.width * .78f;
            float y = r.yMin + r.height * .91f;
            var a = new Vector2(x, y);
            var b = a + new Vector2(-.012f * r.width, -.035f * r.height);
            var c = b + new Vector2(.019f * r.width, -.023f * r.height);
            var d = c + new Vector2(-.011f * r.width, -.042f * r.height);
            var tint = new Color(.82f, .91f, 1f, .14f * fade);
            SoftStroke(vh, a, b, Mathf.Max(1.4f, r.width * .0012f), tint);
            SoftStroke(vh, b, c, Mathf.Max(1.4f, r.width * .0012f), tint);
            SoftStroke(vh, c, d, Mathf.Max(1.2f, r.width * .001f), tint);
        }
    }
}
