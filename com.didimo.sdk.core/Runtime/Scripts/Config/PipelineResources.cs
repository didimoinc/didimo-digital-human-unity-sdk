using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;

namespace Didimo.Core.Config
{
    [CreateAssetMenu(fileName = "PipelineResources", menuName = "Didimo/Pipeline Resources")]
    public class PipelineResources : ScriptableObject
    {
        public RenderPipelineAsset URPPipelineAsset;
        public RenderPipelineAsset HDRPPipelineAsset;
    }
}