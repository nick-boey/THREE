using System.Collections;
using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLMultiview
{
    private bool available;

    private List<Camera> CameraArray = new();

    private GLRenderTarget currentRenderTarget;
    public int DEFAULT_NUMVIEWS = 2;

    private GLExtensions extensions;

    private List<Matrix3> mat3 = new();

    private List<Matrix4> mat4 = new();

    private int maxNumViews;

    private GLProperties properties;

    private GLRenderer renderer;

    private Vector2 renderSize;

    private GLRenderTarget renderTarget;

    public GLMultiview(GLRenderer renderer)
    {
        extensions = renderer.Extensions;
        properties = renderer.Properties;
        this.renderer = renderer;
    }

    public bool IsAvailable()
    {
        available = false;
        var extension = extensions.Get("GL_OVR_multiview2");

        if (extension != -1)
        {
            available = true;
            maxNumViews = GL.GetInteger(GetPName.MaxViewportDims);

            renderTarget = new GLMultiviewRenderTarget(0, 0, DEFAULT_NUMVIEWS);

            renderSize = Vector2.Zero();

            for (var i = 0; i < maxNumViews; i++)
            {
                mat4.Add(new Matrix4());
                mat3.Add(new Matrix3());
            }
        }

        return available;
    }

    public List<Camera> GetCameraArray(Camera camera)
    {
        if (camera is ArrayCamera) return (camera as ArrayCamera).Cameras;

        CameraArray.Add(camera);

        return CameraArray;
    }

    public void UpdateCameraProjectionMatricesUniform(Camera camera, Hashtable uniforms)
    {
        var cameras = GetCameraArray(camera);

        for (var i = 0; i < cameras.Count; i++) mat4[i] = cameras[i].ProjectionMatrix;

        if (uniforms.ContainsKey("projectionMatrices"))
            uniforms["projectionMatrices"] = mat4;
        else
            uniforms.Add("projectionMatrices", mat4);
    }

    public void UpdateCameraViewMatricesUniform(Camera camera, Hashtable uniforms)
    {
        var cameras = GetCameraArray(camera);

        for (var i = 0; i < cameras.Count; i++) mat4[i] = cameras[i].MatrixWorldInverse;

        if (uniforms.ContainsKey("viewMatrices"))
            uniforms["viewMatrices"] = mat4;
        else
            uniforms.Add("viewMatrices", mat4);
    }

    public void UpdateObjectMatricesUniforms(Object3D object3D, Camera camera, Hashtable uniforms)
    {
    }

    public bool IsMultiviewCompatible(Camera camera)
    {
        return true;
    }

    public void ResizeRenderTarget(Camera camera)
    {
    }

    public void AttachCamera(Camera camera)
    {
    }

    public void DetachCamera(Camera camera)
    {
    }

    public void Flush(Camera camera)
    {
    }
}