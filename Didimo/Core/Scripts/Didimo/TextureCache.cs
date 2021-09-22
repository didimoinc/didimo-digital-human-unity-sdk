using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Didimo.Builder;
using UnityEngine;

namespace Didimo
{
    public class TextureCache
    {
        private readonly Dictionary<string, Texture2D> idToTexture;

        public IReadOnlyDictionary<string, Texture2D> IDToTexture => idToTexture;

        public TextureCache() { idToTexture = new Dictionary<string, Texture2D>(); }

        public void Clear() { idToTexture.Clear(); }

        public async Task<(bool success, Texture2D texture)> TryGetOrLoad(DidimoBuildContext context, string id)
        {
            if (TryGet(id, out Texture2D texture))
            {
                return (true, texture);
            }

            string path = Path.Combine(context.RootDirectory, id);

            Task<Texture2D> loadTask = TextureLoader.LoadFromFilePath(path);
            await loadTask;
            if (loadTask.Result == null)
            {
                Debug.Log($"Load from path failed for: {id}");
                return (false, null);
            }

            Add(id, loadTask.Result);

            return (true, loadTask.Result);
        }

        public async Task Add(string id, string filePath)
        {
            if (TryGet(id, out _))
            {
                string msg = $"Skipping texture load ({id}) as a texture is already loaded with that id.";
                Debug.Log(msg);
                return;
            }

            Task<Texture2D> loadTask = TextureLoader.LoadFromFilePath(filePath);
            await loadTask;
            if (loadTask.Result == null)
            {
                string msg = $"Failed to load texture ({id}) from path: {filePath}";
                Debug.Log(msg);
                return;
            }

            Add(id, loadTask.Result);
        }

        public bool Add(string id, Texture2D texture)
        {
            if (TryGet(id, out _))
            {
                string msg = $"Skipping texture load ({id}) as a texture is already loaded with that id.";
                Debug.Log(msg);
                return false;
            }

            idToTexture.Add(id, texture);
            return true;
        }

        public bool TryGet(string id, out Texture2D texture) => idToTexture.TryGetValue(id, out texture);
    }
}