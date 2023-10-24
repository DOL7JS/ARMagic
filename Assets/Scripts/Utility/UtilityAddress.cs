using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
namespace Utility
{
    public static class UtilityAddress
    {
        public static string GetURLToWebserver()
        {
            string URL = Path.Combine("http://", PlayerPrefs.GetString("IPAddress"), PlayerPrefs.GetString("FolderOnServer")).Replace("\\", "/") ;
            return URL;
        }

        public static string GetIPAddressOfWebserver()
        {
            return PlayerPrefs.GetString("IPAddress");
        }
        
        public static string GetNameOfFolderOfWebserver()
        {
            return PlayerPrefs.GetString("FolderOnServer");
        }

        public static void SetURLToWebserver(string IPAddress, string nameOfFolder)
        {
            PlayerPrefs.SetString("IPAddress", IPAddress);
            PlayerPrefs.SetString("FolderOnServer", nameOfFolder);
        }
    }
}
