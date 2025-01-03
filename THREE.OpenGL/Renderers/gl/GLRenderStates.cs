using System.Collections;

namespace THREE;

[Serializable]
public class GLRenderStates
{
    private GLCapabilities capabilities;
    private GLExtensions extensions;
    public Hashtable renderStates = new();

    public GLRenderStates(GLExtensions extensions, GLCapabilities capabilities)
    {
        this.extensions = extensions;
        this.capabilities = capabilities;
    }

    public GLRenderState Get(Object3D scene, int renderCallDepth = 0)
    {
        GLRenderState renderState;

        if (!renderStates.Contains(scene))
        {
            renderState = new GLRenderState(extensions, capabilities);
            var list = new List<GLRenderState> { renderState };
            renderStates.Add(scene, list);
        }
        else
        {
            if (renderCallDepth >= (renderStates[scene] as List<GLRenderState>).Count)
            {
                renderState = new GLRenderState(extensions, capabilities);
                (renderStates[scene] as List<GLRenderState>).Add(renderState);
            }
            else
            {
                renderState = (renderStates[scene] as List<GLRenderState>)[renderCallDepth];
            }
        }

        return renderState;
    }
}