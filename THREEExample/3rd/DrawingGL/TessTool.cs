//BSD, 2014-present, WinterDev

/*
 * Created by SharpDevelop.
 * User: lbrubaker
 * Date: 3/26/2010
 * Time: 4:37 PM
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;

namespace Tesselate;

public readonly struct TessVertex2d
{
    public readonly double x;
    public readonly double y;

    public TessVertex2d(double x, double y)
    {
        this.x = x;
        this.y = y;
    }
}

/// <summary>
///     listen and handle the event from tesslator
/// </summary>
internal class TessListener : Tesselator.ITessListener
{
    private int _inputVertexCount;
    internal List<ushort> _resultIndexList = new();
    internal List<TessVertex2d> _tempVertexList = new();

    //Tesselator.TriangleListType _triangleListType; 
    public TessListener()
    {
        //empty not use
        //not use first item in temp
        _tempVertexList.Add(new TessVertex2d(0, 0));
    }

    void Tesselator.ITessListener.BeginRead()
    {
    }

    void Tesselator.ITessListener.Begin(Tesselator.TriangleListType type)
    {
#if DEBUG

        if (type != Tesselator.TriangleListType.Triangles)
        {
        }
#endif
        //_triangleListType = type;

        //what type of triangle list
        //Console.WriteLine("begin: " + type.ToString());
        //Assert.IsTrue(GetNextOutputAsString() == "B");
        //switch (type)
        //{
        //    case Tesselator.TriangleListType.Triangles:
        //        Assert.IsTrue(GetNextOutputAsString() == "TRI");
        //        break;

        //    case Tesselator.TriangleListType.TriangleFan:
        //        Assert.IsTrue(GetNextOutputAsString() == "FAN");
        //        break;

        //    case Tesselator.TriangleListType.TriangleStrip:
        //        Assert.IsTrue(GetNextOutputAsString() == "STRIP");
        //        break;

        //    default:
        //        throw new Exception("unknown TriangleListType '" + type.ToString() + "'.");
        //}
    }

    void Tesselator.ITessListener.End()
    {
        //Assert.IsTrue(GetNextOutputAsString() == "E");
        //Console.WriteLine("end");
    }

    void Tesselator.ITessListener.Vertext(int index)
    {
        //Assert.IsTrue(GetNextOutputAsString() == "V");
        //Assert.AreEqual(GetNextOutputAsInt(), index); 
        if (index < 0)
            //use data from temp store***
            //that will be appended to the end of result
            _resultIndexList.Add((ushort)(_inputVertexCount + -index)); //** minus,=> make it positive sign.
        //resultVertexList.Add(this.tempVertextList[-index]);
        //Console.WriteLine("temp_v_cb:" + index + ":(" + tempVertextList[-index] + ")");
        else
            _resultIndexList.Add((ushort)index);
        //resultVertexList.Add(this.inputVertextList[index]);
        // Console.WriteLine("v_cb:" + index + ":(" + inputVertextList[index] + ")");
    }


    public bool NeedEdgeFlag { get; set; }

    void Tesselator.ITessListener.EdgeFlag(bool boundaryEdge_isEdge)
    {
        //Console.WriteLine("edge: " + IsEdge);
        //Assert.IsTrue(GetNextOutputAsString() == "F");
        //Assert.AreEqual(GetNextOutputAsBool(), IsEdge);
    }

    void Tesselator.ITessListener.Combine(double v0,
        double v1,
        double v2,
        ref Tesselator.CombineParameters combinePars,
        out int outData)
    {
        //double error = .001;
        //Assert.IsTrue(GetNextOutputAsString() == "C");
        //Assert.AreEqual(GetNextOutputAsDouble(), v0, error);
        //Assert.AreEqual(GetNextOutputAsDouble(), v1, error);
        //Assert.AreEqual(GetNextOutputAsInt(), data4[0]);
        //Assert.AreEqual(GetNextOutputAsInt(), data4[1]);
        //Assert.AreEqual(GetNextOutputAsInt(), data4[2]);
        //Assert.AreEqual(GetNextOutputAsInt(), data4[3]);
        //Assert.AreEqual(GetNextOutputAsDouble(), weight4[0], error);
        //Assert.AreEqual(GetNextOutputAsDouble(), weight4[1], error);
        //Assert.AreEqual(GetNextOutputAsDouble(), weight4[2], error);
        //Assert.AreEqual(GetNextOutputAsDouble(), weight4[3], error); 
        //here , outData = index of newly add vertext 
        //----------------------------------------------------------------------
        //*** new vertext is added into user vertext list ***            
        //use negative to note that this vertext is from temporary source 

        //other implementation:
        // append to end of input list is ok if the input list can grow up ***
        //----------------------------------------------------------------------
        outData = -_tempVertexList.Count;
        //----------------------------------------
        _tempVertexList.Add(new TessVertex2d(v0, v1));
        //----------------------------------------
    }


    public bool NeedMash { get; set; }

    void Tesselator.ITessListener.Mesh(Mesh mesh)
    {
    }


    /// <summary>
    ///     connect to actual Tesselator
    /// </summary>
    /// <param name="tesselator"></param>
    /// <param name="setEdgeFlag"></param>
    public void Connect(Tesselator tesselator, bool setEdgeFlag)
    {
        NeedEdgeFlag = setEdgeFlag;
        tesselator.SetListener(this);

        //tesselator.callBegin = OnBegin;
        //tesselator.callEnd = OnEnd;
        //tesselator.callVertex = OnVertex;
        //tesselator.callCombine = OnCombine;
        //if (setEdgeFlag)
        //{
        //    tesselator.callEdgeFlag = OnEdgeFlag;
        //}
    }

    /// <summary>
    ///     clear previous results and load a new input vertex list
    /// </summary>
    /// <param name="inputVertexCount"></param>
    public void ResetAndLoadInputVertexList(int inputVertexCount)
    {
        _inputVertexCount = inputVertexCount;
        //1. reset
        //_triangleListType = Tesselator.TriangleListType.LineLoop;//?
        _tempVertexList.Clear();
        _resultIndexList.Clear();
    }
}

public class TessTool
{
    private readonly Tesselator _tess;
    private readonly TessListener _tessListener;

    public TessTool() : this(new Tesselator { WindingRule = Tesselator.WindingRuleType.NonZero })
    {
    }

    public TessTool(Tesselator tess)
    {
        _tess = tess;
        _tessListener = new TessListener();
        _tessListener.Connect(tess, true);
    }

    public Tesselator.WindingRuleType WindingRuleType
    {
        get => _tess.WindingRule;
        set => _tess.WindingRule = value;
    }

    internal List<ushort> TessIndexList => _tessListener._resultIndexList;
    internal List<TessVertex2d> TempVertexList => _tessListener._tempVertexList;

    public bool TessPolygon(float[] vertex2dCoords, int[] contourEndPoints)
    {
        //internal tess the polygon

        var ncoords = vertex2dCoords.Length / 2;
        _tessListener.ResetAndLoadInputVertexList(ncoords);
        if (ncoords == 0) return false;
        //-----------------------
        //this support sub contour in the same array of  vertex2dCoords
        _tess.BeginPolygon();
        if (contourEndPoints == null)
        {
            //only 1 contour
            var beginAt = 0;
            var thisContourEndAt = vertex2dCoords.Length / 2;
            _tess.BeginContour();
            for (var i = beginAt; i < thisContourEndAt; ++i)
                _tess.AddVertex(
                    vertex2dCoords[i << 1], //*2
                    vertex2dCoords[(i << 1) + 1], i); //*2+1
            beginAt = thisContourEndAt + 1;
            _tess.EndContour();
        }
        else
        {
            //may have more than 1 contour
            var nContourCount = contourEndPoints.Length;
            var beginAt = 0;
            for (var m = 0; m < nContourCount; ++m)
            {
                var thisContourEndAt = (contourEndPoints[m] + 1) / 2;
                _tess.BeginContour();
                for (var i = beginAt; i < thisContourEndAt; ++i)
                    _tess.AddVertex(
                        vertex2dCoords[i << 1], //*2
                        vertex2dCoords[(i << 1) + 1], //*2+1 
                        i);
                beginAt = thisContourEndAt + 1;
                _tess.EndContour();
            }
        }

        _tess.EndPolygon();
        //-----------------------
        return true;
    }
}

public static class TessToolExtensions
{
    /// <summary>
    ///     tess and read result as triangle list vertex array (for GLES draw-array)
    /// </summary>
    /// <param name="tessTool"></param>
    /// <param name="vertex2dCoords"></param>
    /// <param name="contourEndPoints"></param>
    /// <param name="vertexCount"></param>
    /// <returns></returns>
    public static float[] TessAsTriVertexArray(this TessTool tessTool,
        float[] vertex2dCoords,
        int[] contourEndPoints,
        out int vertexCount)
    {
        if (!tessTool.TessPolygon(vertex2dCoords, contourEndPoints))
        {
            vertexCount = 0;
            return null;
        }

        //results
        //1.
        var indexList = tessTool.TessIndexList;
        //2.
        var tempVertexList = tessTool.TempVertexList;
        //3.
        vertexCount = indexList.Count;
        //-----------------------------    
        var orgVertexCount = vertex2dCoords.Length / 2;
        var vtx = new float[vertexCount * 2]; //***
        var n = 0;

        for (var p = 0; p < vertexCount; ++p)
        {
            var index = indexList[p];
            if (index >= orgVertexCount)
            {
                //extra coord (newly created)
                var extraVertex = tempVertexList[index - orgVertexCount];
                vtx[n] = (float)extraVertex.x;
                vtx[n + 1] = (float)extraVertex.y;
            }
            else
            {
                //original corrd
                vtx[n] = vertex2dCoords[index * 2];
                vtx[n + 1] = vertex2dCoords[index * 2 + 1];
            }

            n += 2;
        }

        //triangle list
        return vtx;
    }

    /// <summary>
    ///     tess and read result as triangle list index array (for GLES draw element)
    /// </summary>
    /// <param name="tessTool"></param>
    /// <param name="vertex2dCoords"></param>
    /// <param name="contourEndPoints"></param>
    /// <param name="outputCoords"></param>
    /// <param name="vertexCount"></param>
    /// <returns></returns>
    public static ushort[] TessAsTriIndexArray(this TessTool tessTool,
        float[] vertex2dCoords,
        int[] contourEndPoints,
        out float[] outputCoords,
        out int vertexCount)
    {
        if (!tessTool.TessPolygon(vertex2dCoords, contourEndPoints))
        {
            vertexCount = 0;
            outputCoords = null;
            return null; //* early exit
        }

        //results
        //1.
        var indexList = tessTool.TessIndexList;
        //2.
        var tempVertexList = tessTool.TempVertexList;
        //3.
        vertexCount = indexList.Count;
        //-----------------------------   

        //create a new array and append with original and new tempVertex list 
        var tempVertListCount = tempVertexList.Count;
        outputCoords = new float[vertex2dCoords.Length + tempVertListCount * 2];
        //1. copy original array
        Array.Copy(vertex2dCoords, outputCoords, vertex2dCoords.Length);
        //2. append with newly create vertex (from tempVertList)
        var endAt = vertex2dCoords.Length + tempVertListCount;
        var p = 0;
        var q = vertex2dCoords.Length; //start adding at
        for (var i = vertex2dCoords.Length; i < endAt; ++i)
        {
            var v = tempVertexList[p];
            outputCoords[q] = (float)v.x;
            outputCoords[q + 1] = (float)v.y;
            p++;
            q += 2;
        }

        return indexList.ToArray();
    }
}

public class Tesselator
{
    public enum TriangleListType
    {
        LineLoop,
        Triangles,
        TriangleStrip,
        TriangleFan
    }

    public enum WindingRuleType
    {
        //see: https://www.glprogramming.com/red/chapter11.html
        //http://what-when-how.com/opengl-programming-guide/polygon-tessellation-tessellators-and-quadrics-opengl-programming-part-2/

        Odd,
        NonZero,
        Positive,
        Negative,
        ABS_GEQ_Two
    }

    // We cache vertex data for single-contour polygons so that we can
    // try a quick-and-dirty decomposition first.
    private const int MAX_CACHE_SIZE = 100;
    internal const double MAX_COORD = 1.0e150;


    /************************ Quick-and-dirty decomposition ******************/

    private const int SIGN_INCONSISTENT = 2;

    private readonly int[] _indexCached = new int[MAX_CACHE_SIZE];
    private readonly TessVertex2d[] _simpleVertexCache = new TessVertex2d[MAX_CACHE_SIZE]; /* the vertex data */

    /*** state needed for rendering callbacks (see render.c) ***/
    private int _cacheCount; /* number of cached vertices */
    private bool _doMeshCallback;

    internal Dictionary _edgeDictionary; /* edge dictionary for sweep line */
    /* list of triangles which could not be rendered as strips or fans */

    //public delegate void CallBeginDelegate(TriangleListType type);
    //public CallBeginDelegate callBegin;
    //public delegate void CallEdgeFlagDelegate(bool boundaryEdge);
    //public CallEdgeFlagDelegate callEdgeFlag;
    //public delegate void CallVertexDelegate(int data);
    //public CallVertexDelegate callVertex;
    //public delegate void CallEndDelegate();
    //public CallEndDelegate callEnd;
    //public delegate void CallMeshDelegate(Mesh mesh);
    //public CallMeshDelegate callMesh;

    ////----------------
    //public delegate void CallCombineDelegate(
    //   double c1, double c2, double c3, ref CombineParameters combinePars, out int outData);
    //public CallCombineDelegate callCombine; 
    //----------------


    //
    /*** state needed to cache single-contour polygons for renderCache() */

    private bool _emptyCache; /* empty cache on next vertex() call */
    private HalfEdge _lastHalfEdge; /* lastEdge.Org is the most recent vertex */

    private Face _lonelyTriList;

    //
    internal Mesh _mesh; /* stores the input contours, and eventually the tessellation itself */
    private ProcessingState _processingState; /* what begin/end calls have we seen? */


    //----------------
    private ITessListener _tessListener;
    internal MaxFirstList<ContourVertex> _vertexPriorityQue = new();


    internal ContourVertex currentSweepVertex; /* current sweep event being processed */

    //
    public Tesselator()
    {
        /* Only initialize fields which can be changed by the api.  Other fields
         * are initialized where they are used.
         */
        _processingState = ProcessingState.Dormant;
        WindingRule = WindingRuleType.NonZero; //default
        BoundaryOnly = false;
    }

    private bool EdgeCallBackSet { get; set; }

    public WindingRuleType WindingRule { get; set; }

    public bool BoundaryOnly { get; set; }

    ~Tesselator()
    {
        //TODO: review here...
        RequireState(ProcessingState.Dormant);
    }

    public void SetListener(ITessListener listener)
    {
        _tessListener = listener;
        EdgeCallBackSet = listener.NeedEdgeFlag;
        _doMeshCallback = listener.NeedMash;
    }

    public bool IsWindingInside(int numCrossings)
    {
        switch (WindingRule)
        {
            case WindingRuleType.Odd:
                return (numCrossings & 1) != 0;
            case WindingRuleType.NonZero:
                return numCrossings != 0;
            case WindingRuleType.Positive:
                return numCrossings > 0;
            case WindingRuleType.Negative:
                return numCrossings < 0;
            case WindingRuleType.ABS_GEQ_Two:
                return numCrossings >= 2 || numCrossings <= -2;
        }

        throw new Exception();
    }

    private void CallBegin(TriangleListType triangleType)
    {
        _tessListener.Begin(triangleType);
        //callBegin?.Invoke(triangleType);
    }

    private void CallVertex(int vertexData)
    {
        _tessListener.Vertext(vertexData);
        //callVertex?.Invoke(vertexData);
    }

    private void CallEdgeFlag(bool edgeState)
    {
        _tessListener.EdgeFlag(edgeState);
        //callEdgeFlag?.Invoke(edgeState);
    }

    private void CallEnd()
    {
        _tessListener.End();
    }

    internal void CallCombine(double v0,
        double v1, double v2,
        ref CombineParameters combinePars,
        out int outData)
    {
        outData = 0;
        _tessListener.Combine(v0, v1, v2, ref combinePars, out outData);
    }

    private void GotoState(ProcessingState newProcessingState)
    {
        while (_processingState != newProcessingState)
            /* We change the current state one level at a time, to get to
             * the desired state.
             */
            if (_processingState < newProcessingState)
                switch (_processingState)
                {
                    case ProcessingState.Dormant:
                        throw new Exception("MISSING_BEGIN_POLYGON");
                    case ProcessingState.InPolygon:
                        throw new Exception("MISSING_BEGIN_CONTOUR");
                }
            else
                switch (_processingState)
                {
                    case ProcessingState.InContour:
                        throw new Exception("MISSING_END_CONTOUR");
                    case ProcessingState.InPolygon:
                        throw new Exception("MISSING_END_POLYGON");
                }
    }

    private void RequireState(ProcessingState state)
    {
        if (_processingState != state) GotoState(state);
    }

    public virtual void BeginPolygon()
    {
        RequireState(ProcessingState.Dormant);
        _processingState = ProcessingState.InPolygon;
        _cacheCount = 0;
        _emptyCache = false;
        _mesh = null;
    }

    public void BeginContour()
    {
        RequireState(ProcessingState.InPolygon);
        _processingState = ProcessingState.InContour;
        _lastHalfEdge = null;
        if (_cacheCount > 0)
            // Just set a flag so we don't get confused by empty contours
            _emptyCache = true;
    }

    private bool InnerAddVertex(double x, double y, int data)
    {
        HalfEdge e;
        e = _lastHalfEdge;
        if (e == null)
        {
            /* Make a self-loop (one vertex, one edge). */
            e = _mesh.MakeEdge();
            Mesh.meshSplice(e, e._otherHalfOfThisEdge);
        }
        else
        {
            /* Create a new vertex and edge which immediately follow e
             * in the ordering around the left face.
             */
            if (Mesh.meshSplitEdge(e) == null) return false;
            e = e._nextEdgeCCWAroundLeftFace;
        }

        /* The new vertex is now e.Org. */
        e._originVertex._clientIndex = data;
        e._originVertex._C_0 = x;
        e._originVertex._C_1 = y;
        /* The winding of an edge says how the winding number changes as we
         * cross from the edge''s right face to its left face.  We add the
         * vertices in such an order that a CCW contour will add +1 to
         * the winding number of the region inside the contour.
         */
        e._winding = 1;
        e._otherHalfOfThisEdge._winding = -1;
        _lastHalfEdge = e;
        return true;
    }

    private void EmptyCache()
    {
        var vCaches = _simpleVertexCache;
        var index_caches = _indexCached;
        _mesh = new Mesh();
        var count = _cacheCount;
        for (var i = 0; i < count; i++)
        {
            var v = vCaches[i];
            InnerAddVertex(v.x, v.y, index_caches[i]);
        }

        _cacheCount = 0;
        _emptyCache = false;
    }

    private void CacheVertex(double x, double y, double z, int data)
    {
        _simpleVertexCache[_cacheCount] = new TessVertex2d(x, y);
        _indexCached[_cacheCount] = data;
        ++_cacheCount;
    }

    private void CacheVertex(double x, double y, int data)
    {
        _simpleVertexCache[_cacheCount] = new TessVertex2d(x, y);
        _indexCached[_cacheCount] = data;
        ++_cacheCount;
    }

    public void AddVertex(double x, double y, int data)
    {
        RequireState(ProcessingState.InContour);

        if (_emptyCache)
        {
            EmptyCache();
            _lastHalfEdge = null;
        }

        //....
        if (x < -MAX_COORD || x > MAX_COORD ||
            y < -MAX_COORD || y > MAX_COORD)
            throw new Exception("Your coordinate exceeded -" + MAX_COORD + ".");
        //....
        //
        if (_mesh == null)
        {
            if (_cacheCount < MAX_CACHE_SIZE)
            {
                CacheVertex(x, y, data);
                return;
            }

            EmptyCache();
        }

        InnerAddVertex(x, y, data);
    }

    public void AddVertex(double x, double y, double z, int data)
    {
        RequireState(ProcessingState.InContour);

        if (_emptyCache)
        {
            EmptyCache();
            _lastHalfEdge = null;
        }

        //....
        if (x < -MAX_COORD || x > MAX_COORD ||
            y < -MAX_COORD || y > MAX_COORD ||
            z < -MAX_COORD || z > MAX_COORD)
            throw new Exception("Your coordinate exceeded -" + MAX_COORD + ".");
        //....
        //
        if (_mesh == null)
        {
            if (_cacheCount < MAX_CACHE_SIZE)
            {
                CacheVertex(x, y, data);
                return;
            }

            EmptyCache();
        }

        InnerAddVertex(x, y, data);
    }

    public void EndContour()
    {
        RequireState(ProcessingState.InContour);
        _processingState = ProcessingState.InPolygon;
    }

    private void CheckOrientation()
    {
        double area = 0;
        Face curFace, faceHead = _mesh._faceHead;
        var vHead = _mesh._vertexHead;
        HalfEdge curHalfEdge;
        /* When we compute the normal automatically, we choose the orientation
         * so that the sum of the signed areas of all contours is non-negative.
         */
        for (curFace = faceHead._nextFace; curFace != faceHead; curFace = curFace._nextFace)
        {
            curHalfEdge = curFace._halfEdgeThisIsLeftFaceOf;
            if (curHalfEdge._winding <= 0) continue;

            do
            {
                area += (curHalfEdge._originVertex.x - curHalfEdge.DirectionVertex.x)
                        * (curHalfEdge._originVertex.y + curHalfEdge.DirectionVertex.y);
                curHalfEdge = curHalfEdge._nextEdgeCCWAroundLeftFace;
            } while (curHalfEdge != curFace._halfEdgeThisIsLeftFaceOf);
        }

        if (area < 0)
            /* Reverse the orientation by flipping all the t-coordinates */
            for (var curVertex = vHead._nextVertex; curVertex != vHead; curVertex = curVertex._nextVertex)
                curVertex.y = -curVertex.y;
    }

    private void ProjectPolygon()
    {
        ContourVertex v, vHead = _mesh._vertexHead;
        // Project the vertices onto the sweep plane
        for (v = vHead._nextVertex; v != vHead; v = v._nextVertex)
        {
            v.x = v._C_0;
            v.y = -v._C_1;
        }

        CheckOrientation();
    }

    public void EndPolygon()
    {
        RequireState(ProcessingState.InPolygon);
        _processingState = ProcessingState.Dormant;
        if (_mesh == null)
        {
            if (!EdgeCallBackSet && !_doMeshCallback)
                /* Try some special code to make the easy cases go quickly
                 * (eg. convex polygons).  This code does NOT handle multiple contours,
                 * intersections, edge flags, and of course it does not generate
                 * an explicit mesh either.
                 */
                if (RenderCache())
                    return;

            EmptyCache(); /* could've used a label*/
        }

        /* Determine the polygon normal and project vertices onto the plane
         * of the polygon.
         */
        ProjectPolygon();
        /* __gl_computeInterior( this ) computes the planar arrangement specified
         * by the given contours, and further subdivides this arrangement
         * into regions.  Each region is marked "inside" if it belongs
         * to the polygon, according to the rule given by this.windingRule.
         * Each interior region is guaranteed to be monotone.
         */
        ActiveRegion.ComputeInterior(this);
        var rc = true;
        /* If the user wants only the boundary contours, we throw away all edges
         * except those which separate the interior from the exterior.
         * Otherwise we tessellate all the regions marked "inside".
         */
        if (BoundaryOnly)
            rc = _mesh.SetWindingNumber(1, true);
        else
            rc = _mesh.TessellateInterior();

        _mesh.CheckMesh();

        //if (this.callBegin != null || this.callEnd != null
        //    || this.callVertex != null || this.callEdgeFlag != null)
        //{
        if (BoundaryOnly)
            RenderBoundary(_mesh); /* output boundary contours */
        else
            RenderMesh(_mesh); /* output strips and fans */
        //}

        if (_doMeshCallback)
        {
            /* Throw away the exterior faces, so that all faces are interior.
             * This way the user doesn't have to check the "inside" flag,
             * and we don't need to even reveal its existence.  It also leaves
             * the freedom for an implementation to not generate the exterior
             * faces in the first place.
             */
            _mesh.DiscardExterior();
            _tessListener.Mesh(_mesh); /* user wants the mesh itself */
            //callMesh(mesh); /* user wants the mesh itself */
            _mesh = null;
            return;
        }

        _mesh = null;
    }

    /************************ Strips and Fans decomposition ******************/

    /* __gl_renderMesh( tess, mesh ) takes a mesh and breaks it into triangle
     * fans, strips, and separate triangles.  A substantial effort is made
     * to use as few rendering primitives as possible (ie. to make the fans
     * and strips as large as possible).
     *
     * The rendering output is provided as callbacks (see the api).
     */
    private void RenderMesh(Mesh mesh)
    {
        Face f;
        /* Make a list of separate triangles so we can render them all at once */
        _lonelyTriList = null;
        for (f = mesh._faceHead._nextFace; f != mesh._faceHead; f = f._nextFace) f._marked = false;
        for (f = mesh._faceHead._nextFace; f != mesh._faceHead; f = f._nextFace)
            /* We examine all faces in an arbitrary order.  Whenever we find
             * an unprocessed face F, we output a group of faces including F
             * whose size is maximum.
             */
            if (f._isInterior && !f._marked)
            {
                RenderMaximumFaceGroup(f);
                if (!f._marked) throw new Exception();
            }

        if (_lonelyTriList != null)
        {
            RenderLonelyTriangles(_lonelyTriList);
            _lonelyTriList = null;
        }
    }


    private void RenderMaximumFaceGroup(Face fOrig)
    {
        /* We want to find the largest triangle fan or strip of unmarked faces
         * which includes the given face fOrig.  There are 3 possible fans
         * passing through fOrig (one centered at each vertex), and 3 possible
         * strips (one for each CCW permutation of the vertices).  Our strategy
         * is to try all of these, and take the primitive which uses the most
         * triangles (a greedy approach).
         */
        var e = fOrig._halfEdgeThisIsLeftFaceOf;
        var max = new FaceCount(1, e, RenderTriangle);
        FaceCount newFace;
        max.size = 1;
        max.eStart = e;
        if (!EdgeCallBackSet)
        {
            newFace = MaximumFan(e);
            if (newFace.size > max.size) max = newFace;
            newFace = MaximumFan(e._nextEdgeCCWAroundLeftFace);
            if (newFace.size > max.size) max = newFace;
            newFace = MaximumFan(e.Lprev);
            if (newFace.size > max.size) max = newFace;

            newFace = MaximumStrip(e);
            if (newFace.size > max.size) max = newFace;
            newFace = MaximumStrip(e._nextEdgeCCWAroundLeftFace);
            if (newFace.size > max.size) max = newFace;
            newFace = MaximumStrip(e.Lprev);
            if (newFace.size > max.size) max = newFace;
        }

        max.CallRender(this, max.eStart, max.size);
    }

    private FaceCount MaximumFan(HalfEdge eOrig)
    {
        /* eOrig.Lface is the face we want to render.  We want to find the size
         * of a maximal fan around eOrig.Org.  To do this we just walk around
         * the origin vertex as far as possible in both directions.
         */
        var newFace = new FaceCount(0, null, RenderFan);
        Face trail = null;
        HalfEdge e;
        for (e = eOrig; !e._leftFace.Marked(); e = e._nextEdgeCCWAroundOrigin)
        {
            Face.AddToTrail(ref e._leftFace, ref trail);
            ++newFace.size;
        }

        for (e = eOrig; !e.rightFace.Marked(); e = e.Oprev)
        {
            var f = e.rightFace;
            Face.AddToTrail(ref f, ref trail);
            e.rightFace = f;
            ++newFace.size;
        }

        newFace.eStart = e;
        Face.FreeTrail(ref trail);
        return newFace;
    }


    private static bool IsEven(int n)
    {
        return (n & 1) == 0;
    }

    private FaceCount MaximumStrip(HalfEdge eOrig)
    {
        /* Here we are looking for a maximal strip that contains the vertices
         * eOrig.Org, eOrig.Dst, eOrig.Lnext.Dst (in that order or the
         * reverse, such that all triangles are oriented CCW).
         *
         * Again we walk forward and backward as far as possible.  However for
         * strips there is a twist: to get CCW orientations, there must be
         * an *even* number of triangles in the strip on one side of eOrig.
         * We walk the strip starting on a side with an even number of triangles;
         * if both side have an odd number, we are forced to shorten one side.
         */
        var newFace = new FaceCount(0, null, RenderStrip);
        int headSize = 0, tailSize = 0;
        Face trail = null;
        HalfEdge e, eTail, eHead;
        for (e = eOrig; !e._leftFace.Marked(); ++tailSize, e = e._nextEdgeCCWAroundOrigin)
        {
            Face.AddToTrail(ref e._leftFace, ref trail);
            ++tailSize;
            e = e.Dprev;
            if (e._leftFace.Marked()) break;
            Face.AddToTrail(ref e._leftFace, ref trail);
        }

        eTail = e;
        for (e = eOrig; !e.rightFace.Marked(); ++headSize, e = e.Dnext)
        {
            var f = e.rightFace;
            Face.AddToTrail(ref f, ref trail);
            e.rightFace = f;
            ++headSize;
            e = e.Oprev;
            if (e.rightFace.Marked()) break;
            f = e.rightFace;
            Face.AddToTrail(ref f, ref trail);
            e.rightFace = f;
        }

        eHead = e;
        newFace.size = tailSize + headSize;
        if (IsEven(tailSize))
        {
            newFace.eStart = eTail._otherHalfOfThisEdge;
        }
        else if (IsEven(headSize))
        {
            newFace.eStart = eHead;
        }
        else
        {
            /* Both sides have odd length, we must shorten one of them.  In fact,
             * we must start from eHead to guarantee inclusion of eOrig.Lface.
             */
            --newFace.size;
            newFace.eStart = eHead._nextEdgeCCWAroundOrigin;
        }

        Face.FreeTrail(ref trail);
        return newFace;
    }


    private void RenderTriangle(Tesselator tess, HalfEdge e, int size)
    {
        /* Just add the triangle to a triangle list, so we can render all
         * the separate triangles at once.
         */
        if (size != 1) throw new Exception();
        Face.AddToTrail(ref e._leftFace, ref _lonelyTriList);
    }


    private void RenderLonelyTriangles(Face f)
    {
        /* Now we render all the separate triangles which could not be
         * grouped into a triangle fan or strip.
         */
        HalfEdge e;
        var newState = false;
        var edgeState = false; /* force edge state output for first vertex */
        var sentFirstEdge = false;
        CallBegin(TriangleListType.Triangles);
        for (; f != null; f = f._trail)
        {
            /* Loop once for each edge (there will always be 3 edges) */

            e = f._halfEdgeThisIsLeftFaceOf;
            do
            {
                if (EdgeCallBackSet)
                {
                    /* Set the "edge state" to TRUE just before we output the
                     * first vertex of each edge on the polygon boundary.
                     */
                    newState = !e.rightFace._isInterior;
                    if (edgeState != newState || !sentFirstEdge)
                    {
                        sentFirstEdge = true;
                        edgeState = newState;
                        CallEdgeFlag(edgeState);
                    }
                }

                CallVertex(e._originVertex._clientIndex);
                e = e._nextEdgeCCWAroundLeftFace;
            } while (e != f._halfEdgeThisIsLeftFaceOf);
        }

        CallEnd();
    }


    private static void RenderFan(Tesselator tess, HalfEdge e, int size)
    {
        /* Render as many CCW triangles as possible in a fan starting from
         * edge "e".  The fan *should* contain exactly "size" triangles
         * (otherwise we've goofed up somewhere).
         */
        tess.CallBegin(TriangleListType.TriangleFan);
        tess.CallVertex(e._originVertex._clientIndex);
        tess.CallVertex(e.DirectionVertex._clientIndex);
        while (!e._leftFace.Marked())
        {
            e._leftFace._marked = true;
            --size;
            e = e._nextEdgeCCWAroundOrigin;
            tess.CallVertex(e.DirectionVertex._clientIndex);
        }

        if (size != 0) throw new Exception();
        tess.CallEnd();
    }


    private static void RenderStrip(Tesselator tess, HalfEdge halfEdge, int size)
    {
        /* Render as many CCW triangles as possible in a strip starting from
         * edge "e".  The strip *should* contain exactly "size" triangles
         * (otherwise we've goofed up somewhere).
         */
        tess.CallBegin(TriangleListType.TriangleStrip);
        tess.CallVertex(halfEdge._originVertex._clientIndex);
        tess.CallVertex(halfEdge.DirectionVertex._clientIndex);
        while (!halfEdge._leftFace.Marked())
        {
            halfEdge._leftFace._marked = true;
            --size;
            halfEdge = halfEdge.Dprev;
            tess.CallVertex(halfEdge._originVertex._clientIndex);
            if (halfEdge._leftFace.Marked()) break;
            halfEdge._leftFace._marked = true;
            --size;
            halfEdge = halfEdge._nextEdgeCCWAroundOrigin;
            tess.CallVertex(halfEdge.DirectionVertex._clientIndex);
        }

        if (size != 0) throw new Exception();
        tess.CallEnd();
    }


    /************************ Boundary contour decomposition ******************/

    /* Takes a mesh, and outputs one
     * contour for each face marked "inside".  The rendering output is
     * provided as callbacks.
     */
    private void RenderBoundary(Mesh mesh)
    {
        for (var curFace = mesh._faceHead._nextFace; curFace != mesh._faceHead; curFace = curFace._nextFace)
            if (curFace._isInterior)
            {
                CallBegin(TriangleListType.LineLoop);
                var curHalfEdge = curFace._halfEdgeThisIsLeftFaceOf;
                do
                {
                    CallVertex(curHalfEdge._originVertex._clientIndex);
                    curHalfEdge = curHalfEdge._nextEdgeCCWAroundLeftFace;
                } while (curHalfEdge != curFace._halfEdgeThisIsLeftFaceOf);

                CallEnd();
            }
    }

    private int ComputeNormal(ref double nx, ref double ny, ref double nz)
        /*
         * Check that each triangle in the fan from v0 has a
         * consistent orientation with respect to norm3[].  If triangles are
         * consistently oriented CCW, return 1; if CW, return -1; if all triangles
         * are degenerate return 0; otherwise (no consistent orientation) return
         * SIGN_INCONSISTENT.
         */
    {
        var vCache = _simpleVertexCache;
        var v0 = vCache[0];
        int vcIndex;
        double dot, xc, yc, xp, yp;
        double n0;
        double n1;
        double n2;
        var sign = 0;
        /* Find the polygon normal.  It is important to get a reasonable
         * normal even when the polygon is self-intersecting (eg. a bowtie).
         * Otherwise, the computed normal could be very tiny, but perpendicular
         * to the true plane of the polygon due to numerical noise.  Then all
         * the triangles would appear to be degenerate and we would incorrectly
         * decompose the polygon as a fan (or simply not render it at all).
         *
         * We use a sum-of-triangles normal algorithm rather than the more
         * efficient sum-of-trapezoids method (used in CheckOrientation()
         * in normal.c).  This lets us explicitly reverse the signed area
         * of some triangles to get a reasonable normal in the self-intersecting
         * case.
         */
        vcIndex = 1;
        var v = vCache[vcIndex];
        xc = v.x - v0.x;
        yc = v.y - v0.y;
        var c_count = _cacheCount;
        while (++vcIndex < c_count)
        {
            xp = xc;
            yp = yc;
            v = vCache[vcIndex];
            xc = v.x - v0.x;
            yc = v.y - v0.y;
            /* Compute (vp - v0) cross (vc - v0) */
            n0 = 0;
            n1 = 0;
            n2 = xp * yc - yp * xc;
            dot = n0 * nx + n1 * ny + n2 * nz;
            if (dot != 0)
            {
                /* Check the new orientation for consistency with previous triangles */
                if (dot > 0)
                {
                    if (sign < 0) return SIGN_INCONSISTENT;
                    sign = 1;
                }
                else
                {
                    if (sign > 0) return SIGN_INCONSISTENT;
                    sign = -1;
                }
            }
        }

        return sign;
    }

    /* Takes a single contour and tries to render it
     * as a triangle fan.  This handles convex polygons, as well as some
     * non-convex polygons if we get lucky.
     *
     * Returns TRUE if the polygon was successfully rendered.  The rendering
     * output is provided as callbacks (see the api).
     */
    private bool RenderCache()
    {
        int sign;
        if (_cacheCount < 3)
            /* Degenerate contour -- no output */
            return true;
        double normal_x = 0;
        double normal_y = 0;
        double normal_z = 1;
        sign = ComputeNormal(ref normal_x, ref normal_y, ref normal_z);
        if (sign == SIGN_INCONSISTENT)
            // Fan triangles did not have a consistent orientation
            return false;
        if (sign == 0)
            // All triangles were degenerate
            return true;

        /* Make sure we do the right thing for each winding rule */
        switch (WindingRule)
        {
            case WindingRuleType.Odd:
            case WindingRuleType.NonZero:
                break;
            case WindingRuleType.Positive:
                if (sign < 0) return true;
                break;
            case WindingRuleType.Negative:
                if (sign > 0) return true;
                break;
            case WindingRuleType.ABS_GEQ_Two:
                return true;
        }

        CallBegin(BoundaryOnly ? TriangleListType.LineLoop
            : _cacheCount > 3 ? TriangleListType.TriangleFan
            : TriangleListType.Triangles);
        CallVertex(_indexCached[0]);
        if (sign > 0)
        {
            var c_count = _cacheCount;
            for (var vcIndex = 1; vcIndex < c_count; ++vcIndex) CallVertex(_indexCached[vcIndex]);
        }
        else
        {
            for (var vcIndex = _cacheCount - 1; vcIndex > 0; --vcIndex) CallVertex(_indexCached[vcIndex]);
        }

        CallEnd();
        return true;
    }

    // The begin/end calls must be properly nested.  We keep track of
    // the current state to enforce the ordering.
    private enum ProcessingState
    {
        Dormant,
        InPolygon,
        InContour
    }


    public struct CombineParameters
    {
        public int d0, d1, d2, d3;
        public double w0, w1, w2, w3;
    }

    public interface ITessListener
    {
        bool NeedEdgeFlag { get; }
        bool NeedMash { get; }
        void BeginRead();

        /*** state needed for rendering callbacks (see render.c) ***/
        void Combine(double c1, double c2, double c3, ref CombineParameters combinePars, out int outData);
        void Begin(TriangleListType type);
        void Vertext(int data);
        void End();

        //
        void EdgeFlag(bool boundaryEdge);

        //
        void Mesh(Mesh mesh);
    }

    private class FaceCount
    {
        public delegate void RenderDelegate(Tesselator tess, HalfEdge edge, int data);

        public HalfEdge eStart; /* edge where this primitive starts */

        public int size; /* number of triangles used */

        public FaceCount(int _size, HalfEdge _eStart, RenderDelegate _render)
        {
            size = _size;
            eStart = _eStart;
            render = _render;
        }

        private event RenderDelegate render;
        // routine to render this primitive

        public void CallRender(Tesselator tess, HalfEdge edge, int data)
        {
            render(tess, edge, data);
        }
    }
}