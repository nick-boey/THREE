using System.Collections;

namespace THREE;

[Serializable]
public class GLCubeRenderTarget : GLRenderTarget
{
    public GLCubeRenderTarget(int size, Hashtable options = null) : base(size, size, options)
    {
    }

    public GLCubeRenderTarget FromEquirectangularTexture(GLRenderer renderer, Texture texture)
    {
        Texture.Type = texture.Type;
        Texture.Format = Constants.RGBAFormat; // see #18859
        Texture.Encoding = texture.Encoding;

        Texture.GenerateMipmaps = texture.GenerateMipmaps;
        Texture.MinFilter = texture.MinFilter;
        Texture.MagFilter = texture.MagFilter;

        var shader = new GLShader
        {
            Uniforms = new GLUniforms { { "tEquirect", new GLUniform { { "value", null } } } },
            VertexShader = @"
                    varying vec3 vWorldDirection;

			        vec3 transformDirection( in vec3 dir, in mat4 matrix ) {

				        return normalize( ( matrix * vec4( dir, 0.0 ) ).xyz );

			        }

			        void main() {

				        vWorldDirection = transformDirection( position, modelMatrix );

				        #include <begin_vertex>
				        #include <project_vertex>

			        }
                ",
            FragmentShader = @"
                    uniform sampler2D tEquirect;

			        varying vec3 vWorldDirection;

			        #include <common>

			        void main() {

				        vec3 direction = normalize( vWorldDirection );

				        vec2 sampleUV = equirectUv( direction );

				        gl_FragColor = texture2D( tEquirect, sampleUV );

			        }
                "
        };

        var geometry = new BoxBufferGeometry(5, 5, 5);

        var material = new ShaderMaterial
        {
            Name = "CubemapFromEquirect",
            Uniforms = UniformsUtils.CloneUniforms(shader.Uniforms),
            VertexShader = shader.VertexShader,
            FragmentShader = shader.FragmentShader,
            Side = Constants.BackSide,
            Blending = Constants.NoBlending
        };

        material.Uniforms["tEquirect"] = new GLUniform { { "value", texture } };

        var mesh = new Mesh(geometry, material);
        var scene = new Scene();
        scene.Add(mesh);
        var currentMinFilter = texture.MinFilter;

        if (texture.MinFilter == Constants.LinearMipmapLinearFilter) texture.MinFilter = Constants.LinearFilter;

        var camera = new CubeCamera(1, 10, this);

        camera.Update(renderer, scene);

        texture.MinFilter = currentMinFilter;
        return this;
    }
}