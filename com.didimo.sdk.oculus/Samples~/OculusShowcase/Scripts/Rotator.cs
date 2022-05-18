using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Range (0, 720)] public float speed = 180;

    // Update is called once per frame
    void Update()
    {
        if (!Input.anyKey)
            transform.eulerAngles = transform.eulerAngles + Vector3.up * speed * Time.deltaTime;
    }
}
