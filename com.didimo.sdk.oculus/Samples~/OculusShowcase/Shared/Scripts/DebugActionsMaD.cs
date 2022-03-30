using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugActionsMaD : MonoBehaviour
{
    public GameObject didimo1;
    public GameObject didimo2;
    public GameObject volume;

    public GameObject lightsHands;
    public GameObject lightsDidimos;
    public GameObject lightsChaperone;

    Didimo.Speech.DidimoSpeech didimoSpeech;
    Didimo.DidimoAnimator didimoAnimator;

    public void ToggleDidimo1()
    {
        if (didimo1.activeInHierarchy)
        {
            didimo1.SetActive(false);
        }
        else
        {
            didimo1.SetActive(true);
        }

    }

    public void ToggleDidimo2()
    {
        if (didimo2.activeInHierarchy)
        {
            didimo2.SetActive(false);
        }
        else
        {
            didimo2.SetActive(true);
        }

    }

    public void ToggleVolumes()
    {
        if (volume.activeInHierarchy)
        {
            volume.SetActive(false);
        }
        else
        {
            volume.SetActive(true);
        }
    }

    public void ToggleLights()
    {
        if (lightsHands.activeInHierarchy)
        {
            lightsHands.SetActive(false);
        }
        else
        {
            lightsHands.SetActive(true);
        }

        if (lightsDidimos.activeInHierarchy)
        {
            lightsDidimos.SetActive(false);
        }
        else
        {
            lightsDidimos.SetActive(true);
        }

        if (lightsChaperone.activeInHierarchy)
        {
            lightsChaperone.SetActive(false);
        }
        else
        {
            lightsChaperone.SetActive(true);
        }
    }

    public void ToggleAnimation1()
    {
        didimoSpeech.StopAnimation();
        didimoAnimator.StopAllAnimations();
    }
}
