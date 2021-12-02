using UnityEngine;

namespace Didimo.Core.Examples.MeetADidimo
{ 
    public class FollowPath : MonoBehaviour
    {
        public float speed;
        public Transform followPath;
        private Transform targetPoint;
        private int index;
    
        protected void Start()
        {
            index = 0;
            targetPoint = followPath.GetChild(index);
        }

        protected void Update()
        {
            transform.position = Vector3.MoveTowards(transform.position,
                targetPoint.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPoint.position) >= 0.0001f)
            {
                return;
            }

            if (index == 1)
            {
                speed = 0.03f;
            }
            else if (index == 2)
            {
                speed = 0f;
            }

            index++;
            index %= followPath.childCount;
            targetPoint = followPath.GetChild(index);
        }
    }
}
