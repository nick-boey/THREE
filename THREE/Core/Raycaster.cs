using System.Collections;

namespace THREE;

[Serializable]
public class Intersection
{
    public float Distance;
    public float DistanceToRay;
    public Face3 Face;
    public int FaceIndex;
    public int Index;
    public int InstanceId;
    public Object3D? Object3D;
    public Vector3 Point;
    public Vector3 PointOnLine;
    public Vector2 Uv;
    public Vector2 Uv2;
}

[Serializable]
public class RaycasterParameters
{
    public Hashtable Line = new();
    public Hashtable Line2 = new();
    public Hashtable LOD = new();
    public Hashtable Mesh = new();
    public Hashtable Points = new();
    public Hashtable Sprite = new();
}

[Serializable]
public class Raycaster
{
    public Camera camera;
    public float far = float.PositiveInfinity;
    public Layers layers;
    public float near;
    public RaycasterParameters parameters = new();
    public Ray ray;

    public Raycaster(Vector3 origin = null, Vector3 direction = null, float? near = null, float? far = null)
    {
        ray = new Ray(origin, direction);
        this.near = near != null ? near.Value : 0;
        this.far = far != null ? far.Value : float.PositiveInfinity;
        camera = null;
        layers = new Layers();
    }

    public Hashtable PointCloud => parameters.Points;

    /**
     * Updates the ray with a new origin and direction.
     * @param origin The origin vector where the ray casts from.
     * @param direction The normalized direction vector that gives direction to the ray.
     */
    public void Set(Vector3 origin, Vector3 direction)
    {
        ray.Set(origin, direction);
    }

    /**
     * Updates the ray with a new origin and direction.
     * @param coords 2D coordinates of the mouse, in normalized device coordinates (NDC)---X and Y components should be between -1 and 1.
     * @param camera camera from which the ray should originate
     */
    public void SetFromCamera(Vector2 coords, Camera camera)
    {
        if (camera != null && camera is PerspectiveCamera)
        {
            ray.origin.SetFromMatrixPosition(camera.MatrixWorld);
            ray.direction.Set(coords.X, coords.Y, 0.5f).UnProject(camera).Sub(ray.origin).Normalize();
            this.camera = camera;
        }
        else if (camera != null && camera is OrthographicCamera)
        {
            ray.origin.Set(coords.X, coords.Y, (camera.Near + camera.Far) / (camera.Near - camera.Far))
                .UnProject(camera); // set origin in plane of camera
            ray.direction.Set(0, 0, -1).TransformDirection(camera.MatrixWorld);
            this.camera = camera;
        }
        else
        {
            throw new SystemException("THREE.Raycaster: Unsupported camera type.");
        }
    }

    private void IntersectObject(Object3D object3D, Raycaster raycaster, List<Intersection> intersects,
        bool recursive = false)
    {
        if (object3D.Layers.Test(raycaster.layers)) object3D.Raycast(raycaster, intersects);

        if (recursive)
        {
            var children = object3D.Children;

            for (var i = 0; i < children.Count; i++) IntersectObject(children[i], raycaster, intersects, true);
        }
    }

    /**
     * Checks all intersection between the ray and the object with or without the descendants. Intersections are returned sorted by distance, closest first.
     * @param object The object to check for intersection with the ray.
     * @param recursive If true, it also checks all descendants. Otherwise it only checks intersecton with the object. Default is false.
     * @param optionalTarget (optional) target to set the result. Otherwise a new Array is instantiated. If set, you must clear this array prior to each call (i.e., array.length = 0;).
     */
    public List<Intersection> IntersectObject(Object3D object3D, bool? recursive = null,
        List<Intersection> optionalTarget = null)
    {
        var intersects = optionalTarget != null ? optionalTarget : new List<Intersection>();

        IntersectObject(object3D, this, intersects, recursive != null ? recursive.Value : false);
        //Sort(delegate (RenderItem a, RenderItem b)

        intersects.Sort(delegate(Intersection a, Intersection b) { return (int)(a.Distance - b.Distance); });

        return intersects;
    }


    /**
     * Checks all intersection between the ray and the objects with or without the descendants. Intersections are returned sorted by distance, closest first. Intersections are of the same form as those returned by .intersectObject.
     * @param objects The objects to check for intersection with the ray.
     * @param recursive If true, it also checks all descendants of the objects. Otherwise it only checks intersecton with the objects. Default is false.
     * @param optionalTarget (optional) target to set the result. Otherwise a new Array is instantiated. If set, you must clear this array prior to each call (i.e., array.length = 0;).
     */
    public List<Intersection> IntersectObjects(List<Object3D> objects, bool? recursive = null,
        List<Intersection> optionalTarget = null)
    {
        var intersects = optionalTarget != null ? optionalTarget : new List<Intersection>();

        for (var i = 0; i < objects.Count; i++)
            IntersectObject(objects[i], this, intersects, recursive != null ? recursive.Value : false);

        intersects.Sort(delegate(Intersection a, Intersection b) { return (int)(a.Distance - b.Distance); });

        return intersects;
    }
}