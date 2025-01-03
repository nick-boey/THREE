using System.Collections;

namespace THREE;

[Serializable]
public class GLRenderLists : Hashtable
{
    private GLProperties properties;

    public GLRenderLists(GLProperties properties)
    {
        this.properties = properties;
    }

    public GLRenderList Get(Object3D scene, int renderCallDepth)
    {
        GLRenderList list = null;

        if (!ContainsKey(scene))
        {
            list = new GLRenderList(properties);
            var lists = new List<GLRenderList> { list };

            Add(scene, lists);
        }
        else
        {
            if (renderCallDepth >= (this[scene] as List<GLRenderList>).Count)
            {
                list = new GLRenderList(properties);
                (this[scene] as List<GLRenderList>).Add(list);
            }
            else
            {
                list = (this[scene] as List<GLRenderList>)[renderCallDepth];
            }
        }

        return list;
    }
}