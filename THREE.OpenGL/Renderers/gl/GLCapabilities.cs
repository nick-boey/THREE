using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public struct GLCapabilitiesParameters
{
    public string precision;

    public bool logarithmicDepthBuffer;
}

[Serializable]
public class GLCapabilities
{
    private GLExtensions Extensions;

    public bool floatFragmentTextures;

    public bool floatVertexTextures;
    public bool IsGL2;

    public bool logarithmicDepthBuffer;

    public float maxAnisotropy;

    public int maxAttributes;

    public int maxCubemapSize;

    public int maxFragmentUniforms;

    public int maxSamples;

    public int maxTextures;

    public int maxTextureSize;

    public int maxVaryings;

    public int maxVertexTextures;

    public int maxVertexUniforms;

    public string precision;

    public bool vertexTextures;

    public GLCapabilities(GLExtensions Extensions, ref GLCapabilitiesParameters parameters)
    {
        IsGL2 = Extensions.Get("GL_ARB_ES3_compatibility") > -1 ? true : false;

        //this.IsGL2 = false;

        this.Extensions = Extensions;

        if (parameters.precision != null)
            precision = parameters.precision;
        else
            precision = "highp";

        var maxPrecision = GetMaxPrecision(precision);

        if (!maxPrecision.Equals(precision)) precision = maxPrecision;

        //this.logarithmicDepthBuffer = parameters.logarithmicDepthBuffer == true;

        GL.GetInteger(GetPName.MaxTextureImageUnits, out maxTextures);
        GL.GetInteger(GetPName.MaxVertexTextureImageUnits, out maxVertexTextures);
        GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
        GL.GetInteger(GetPName.MaxCubeMapTextureSize, out maxCubemapSize);
        GL.GetInteger(GetPName.MaxVertexAttribs, out maxAttributes);
        GL.GetInteger(GetPName.MaxVertexUniformVectors, out maxVertexUniforms);
        GL.GetInteger(GetPName.MaxVaryingVectors, out maxVaryings);
        GL.GetInteger(GetPName.MaxFragmentUniformVectors, out maxFragmentUniforms);

        vertexTextures = maxVertexTextures > 0;
        floatFragmentTextures = IsGL2 || Extensions.ExtensionsName.Contains("GL_ARB_texture_float");
        floatVertexTextures = vertexTextures && floatFragmentTextures;

        GL.GetInteger(GetPName.MaxSamples, out maxSamples);

        maxSamples = IsGL2 ? maxSamples : 0;
    }

    public float GetMaxAnisotropy()
    {
        if (Extensions.ExtensionsName.Contains("GL_ARB_texture_filter_anisotropic"))
            GL.GetFloat((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt, out maxAnisotropy);
        else
            maxAnisotropy = 0;

        return maxAnisotropy;


        throw new NotImplementedException();
    }

    public string GetMaxPrecision(string precision)
    {
        if (precision.Equals("highp"))
        {
            int range, value1, value2;
            GL.GetShaderPrecisionFormat(ShaderType.VertexShader, ShaderPrecision.HighFloat, out range, out value1);
            GL.GetShaderPrecisionFormat(ShaderType.FragmentShader, ShaderPrecision.HighFloat, out range, out value2);

            if (value1 > 0 && value2 > 0) return "highp";
            precision = "mediump";
        }

        if (precision.Equals("mediump"))
        {
            int range, value1, value2;
            GL.GetShaderPrecisionFormat(ShaderType.VertexShader, ShaderPrecision.MediumFloat, out range, out value1);
            GL.GetShaderPrecisionFormat(ShaderType.FragmentShader, ShaderPrecision.MediumFloat, out range, out value2);

            if (value1 > 0 && value2 > 0) return "mediump";
        }

        return "lowp";
    }
}