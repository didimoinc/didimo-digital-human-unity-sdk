using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class that controls the arms' movement by using the Animator's Inverse Kinematic solutions.
/// It needs a reference to the headset and each controller's transform and then calls the SetIK animator functions.
/// It first sets the weight of an IK goal (0 = at the original animation before IK, 1 = at the goal) and after, the actual position and rotation of the arms
/// infered from the references to the controllers position. compared to the headset. 
/// </summary>
[RequireComponent(typeof(Animator))]
public class VRArmsIK : MonoBehaviour
{
    private Animator animator;
    private float IKWeight = 1.0f;

    [SerializeField] private Transform IKTarget_Head;
    [SerializeField] private Transform IKTarget_LH;
    [SerializeField] private Transform IKTarget_RH;

    void OnEnable() { }
    void OnDisable() { }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        animator.SetLookAtWeight(IKWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IKWeight);
        animator.SetIKPositionWeight(AvatarIKGoal.RightHand, IKWeight);

        animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IKWeight);
        animator.SetIKRotationWeight(AvatarIKGoal.RightHand, IKWeight);


        animator.SetLookAtPosition(IKTarget_Head.position);
        animator.SetIKPosition(AvatarIKGoal.LeftHand, IKTarget_LH.position);
        animator.SetIKPosition(AvatarIKGoal.RightHand, IKTarget_RH.position);

        animator.SetIKRotation(AvatarIKGoal.LeftHand, IKTarget_LH.rotation);
        animator.SetIKRotation(AvatarIKGoal.RightHand, IKTarget_RH.rotation);
    }
}