﻿using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class LuminosityShader : ShaderMaterial
{
    public LuminosityShader()
    {
        Uniforms.Add("tDiffuse", new GLUniform { { "value", null } });


        VertexShader = @"
                varying vec2 vUv; 


                void main() {

					vUv = uv;
			        gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );

		        }


            "
            ;

        FragmentShader = @"
                #include <common>


                uniform sampler2D tDiffuse;

		        varying vec2 vUv;

		        void main() {

		        	vec4 texel = texture2D( tDiffuse, vUv );

		        	float l = linearToRelativeLuminance( texel.rgb );

		        	gl_FragColor = vec4( l, l, l, texel.w );

		        }

            ";
    }

    public LuminosityShader(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}