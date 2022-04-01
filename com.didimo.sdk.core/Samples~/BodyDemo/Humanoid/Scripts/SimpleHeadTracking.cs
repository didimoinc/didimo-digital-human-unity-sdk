using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SimpleHeadTracking : MonoBehaviour
{
    [Header("Head Tracking")]
    [Range(0.1f, 15)] public float headTrackingDistance = 8;
    [Range(0.1f, 180)] public float headTrackingAngle = 90;
    [Range(0.1f, 2.5f)] public float eyeHeight = 1.7f;

    Vector3 eyePosition => transform.position + transform.up * eyeHeight;
    Vector3 resetPosition => eyePosition + transform.forward * 3;
    Animator animator => GetComponentInChildren<Animator>();

    Vector3 direction;

    IEnumerator Start()
    {
        POILook current = null;
        HashSet<POI> pois = new HashSet<POI>() { new POI(null, 1) };

        while (enabled)
        {
            var target = FindObjectsOfType<POILook>().
                Select(poi => new { distance = Vector3.Distance(poi.position, eyePosition), position = poi.position, poi = poi }).
                Where(poi => poi.distance > 1 && poi.distance < headTrackingDistance).
                Where(poi => Quaternion.Angle(Quaternion.LookRotation(poi.position - eyePosition), transform.rotation) < headTrackingAngle).
                OrderBy(poi => poi.distance).FirstOrDefault()?.poi ?? null;

            current = target;
            if (!pois.Any(p => p.poi == target))
                pois.Add(new POI(target));

            direction = Vector3.zero;
            foreach (var p in pois.ToArray())
            {
                direction += ((p.poi == null ? resetPosition : p.poi.position) - eyePosition) * p.weight;
                if (current == p.poi) p.weight = Mathf.Min(1, p.weight + Time.deltaTime);
                else if ((p.weight -= Time.deltaTime) < 0) pois.Remove(p);
            }
            yield return null;
        }
    }


    void OnAnimatorIK()
    {
        animator.SetLookAtPosition(eyePosition + direction);
        animator.SetLookAtWeight(1, 0.2f, 0.75f, 1f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(eyePosition, 0.01f);
    }

    class POI
    {
        public POILook poi;
        public float weight;
        public POI(POILook poi, float weight)
        {
            this.poi = poi;
            this.weight = weight;
        }
        public POI(POILook poi) : this(poi, 0) { }
    }
}
