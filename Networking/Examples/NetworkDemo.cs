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
        [SerializeField, Readonly]
        protected string didimoKey;

        [SerializeField, Readonly]
        protected DidimoComponents didimoComponents;

        [SerializeField]
        protected string downloadArtifactType = "gltf";

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
            if (ProgressMessage != null)
            {
                EditorUtility.DisplayDialog("Error", "Please wait for the current request to complete.", "OK");
                return;

            }
            
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
        protected async Task AddRandomHair()
        {
            if (ProgressMessage != null)
            {
                EditorUtility.DisplayDialog("Error", "Please wait for the current request to complete.", "OK");
                return;

            }
            
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Error", "To use this feature you must first enter play mode", "OK");
                return;
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
            
            deformable.gameObject.SetActive(false);
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
            deformable.gameObject.SetActive(true);
        }

        [Button]
        private void ChangeHairColor()
        {
            if (didimoComponents.Deformables.TryFind(out Hair hair))
            {
                Selection.objects = new Object[] { hair };
                // You can also set a preset with:
                // hair.SetPreset(0);
            }
            else
            {
                EditorUtility.DisplayDialog("Error", "Please add a hairstyle to your didimo first", "OK");
            }
        }

        [Button]
        private void RemoveHair()
        {
            if (didimoComponents.Deformables.TryFind(out Hair hair))
            {
                if (Application.isPlaying) Destroy(hair.gameObject);
                else DestroyImmediate(hair.gameObject);
            }
            else
            {
                Debug.Log("Could not find hair to remove.");
            }
        }
#endif
    }
}