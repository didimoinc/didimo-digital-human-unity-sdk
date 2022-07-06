using System.ComponentModel;

namespace Didimo.AssetFitter.Editor.Graph
{
    [System.Serializable]
    [MenuPath("Object/Bone Map")]
    [DisplayName("Bone Map")]
    //195, 82, 20
    [Width(200)]
    [HeaderColor(TypeColors.BoneIndexRemap)]
    public class CommandRigRemapValue : GraphNode
    {
        [Output("Bone Map"), Expose] public BoneIndexRemap boneIndexRemapOutput;
    }
}
