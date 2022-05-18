using UnityEngine;
/// <summary>
/// Class that controls the sequence of events of the Meet a didimo scene
/// </summary>
public class ManageSpeakVRSC : MonoBehaviour
{
    [SerializeField]
    private GameObject man;
    [SerializeField]
    private GameObject woman;

    void Start()
    {
        Didimo.MeetADidimoVRSC script1 = woman.GetComponent<Didimo.MeetADidimoVRSC>();
        Didimo.MeetADidimoVRSC script2 = man.GetComponent<Didimo.MeetADidimoVRSC>();

        script1.SpeakDidimoFirst();
        script2.SpeakDidimoSecond();
    }
}
