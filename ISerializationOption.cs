using SuperOne.Utils;

namespace SuperOne.Network
{
    public interface ISerializationOption
    {
        string ContentType { get; }

        UserIdentifier UserIdentifier { get; }

        T Deserialize<T>(string text);
    }
}
