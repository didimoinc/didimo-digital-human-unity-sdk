using System.Linq;

using UnityEngine;
using static UnityEngine.ColorUtility;
using static UnityEngine.Object;
using static Didimo.AssetFitter.Editor.Graph.AssetTools;
using static Didimo.AssetFitter.Editor.Graph.MeshTools;
using System.IO;
using UnityEditor;

public static class MenuOverrides
{
    [MenuItem("CONTEXT/MeshRenderer/Convert to Skinned Mesh")]
    static void ConvertToSkinnedMesh(MenuCommand command)
    {
        MeshRenderer renderer = command.context as MeshRenderer;
        MeshFilter filter = renderer.GetComponent<MeshFilter>();

        SkinnedMeshRenderer skin = renderer.gameObject.AddComponent<SkinnedMeshRenderer>();
        skin.sharedMaterials = renderer.sharedMaterials;
        skin.sharedMesh = filter.sharedMesh;

        EditorApplication.delayCall += () => EditorApplication.delayCall += () => { DestroyImmediate(filter); DestroyImmediate(renderer); };
    }

    [MenuItem("CONTEXT/SkinnedMeshRenderer/Convert to Mesh Renderer & Filter")]
    static void ConvertToMeshRenderer(MenuCommand command)
    {
        SkinnedMeshRenderer renderer = command.context as SkinnedMeshRenderer;
        renderer.gameObject.AddComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
        renderer.gameObject.AddComponent<MeshFilter>().sharedMesh = renderer.sharedMesh;
        EditorApplication.delayCall += () => UnityEngine.Object.DestroyImmediate(renderer);
    }

    [MenuItem("CONTEXT/SkinnedMeshRenderer/Convert to Mesh Renderer & Filter (Baked)")]
    static void ConvertToMeshRendererBaked(MenuCommand command)
    {
        SkinnedMeshRenderer renderer = command.context as SkinnedMeshRenderer;
        Mesh mesh = UnityEngine.Object.Instantiate(renderer.sharedMesh);
        mesh.name = renderer.sharedMesh.name + " (Baked)";
        renderer.BakeMesh(mesh);
        renderer.gameObject.AddComponent<MeshRenderer>().sharedMaterials = renderer.sharedMaterials;
        renderer.gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
        EditorApplication.delayCall += () => UnityEngine.Object.DestroyImmediate(renderer);
    }

    [MenuItem("CONTEXT/MeshRenderer/Set Submesh Materials")]
    static void SetMRSubMeshMaterials(MenuCommand command)
    {
        MeshRenderer renderer = command.context as MeshRenderer;
        renderer.sharedMaterials = GetSubMeshUVColors(renderer.GetComponent<MeshFilter>().sharedMesh.subMeshCount, GetDefaultShader());
    }

    [MenuItem("CONTEXT/SkinnedMeshRenderer/Set Submesh Materials")]
    static void SetSMSubMeshMaterials(MenuCommand command)
    {
        SkinnedMeshRenderer renderer = command.context as SkinnedMeshRenderer;
        renderer.sharedMaterials = GetSubMeshUVColors(renderer.sharedMesh.subMeshCount, GetDefaultShader());
    }

    [MenuItem("CONTEXT/SkinnedMeshRenderer/Remove Root Bone Reference and Re-Assign", true)]
    static bool RemoveRootBone_Validation(MenuCommand command)
    {
        SkinnedMeshRenderer renderer = command.context as SkinnedMeshRenderer;
        return renderer && renderer.rootBone.name != "GrpMRealtimeRigExport" && renderer.GetComponentInParent<Didimo.DidimoComponents>();
    }

    [MenuItem("CONTEXT/SkinnedMeshRenderer/Remove Root Bone Reference and Re-Assign")]
    static void RemoveRootBone(MenuCommand command)
    {
        SkinnedMeshRenderer renderer = command.context as SkinnedMeshRenderer;
        Mesh mesh = renderer.sharedMesh;
        Transform[] bones = renderer.bones;
        Transform rootBone = bones.FirstOrDefault(b => b.name == "GrpMRealtimeRigExport");

        int rootBoneIndex = System.Array.IndexOf(renderer.bones, renderer.rootBone);
        renderer.bones = bones.Where(b => renderer.rootBone != b && !b.GetComponent<SkinnedMeshRenderer>()).ToArray();
        renderer.rootBone = bones.FirstOrDefault(b => b.name == "GrpMRealtimeRigExport");
        Mesh nmesh = CloneAsset(mesh);

        BoneWeights weights = new BoneWeights(nmesh.boneWeights);
        BoneWeight[] nWeights = new BoneWeight[weights.weights.Count];

        for (int i = 0; i < weights.weights.Count; i++)
        {
            BoneWeights.WeightIndex ReIndex(BoneWeights.WeightIndex weightIndex)
            {
                weightIndex.index = weightIndex.index >= rootBoneIndex ? weightIndex.index - 1 : weightIndex.index;
                return weightIndex;
            }

            nWeights[i] = BoneWeights.Create(weights.weights[i].Select(wi => ReIndex(wi)).ToArray());
        }

        nmesh.boneWeights = nWeights;
        nmesh.bindposes = nmesh.bindposes.Take(rootBoneIndex).Concat(mesh.bindposes.Skip(rootBoneIndex + 1).TakeWhile(b => true)).Take(renderer.bones.Length).ToArray();
        renderer.sharedMesh = nmesh;
    }

    [MenuItem("Assets/Didimo/Create Materials From Texture", true)]
    static bool CreateMaterials_Validation()
    {
        return Selection.objects.Any(o => o as Texture2D);
    }

    [MenuItem("Assets/Didimo/Create Materials From Texture")]
    static void CreateMaterials()
    {
        Shader shader = GetDefaultShader();
        Material CreateAsset(Texture2D texture)
        {
            var m = new Material(shader) { name = texture.name };
            m.mainTexture = texture;
            string path = AssetDatabase.GetAssetPath(texture);
            string assetPath = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path) + ".mat";
            Debug.Log("Create material asset at " + assetPath);
            AssetDatabase.CreateAsset(m, assetPath);
            return m;
        }
        Material[] materials = Selection.objects.Where(o => o as Texture2D).Select(t => CreateAsset(t as Texture2D)).ToArray();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.objects = materials;
    }

    static void SetRendererMaterials(Renderer renderer, Material[] materials) => renderer.sharedMaterials = materials;

    static Shader DefaultShader;
    static Shader GetDefaultShader()
    {
        if (!DefaultShader)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DefaultShader = cube.GetComponent<MeshRenderer>().sharedMaterial.shader;
            UnityEngine.Object.DestroyImmediate(cube);
        }
        return DefaultShader;
    }

    static Material[] GetSubMeshUVColors(int count, Shader shader) =>
        Colors32.OrderBy(c => UnityEngine.Random.value).Take(count).
            Select((h, i) => new Material(shader) { name = h[1], color = TryParseHtmlString(h[0], out Color color) ? color : Color.magenta }).ToArray();

    public static string[][] Colors32 = new string[][]
    {
       new [] {"#556b2f","darkolivegreen"},
       new [] {"#800000","maroon"},
       new [] {"#483d8b","darkslateblue"},
       new [] {"#008000","green"},
       new [] {"#3cb371","mediumseagreen"},
       new [] {"#b8860b","darkgoldenrod"},
       new [] {"#008b8b","darkcyan"},
       new [] {"#000080","navy"},
       new [] {"#32cd32","limegreen"},
       new [] {"#8b008b","darkmagenta"},
       new [] {"#ff4500","orangered"},
       new [] {"#ff8c00","darkorange"},
       new [] {"#40e0d0","turquoise"},
       new [] {"#00ff00","lime"},
       new [] {"#9400d3","darkviolet"},
       new [] {"#00fa9a","mediumspringgreen"},
       new [] {"#dc143c","crimson"},
       new [] {"#00bfff","deepskyblue"},
       new [] {"#0000ff","blue"},
       new [] {"#adff2f","greenyellow"},
       new [] {"#ff00ff","fuchsia"},
       new [] {"#1e90ff","dodgerblue"},
       new [] {"#db7093","palevioletred"},
       new [] {"#f0e68c","khaki"},
       new [] {"#ffff54","laserlemon"},
       new [] {"#add8e6","lightblue"},
       new [] {"#ff1493","deeppink"},
       new [] {"#7b68ee","mediumslateblue"},
       new [] {"#ffa07a","lightsalmon"},
       new [] {"#ee82ee","violet"},
       new [] {"#ffc0cb","pink"},
       new [] {"#696969","dimgray"},
    };
}