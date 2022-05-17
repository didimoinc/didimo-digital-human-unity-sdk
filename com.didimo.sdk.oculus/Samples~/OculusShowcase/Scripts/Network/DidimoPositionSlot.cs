using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DidimoPositionSlot : MonoBehaviour
{
    public List<GameObject> activateObjects;
    public bool occupied;

    public void Occupy()
    {
        activateObjects.ForEach(o => o.SetActive(false));
        occupied = true;
    }

    public void Vacate()
    {
        activateObjects.ForEach(o => o.SetActive(true));
        occupied = false;
    }
}
