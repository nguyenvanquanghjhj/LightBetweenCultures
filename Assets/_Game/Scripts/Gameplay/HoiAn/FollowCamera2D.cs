using UnityEngine;
namespace LightBetweenCultures.HoiAn
{
    [RequireComponent(typeof(Camera))]
    public sealed class FollowCamera2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector2 framingOffset;
        private Camera view;
        public void Configure(Transform player) { target=player; }
        private void Awake() { view=GetComponent<Camera>(); }
        private void LateUpdate() => Snap();
        public void Snap()
        {
            if(target==null) return;
            var height=Mathf.Min(3.6f,10f/Mathf.Max(.1f,view.aspect));
            view.orthographicSize=height;
            var halfWidth=height*view.aspect;
            transform.position=new Vector3(Mathf.Clamp(target.position.x+framingOffset.x,-10+halfWidth,10-halfWidth),Mathf.Clamp(target.position.y+framingOffset.y,-7+height,7-height),-10);
        }
    }
}
