﻿using System.Collections;
using OpenTK.Graphics.ES30;

namespace THREE;

[Serializable]
public struct BindingStateStruct
{
    public Guid uuid;
    public int? geometry;
    public int? program;
    public bool wireframe;
    public List<int> newAttributes;
    public List<int> enabledAttributes;
    public List<int> attributeDivisors;
    public int? vao;
    public Hashtable attributes;
    public BufferAttribute<int> index;
    public int AttributesNum;

    public bool Equals(BindingStateStruct other)
    {
        return uuid.Equals(other.uuid);
    }
}

[Serializable]
public class GLBindingStates : IDisposable
{
    private GLAttributes attributes;

    private Hashtable bindingStates = new();

    private GLCapabilities capabilities;

    private IGraphicsContext Context;

    public BindingStateStruct currentState;

    public BindingStateStruct defaultState;
    private bool disposed;

    private GLExtensions extensions;

    private int maxVertexAttributes;

    private bool vaoAvailable;

    public GLBindingStates(IGraphicsContext context, GLExtensions extensions, GLAttributes attributes,
        GLCapabilities capabilities)
    {
        Context = context;
        this.extensions = extensions;
        this.attributes = attributes;
        this.capabilities = capabilities;

        GL.GetInteger(GetPName.MaxVertexAttribs, out maxVertexAttributes);

        vaoAvailable = capabilities.IsGL2;

        defaultState = createBindingState(null);
        currentState = defaultState;
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }

    public event EventHandler<EventArgs> Disposed;

    ~GLBindingStates()
    {
        if (Context.IsCurrent)
        {
            Reset();
            foreach (int geometryId in bindingStates.Keys)
            {
                var programMap = bindingStates[geometryId] as Hashtable;
                foreach (int programId in programMap.Keys)
                {
                    var stateMap = programMap[programId] as Hashtable;
                    foreach (bool wireframe in stateMap.Keys)
                    {
                        var bindingState = (BindingStateStruct)stateMap[wireframe];
                        deleteVertexArrayObject(bindingState.vao.Value);

                        stateMap.Remove(wireframe);
                    }

                    programMap.Remove(programId);
                }

                bindingStates.Remove(geometryId);
            }
        }

        Dispose(false);
    }

    private int createVertexArrayObject()
    {
        return GL.GenVertexArray();
    }

    private void bindVertexArrayObject(int vao)
    {
        GL.BindVertexArray(vao);
    }

    private void deleteVertexArrayObject(int vao)
    {
        GL.DeleteVertexArray(vao);
    }

    private BindingStateStruct getBindingState(Geometry geometry, GLProgram program, Material material)
    {
        var wireframe = material.Wireframe;

        Hashtable programMap;

        if (!bindingStates.ContainsKey(geometry.Id))
        {
            programMap = new Hashtable();
            bindingStates.Add(geometry.Id, new Hashtable());
        }

        programMap = bindingStates[geometry.Id] as Hashtable;

        Hashtable stateMap;

        if (!programMap.ContainsKey(program.Id))
        {
            stateMap = new Hashtable();
            programMap.Add(program.Id, stateMap);
        }

        stateMap = programMap[program.Id] as Hashtable;

        var state = stateMap[wireframe] as BindingStateStruct?;

        if (state == null)
        {
            state = createBindingState(createVertexArrayObject());
            stateMap[wireframe] = state;
        }

        return state.Value;
    }

    private BindingStateStruct createBindingState(int? vao)
    {
        var newAttributes = new List<int>();
        var enabledAttributes = new List<int>();
        var attributeDivisors = new List<int>();

        for (var i = 0; i < maxVertexAttributes; i++)
        {
            newAttributes.Add(0);
            enabledAttributes.Add(0);
            attributeDivisors.Add(0);
        }

        return new BindingStateStruct
        {
            uuid = Guid.NewGuid(), newAttributes = newAttributes, enabledAttributes = enabledAttributes,
            attributeDivisors = attributeDivisors, vao = vao, attributes = new Hashtable(), index = null
        };
    }

    private bool needsUpdate(Geometry geometry, BufferAttribute<int> index)
    {
        var cachedAttributes = currentState.attributes;
        var geometryAttributes = (geometry as BufferGeometry).Attributes;

        var attributesNum = 0;

        if (cachedAttributes.Count != geometryAttributes.Count) return true;

        foreach (var key in geometryAttributes)
        {
            var cachedAttribute = cachedAttributes[key.Key];
            var geometryAttribute = geometryAttributes[key.Key];

            if (cachedAttribute == null) return true;


            if ((cachedAttribute as Hashtable)["attribute"] != geometryAttribute) return true;

            if (geometryAttribute is InterleavedBufferAttribute<float>)
                if ((cachedAttribute as Hashtable)["data"] !=
                    (geometryAttribute as InterleavedBufferAttribute<float>).Data)
                    return true;
            attributesNum++;
        }

        if (currentState.AttributesNum != attributesNum) return true;
        if (currentState.index != index) return true;

        return false;
    }

    private void saveCache(Geometry geometry, BufferAttribute<int> index, GLProgram program, Material material)
    {
        var cache = new Hashtable();
        var attributesNum = 0;
        foreach (var items in (geometry as BufferGeometry).Attributes)
        {
            var attribute = items.Value; // as BufferAttribute<float>;
            var data = new Hashtable();
            data.Add("attribute", attribute);
            if (attribute is InterleavedBufferAttribute<float>)
                data.Add("data", (attribute as InterleavedBufferAttribute<float>).Data);
            else if (attribute is InterleavedBufferAttribute<int>)
                data.Add("data", (attribute as InterleavedBufferAttribute<int>).Data);
            else if (attribute is InterleavedBufferAttribute<byte>)
                data.Add("data", (attribute as InterleavedBufferAttribute<byte>).Data);
            cache.Add(items.Key, data);
            attributesNum++;
        }

        currentState.attributes = cache;
        currentState.AttributesNum = attributesNum;
        currentState.index = index;

        var programMap = bindingStates[geometry.Id] as Hashtable;
        var stateMap = programMap[program.Id] as Hashtable;
        var wireframe = material.Wireframe;
        stateMap[wireframe] = currentState;
    }

    public void InitAttributes()
    {
        var newAttributes = currentState.newAttributes;

        for (var i = 0; i < newAttributes.Count; i++) newAttributes[i] = 0;
    }

    public void Setup(Object3D object3D, Material material, GLProgram program, Geometry geometry,
        BufferAttribute<int> index)
    {
        var updateBuffers = false;

        if (vaoAvailable)
        {
            var state = getBindingState(geometry, program, material);

            if (!currentState.Equals(state))
            {
                currentState = state;
                bindVertexArrayObject(currentState.vao.Value);
            }

            updateBuffers = needsUpdate(geometry, index);

            if (updateBuffers) saveCache(geometry, index, program, material);
        }
        else
        {
            var wireframe = material.Wireframe;

            if (currentState.geometry != geometry.Id ||
                currentState.program != program.Id ||
                currentState.wireframe != wireframe)
            {
                currentState.geometry = geometry.Id;
                currentState.program = program.Id;
                currentState.wireframe = wireframe;

                updateBuffers = true;
            }
        }

        if (object3D is InstancedMesh) updateBuffers = true;

        if (index != null) attributes.Update<int>(index, BufferTarget.ElementArrayBuffer);

        if (updateBuffers)
        {
            setupVertexAttributes(object3D, material, program, geometry);

            if (index != null) GL.BindBuffer(BufferTarget.ElementArrayBuffer, attributes.Get<int>(index).buffer);
        }
    }

    public void Reset()
    {
        ResetDefaultState();

        if (currentState.Equals(defaultState)) return;


        currentState = defaultState;

        if (currentState.vao == null)
        {
            bindVertexArrayObject(0);
            return;
        }

        if (!currentState.vao.HasValue)
            bindVertexArrayObject(0);
        else
            bindVertexArrayObject(currentState.vao.Value);
    }

    public void ResetDefaultState()
    {
        defaultState.geometry = null;
        defaultState.program = null;
        defaultState.wireframe = false;
    }

    public void ReleaseStatesOfGeometry(Geometry geometry)
    {
        if (bindingStates[geometry.Id] == null) return;

        var programMap = bindingStates[geometry.Id] as Hashtable;

        foreach (int programId in programMap.Keys)
        {
            var stateMap = (Hashtable)programMap[programId];

            foreach (bool wireframe in stateMap.Keys)
            {
                var binding = (BindingStateStruct)stateMap[wireframe];
                deleteVertexArrayObject(binding.vao.Value);

                stateMap.Remove(wireframe);
            }

            programMap.Remove(programId);
        }

        bindingStates.Remove(geometry.Id);
    }

    public void ReleaseStatesOfProgram(GLProgram program)
    {
        foreach (int geometryId in bindingStates.Keys)
        {
            var programMap = (Hashtable)bindingStates[geometryId];

            if (programMap[program.Id] == null) continue;

            var stateMap = (Hashtable)programMap[program.Id];

            foreach (bool wireframe in stateMap.Keys)
            {
                var binding = (BindingStateStruct)stateMap[wireframe];
                deleteVertexArrayObject(binding.vao.Value);

                stateMap.Remove(wireframe);
            }

            programMap.Remove(program.Id);
        }
    }

    public void EnableAttribute(int attribute)
    {
        enableAttributeAndDivisor(attribute, 0);
    }

    private void enableAttributeAndDivisor(int attribute, int meshPerAttribute)
    {
        var newAttributes = currentState.newAttributes;
        var enabledAttributes = currentState.enabledAttributes;
        var attributeDivisors = currentState.attributeDivisors;

        newAttributes[attribute] = 1;

        if (enabledAttributes[attribute] == 0)
        {
            GL.EnableVertexAttribArray(attribute);
            enabledAttributes[attribute] = 1;
        }

        if (attributeDivisors[attribute] != meshPerAttribute)
        {
            //const extension = capabilities.isWebGL2 ? gl : extensions.get('ANGLE_instanced_arrays');
            //extension[capabilities.isWebGL2 ? 'vertexAttribDivisor' : 'vertexAttribDivisorANGLE'](attribute, meshPerAttribute);

            GL.VertexAttribDivisor(attribute, meshPerAttribute);
            attributeDivisors[attribute] = meshPerAttribute;
        }
    }

    public void DisableUnusedAttributes()
    {
        var newAttributes = currentState.newAttributes;
        var enabledAttributes = currentState.enabledAttributes;

        for (var i = 0; i < enabledAttributes.Count; i++)
            if (enabledAttributes[i] != newAttributes[i])
            {
                GL.DisableVertexAttribArray(i);
                enabledAttributes[i] = 0;
            }
    }

    private void vertexAttribPointer(int index, int size, VertexAttribPointerType type, bool normalized, int stride,
        int offset)
    {
        if (capabilities.IsGL2 && (type == VertexAttribPointerType.Int || type == VertexAttribPointerType.UnsignedInt))
            GL.VertexAttribIPointer(index, size, (VertexAttribIntegerType)type, stride, IntPtr.Zero);
        else
            GL.VertexAttribPointer(index, size, type, normalized, stride, offset);
    }

    private void setupVertexAttributes(Object3D object3D, Material material, GLProgram program, Geometry geometry)
    {
        if (capabilities.IsGL2 == false && (object3D is InstancedMesh || geometry is InstancedBufferGeometry))
            if (extensions.Get("GL_ARB_instanced_arrays") == -1)
                return;

        InitAttributes();

        var geometryAttributes = (geometry as BufferGeometry).Attributes;

        var programAttributes = program.GetAttributes();

        Hashtable materialDefaultAttributeValues = null;
        if (material is ShaderMaterial)
            materialDefaultAttributeValues = (material as ShaderMaterial).DefaultAttributeValues;

        foreach (string name in programAttributes.Keys)
        {
            var programAttribute = (int)programAttributes[name];

            if (programAttribute >= 0)
            {
                object geometryAttribute = null;
                //const geometryAttribute = geometryAttributes[name];

                if (geometryAttributes.TryGetValue(name, out geometryAttribute))
                {
                    if (geometryAttribute != null)
                    {
                        var normalized = false;
                        var size = 0;
                        BufferType attribute = null;
                        if (geometryAttribute is BufferAttribute<float>)
                        {
                            normalized = (geometryAttribute as BufferAttribute<float>).Normalized;
                            size = (geometryAttribute as BufferAttribute<float>).ItemSize;
                            attribute = attributes.Get<float>(geometryAttribute);
                        }

                        if (geometryAttribute is BufferAttribute<int>)
                        {
                            normalized = (geometryAttribute as BufferAttribute<int>).Normalized;
                            size = (geometryAttribute as BufferAttribute<int>).ItemSize;
                            attribute = attributes.Get<int>(geometryAttribute);
                        }

                        if (geometryAttribute is BufferAttribute<uint>)
                        {
                            normalized = (geometryAttribute as BufferAttribute<uint>).Normalized;
                            size = (geometryAttribute as BufferAttribute<uint>).ItemSize;
                            attribute = attributes.Get<uint>(geometryAttribute);
                        }

                        if (geometryAttribute is BufferAttribute<byte>)
                        {
                            normalized = (geometryAttribute as BufferAttribute<byte>).Normalized;
                            size = (geometryAttribute as BufferAttribute<byte>).ItemSize;
                            attribute = attributes.Get<byte>(geometryAttribute);
                        }

                        if (geometryAttribute is BufferAttribute<ushort>)
                        {
                            normalized = (geometryAttribute as BufferAttribute<ushort>).Normalized;
                            size = (geometryAttribute as BufferAttribute<ushort>).ItemSize;
                            attribute = attributes.Get<ushort>(geometryAttribute);
                        }
                        // TODO Attribute may not be available on context restore

                        if (attribute == null) continue;

                        var buffer = attribute.buffer;
                        var type = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType),
                            attribute.Type);
                        var bytesPerElement = attribute.BytesPerElement;

                        if (geometryAttribute is InterleavedBufferAttribute<float>)
                        {
                            var data = (geometryAttribute as InterleavedBufferAttribute<float>).Data;
                            var stride = data.Stride;
                            var offset = (geometryAttribute as InterleavedBufferAttribute<float>).Offset;

                            if (data != null && data is InstancedInterleavedBuffer<float>)
                            {
                                enableAttributeAndDivisor(programAttribute,
                                    (data as InstancedInterleavedBuffer<float>).MeshPerAttribute);

                                if ((geometry as InstancedBufferGeometry).MaxInstanceCount == null)
                                    (geometry as InstancedBufferGeometry).MaxInstanceCount =
                                        (data as InstancedInterleavedBuffer<float>).MeshPerAttribute *
                                        (data as InstancedInterleavedBuffer<float>).count;
                            }
                            else
                            {
                                EnableAttribute(programAttribute);
                            }

                            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                            vertexAttribPointer(programAttribute, size, type, normalized, stride * bytesPerElement,
                                offset * bytesPerElement);
                        }
                        else
                        {
                            if (geometryAttribute is InstancedBufferAttribute<float>)
                            {
                                enableAttributeAndDivisor(programAttribute,
                                    (geometryAttribute as InstancedBufferAttribute<float>).MeshPerAttribute);

                                if ((geometry as InstancedBufferGeometry).MaxInstanceCount == null)
                                    (geometry as InstancedBufferGeometry).MaxInstanceCount =
                                        (geometryAttribute as InstancedBufferAttribute<float>).MeshPerAttribute *
                                        (geometryAttribute as InstancedBufferAttribute<float>).count;
                            }
                            else
                            {
                                EnableAttribute(programAttribute);
                            }

                            GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
                            vertexAttribPointer(programAttribute, size, type, normalized, 0, 0);
                        }
                    }
                }
                else if (name.Equals("instanceMatrix"))
                {
                    var attribute = attributes.Get<float>((object3D as InstancedMesh).InstanceMatrix);

                    // TODO Attribute may not be available on context restore

                    if (attribute == null) continue;

                    var buffer = attribute.buffer;
                    var type = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType), attribute.Type);

                    enableAttributeAndDivisor(programAttribute + 0, 1);
                    enableAttributeAndDivisor(programAttribute + 1, 1);
                    enableAttributeAndDivisor(programAttribute + 2, 1);
                    enableAttributeAndDivisor(programAttribute + 3, 1);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);

                    GL.VertexAttribPointer(programAttribute + 0, 4, type, false, 64, 0);
                    GL.VertexAttribPointer(programAttribute + 1, 4, type, false, 64, 16);
                    GL.VertexAttribPointer(programAttribute + 2, 4, type, false, 64, 32);
                    GL.VertexAttribPointer(programAttribute + 3, 4, type, false, 64, 48);
                }
                else if (name.Equals("instanceColor"))
                {
                    var attribute = attributes.Get<float>((object3D as InstancedMesh).InstanceColor);

                    // TODO Attribute may not be available on context restore

                    if (attribute == null) continue;

                    var buffer = attribute.buffer;
                    var type = (VertexAttribPointerType)Enum.ToObject(typeof(VertexAttribPointerType), attribute.Type);

                    enableAttributeAndDivisor(programAttribute, 1);

                    GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);

                    GL.VertexAttribPointer(programAttribute, 3, type, false, 12, 0);
                }
                else if (materialDefaultAttributeValues != null)
                {
                    var value = (float[])materialDefaultAttributeValues[name];

                    if (value != null)
                        switch (value.Length)
                        {
                            case 2:
                                GL.VertexAttrib2(programAttribute, value);
                                break;

                            case 3:
                                GL.VertexAttrib3(programAttribute, value);
                                break;

                            case 4:
                                GL.VertexAttrib4(programAttribute, value);
                                break;

                            default:
                                GL.VertexAttrib1(programAttribute, value[0]);
                                break;
                        }
                }
            }
        }

        DisableUnusedAttributes();
    }

    protected virtual void RaiseDisposed()
    {
        var handler = Disposed;
        if (handler != null)
            handler(this, new EventArgs());
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed) return;
        RaiseDisposed();
        disposed = true;
        disposed = true;
    }
}