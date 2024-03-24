using System;
using System.IO;
using UnityEngine;
using Logger = Funky.Log.Logger;

namespace Funky.Utility
{
    public class IOUtility
    {
        public static bool Exists(string path)
        {
            try
            {
                return File.Exists(path);
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            return false;
        }
        
        public static string ReadAllText(string path)
        {
            try
            {
                var text = File.ReadAllText(path);
                return text;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            return string.Empty;
        }
    }
}