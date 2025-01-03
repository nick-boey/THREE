using System.Collections;
using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public class GLObjects
{
    public GLAttributes Attributes;

    public GLGeometries Geometries;

    private GLInfo info;
    public Hashtable UpdateList = new();

    public GLObjects(GLGeometries geometries, GLAttributes attributes, GLInfo info)
    {
        Geometries = geometries;
        Attributes = attributes;
        this.info = info;
    }

    public BufferGeometry Update(Object3D object3D)
    {
        var frame = info.render.Frame;

        var geometry = object3D.Geometry;

        var bufferGeometry = Geometries.Get(object3D, geometry);

        // Update once per frame

        if (!UpdateList.ContainsKey(bufferGeometry.Id) || (int)UpdateList[bufferGeometry.Id] != frame)
        {
            if (!(geometry is BufferGeometry)) (bufferGeometry as BufferGeometry).UpdateFromObject(object3D);

            Geometries.Update(bufferGeometry);

            if (!UpdateList.ContainsKey(bufferGeometry.Id))
                UpdateList.Add(bufferGeometry.Id, frame);
            else
                UpdateList[bufferGeometry.Id] = frame;
        }

        //bool objectExists = UpdateList.ContainsKey(bufferGeometry.Id) ? true : false;

        //if (!objectExists)
        //{
        //    if (geometry is BufferGeometry)
        //    {
        //        (bufferGeometry as BufferGeometry).UpdateFromObject(object3D);
        //    }

        //    Geometries.Update(bufferGeometry);

        //    UpdateList.Add(bufferGeometry.Id, frame);
        //}

        //if ((int)UpdateList[bufferGeometry.Id] != frame)
        //{
        //    if (geometry is BufferGeometry)
        //    {
        //        (bufferGeometry as BufferGeometry).UpdateFromObject(object3D);
        //    }

        //    Geometries.Update(bufferGeometry);

        //    UpdateList[bufferGeometry.Id] = frame;
        //}

        if (object3D is InstancedMesh)
        {
            Attributes.Update<float>((object3D as InstancedMesh).InstanceMatrix, BufferTarget.ArrayBuffer);

            if ((object3D as InstancedMesh).InstanceColor != null)
                Attributes.Update<float>((object3D as InstancedMesh).InstanceColor, BufferTarget.ArrayBuffer);
        }

        return bufferGeometry as BufferGeometry;
    }
}