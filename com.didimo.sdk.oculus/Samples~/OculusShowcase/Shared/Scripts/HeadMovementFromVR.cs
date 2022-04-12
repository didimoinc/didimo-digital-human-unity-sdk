using UnityEngine;
using Didimo.Core.Utility;
/// <summary>
/// Class that provides some head movement to the didimo.
/// </summary>
public class HeadMovementFromVR : DidimoBehaviour
{
    [SerializeField]
    private Transform headTransform;

    private void Update()
    {
        if (headTransform != null)
        {
            UpdateHeadTransform();
        }
    }

    private void UpdateHeadTransform()
    {
        Vector3 eulerRotation = headTransform.rotation.eulerAngles;

        Mathf.Clamp(eulerRotation.x, -50, 50);
        Mathf.Clamp(eulerRotation.y = -eulerRotation.y, -80, 80);
        Mathf.Clamp(eulerRotation.z = -eulerRotation.z, -80, 80);

        DidimoComponents.PoseController.SetHeadRotation(Quaternion.Euler(eulerRotation));
    }
}
