using System.Collections;

namespace THREE;

[Serializable]
public class OutlinePass : Pass, IDisposable
{
    public static Vector2 BlurDirectionX = new(1.0f, 0.0f);
    public static Vector2 BlurDirectionY = new(0.0f, 1.0f);

    private GLUniforms copyUniforms;
    private MeshDepthMaterial depthMaterial;
    private bool disposed;
    private float downSampleRatio;
    private ShaderMaterial edgeDetectionMaterial;
    public float edgeGlow;
    public float edgeStrength;
    public float edgeThickness;
    public Color hiddenEdgeColor;
    private MeshBasicMaterial maskBufferMaterial;
    private ShaderMaterial materialCopy;
    private float oldClearAlpha;
    private Color oldClearColor;
    private ShaderMaterial overlayMaterial;
    private Texture patternTexture;
    private ShaderMaterial prepareMaskMaterial;
    public float pulsePeriod;
    private Camera renderCamera;
    private Scene renderScene;
    private GLRenderTarget renderTargetBlurBuffer1;
    private GLRenderTarget renderTargetBlurBuffer2;
    private GLRenderTarget renderTargetDepthBuffer;
    private GLRenderTarget renderTargetEdgeBuffer1;
    private GLRenderTarget renderTargetEdgeBuffer2;
    private GLRenderTarget renderTargetMaskBuffer;
    private GLRenderTarget renderTargetMaskDownSampleBuffer;
    private Vector2 resolution;
    private List<Object3D> selectedObjects = new();
    private ShaderMaterial separableBlurMaterial1;
    private ShaderMaterial separableBlurMaterial2;
    private Color tempPulseColor1;
    private Color tempPulseColor2;
    private Matrix4 textureMatrix;
    public bool usePatternTexture;
    public Color visibleEdgeColor;

    public OutlinePass(Vector2 resolution, Scene scene, Camera camera, List<Object3D> selectedObjects = null)
    {
        renderScene = scene;
        renderCamera = camera;
        if (selectedObjects != null) this.selectedObjects = selectedObjects;
        visibleEdgeColor = new Color(1, 1, 1);
        hiddenEdgeColor = new Color(0.1f, 0.04f, 0.02f);
        edgeGlow = 0.0f;
        usePatternTexture = false;
        edgeThickness = 1.0f;
        edgeStrength = 3.0f;
        downSampleRatio = 2.0f;
        pulsePeriod = 0.0f;


        this.resolution = resolution != null ? new Vector2(resolution.X, resolution.Y) : new Vector2(256, 256);

        var pars = new Hashtable
        {
            { "minFilter", Constants.LinearFilter }, { "magFilter", Constants.LinearFilter },
            { "format", Constants.RGBAFormat }
        };

        var resx = Math.Round(this.resolution.X / downSampleRatio);
        var resy = Math.Round(this.resolution.Y / downSampleRatio);

        maskBufferMaterial = new MeshBasicMaterial { Color = new Color(0xffffff) };
        maskBufferMaterial.Side = Constants.DoubleSide;
        renderTargetMaskBuffer = new GLRenderTarget((int)this.resolution.X, (int)this.resolution.Y, pars);
        renderTargetMaskBuffer.Texture.Name = "OutlinePass.mask";
        renderTargetMaskBuffer.Texture.GenerateMipmaps = false;

        depthMaterial = new MeshDepthMaterial();
        depthMaterial.Side = Constants.DoubleSide;
        depthMaterial.DepthPacking = Constants.RGBADepthPacking;
        depthMaterial.Blending = Constants.NoBlending;

        prepareMaskMaterial = GetPrepareMaskMaterial();
        prepareMaskMaterial.Side = Constants.DoubleSide;
        prepareMaskMaterial.FragmentShader = ReplaceDepthToViewZ(prepareMaskMaterial.FragmentShader, renderCamera);

        renderTargetDepthBuffer = new GLRenderTarget((int)this.resolution.X, (int)this.resolution.Y, pars);
        renderTargetDepthBuffer.Texture.Name = "OutlinePass.depth";
        renderTargetDepthBuffer.Texture.GenerateMipmaps = false;

        renderTargetMaskDownSampleBuffer = new GLRenderTarget((int)resx, (int)resy, pars);
        renderTargetMaskDownSampleBuffer.Texture.Name = "OutlinePass.depthDownSample";
        renderTargetMaskDownSampleBuffer.Texture.GenerateMipmaps = false;

        renderTargetBlurBuffer1 = new GLRenderTarget((int)resx, (int)resy, pars);
        renderTargetBlurBuffer1.Texture.Name = "OutlinePass.blur1";
        renderTargetBlurBuffer1.Texture.GenerateMipmaps = false;
        renderTargetBlurBuffer2 = new GLRenderTarget((int)Math.Round(resx / 2), (int)Math.Round(resy / 2), pars);
        renderTargetBlurBuffer2.Texture.Name = "OutlinePass.blur2";
        renderTargetBlurBuffer2.Texture.GenerateMipmaps = false;

        edgeDetectionMaterial = GetEdgeDetectionMaterial();
        renderTargetEdgeBuffer1 = new GLRenderTarget((int)resx, (int)resy, pars);
        renderTargetEdgeBuffer1.Texture.Name = "OutlinePass.edge1";
        renderTargetEdgeBuffer1.Texture.GenerateMipmaps = false;
        renderTargetEdgeBuffer2 = new GLRenderTarget((int)Math.Round(resx / 2), (int)Math.Round(resy / 2), pars);
        renderTargetEdgeBuffer2.Texture.Name = "OutlinePass.edge2";
        renderTargetEdgeBuffer2.Texture.GenerateMipmaps = false;

        var MAX_EDGE_THICKNESS = 4;
        var MAX_EDGE_GLOW = 4;

        separableBlurMaterial1 = GetSeperableBlurMaterial(MAX_EDGE_THICKNESS);
        ((separableBlurMaterial1.Uniforms["texSize"] as GLUniform)["value"] as Vector2).Set((float)resx, (float)resy);
        (separableBlurMaterial1.Uniforms["kernelRadius"] as GLUniform)["value"] = 1;
        separableBlurMaterial2 = GetSeperableBlurMaterial(MAX_EDGE_GLOW);
        ((separableBlurMaterial2.Uniforms["texSize"] as GLUniform)["value"] as Vector2).Set((float)Math.Round(resx / 2),
            (float)Math.Round(resy / 2));
        (separableBlurMaterial2.Uniforms["kernelRadius"] as GLUniform)["value"] = MAX_EDGE_GLOW;

        // Overlay material
        overlayMaterial = GetOverlayMaterial();

        // copy material

        var copyShader = new CopyShader();

        copyUniforms = UniformsUtils.CloneUniforms(copyShader.Uniforms);
        (copyUniforms["opacity"] as GLUniform)["value"] = 1.0f;

        materialCopy = new ShaderMaterial
        {
            Uniforms = copyUniforms,
            VertexShader = copyShader.VertexShader,
            FragmentShader = copyShader.FragmentShader,
            Blending = Constants.NoBlending,
            DepthTest = false,
            DepthWrite = false,
            Transparent = true
        };

        Enabled = true;
        NeedsSwap = false;

        oldClearColor = new Color();
        oldClearAlpha = 1;

        fullScreenQuad = new FullScreenQuad();

        tempPulseColor1 = new Color();
        tempPulseColor2 = new Color();
        textureMatrix = new Matrix4();
    }

    public virtual void Dispose()
    {
        Dispose(disposed);
    }

    public event EventHandler<EventArgs> Disposed;

    private string ReplaceDepthToViewZ(string s, Camera camera)
    {
        var type = camera is PerspectiveCamera ? "perspective" : "orthographic";

        return s.Replace("DEPTH_TO_VIEW_Z", type + "DepthToViewZ");
    }

    private void UpdateTextureMatrix()
    {
        textureMatrix.Set(0.5f, 0.0f, 0.0f, 0.5f,
            0.0f, 0.5f, 0.0f, 0.5f,
            0.0f, 0.0f, 0.5f, 0.5f,
            0.0f, 0.0f, 0.0f, 1.0f);
        textureMatrix.Multiply(renderCamera.ProjectionMatrix);
        textureMatrix.Multiply(renderCamera.MatrixWorldInverse);
    }

    private void ChangeVisibilityOfSelectedObjects(bool bVisible)
    {
        for (var i = 0; i < selectedObjects.Count; i++)
        {
            var selectedObject = selectedObjects[i];
            selectedObject.Traverse(obj =>
            {
                if (obj is Mesh)
                {
                    if (bVisible)
                    {
                        obj.Visible = (bool)obj.UserData["oldVisible"];
                        obj.UserData.Remove("oldVisible");
                    }
                    else
                    {
                        obj.UserData["oldVisible"] = obj.Visible;
                        obj.Visible = bVisible;
                    }
                }
            });
        }
    }

    private void ChangeVisibilityOfNonSelectedObjects(bool bVisible)
    {
        var selectedMeshes = new List<Object3D>();

        for (var i = 0; i < selectedObjects.Count; i++)
        {
            var selectedObject = selectedObjects[i];
            selectedObject.Traverse(obj =>
            {
                if (obj is Mesh) selectedMeshes.Add(obj);
            });
        }

        renderScene.Traverse(obj =>
        {
            if (obj is Mesh || obj is Line || obj is Sprite)
            {
                var bFound = false;

                for (var i = 0; i < selectedMeshes.Count; i++)
                {
                    var selectedObjectId = selectedMeshes[i].Id;

                    if (selectedObjectId == obj.Id)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (!bFound)
                {
                    var visibility = obj.Visible;

                    if (!bVisible || obj.bVisible) obj.Visible = bVisible;

                    obj.bVisible = visibility;
                }
            }
        });
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        if (selectedObjects.Count > 0)
        {
            oldClearColor.Copy(renderer.GetClearColor());
            oldClearAlpha = renderer.GetClearAlpha();
            var oldAutoClear = renderer.AutoClear;

            renderer.AutoClear = false;

            if (maskActive != null && maskActive.Value) renderer.State.buffers.stencil.SetTest(false);

            renderer.SetClearColor(Color.Hex(0xffffff));

            // Make selected objects invisible
            ChangeVisibilityOfSelectedObjects(false);

            var currentBackground = renderScene.Background;
            renderScene.Background = null;

            // 1. Draw Non Selected objects in the depth buffer
            renderScene.OverrideMaterial = depthMaterial;
            renderer.SetRenderTarget(renderTargetDepthBuffer);
            renderer.Clear();
            renderer.Render(renderScene, renderCamera);

            // Make selected objects visible
            ChangeVisibilityOfSelectedObjects(true);

            // Update Texture Matrix for Depth compare
            UpdateTextureMatrix();

            // Make non selected objects invisible, and draw only the selected objects, by comparing the depth buffer of non selected objects
            ChangeVisibilityOfNonSelectedObjects(false);
            renderScene.OverrideMaterial = prepareMaskMaterial;
            ((prepareMaskMaterial.Uniforms["cameraNearFar"] as GLUniform)["value"] as Vector2).Set(renderCamera.Near,
                renderCamera.Far);
            (prepareMaskMaterial.Uniforms["depthTexture"] as GLUniform)["value"] = renderTargetDepthBuffer.Texture;
            (prepareMaskMaterial.Uniforms["textureMatrix"] as GLUniform)["value"] = textureMatrix;
            renderer.SetRenderTarget(renderTargetMaskBuffer);
            renderer.Clear();
            renderer.Render(renderScene, renderCamera);
            renderScene.OverrideMaterial = null;
            ChangeVisibilityOfNonSelectedObjects(true);

            renderScene.Background = currentBackground;

            // 2. Downsample to Half resolution
            fullScreenQuad.material = materialCopy;
            (copyUniforms["tDiffuse"] as GLUniform)["value"] = renderTargetMaskBuffer.Texture;
            renderer.SetRenderTarget(renderTargetMaskDownSampleBuffer);
            renderer.Clear();
            fullScreenQuad.Render(renderer);

            tempPulseColor1.Copy(visibleEdgeColor);
            tempPulseColor2.Copy(hiddenEdgeColor);

            if (pulsePeriod > 0)
            {
                var scalar = (1 + 0.25f) / 2 +
                             (float)Math.Cos(DateTime.Now.Ticks * 0.01f / pulsePeriod) * (1.0f - 0.25f) / 2;
                tempPulseColor1.MultiplyScalar(scalar);
                tempPulseColor2.MultiplyScalar(scalar);
            }

            // 3. Apply Edge Detection Pass
            fullScreenQuad.material = edgeDetectionMaterial;
            (edgeDetectionMaterial.Uniforms["maskTexture"] as GLUniform)["value"] =
                renderTargetMaskDownSampleBuffer.Texture;
            ((edgeDetectionMaterial.Uniforms["texSize"] as GLUniform)["value"] as Vector2).Set(
                renderTargetMaskDownSampleBuffer.Width, renderTargetMaskDownSampleBuffer.Height);
            (edgeDetectionMaterial.Uniforms["visibleEdgeColor"] as GLUniform)["value"] = tempPulseColor1;
            (edgeDetectionMaterial.Uniforms["hiddenEdgeColor"] as GLUniform)["value"] = tempPulseColor2;
            renderer.SetRenderTarget(renderTargetEdgeBuffer1);
            renderer.Clear();
            fullScreenQuad.Render(renderer);

            // 4. Apply Blur on Half res
            fullScreenQuad.material = separableBlurMaterial1;
            (separableBlurMaterial1.Uniforms["colorTexture"] as GLUniform)["value"] = renderTargetEdgeBuffer1.Texture;
            (separableBlurMaterial1.Uniforms["direction"] as GLUniform)["value"] = BlurDirectionX;
            (separableBlurMaterial1.Uniforms["kernelRadius"] as GLUniform)["value"] = edgeThickness;
            renderer.SetRenderTarget(renderTargetBlurBuffer1);
            renderer.Clear();
            fullScreenQuad.Render(renderer);
            (separableBlurMaterial1.Uniforms["colorTexture"] as GLUniform)["value"] = renderTargetBlurBuffer1.Texture;
            (separableBlurMaterial1.Uniforms["direction"] as GLUniform)["value"] = BlurDirectionY;
            renderer.SetRenderTarget(renderTargetEdgeBuffer1);
            renderer.Clear();
            fullScreenQuad.Render(renderer);

            // Apply Blur on quarter res
            fullScreenQuad.material = separableBlurMaterial2;
            (separableBlurMaterial2.Uniforms["colorTexture"] as GLUniform)["value"] = renderTargetEdgeBuffer1.Texture;
            (separableBlurMaterial2.Uniforms["direction"] as GLUniform)["value"] = BlurDirectionX;
            renderer.SetRenderTarget(renderTargetBlurBuffer2);
            renderer.Clear();
            fullScreenQuad.Render(renderer);
            (separableBlurMaterial2.Uniforms["colorTexture"] as GLUniform)["value"] = renderTargetBlurBuffer2.Texture;
            (separableBlurMaterial2.Uniforms["direction"] as GLUniform)["value"] = BlurDirectionY;
            renderer.SetRenderTarget(renderTargetEdgeBuffer2);
            renderer.Clear();
            fullScreenQuad.Render(renderer);

            // Blend it additively over the input texture
            fullScreenQuad.material = overlayMaterial;
            (overlayMaterial.Uniforms["maskTexture"] as GLUniform)["value"] = renderTargetMaskBuffer.Texture;
            (overlayMaterial.Uniforms["edgeTexture1"] as GLUniform)["value"] = renderTargetEdgeBuffer1.Texture;
            (overlayMaterial.Uniforms["edgeTexture2"] as GLUniform)["value"] = renderTargetEdgeBuffer2.Texture;
            (overlayMaterial.Uniforms["patternTexture"] as GLUniform)["value"] = patternTexture;
            (overlayMaterial.Uniforms["edgeStrength"] as GLUniform)["value"] = edgeStrength;
            (overlayMaterial.Uniforms["edgeGlow"] as GLUniform)["value"] = edgeGlow;
            (overlayMaterial.Uniforms["usePatternTexture"] as GLUniform)["value"] = usePatternTexture;


            if (maskActive != null && maskActive.Value) renderer.State.buffers.stencil.SetTest(true);

            renderer.SetRenderTarget(readBuffer);
            fullScreenQuad.Render(renderer);

            renderer.SetClearColor(oldClearColor, oldClearAlpha);
            renderer.AutoClear = oldAutoClear;
        }

        if (RenderToScreen)
        {
            fullScreenQuad.material = materialCopy;
            (copyUniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
            renderer.SetRenderTarget(null);
            fullScreenQuad.Render(renderer);
        }
    }

    public override void SetSize(float width, float height)
    {
        renderTargetMaskBuffer.SetSize((int)width, (int)height);

        var resx = (int)Math.Round(width / downSampleRatio);
        var resy = (int)Math.Round(height / downSampleRatio);
        renderTargetMaskDownSampleBuffer.SetSize(resx, resy);
        renderTargetBlurBuffer1.SetSize(resx, resy);
        renderTargetEdgeBuffer1.SetSize(resx, resy);
        var texSize = (separableBlurMaterial1.Uniforms["texSize"] as GLUniform)["value"] as Vector2;
        texSize.X = resx;
        texSize.Y = resy;

        resx = (int)Math.Round(resx / 2.0f);
        resy = (int)Math.Round(resy / 2.0f);

        renderTargetBlurBuffer2.SetSize(resx, resy);
        renderTargetEdgeBuffer2.SetSize(resx, resy);

        texSize = (separableBlurMaterial2.Uniforms["texSize"] as GLUniform)["value"] as Vector2;
        texSize.X = resx;
        texSize.Y = resy;
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
        renderTargetMaskBuffer.Dispose();
        renderTargetDepthBuffer.Dispose();
        renderTargetMaskDownSampleBuffer.Dispose();
        renderTargetBlurBuffer1.Dispose();
        renderTargetBlurBuffer2.Dispose();
        renderTargetEdgeBuffer1.Dispose();
        renderTargetEdgeBuffer2.Dispose();
        RaiseDisposed();
        disposed = true;
        disposed = true;
    }

    private ShaderMaterial GetPrepareMaskMaterial()
    {
        return new ShaderMaterial
        {
            Uniforms = new GLUniforms
            {
                { "depthTexture", new GLUniform { { "value", null } } },
                { "cameraNearFar", new GLUniform { { "value", new Vector2(0.5f, 0.5f) } } },
                { "textureMatrix", new GLUniform { { "value", null } } }
            },

            VertexShader = @"
				#include <morphtarget_pars_vertex>
				#include <skinning_pars_vertex>

				varying vec4 projTexCoord;
				varying vec4 vPosition;
				uniform mat4 textureMatrix;

				void main() {

					#include <skinbase_vertex>
					#include <begin_vertex>
					#include <morphtarget_vertex>
					#include <skinning_vertex>
					#include <project_vertex>

					vPosition = mvPosition;
					vec4 worldPosition = modelMatrix * vec4( position, 1.0 );
					projTexCoord = textureMatrix * worldPosition;

				}
				",


            FragmentShader = @"
				#include <packing>
				varying vec4 vPosition;
				varying vec4 projTexCoord;
				uniform sampler2D depthTexture;
				uniform vec2 cameraNearFar;

				void main() {

					float depth = unpackRGBAToDepth(texture2DProj( depthTexture, projTexCoord ));
					float viewZ = - DEPTH_TO_VIEW_Z( depth, cameraNearFar.x, cameraNearFar.y );
					float depthTest = (-vPosition.z > viewZ) ? 1.0 : 0.0;
					gl_FragColor = vec4(0.0, depthTest, 1.0, 1.0);

				}
				"
        };
    }

    private ShaderMaterial GetEdgeDetectionMaterial()
    {
        return new ShaderMaterial
        {
            Uniforms = new GLUniforms
            {
                { "maskTexture", new GLUniform { { "value", null } } },
                { "texSize", new GLUniform { { "value", new Vector2(0.5f, 0.5f) } } },
                { "visibleEdgeColor", new GLUniform { { "value", new Vector3(1.0f, 1.0f, 1.0f) } } },
                { "hiddenEdgeColor", new GLUniform { { "value", new Vector3(1.0f, 1.0f, 1.0f) } } }
            },

            VertexShader = @"
			varying vec2 vUv;
			void main()
			{
					vUv = uv;
					gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
			}
			",

            FragmentShader = @"
			varying vec2 vUv;
			uniform sampler2D maskTexture;
			uniform vec2 texSize;
			uniform vec3 visibleEdgeColor;
			uniform vec3 hiddenEdgeColor;
				
			void main()
			{
					vec2 invSize = 1.0 / texSize;
					vec4 uvOffset = vec4(1.0, 0.0, 0.0, 1.0) * vec4(invSize, invSize);
					vec4 c1 = texture2D(maskTexture, vUv + uvOffset.xy);
					vec4 c2 = texture2D(maskTexture, vUv - uvOffset.xy);
					vec4 c3 = texture2D(maskTexture, vUv + uvOffset.yw);
					vec4 c4 = texture2D(maskTexture, vUv - uvOffset.yw);
					float diff1 = (c1.r - c2.r) * 0.5;
					float diff2 = (c3.r - c4.r) * 0.5;
					float d = length(vec2(diff1, diff2));
					float a1 = min(c1.g, c2.g);
					float a2 = min(c3.g, c4.g);
					float visibilityFactor = min(a1, a2);
					vec3 edgeColor = 1.0 - visibilityFactor > 0.001 ? visibleEdgeColor : hiddenEdgeColor;
					gl_FragColor = vec4(edgeColor, 1.0) * vec4(d);
				}
			"
        };
    }

    private ShaderMaterial GetSeperableBlurMaterial(float maxRadius)
    {
        return new ShaderMaterial
        {
            Defines = new Hashtable
            {
                { "MAX_RADIUS", maxRadius.ToString() }
            },

            Uniforms = new GLUniforms
            {
                { "colorTexture", new GLUniform { { "value", null } } },
                { "texSize", new GLUniform { { "value", new Vector2(0.5f, 0.5f) } } },
                { "direction", new GLUniform { { "value", new Vector2(0.5f, 0.5f) } } },
                { "kernelRadius", new GLUniform { { "value", 1.0f } } }
            },

            VertexShader = @"
			varying vec2 vUv;
			void main()
			{
				vUv = uv;
				gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
			}
			",

            FragmentShader = @"
			#include <common>
			varying vec2 vUv;
			uniform sampler2D colorTexture;
			uniform vec2 texSize;
			uniform vec2 direction;
			uniform float kernelRadius;
			
			float gaussianPdf(in float x, in float sigma)
			{
				return 0.39894 * exp(-0.5 * x * x / (sigma * sigma)) / sigma;
			}
			void main()
			{
				vec2 invSize = 1.0 / texSize;
				float weightSum = gaussianPdf(0.0, kernelRadius);
				vec4 diffuseSum = texture2D(colorTexture, vUv) * weightSum;
				vec2 delta = direction * invSize * kernelRadius / float(MAX_RADIUS);
				vec2 uvOffset = delta;
				for (int i = 1; i <= MAX_RADIUS; i++)
				{
					float w = gaussianPdf(uvOffset.x, kernelRadius);
					vec4 sample1 = texture2D(colorTexture, vUv + uvOffset);
					vec4 sample2 = texture2D(colorTexture, vUv - uvOffset);
					diffuseSum += ((sample1 + sample2) * w);
					weightSum += (2.0 * w);
					uvOffset += delta;
				}
				gl_FragColor = diffuseSum / weightSum;
			}
			"
        };
    }

    private ShaderMaterial GetOverlayMaterial()
    {
        return new ShaderMaterial
        {
            Uniforms = new GLUniforms
            {
                { "maskTexture", new GLUniform { { "value", null } } },
                { "edgeTexture1", new GLUniform { { "value", null } } },
                { "edgeTexture2", new GLUniform { { "value", null } } },
                { "patternTexture", new GLUniform { { "value", null } } },
                { "edgeStrength", new GLUniform { { "value", 1.0 } } },
                { "edgeGlow", new GLUniform { { "value", 1.0 } } },
                { "usePatternTexture", new GLUniform { { "value", 0.0 } } }
            },

            VertexShader = @"
				varying vec2 vUv;
				void main()
				{
						vUv = uv;
						gl_Position = projectionMatrix * modelViewMatrix * vec4(position, 1.0);
					}
				",

            FragmentShader = @"
				varying vec2 vUv;
				uniform sampler2D maskTexture;
				uniform sampler2D edgeTexture1;
				uniform sampler2D edgeTexture2;
				uniform sampler2D patternTexture;
				uniform float edgeStrength;
				uniform float edgeGlow;
				uniform bool usePatternTexture;
				
				void main()
				{
					vec4 edgeValue1 = texture2D(edgeTexture1, vUv);
					vec4 edgeValue2 = texture2D(edgeTexture2, vUv);
					vec4 maskColor = texture2D(maskTexture, vUv);
					vec4 patternColor = texture2D(patternTexture, 6.0 * vUv);
					float visibilityFactor = 1.0 - maskColor.g > 0.0 ? 1.0 : 0.5;
					vec4 edgeValue = edgeValue1 + edgeValue2 * edgeGlow;
					vec4 finalColor = edgeStrength * maskColor.r * edgeValue;
					if (usePatternTexture)
						finalColor += +visibilityFactor * (1.0 - maskColor.r) * (1.0 - patternColor.r);
					gl_FragColor = finalColor;
				}
				",
            Blending = Constants.AdditiveBlending,
            DepthTest = false,
            DepthWrite = false,
            Transparent = true
        };
    }
}