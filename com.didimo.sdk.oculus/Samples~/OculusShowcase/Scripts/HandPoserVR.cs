using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class that takes inputs from the Oculus controllers and triggers the Animator to play the Hand animations for each hand.
/// </summary>
public class HandPoserVR : MonoBehaviour
{
    public Animator animator;

    [HideInInspector]
    public bool isPressingA = false;
    [HideInInspector]
    public bool isPressingX = false;
    [HideInInspector]
    public bool isPressingY = false;
    [HideInInspector]
    public bool isPressingB = false;
    [HideInInspector]
    public bool isPressingRITrigger = false;
    [HideInInspector]
    public bool isPressingLITrigger = false;
    [HideInInspector]
    public bool isPressingLHTrigger = false;
    [HideInInspector]
    public bool isPressingRHTrigger = false;

    public void Update()
    {
        PressOVRButtons();
    }

    private void PressOVRButtons()
    {
#if UNITY_ANDROID
        //Press A
        PressButton(OVRInput.RawButton.A);
        SetPose("SetRightAButton", isPressingA);

        //Press X
        PressButton(OVRInput.RawButton.X);
        SetPose("SetLeftXButton", isPressingX);

        //Press B
        PressButton(OVRInput.RawButton.B);
        SetPose("SetRightBButton", isPressingB);

        //Press Y
        PressButton(OVRInput.RawButton.Y);
        SetPose("SetLeftYButton", isPressingY);

        //Press Right Hand Trigger
        PressButton(OVRInput.RawButton.RHandTrigger);
        SetPose("SetRightHandTrigger", isPressingRHTrigger);

        //Press Left Hand Trigger
        PressButton(OVRInput.RawButton.LHandTrigger);
        SetPose("SetLeftHandTrigger", isPressingLHTrigger);

        //Press Right Index Trigger
        PressButton(OVRInput.RawButton.RIndexTrigger);
        SetPose("SetRightIndexTrigger", isPressingRITrigger);

        //Press Left Index Trigger
        PressButton(OVRInput.RawButton.LIndexTrigger);
        SetPose("SetLeftIndexTrigger", isPressingLITrigger);
    }

    private void TriggerBools(bool isPressing, OVRInput.RawButton pressedButton)
    {
        if (pressedButton == OVRInput.RawButton.A)
        {
            isPressingA = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.X)
        {
            isPressingX = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.B)
        {
            isPressingB = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.Y)
        {
            isPressingY = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.RIndexTrigger)
        {
            isPressingRITrigger = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.LIndexTrigger)
        {
            isPressingLITrigger = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.LHandTrigger)
        {
            isPressingLHTrigger = isPressing;
        }
        else if (pressedButton == OVRInput.RawButton.RHandTrigger)
        {
            isPressingRHTrigger = isPressing;
        }
    }

    private void PressButton(OVRInput.RawButton pressedButton)
    {
        bool isPressing = OVRInput.Get(pressedButton);
        TriggerBools(isPressing, pressedButton);
#endif
    }

    private void SetPose(string trigger, bool isPressing)
    {
        if (isPressing)
        {
            animator.SetBool(trigger, true);
        }
        else
        {
            animator.SetBool(trigger, false);
        }
    }
}
