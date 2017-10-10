using UnityEngine;
using Utilities;

namespace Frontend.Forest
{
    public class Root : MonoBehaviour
    {
        private Transform start;
        private Transform end;
        public float LineWidth = 0.02f;

        private LineRenderer line;
        private CapsuleCollider capsule;
        
        private void Render()
        {
            var length = (end.localPosition - start.localPosition).magnitude;
            var yScale =  gameObject.SizeToScale(Axis.Y, length);
            var xzScale = gameObject.SizeToLocalScale(Axis.X, LineWidth) / 2;
            
            transform.localScale = new Vector3(xzScale, yScale, xzScale);
            transform.LookAt(end);
            transform.localEulerAngles = transform.localEulerAngles + Vector3.right * 90;
        }

        private void OnEnable()
        {
            // Root -> Roots -> Forest -> Trees -> Tree[i]
            start = transform.parent.parent.Find("Trees").GetChild(0);
            end = transform.parent.parent.Find("Trees").GetChild(1);
            Render();
        }
    }
}