using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Class to instantiate the UI and the Menu Manager in Multi User scene.
/// </summary>
public class UIInstancer : MonoBehaviour
{
    [SerializeField]
    private GameObject desc;
    [SerializeField]
    private GameObject menuMan;

    private List<GameObject> objs;

    private void OnEnable()
    {
        if (objs != null) return;
        objs = new List<GameObject>
        {
            Instantiate(desc, transform.position + transform.forward + Vector3.up * (-1.75f), transform.rotation),
            Instantiate(menuMan)
        };
    }

    private void OnDisable()
    {
        if (objs == null) return;
        objs.ForEach(o => DestroyImmediate(o));
        objs = null;
    }
}
