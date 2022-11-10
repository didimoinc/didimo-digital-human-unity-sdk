using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Didimo.AssetFitter.Editor.Graph
{
    public class DidimoVersioning : ScriptableObject
    {
        public Version[] versions;

        [Serializable]
        public class Version
        {
            public GraphData graphData;
        }
    }
}