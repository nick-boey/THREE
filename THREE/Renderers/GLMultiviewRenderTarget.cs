using System.Collections;

namespace THREE;

[Serializable]
public class GLMultiviewRenderTarget : GLRenderTarget
{
    public int numViews;

    public GLMultiviewRenderTarget(int width, int height, int numViews, Hashtable options = null) : base(width, height,
        options)
    {
        IsGLMultiviewRenderTarget = true;
        this.numViews = numViews;
    }
}