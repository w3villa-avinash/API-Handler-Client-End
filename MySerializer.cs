using Newtonsoft.Json;
using UnityEngine;

namespace SuperOne.Network
{
    public class MySerializer
    {
        public static string Serialize<T>(T @object)
        {
            try
            {
                var result = JsonConvert.SerializeObject(@object);
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could not serialize {@object.GetType()}. {e.Message}");
                return string.Empty;
            }
        }

        public static T Deserialize<T>(string text)
        {
            try
            {
                var result = JsonConvert.DeserializeObject<T>(text);
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Could not parse json {text}. {e.Message}");
                return default;
            }
        }
    }
}
