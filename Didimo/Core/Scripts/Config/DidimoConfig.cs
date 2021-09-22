using Didimo;
using UnityEngine;

[CreateAssetMenu(fileName = "DidimoConfig", menuName = "Didimo/Didimo Config")]
public class DidimoConfig : ScriptableObject
{
    [Header("Features")]
    [SerializeField]
    protected SupportedRenderPipelines pipeline;

    public static SupportedRenderPipelines Pipeline => Instance.pipeline;

    protected static DidimoConfig Instance => DidimoResources.DidimoConfig;
}