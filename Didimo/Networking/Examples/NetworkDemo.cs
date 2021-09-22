using System.Collections.Generic;
using System.Threading.Tasks;
using Didimo.Inspector;
using DigitalSalmon.Extensions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Didimo.Networking
{
    public class NetworkDemo : MonoBehaviour
    {
        [SerializeField]
        protected string didimoKey;

        [SerializeField]
        protected string downloadArtifactType = "gltf";

        [SerializeField]
        protected Color hairColor;

        [FormerlySerializedAs("didimoComponent")]
        [FormerlySerializedAs("didimo")]
        [SerializeField]
        protected DidimoComponents didimoComponents;
#if UNITY_EDITOR

        public string ProgressMessage { get; private set; }
        public float Progress { get; private set; }

        private void OnValidate()
        {
            ProgressMessage = null;
            Progress = 0f;
        }

        [Button]
        void OpenDeveloperPortalDocumentation() { Application.OpenURL(UsefulLinks.CREATING_A_DIDIMO_DEVELOPER_PORTAL); }

        [Button("Create didimo And Import")]
        public async Task CreateDidimoAndImport()
        {
            string photoFilePath = EditorUtility.OpenFilePanel("Choose a photo", "", "png,jpg");
            if (string.IsNullOrEmpty(photoFilePath)) return;
            ProgressMessage = "Creating your didimo";
            Progress = 0f;
            Debug.Log("Creating didimo");
            Task<(bool success, DidimoComponents didimo)> createDidimoTask = Api.Instance.CreateDidimoAndImportGltf(photoFilePath,
                null,
                progress =>
                {
                    Progress = progress;
                });
            await createDidimoTask;
            ProgressMessage = null;

            if (!createDidimoTask.Result.success)
            {
                EditorUtility.DisplayDialog("Failed to create didimo", "Failed to create your didimo. Please check the console for logs.", "OK");
                return;
            }

            EditorUtility.DisplayDialog("Created didimo", "Created and imported your didimo with success. You can inspect it in the scene.", "OK");
            didimoKey = createDidimoTask.Result.didimo.DidimoKey;
            didimoComponents = createDidimoTask.Result.didimo;
        }

        [Button("Delete didimo")]
        public async Task DeleteDidimo()
        {
            Task<bool> deleteTask = Api.Instance.DeleteDidimo(didimoKey);
            await deleteTask;
            if (deleteTask.Result)
            {
                EditorUtility.DisplayDialog("Deleted didimo", $"Deleted didimo with key: {didimoKey}", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Failed to delete didimo", $"Failed to delete didimo with key {didimoKey}", "OK");
            }
        }

        [Button("List didimos")]
        public async Task ListDidimos()
        {
            Task<(bool success, List<DidimoDetailsResponse> didimos)> listTask = Api.Instance.GetAllDidimos();
            await listTask;
            if (!listTask.Result.success) return;

            foreach (DidimoDetailsResponse didimoStatus in listTask.Result.didimos)
            {
                Debug.Log($"didimo key: {didimoStatus.DidimoKey}, percent: {didimoStatus.Percent}");
            }
        }

        [Button]
        public void AddRandomDeformableAsset()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "To use this feature you must first enter play mode", "OK");
            }

            if (didimoComponents == null)
            {
                EditorUtility.DisplayDialog("Error", "First, create a didimo", "OK");
                return;
            }

            string deformableId = DeformableDatabase.AllIDs.RandomOrDefault();
            if (!didimoComponents.Deformables.TryCreate(deformableId, out Deformable deformable))
            {
                EditorUtility.DisplayDialog("Error", "Failed to create deformable", "OK");
                return;
            }

            // Color color = Color.HSVToRGB(Random.value, Random.Range(0.5f, 0.7f), Random.Range(0.2f, 0.7f));
            Hair hair = deformable as Hair;
            if (hair)
            {
                hair.Color = hairColor;
            }
        }

        [Button]
        protected async Task DeformAsset()
        {
            if (didimoComponents.Deformables.TryFind(out Deformable deformable))
            {
                byte[] undeformedMeshData = deformable.GetUndeformedMeshData();
                Progress = 0.0f;
                ProgressMessage = "Deforming asset";
                (bool success, byte[] deformedMeshData) = await Api.Instance.Deform(didimoComponents.DidimoKey, undeformedMeshData, progress => { Progress = progress; });
                if (!success)
                {
                    EditorUtility.DisplayDialog("Error", "Error with api call", "OK");
                    ProgressMessage = null;
                    return;
                }

                ProgressMessage = null;

                deformable.SetDeformedMeshData(deformedMeshData);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please add a deformable to your didimo first", "OK");
            }
        }

        [Button]
        private void SetHairColor()
        {
            if (didimoComponents.Deformables.TryFind(out Hair hair))
            {
                hair.Color = hairColor;
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please add a hairstyle to your didimo first", "OK");
            }
        }

        [Button]
        private void RemoveDeformable() { didimoComponents.Deformables.DestroyAll<Deformable>(); }
#endif
    }
}