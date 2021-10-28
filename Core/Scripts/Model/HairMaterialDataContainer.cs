using Didimo.Builder;
using Newtonsoft.Json;
using UnityEngine;

namespace Didimo
{
    public class HairMaterialDataContainer : MaterialDataContainer
    {
        [JsonProperty("hairColor")] public Vector4MaterialDataParameter HairColor { get; set; }

        [JsonProperty("hairCap")] public TextureMaterialDataParameter HairCapTexture { get; set; }

        public override void ApplyToHierarchy(DidimoBuildContext context, MaterialBuilder builder)
        {
            // We don't call the base implementation, instead we just apply the cap/colour without a geo map.
            // This, along with ApplyTargetted will be a complete application of a Hair Material Data container.

            Debug.Log("Applying hair cap");
            foreach (Renderer renderer in context.MeshHierarchyRoot.GetComponentsInChildren<Renderer>())
            {
                if (renderer.sharedMaterial == null) continue;
                Material material = renderer.sharedMaterial;
                HairColor.ApplyToMaterial(context, material);
                HairCapTexture.ApplyToMaterial(context, material);
            }
        }
    }
}