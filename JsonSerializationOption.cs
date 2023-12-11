using Newtonsoft.Json;
using UnityEngine;
using SuperOne.Utils;

namespace SuperOne.Network
{
    public class JsonSerializationOption : ISerializationOption
    {
        //Move somewhere logical like in a user stats
        //public static UserIdentifier _UserIdentifier;

        public string ContentType => "application/json";

        public UserIdentifier UserIdentifier { get { return _userIdentifier; } }
        private UserIdentifier _userIdentifier;

        public JsonSerializationOption()
        {
            if (UniversalConstants.UserIdentifier != null && !string.IsNullOrEmpty(UniversalConstants.UserIdentifier.token))
                _userIdentifier = UniversalConstants.UserIdentifier;
            else
                _userIdentifier = MySerializer.Deserialize<UserIdentifier>("{\"vNo\":\"51\",\"token\":\"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1c2VySWQiOjgxNTM4LCJpYXQiOjE2NzA1ODg3MDUsImV4cCI6MTY3MzE4MDcwNX0.OYYnlo9HHVGkci3RfudHBkc_uxyHmVXbzkff3MQtxR8\",\"device-Type\":\"IOS\",\"baseURL\":\"https://quickdev2.super.one\",\"langVersion\":0,\"currentLanguage\":\"en\",\"member-id\":2000,\"userName\":\"DefBob\"}");
        }

        public JsonSerializationOption(UserIdentifier userIdentifier)
        {
            _userIdentifier = userIdentifier;
        }

        public T Deserialize<T>(string text)
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
