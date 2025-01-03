using System.Collections;

namespace THREE;

[Serializable]
public class GLClipping
{
    private List<float> globalState;

    private bool localClippingEnabled;

    private int numGlobalPlanes;

    public int numIntersection;

    public int numPlanes;

    private Plane plane = new();

    private bool renderingShadows;
    public GLUniform uniform;

    private Matrix3 viewNormalMatrix = new();

    public GLClipping()
    {
        uniform = new GLUniform();
        uniform.Add("value", null);
        uniform.Add("needsUpdate", false);
    }

    public bool Init(List<Plane> planes, bool enableLocalClipping, Camera camera)
    {
        var enabled = planes.Count > 0 || enableLocalClipping || numGlobalPlanes != 0 || localClippingEnabled;

        localClippingEnabled = enableLocalClipping;

        globalState = ProjectPlanes(planes, camera, 0);
        numGlobalPlanes = planes.Count;

        return enabled;
    }

    public void BeginShadows()
    {
        renderingShadows = true;
        ProjectPlanes();
    }

    public void EndShadows()
    {
        renderingShadows = false;
        ProjectPlanes();
    }

    public void SetState(List<Plane> planes, bool clipIntersection, bool clipShadows, Camera camera, Hashtable cache,
        bool fromCache)
    {
        if (!localClippingEnabled || planes.Count == 0 || (renderingShadows && !clipShadows))
        {
            if (renderingShadows)
                ProjectPlanes();
            else
                ResetGlobalState();
        }
        else
        {
            var nGlobal = renderingShadows ? 0 : numGlobalPlanes;
            var lGlobal = nGlobal * 4;

            var dstArray = (List<float>)cache["clippingState"];

            uniform["value"] = dstArray;

            dstArray = ProjectPlanes(planes, camera, lGlobal, fromCache);

            for (var i = 0; i != lGlobal; i++) dstArray[i] = globalState[i];

            cache["clippingState"] = dstArray;

            numIntersection = clipIntersection ? numPlanes : 0;
            numPlanes += nGlobal;
        }
    }

    private void ResetGlobalState()
    {
        if (globalState != null && !globalState.Equals(uniform["value"]))
        {
            uniform["value"] = globalState;
            uniform["needsUpdate"] = numGlobalPlanes > 0;
        }

        numPlanes = numGlobalPlanes;
        numIntersection = 0;
    }

    public List<float> ProjectPlanes(List<Plane> planes = null, Camera camera = null, int? dstOffset = null,
        bool? skipTransform = null)
    {
        var nPlanes = planes != null ? planes.Count : 0;
        List<float> dstArray = null;
        float[] array = null;
        if (nPlanes != 0)
        {
            dstArray = (List<float>)uniform["value"];

            if (dstArray != null)
                array = dstArray.ToArray();

            if (skipTransform == null || (bool)skipTransform != true || dstArray == null)
            {
                var flatSize = (int)dstOffset + nPlanes * 4;
                var viewMatrix = camera.MatrixWorldInverse;

                viewNormalMatrix.GetNormalMatrix(viewMatrix);

                if (dstArray == null || dstArray.Count < flatSize) array = new float[flatSize];

                for (int i = 0, i4 = (int)dstOffset; i != nPlanes; ++i, i4 += 4)
                {
                    plane.Copy(planes[i]).ApplyMatrix4(viewMatrix, viewNormalMatrix);

                    plane.Normal.ToArray(array, i4);
                    array[i4 + 3] = plane.Constant;
                }
            }

            dstArray = array.ToList();

            uniform["value"] = dstArray;
            uniform["needsUpdate"] = true;
        }

        numPlanes = nPlanes;
        numIntersection = 0;
        return dstArray;
    }
}