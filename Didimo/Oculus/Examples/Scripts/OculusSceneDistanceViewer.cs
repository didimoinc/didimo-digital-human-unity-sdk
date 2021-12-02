using UnityEngine;

namespace Didimo.Oculus.Example
{
    /// <summary>
    /// Class that moves the didimo closer of further to the camera.
    /// You can view this script in action on the OculusTestApplication scene.
    /// </summary>
    public class OculusSceneDistanceViewer : MonoBehaviour
    {
#if USING_OCULUS_INTEGRATION_PACKAGE

        private const int     DEFAULT_DISPLACEMENT_INDEX = 3; // no displacement
        private       Vector3 initialPosition;

        private static readonly float[] zPositionDisplacements = {0.3f, 0.2f, 0.1f, 0f, -0.1f, -0.2f, -0.3f, -0.4f, -0.5f, -1f, -2f, -3f, -5f};
        private                 int     displacementIndex      = DEFAULT_DISPLACEMENT_INDEX;

        private void OnEnable()
        {
            ResetDisplacement();
        }

        private void Update()
        {
            bool yPress;
            if ((yPress = OVRInput.GetDown(OVRInput.RawButton.Y)) || OVRInput.GetDown(OVRInput.RawButton.X))
            {
                int newDisplacementIndex = yPress ? GetNextDisplacementIndex() : GetPreviousDisplacementIndex();
                if (newDisplacementIndex == displacementIndex) return;
                displacementIndex = newDisplacementIndex;
                transform.position = initialPosition - new Vector3(0f, 0f, zPositionDisplacements[displacementIndex]);
            }

            if (OVRInput.GetDown(OVRInput.RawButton.RHandTrigger) ||
                OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger) ||
                OVRInput.GetDown(OVRInput.RawButton.LHandTrigger) ||
                OVRInput.GetDown(OVRInput.RawButton.LIndexTrigger))
            {
                ResetDisplacement();
            }
        }

        private void OnDisable() { ResetDisplacement(); }

        private void ResetDisplacement()
        {
            transform.position = initialPosition;
            displacementIndex = DEFAULT_DISPLACEMENT_INDEX;
        }

        private int GetNextDisplacementIndex() => Mathf.Clamp(displacementIndex + 1, 0, zPositionDisplacements.Length - 1);

        private int GetPreviousDisplacementIndex() => Mathf.Clamp(displacementIndex - 1, 0, zPositionDisplacements.Length - 1);
        
#endif
    }
}
