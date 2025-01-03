using System.Collections;
using Newtonsoft.Json.Linq;

namespace THREE;

[Serializable]
public class BufferGeometryLoader
{
    private Hashtable arrayBufferMap = new();
    private Hashtable interleavedBufferMap = new();

    public BufferGeometry Load(string fileName)
    {
        var text = File.ReadAllText(fileName);
        var jobject = JObject.Parse(text);
        return Parse(jobject);
    }

    public BufferGeometry Parse(JObject json)
    {
        BufferGeometry geometry = null;

        if (json.ContainsKey("isInstancedBufferGeometry"))
            geometry = new InstancedBufferGeometry();
        else
            geometry = new BufferGeometry();

        object indexObj = json["data"]["index"];

        if (indexObj != null)
        {
            var itemSize = (int)(indexObj as JObject)["itemSize"];
            var arrayToken = (indexObj as JObject)["array"];
            var index = arrayToken.ToObject<int[]>();
            geometry.SetIndex(index.ToList());
        }

        var data = (JObject)json["data"];

        if (data != null)
        {
            var attributes = (JObject)data["attributes"];
            if (attributes != null)
                foreach (var o in attributes)
                {
                    var attribute = (JObject)attributes[o.Key];
                    if (attribute != null)
                    {
                        var itemSize = (int)attribute["itemSize"];
                        var arrayToken = attribute["array"];
                        var floatArray = arrayToken.ToObject<float[]>();
                        geometry.SetAttribute(o.Key, new BufferAttribute<float>(floatArray, itemSize));
                    }
                }
        }

        return geometry;
    }
}