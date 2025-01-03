using System.Collections;
using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class ShaderMaterial : Material
{
    public Dictionary<string, object> Attributes;

    public Hashtable DefaultAttributeValues = new();

    public Extensions extensions;

    public string FragmentShader =
        "void main() {\n\t" +
        "   gl_FragColor = vec4(1.0,0.0,0.0,1.0);\n" +
        "}";

    //public bool Skinning = false;

    //public bool MorphTargets { get; set; }

    //public bool MorphNormals = false;

    public string Index0AttributeName = null;

    public bool Lights;

    public int Shading = Constants.SmoothShading;

    public GLUniforms Uniforms;

    public bool UniformsNeedUpdate = false;

    public string VertexShader =
        "void main() {\n\t" +
        "   gl_Position = projectionMatrix*modelViewMatrix*vec4(position,1.0);\n" +
        "}";

    public ShaderMaterial(Hashtable parameters = null)
    {
        type = "ShaderMaterial";

        Attributes = new Dictionary<string, object>();

        Uniforms = new GLUniforms();

        Wireframe = false;

        WireframeLineWidth = 1;

        extensions = new Extensions
        {
            derivatives = false,
            fragDepth = false,
            drawBuffers = false,
            shaderTextureLOD = false
        };

        DefaultAttributeValues = new Hashtable
        {
            { "color", new float[] { 1, 1, 1 } },
            { "uv", new float[] { 0, 0 } },
            { "uv2", new float[] { 0, 0 } }
        };

        if (parameters != null)
            SetValues(parameters);
    }

    public ShaderMaterial(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    protected ShaderMaterial(ShaderMaterial other) : base(other)
    {
        FragmentShader = other.FragmentShader;
        VertexShader = other.VertexShader;
        Uniforms = UniformsUtils.CloneUniforms(other.Uniforms);
        Defines = (Hashtable)other.Defines.Clone();

        Wireframe = other.Wireframe;
        WireframeLineWidth = other.WireframeLineWidth;

        Lights = other.Lights;
        Clipping = other.Clipping;
        Skinning = other.Skinning;

        MorphTargets = other.MorphTargets;
        MorphNormals = other.MorphNormals;

        extensions = other.extensions;
    }

    public new object Clone()
    {
        return new ShaderMaterial(this);
    }

    [Serializable]
    public struct Extensions
    {
        public bool derivatives;
        public bool fragDepth;
        public bool drawBuffers;
        public bool shaderTextureLOD;
    }
}