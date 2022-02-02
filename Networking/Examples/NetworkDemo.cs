using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Didimo.Core.Deformables;
using Didimo.Core.Inspector;
using Didimo.Core.Utility;
using Didimo.Extensions;

namespace Didimo.Networking
{
    public class NetworkDemo : MonoBehaviour, ListToPopupAttribute.IListToPopup
    {
        [SerializeField, Readonly]
        protected string didimoKey;

        [SerializeField, Readonly]
        protected DidimoComponents didimoComponents;

        private List<string> didimoList;

        [ListToPopup]
        public int selectedDidimo;

        public List<string> ListToPopupGetValues() => didimoList;

#if UNITY_EDITOR

        public void ListToPopupSetSelectedValue(int i) { selectedDidimo = i; }
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
            didimoList.Clear();
            Task<(bool success, List<DidimoDetailsResponse> didimos)> listTask = Api.Instance.GetAllDidimos();
            await listTask;
            if (!listTask.Result.success) return;
            
            didimoList.AddRange(listTask.Result.didimos.Select(item => item.DidimoKey));
        }

        [Button]
        public async Task ImportFromKey()
        {
            if (ProgressMessage != null)
            {
                EditorUtility.DisplayDialog("Error", "Please wait for the current request to complete.", "OK");
                return;

            }

            string chosenKey = didimoList[selectedDidimo];

            if (string.IsNullOrEmpty(chosenKey)) return;

            ProgressMessage = "Importing your didimo";
            Progress = 0f;
            Task<(bool success, DidimoComponents didimo)> importFromKeyTask = Api.Instance.DidimoFromKey(chosenKey, null, progress => { Progress = progress; });
            await importFromKeyTask;
            ProgressMessage = null;

            if (!importFromKeyTask.Result.success)
            {
                EditorUtility.DisplayDialog("Failed to import didimo", "Failed to import your didimo. Please check the console for logs.", "OK");
                return;
            }

            EditorUtility.DisplayDialog("Imported didimo", "Imported your didimo with success. You can inspect it in the scene.", "OK");
            didimoKey = importFromKeyTask.Result.didimo.DidimoKey;
            didimoComponents = importFromKeyTask.Result.didimo;
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

            var deformableDatabase = UnityEngine.Resources.Load<DeformableDatabase>("DeformableDatabase");
            string deformableId = deformableDatabase.AllIDs.RandomOrDefault();
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
            if (didimoComponents == null)
            {
                EditorUtility.DisplayDialog("Error", "First, create a didimo", "OK");
                return;
            }
            
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
