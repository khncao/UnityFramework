/// <summary>
/// Adopted from Unity Wiki's implementation
/// </summary>

using UnityEngine;

namespace m4k
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private readonly static object m_Lock = new();
        protected static T _instance;

        protected bool m_ShuttingDown = false;

        public static T I
        {
            get
            {
                lock (m_Lock)
                {
                    if (_instance != null)
                    {
                        return _instance;
                    }
                    else {
                        var objs = FindObjectsOfType<T>();
                        if (objs.Length > 1)
                        {
                            Debug.LogError("Unexpected: found more than 1 instance of singleton");
                        }
                        else if(objs.Length == 1)
                        {
                            _instance = objs[0];
                        }
                        if (_instance == null)
                        {
                            GameObject obj = new(typeof(T).Name);
                            _instance = obj.AddComponent<T>();
                        }
                    }

                    return _instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                m_ShuttingDown = false;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                m_ShuttingDown = true;
                Debug.LogWarning($"Existing instance of type {typeof(T).ToString()}");
            }
        }

        protected virtual void OnDestroy()
        {
            lock (m_Lock)
            {
                _instance = null;
            }
        }
    }
}