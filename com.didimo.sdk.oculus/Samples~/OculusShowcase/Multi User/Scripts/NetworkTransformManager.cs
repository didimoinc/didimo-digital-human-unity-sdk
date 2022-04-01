using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Samples;
using UnityEngine;

public class NetworkTransformManager : MonoBehaviour
{
    [Button]
    void AddNetworkTransformsToHierarchy(Transform t = null)
    {
        if (t == null) t = transform;
        DidimoNetworkTransform networkTransform = t.gameObject.GetComponent<DidimoClientNetworkTransform>();
        if (networkTransform == null)
        {
            networkTransform = t.gameObject.AddComponent<DidimoClientNetworkTransform>();
        }

        networkTransform.SyncScaleX = false;
        networkTransform.SyncScaleY = false;
        networkTransform.SyncScaleZ = false;
        networkTransform.InLocalSpace = true;
        networkTransform.Interpolate = true;

        for (int i = 0; i < t.childCount; i++)
        {
            AddNetworkTransformsToHierarchy(t.GetChild(i));
        }
    }
}