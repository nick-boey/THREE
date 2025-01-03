using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class LineBasicMaterial : Material
{
    //public Color Color;

    public string LineCap = "round";

    public string LineJoin = "round";


    public LineBasicMaterial(Hashtable parameters = null)
    {
        Color = new Color().SetHex(0xffffff);

        type = "LineBasicMaterial";

        LineWidth = 1.0f;
        LineCap = "round";
        LineJoin = "round";

        if (parameters != null)
            SetValues(parameters);
    }

    protected LineBasicMaterial(LineBasicMaterial source) : base(source)
    {
        Color = source.Color;

        LineWidth = source.LineWidth;

        LineCap = source.LineCap;

        LineJoin = source.LineJoin;
    }

    public LineBasicMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public new LineBasicMaterial Clone()
    {
        return new LineBasicMaterial(this);
    }
}