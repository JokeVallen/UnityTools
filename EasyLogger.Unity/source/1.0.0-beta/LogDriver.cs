using UnityEngine;

namespace EasyLogger.Unity
{
    internal sealed class LogDriver : MonoBehaviour
    {
        public static LogDriver Instance
        {
            get
            {
                if (instance == null)
                {
                    var go = new GameObject("LogDriver");
                    DontDestroyOnLoad(go);
                    go.hideFlags = HideFlags.DontSave;
                    instance = go.AddComponent<LogDriver>();
                }
                return instance;
            }
        }
        private static LogDriver instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                DestroyImmediate(this);
                return;
            }
            hideFlags = HideFlags.DontSave;
        }

        private void LateUpdate()
        {
            Debug.Flush();
        }

        private void OnDestroy()
        {
            Debug.Flush();
        }
    }
}