using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MacRunInEditor : MonoBehaviour
{
    public GameObject oVRRig;

    private void Awake()
    {
#if UNITY_EDITOR
        oVRRig.SetActive(false);
#else
        gameObject.SetActive(false);
#endif

    }
}
