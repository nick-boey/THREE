namespace THREE;

[Serializable]
public class MaskPass : Pass
{
    public Camera camera;

    public bool Inverse;
    public Scene scene;

    public MaskPass(Scene scene, Camera camera)
    {
        this.scene = scene;
        this.camera = camera;
        Clear = true;
        NeedsSwap = false;
        Inverse = false;
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        var state = renderer.State;

        // don't update color or depth
        state.buffers.color.SetMask(false);
        state.buffers.depth.SetMask(false);

        // lock buffers

        state.buffers.color.SetLocked(true);
        state.buffers.depth.SetLocked(true);

        // set up stencil

        int writeValue, clearValue;

        if (Inverse)
        {
            writeValue = 0;
            clearValue = 1;
        }
        else
        {
            writeValue = 1;
            clearValue = 0;
        }

        state.buffers.stencil.SetTest(true);
        state.buffers.stencil.SetOp(Constants.ReplaceStencilOp, Constants.ReplaceStencilOp, Constants.ReplaceStencilOp);
        unchecked
        {
            state.buffers.stencil.SetFunc(Constants.AlwaysStencilFunc, writeValue, (int)0xffffffff);
        }

        state.buffers.stencil.SetClear(clearValue);
        state.buffers.stencil.SetLocked(true);

        // draw into the stencil buffer

        renderer.SetRenderTarget(readBuffer);
        if (Clear) renderer.Clear();
        renderer.Render(scene, camera);

        renderer.SetRenderTarget(writeBuffer);
        if (Clear) renderer.Clear();
        renderer.Render(scene, camera);

        // unlock color and depth buffer for subsequent rendering

        state.buffers.color.SetLocked(false);
        state.buffers.depth.SetLocked(false);

        // only render where stencil is set to 1

        state.buffers.stencil.SetLocked(false);
        unchecked
        {
            state.buffers.stencil.SetFunc(Constants.EqualStencilFunc, 1, (int)0xffffffff); // draw if == 1
        }

        state.buffers.stencil.SetOp(Constants.KeepStencilOp, Constants.KeepStencilOp, Constants.KeepStencilOp);
        state.buffers.stencil.SetLocked(true);
    }

    public override void SetSize(float width, float height)
    {
    }
}

public class ClearMaskPass : Pass
{
    public ClearMaskPass()
    {
        NeedsSwap = false;
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        renderer.State.buffers.stencil.SetLocked(false);
        renderer.State.buffers.stencil.SetTest(false);
    }

    public override void SetSize(float width, float height)
    {
    }
}