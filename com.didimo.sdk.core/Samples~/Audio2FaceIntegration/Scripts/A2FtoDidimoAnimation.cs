using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Didimo;
using Didimo.Core.Inspector;
using Newtonsoft.Json;
using UnityEngine;

public class A2FtoDidimoAnimation : MonoBehaviour 
{
    public class BSData 
    {
        public int       numPoses;
        public int       numFrames;
        public string[]  facsNames;
        public float[][] weightMat;
    }
    
    [SerializeField]
    private GameObject didimo;
    
    [SerializeField]
    private TextAsset bsWeightsFile;

    [SerializeField]
    private AudioClip audioClip;

    [SerializeField]
    private int frameRate = 60;
    
    
    private BSData           bsData;
    private DidimoComponents didimoComponents;

    public void Start() 
    {
        PlayAnimation();
    }

    public void PlayAnimation()
    {
        didimoComponents = didimo.GetComponent<DidimoComponents>();
        ParseBsDataFile();
        DidimoAnimation audio2FaceAnimation = CreateA2FDidimoAnimation();
        AnimationCache.Add(audio2FaceAnimation.AnimationName, audio2FaceAnimation);
        didimoComponents.Animator.PlayAnimation(audio2FaceAnimation.AnimationName);
    }


    public DidimoAnimation CreateA2FDidimoAnimation()
    {
        Dictionary<string, List<float>> poseData = new Dictionary<string, List<float>>();

        for (int poseIx = 0; poseIx < bsData.numPoses; poseIx++)
        {
            string poseName = bsData.facsNames[poseIx];
            List<float> poseValues = new List<float>();
            for (int frameIx = 0; frameIx < bsData.numFrames; frameIx++)
            {
                poseValues.Add(bsData.weightMat[frameIx][poseIx]);
            }
            poseData[poseName] = poseValues;
        }

        int frameCount = bsData.weightMat.Length;
        float animationLength = (float) frameCount / frameRate;

        return DidimoAnimation.FromPosesDictionary(bsWeightsFile.name, poseData, frameRate, animationLength, frameCount, audioClip);
    }


    public void ParseBsDataFile()
    {
        if (didimoComponents == null) didimoComponents = didimo.GetComponent<DidimoComponents>();
        IReadOnlyList<string> fullPoseNames = didimoComponents.PoseController.GetAllIncludedPoses();
        BSData originalBsData = JsonConvert.DeserializeObject<BSData>(bsWeightsFile.text);
        
        List<string> bsDataPoseNames = new List<string>();
        for (int i = 0; i < originalBsData.numPoses; i++)
        {
            string shortPoseName = originalBsData.facsNames[i];
            string correctedName = fullPoseNames.FirstOrDefault(poseName => poseName.EndsWith(shortPoseName));
            bsDataPoseNames.Add(string.IsNullOrEmpty(correctedName) ? shortPoseName : correctedName);
        }

        bsData = new BSData
        {
            facsNames = bsDataPoseNames.ToArray(),
            weightMat = originalBsData.weightMat,
            numFrames = originalBsData.numFrames,
            numPoses = bsDataPoseNames.Count
        };
    }
}
