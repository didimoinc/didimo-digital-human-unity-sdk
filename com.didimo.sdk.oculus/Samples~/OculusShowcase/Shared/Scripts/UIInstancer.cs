using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInstancer : MonoBehaviour
{
    [SerializeField]
    private GameObject desc;
    [SerializeField]
    private GameObject menuMan;

    private List<GameObject> objs;

    void OnEnable()
    {
        if (objs != null) return;
        objs = new List<GameObject>
        {
            Instantiate(desc, transform.position + transform.forward + Vector3.up * (-1.75f), transform.rotation),
            Instantiate(menuMan)
        };
    }

    void OnDisable()
    {
        if (objs == null) return;
        objs.ForEach(o => DestroyImmediate(o));
        objs = null;
    }
}
