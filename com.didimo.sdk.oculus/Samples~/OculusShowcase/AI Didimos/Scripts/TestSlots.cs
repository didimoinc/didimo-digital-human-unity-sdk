using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo.Core.Inspector;
using UnityEngine;

public class TestSlots : MonoBehaviour
{
    private void Toggle(int index)
    {
        var slot = GetComponentsInChildren<DidimoPositionSlot>(true).ToArray()[index];
        if (slot.occupied) slot.Vacate(); else slot.Occupy();
        // gameObject.SetActive (!gameObject.activeInHierarchy);
    }

    [Button] void toggle0() => Toggle(0);
    [Button] void toggle1() => Toggle(1);
    [Button] void toggle2() => Toggle(2);
    [Button] void toggle3() => Toggle(3);
    [Button] void toggle4() => Toggle(4);
    [Button] void toggle5() => Toggle(5);
    [Button] void toggle6() => Toggle(6);
    [Button] void toggle7() => Toggle(7);
    [Button] void toggle8() => Toggle(8);
    [Button] void toggle9() => Toggle(9);
}
