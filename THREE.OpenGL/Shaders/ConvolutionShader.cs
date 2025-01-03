﻿using System.Runtime.Serialization;

namespace THREE;

[Serializable]
public class ConvolutionShader : ShaderMaterial
{
    public ConvolutionShader()
    {
        Defines.Add("KERNEL_SIZE_FLOAT", "25.0");
        Defines.Add("KERNEL_SIZE_INT", "25");

        Uniforms.Add("tDiffuse", new GLUniform { { "value", null } });
        Uniforms.Add("uImageIncrement", new GLUniform { { "value", new Vector2(0.001953125f, 0.0f) } });
        Uniforms.Add("cKernel", new GLUniform { { "value", new List<float>() } });

        VertexShader = @"
uniform vec2 uImageIncrement; 


varying vec2 vUv;

void main() {

	vUv = uv - ( ( KERNEL_SIZE_FLOAT - 1.0 ) / 2.0 ) * uImageIncrement;
	gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );

}
"
            ;
        FragmentShader = @"
uniform float cKernel[KERNEL_SIZE_INT]; 

uniform sampler2D tDiffuse;
uniform vec2 uImageIncrement;

varying vec2 vUv;

void main() {

	vec2 imageCoord = vUv;
	vec4 sum = vec4( 0.0, 0.0, 0.0, 0.0 );

	for( int i = 0; i < KERNEL_SIZE_INT; i ++ ) {
		sum += texture2D( tDiffuse, imageCoord ) * cKernel[ i ];
		imageCoord += uImageIncrement;

	}

	gl_FragColor = sum;

}
"
            ;
    }

    public ConvolutionShader(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public List<float> BuildKernel(float sigma)
    {
        var values = new List<float>();

        var kMaxKernelSize = 25;
        var kernelSize = 2 * (int)Math.Ceiling(sigma * 3.0f) + 1;
        if (kernelSize > kMaxKernelSize) kernelSize = kMaxKernelSize;

        var sum = 0.0f;
        var halfWidth = (kernelSize - 1) * 0.5f;

        for (var i = 0; i < kernelSize; i++)
        {
            values.Add(Gauss(i - halfWidth, sigma));
            sum += values[i];
        }

        for (var i = 0; i < kernelSize; i++)
            values[i] = values[i] / sum;

        return values;
    }

    private float Gauss(float x, float sigma)
    {
        return (float)Math.Exp(-(x * x) / (2.0f * sigma * sigma));
    }
}