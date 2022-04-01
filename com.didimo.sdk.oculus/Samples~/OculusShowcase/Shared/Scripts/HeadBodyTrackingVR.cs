using UnityEngine;
/// <summary>
/// Class that controls the head rotation and repositions the body in the Meeting Room Example scene.
/// </summary>
public class HeadBodyTrackingVR : MonoBehaviour
{
    [SerializeField]
    private Transform headTrackingTransform;

    [SerializeField]
    private Transform headBone;

    [SerializeField]
    private Transform target;

    public float down;
    public float back;

    private void Update()
    {
        if (headTrackingTransform != null)
        {
            RepositionBody();
        }
    }

    private void LateUpdate()
    {
        if (headTrackingTransform != null)
        {
            UpdateHeadTransform();
        }
    }

    private void UpdateHeadTransform()
    {
        Vector3 eulerRotation = headTrackingTransform.rotation.eulerAngles;

        Mathf.Clamp(eulerRotation.x, -50, 50);
        Mathf.Clamp(eulerRotation.y, -80, 80);
        Mathf.Clamp(eulerRotation.z, -80, 80);

        headBone.rotation = Quaternion.Euler(eulerRotation);
    }

    private void RepositionBody()
    {
        target.position = headTrackingTransform.position + (Vector3.down * down) + (target.transform.forward * (-back));
    }
}