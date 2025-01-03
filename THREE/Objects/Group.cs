using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class Group : Object3D
{
    public List<string> MaterialLibraries;

    public Group()
    {
    }

    public Group(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}