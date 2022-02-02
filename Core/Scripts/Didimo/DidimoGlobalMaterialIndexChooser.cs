using System.Collections;
using System.Collections.Generic;
using Didimo;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
namespace Didimo
{
    public class DidimoGlobalMaterialIndexChooser : MonoBehaviour
    {      
        int CurrentMaterialSetIndex = 0;
        public IntRange GlobalMaterialSetSelectionIndex = new IntRange(0,0);        
        public void SetCurrentMaterialSetIndex(int index)
        {            
            GlobalMaterialSetSelectionIndex.Value = index;
            OnValidate();
        }

        public void IncrementCurrentMaterialSetIndex()
        {
            GlobalMaterialSetSelectionIndex.Value = (GlobalMaterialSetSelectionIndex.Value + 1) % GlobalMaterialSetSelectionIndex.MaxValue;        
            OnValidate();
        }
        void OnValidate()
        {
            Scene scene = SceneManager.GetActiveScene();
            
            DidimoMaterialSwitcher[] list = Object.FindObjectsOfType<DidimoMaterialSwitcher>();
    
            int maxMaterialGroups = 0;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i].MaterialSets.Count > maxMaterialGroups)
                    maxMaterialGroups = list[i].MaterialSets.Count;
            }
            GlobalMaterialSetSelectionIndex.MaxValue = maxMaterialGroups;
            if (CurrentMaterialSetIndex != GlobalMaterialSetSelectionIndex.Value)
            {
                CurrentMaterialSetIndex = GlobalMaterialSetSelectionIndex.Value;
               
                for (int i = 0; i < list.Length; i++)
                {
                    list[i].SetMaterialSetIndex(CurrentMaterialSetIndex);
                }
            }
        }
    }
}   
