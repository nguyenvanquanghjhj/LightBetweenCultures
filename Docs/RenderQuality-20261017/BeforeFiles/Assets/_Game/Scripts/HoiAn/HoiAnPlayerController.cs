using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LightBetweenCultures.HoiAn
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class HoiAnPlayerController : MonoBehaviour
    {
        public const int ObstacleMask = 1 << 8;
        public static readonly Vector2 Footprint = new Vector2(.64f,.74f);
        private Rigidbody2D body;
        private Vector2 input;
        private readonly Queue<Vector2> path = new Queue<Vector2>();
        public bool Walking => path.Count > 0;
        public Vector2 Position => body.position;
        private void Awake() { body=GetComponent<Rigidbody2D>(); }
        private void Update()
        {
            var k=Keyboard.current; input=Vector2.zero;
            if(k!=null) input=new Vector2((k.dKey.isPressed||k.rightArrowKey.isPressed?1:0)-(k.aKey.isPressed||k.leftArrowKey.isPressed?1:0), (k.wKey.isPressed||k.upArrowKey.isPressed?1:0)-(k.sKey.isPressed||k.downArrowKey.isPressed?1:0));
            input=Vector2.ClampMagnitude(input,1); if(input!=Vector2.zero) path.Clear();
        }
        public void Stop() { path.Clear(); input=Vector2.zero; body.linearVelocity=Vector2.zero; }
        public bool MoveTo(Vector2 destination)
        {
            var route=FindPath(body.position,destination);
            path.Clear(); if(route==null) return false;
            foreach(var point in route) path.Enqueue(point); return true;
        }
        private void FixedUpdate()
        {
            var delta=input*4f*Time.fixedDeltaTime;
            if(input==Vector2.zero && path.Count>0)
            {
                delta=Vector2.ClampMagnitude(path.Peek()-body.position,4f*Time.fixedDeltaTime);
                if(delta.sqrMagnitude<.0004f) { path.Dequeue(); return; }
            }
            if(delta==Vector2.zero) return;
            var hit=Physics2D.BoxCast(body.position,Footprint,0,delta.normalized,delta.magnitude+.015f,ObstacleMask);
            if(hit.collider!=null) delta=delta.normalized*Mathf.Max(0,hit.distance-.015f);
            body.MovePosition(body.position+delta);
        }
        public static bool Walkable(Vector2 p) => p.x>=-7.5f&&p.x<=7.5f&&p.y>=-4.5f&&p.y<=4.5f && Physics2D.OverlapBox(p,Footprint,0,ObstacleMask)==null;
        private static Vector2 PositionOf(Vector2Int p) => new Vector2(p.x*.5f,p.y*.5f);
        private static bool ClearSegment(Vector2 a,Vector2 b)
        {
            var delta=b-a; return Physics2D.BoxCast(a,Footprint,0,delta.normalized,delta.magnitude,ObstacleMask).collider==null;
        }
        public static List<Vector2> FindPath(Vector2 start,Vector2 destination)
        {
            if(!Walkable(destination)) return null;
            var first=new Vector2Int(Mathf.RoundToInt(start.x*2),Mathf.RoundToInt(start.y*2));
            var last=new Vector2Int(Mathf.RoundToInt(destination.x*2),Mathf.RoundToInt(destination.y*2));
            if(!Walkable(PositionOf(first))||!ClearSegment(start,PositionOf(first)))
            {
                bool found=false;
                for(int x=-1;x<=1&&!found;x++) for(int y=-1;y<=1&&!found;y++)
                { var candidate=new Vector2Int(Mathf.RoundToInt(start.x*2)+x,Mathf.RoundToInt(start.y*2)+y); if(Walkable(PositionOf(candidate))&&ClearSegment(start,PositionOf(candidate))) { first=candidate; found=true; } }
                if(!found) return null;
            }
            if(!Walkable(PositionOf(last))||!ClearSegment(PositionOf(last),destination)) return null;
            var previous=new Dictionary<Vector2Int,Vector2Int>{{first,first}}; var queue=new Queue<Vector2Int>(); queue.Enqueue(first);
            var dirs=new[]{Vector2Int.up,Vector2Int.right,Vector2Int.down,Vector2Int.left};
            while(queue.Count>0)
            {
                var current=queue.Dequeue(); if(current==last) break;
                foreach(var d in dirs)
                {
                    var next=current+d; if(previous.ContainsKey(next)||!Walkable(PositionOf(next))||!ClearSegment(PositionOf(current),PositionOf(next))) continue;
                    previous[next]=current; queue.Enqueue(next);
                }
            }
            if(!previous.ContainsKey(last)) return null;
            var result=new List<Vector2>{destination};
            for(var p=last;;p=previous[p]) { result.Add(PositionOf(p)); if(p==first) break; }
            result.Reverse(); return result;
        }
    }
}
