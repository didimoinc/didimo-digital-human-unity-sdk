using UnityEngine;

namespace Didimo
{
    public abstract class ASingletonBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject gameObject = GameObject.Find(typeof(T).Name);
                    if (gameObject == null)
                    {
                        gameObject = new GameObject(typeof(T).Name);
                    }

                    _instance = gameObject.AddComponent<T>();
                }

                return _instance;
            }
        }

        public static bool Exists => _instance != null;

        private void Awake()
        {
            if (_instance != null)
            {
                Debug.LogWarning($"Singleton behaviour '{Instance.name}' already exists, destroying duplicate...");
                Destroy(gameObject);

                return;
            }

            _instance = this as T;
            DontDestroyOnLoad(this);

            OnAwake();
        }

        protected virtual void OnAwake() { }
    }
}