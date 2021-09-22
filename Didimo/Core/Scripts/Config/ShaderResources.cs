using Didimo;
using UnityEngine;

[CreateAssetMenu(fileName = "ShaderResources", menuName = "Didimo/Shader Resources")]
public class ShaderResources : ScriptableObject
{
    [Header("Shader Resources" /*, "Pipeline Shader resources required to load a didimo"*/)]
    [SerializeField]
    protected Shader eye;

    [SerializeField]
    protected Shader skin;

    [SerializeField]
    protected Shader mouth;

    [SerializeField]
    protected Shader eyelash;

    [SerializeField]
    protected Shader unlitTexture;

    [SerializeField]
    protected Shader hair;

    public static Shader Eye => Instance.eye;
    public static Shader Skin => Instance.skin;
    public static Shader Mouth => Instance.mouth;
    public static Shader Eyelash => Instance.eyelash;
    public static Shader UnlitTexture => Instance.unlitTexture;
    public static Shader Hair => Instance.hair;

    protected static ShaderResources Instance => DidimoResources.ShaderResources;
}