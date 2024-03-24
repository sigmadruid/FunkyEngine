using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Funky.Log
{
    public class Logger
    {
        [Conditional("FUNKY_DEBUG")]
        public static void Log(object obj, params object[] paramList)
        {
            if (paramList != null && paramList.Length > 0)
            {
                Debug.LogFormat(obj.ToString(), paramList);
            }
            else
            {
                Debug.Log(obj);
            }
        }
        
        [Conditional("FUNKY_DEBUG")]
        public static void Warn(object obj, params object[] paramList)
        {
            if (paramList != null && paramList.Length > 0)
            {
                Debug.LogWarningFormat(obj.ToString(), paramList);
            }
            else
            {
                Debug.LogWarning(obj);
            }
        }
        
        public static void Error(object obj, params object[] paramList)
        {
            if (paramList != null && paramList.Length > 0)
            {
                Debug.LogErrorFormat(obj.ToString(), paramList);
            }
            else
            {
                Debug.LogError(obj);
            }
        }
    }
}