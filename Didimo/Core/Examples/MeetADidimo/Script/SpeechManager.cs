using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeechManager : MonoBehaviour
{   
    [SerializeField]
    private GameObject man;
    [SerializeField]
    private GameObject woman;

    // Start is called before the first frame update
    void Start()
    {
        Didimo.MeetADidimo script1 = woman.GetComponent<Didimo.MeetADidimo>();
        Didimo.MeetADidimo script2 = man.GetComponent<Didimo.MeetADidimo>();

        script1.SpeakDidimoFirst();
        script2.SpeakDidimoSecond();
    }
}
