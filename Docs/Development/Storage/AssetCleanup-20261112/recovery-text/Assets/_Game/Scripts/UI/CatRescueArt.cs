using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LightBetweenCultures
{
    public static class CatRescueArt
    {
        static readonly Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
        public static Sprite Cat(int index)
        {
            var key = "cat:" + index;
            if (sprites.TryGetValue(key, out var cached)) return cached;
            var texture = Resources.Load<Texture2D>("CatRescue/RescueCats");
            if (texture == null || index < 0 || index > 5) return null;
            var w = texture.width / 3f; var h = texture.height / 2f;
            return sprites[key] = Sprite.Create(texture, new Rect(index % 3 * w, (1 - index / 3) * h, w, h), new Vector2(.5f,.5f), 100);
        }
        public static Sprite Image(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (sprites.TryGetValue(id, out var cached)) return cached;
            var art = AdventureUIArt.Load();
            if (id == "backpack") return art.backpack;
            if (id == "station") return Resources.Load<Sprite>("CatRescue/DaLatStation");
            if (id.StartsWith("city:")) return art.City(id.Substring(5))?.sprite;
            if (id.StartsWith("item:")) return art.Item(id.Substring(5))?.sprite;
            if (id.StartsWith("relic:")) return art.Relic(id.Substring(6));
            if (id.StartsWith("resource:")) return Resources.Load<Sprite>(id.Substring(9));
            if (id.StartsWith("checkin:"))
            {
                var entry = art.CheckIn(id.Substring(8));
                if (entry?.sprite == null) return null;
                var t = entry.sprite.texture; var uv = entry.viewUV;
                return sprites[id] = Sprite.Create(t, new Rect(uv.x*t.width,uv.y*t.height,uv.width*t.width,uv.height*t.height), new Vector2(.5f,.5f), 100);
            }
            return null;
        }
        public static Sprite Piece(string id, int index)
        {
            var key = id + "/piece/" + index;
            if (sprites.TryGetValue(key, out var cached)) return cached;
            var source = Image(id); if (source == null) return null;
            var r = source.rect; var w = r.width / 2; var h = r.height / 2;
            return sprites[key] = Sprite.Create(source.texture, new Rect(r.x + index%2*w, r.y+(1-index/2)*h,w,h),new Vector2(.5f,.5f),100);
        }
    }

    // UI animation only, using the shared reduced-motion preference.
    public sealed class CatRescueMotion : MonoBehaviour
    {
        public bool rescued;
        Vector3 original;
        void OnEnable() { original = transform.localScale; }
        void Update()
        {
            var strength = NodeMarkerView.MotionMode == MarkerMotionMode.Off ? 0 : NodeMarkerView.MotionMode == MarkerMotionMode.Reduced ? .25f : 1;
            var t = Time.unscaledTime;
            transform.localScale = Vector3.Scale(original, new Vector3(1+Mathf.Sin(t*2)*.012f*strength,1+Mathf.Sin(t*2+.6f)*.018f*strength,1));
            transform.localRotation = Quaternion.Euler(0,0,Mathf.Sin(t*(rescued?2.8f:1.5f))*(rescued?2.5f:.8f)*strength);
        }
        void OnDisable() { transform.localScale = original; transform.localRotation = Quaternion.identity; }
    }
}
