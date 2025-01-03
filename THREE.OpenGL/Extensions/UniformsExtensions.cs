using System.Collections;
using System.Text.RegularExpressions;
using OpenTK.Graphics.ES30;

namespace THREE.OpenGL.Extensions;

[Serializable]
public static class UniformsExtensions
{
    public static GLUniforms Merge(this GLUniforms source, GLUniforms target)
    {
        foreach (var entry in target)
            if (source.ContainsKey(entry.Key))
                //Trace.TraceWarning("key is already exist {0}. this key's value update to newer", entry.Key);
                source[entry.Key] = entry.Value;
            else
                source.Add(entry.Key, entry.Value);

        return source;
    }

    public static void LoadProgram(this GLUniforms source, int program)
    {
        source.Program = program;

        var n = 0;

        GL.GetProgram(source.Program, GetProgramParameterName.ActiveUniforms, out n);

        for (var i = 0; i < n; i++)
        {
            int size;
            ActiveUniformType info;

            var name = GL.GetActiveUniform(program, i, out size, out info);
            var addr = GL.GetUniformLocation(program, name);
            ParseUniform(name, info, size, addr, source);
        }
    }

    private static void ParseUniform(string name, ActiveUniformType uniformType, int size, int addr,
        GLUniforms container)
    {
        var path = name;
        var pathLength = name.Length;
        //var RePathPart = /([\w\d_]+)(\])?(\[|\.)?/g;
        var RePathPart = @"([\w\d_]+)(\])?(\[|\.)?";

        var mc = Regex.Matches(path.Trim(), RePathPart, RegexOptions.None);

        foreach (Match m in mc)
        {
            var groups = m.Groups;
            var id = groups[1].Value;
            var idInt = -1;
            var idIsIndex = groups[2].Value.Equals("]");
            var subscript = groups[3].Value;

            if (idIsIndex) idInt = string.IsNullOrEmpty(id) ? 0 : Convert.ToInt32(id);
            //g.Value,g.Value.Length+m.Index);
            if (string.IsNullOrEmpty(subscript) ||
                (subscript.Equals("[") && groups[1].Value.Length + m.Index == pathLength - 3))
            {
                if (string.IsNullOrEmpty(subscript))
                    container.AddUniform(new SingleUniform(id, uniformType, addr));
                else
                    container.AddUniform(new PureArrayUniform(id, uniformType, size, addr));

                break;
            }

            var map = container;
            GLUniforms next = null;
            if (!map.ContainsKey(id))
            {
                next = new GLUniforms(id);
                container.AddUniform(next);
            }
            else
            {
                next = (GLUniforms)map[id];
            }

            container = next;
        }
    }

    private static void AddUniform(this GLUniforms container, GLUniform uniformObject)
    {
        container.Seq.Add(uniformObject);
        container.Add(uniformObject.Id, uniformObject);
    }

    public static void SetProjectionMatrix(this GLUniforms source, Matrix4 projMatrix)
    {
        if (source.ContainsKey("projectionMatrix"))
        {
            var u = (SingleUniform)source["projectionMatrix"];
            GL.UniformMatrix4(u.Addr, 1, false, projMatrix.Elements);
        }
    }

    public static void SetValue(this GLUniforms source, string name, object value, GLTextures textures = null)
    {
        if (source.ContainsKey(name))
        {
            var u = (GLUniform)source[name];
            if (u is SingleUniform)
                (u as SingleUniform).SetValue(value, textures);
            else if (u is PureArrayUniform)
                (u as PureArrayUniform).SetValue(value, textures);
            else
                (u as StructuredUniform).SetValue(value, textures);
        }
    }

    public static void SetOptional(this GLUniforms source, Hashtable objects, string name)
    {
        if (objects.ContainsKey(name))
        {
            var value = objects[name];

            source.SetValue(name, value);
        }
    }
}