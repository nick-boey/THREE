using System.Collections;
using System.Diagnostics;

namespace THREE;

[Serializable]
public class EffectComposer
{
    public ShaderPass CopyPass;

    private CopyShader copyShader = new();
    private float height;

    public List<Pass> Passes = new();

    private float pixelRatio;

    public GLRenderTarget ReadBuffer;
    public GLRenderer Renderer;

    public GLRenderTarget RenderTarget1;

    public GLRenderTarget RenderTarget2;

    public bool RenderToScreen;

    private Stopwatch stopwatch = new();

    private float width;

    public GLRenderTarget WriteBuffer;

    public EffectComposer(GLRenderer renderer, GLRenderTarget renderTarget = null)
    {
        Renderer = renderer;

        var parameters = new Hashtable();

        if (renderTarget == null)
        {
            parameters.Add("minFilter", Constants.LinearFilter);
            parameters.Add("magFilter", Constants.LinearFilter);
            parameters.Add("format", Constants.RGBAFormat);

            var size = renderer.GetSize();

            pixelRatio = renderer.GetPixelRatio();
            width = size.X;
            height = size.Y;

            renderTarget = new GLRenderTarget((int)(width * pixelRatio), (int)(height * pixelRatio), parameters);
            renderTarget.Texture.Name = "EffectComposer.rt1";
        }
        else
        {
            pixelRatio = 1;
            width = renderTarget.Width;
            height = renderTarget.Height;
        }

        RenderTarget1 = renderTarget;
        RenderTarget2 = (GLRenderTarget)renderTarget.Clone();
        //this.RenderTarget2 = new GLRenderTarget((int)(width * pixelRatio), (int)(height * pixelRatio), parameters);
        RenderTarget2.Texture.Name = "EffectComposer.rt2";

        WriteBuffer = RenderTarget1;
        ReadBuffer = RenderTarget2;

        RenderToScreen = true;

        CopyPass = new ShaderPass(copyShader);

        stopwatch.Start();
    }

    public float GetDelta()
    {
        return stopwatch.ElapsedMilliseconds / 1000.0f;
    }

    public void SwapBuffers()
    {
        var tmp = ReadBuffer;
        ReadBuffer = WriteBuffer;
        WriteBuffer = tmp;
    }

    public void AddPass(Pass pass)
    {
        Passes.Add(pass);
        pass.SetSize(width * pixelRatio, height * pixelRatio);
    }

    public void InsertPass(Pass pass, int index)
    {
        //this.Passes.Splice(index, 0, pass);
        Passes.Insert(index, pass);
        pass.SetSize(width * pixelRatio, height * pixelRatio);
    }

    public bool IsLastEnabledPass(int passIndex)
    {
        for (var i = passIndex + 1; i < Passes.Count; i++)
            if (Passes[i].Enabled)
                return false;

        return true;
    }

    public void Render(float? deltaTime = null)
    {
        if (deltaTime == null) deltaTime = GetDelta();

        var currentRenderTarget = Renderer.GetRenderTarget();

        var maskActive = false;

        Pass pass;
        var il = Passes.Count;

        for (var i = 0; i < il; i++)
        {
            pass = Passes[i];

            if (pass.Enabled == false) continue;

            pass.RenderToScreen = RenderToScreen && IsLastEnabledPass(i);
            pass.Render(Renderer, WriteBuffer, ReadBuffer, deltaTime, maskActive);

            if (pass.NeedsSwap)
            {
                if (maskActive)
                {
                    //var context = this.renderer.getContext();
                    var stencil = Renderer.State.buffers.stencil;

                    //context.stencilFunc( context.NOTEQUAL, 1, 0xffffffff );
                    unchecked
                    {
                        stencil.SetFunc(Constants.NotEqualStencilFunc, 1, (int)0xffffffff);

                        CopyPass.Render(Renderer, WriteBuffer, ReadBuffer, deltaTime);

                        //context.stencilFunc( context.EQUAL, 1, 0xffffffff );
                        stencil.SetFunc(Constants.EqualStencilFunc, 1, (int)0xffffffff);
                    }
                }

                SwapBuffers();
            }

            if (pass is MaskPass)
                maskActive = true;
            else if (pass is ClearMaskPass) maskActive = false;
        }


        Renderer.SetRenderTarget(currentRenderTarget);
    }

    public void Reset(GLRenderTarget renderTarget = null)
    {
        if (renderTarget == null)
        {
            var size = Renderer.GetSize(new Vector2());
            pixelRatio = Renderer.GetPixelRatio();
            width = size.X;
            height = size.Y;

            renderTarget = (GLRenderTarget)RenderTarget1.Clone();
            renderTarget.SetSize((int)(width * pixelRatio), (int)(height * pixelRatio));
        }

        RenderTarget1.Dispose();
        RenderTarget2.Dispose();
        RenderTarget1 = renderTarget;
        RenderTarget2 = (GLRenderTarget)renderTarget.Clone();

        WriteBuffer = RenderTarget1;
        ReadBuffer = RenderTarget2;
    }

    public void SetSize(float width, float height)
    {
        this.width = width;
        this.height = height;

        var effectiveWidth = this.width * pixelRatio;
        var effectiveHeight = this.height * pixelRatio;

        RenderTarget1.SetSize((int)effectiveWidth, (int)effectiveHeight);
        RenderTarget2.SetSize((int)effectiveWidth, (int)effectiveHeight);

        for (var i = 0; i < Passes.Count; i++) Passes[i].SetSize(effectiveWidth, effectiveHeight);
    }

    public void SetPixelRatio(float pixelRatio)
    {
        this.pixelRatio = pixelRatio;

        SetSize(width, height);
    }
}