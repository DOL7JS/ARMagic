using System;
using System.Collections.Generic;
public class JsonHelper
{
    public static List<T> FromJson<T>(string json)
    {
        Wrapper<T> wrapper = UnityEngine.JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.ItemsList;
    }

    public static string ToJson<T>(List<T> mods)
    {

        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.ItemsList = mods;
        return UnityEngine.JsonUtility.ToJson(wrapper);
    }
    [Serializable]
    private class Wrapper<T>
    {
        public List<T> ItemsList;
    }
}
