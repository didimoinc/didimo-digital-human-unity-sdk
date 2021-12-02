using UnityEngine;

namespace Didimo.Core.Utility
{
    public abstract class ASingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject gameObject = GameObject.Find(typeof(T).Name);
                    if (gameObject == null)
                    {
                        gameObject = new GameObject(typeof(T).Name);
                    }

                    instance = gameObject.AddComponent<T>();
                }

                return instance;
            }
        }

        public static bool Exists => instance != null;

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogWarning($"Singleton behaviour '{Instance.name}' already exists, destroying duplicate...");
                Destroy(gameObject);

                return;
            }

            instance = this as T;
            DontDestroyOnLoad(this);

            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}