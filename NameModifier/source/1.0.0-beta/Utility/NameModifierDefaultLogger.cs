using System;
using UnityEngine;

namespace EditorTools.NameModifier
{
    internal sealed class NameModifierDefaultLogger : INameModifierLogger
    {
        public void Log(object message)
        {
            if (NameModifierConfig.Instance.LogEnabled)
                Debug.Log(message);
        }

        public void LogError(object message)
        {
            if (NameModifierConfig.Instance.LogEnabled)
                Debug.LogError(message);
        }

        public void LogException(Exception exception)
        {
            if (NameModifierConfig.Instance.LogEnabled)
                Debug.LogException(exception);
        }

        public void LogWarning(object message)
        {
            if (NameModifierConfig.Instance.LogEnabled)
                Debug.LogWarning(message);
        }
    }
}