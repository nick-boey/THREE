﻿namespace THREE;

public class CubeCamera : Camera
{
    private readonly PerspectiveCamera cameraNX;
    private readonly PerspectiveCamera cameraNY;
    private readonly PerspectiveCamera cameraNZ;
    private readonly PerspectiveCamera cameraPX;
    private readonly PerspectiveCamera cameraPY;
    private readonly PerspectiveCamera cameraPZ;
    public GLRenderTarget RenderTarget;

    public CubeCamera(float near, float far, GLRenderTarget renderTarget)
    {
        Fov = 90.0f;
        Aspect = 1.0f;
        RenderTarget = renderTarget;

        cameraPX = new PerspectiveCamera(Fov, Aspect, near, far);
        cameraPX.Layers = Layers;
        cameraPX.Up.Set(0, -1, 0);
        cameraPX.LookAt(new Vector3(1, 0, 0));
        Add(cameraPX);

        cameraNX = new PerspectiveCamera(Fov, Aspect, near, far);
        cameraNX.Layers = Layers;
        cameraNX.Up.Set(0, -1, 0);
        cameraNX.LookAt(new Vector3(-1, 0, 0));
        Add(cameraNX);

        cameraPY = new PerspectiveCamera(Fov, Aspect, near, far);
        cameraPY.Layers = Layers;
        cameraPY.Up.Set(0, 0, 1);
        cameraPY.LookAt(new Vector3(0, 1, 0));
        Add(cameraPY);

        cameraNY = new PerspectiveCamera(Fov, Aspect, near, far);
        cameraNY.Layers = Layers;
        cameraNY.Up.Set(0, 0, -1);
        cameraNY.LookAt(new Vector3(0, -1, 0));
        Add(cameraNY);

        cameraPZ = new PerspectiveCamera(Fov, Aspect, near, far);
        cameraPZ.Layers = Layers;
        cameraPZ.Up.Set(0, -1, 0);
        cameraPZ.LookAt(new Vector3(0, 0, 1));
        Add(cameraPZ);

        cameraNZ = new PerspectiveCamera(Fov, Aspect, near, far);
        cameraNZ.Layers = Layers;
        cameraNZ.Up.Set(0, -1, 0);
        cameraNZ.LookAt(new Vector3(0, 0, -1));
        Add(cameraNZ);
    }

    public void Update(IGLRenderer renderer, Scene scene)
    {
        if (Parent == null) UpdateMatrixWorld();

        //var currentXrEnabled = renderer.xr.enabled;
        var currentRenderTarget = renderer.GetRenderTarget();

        //renderer.xr.enabled = false;

        var generateMipmaps = RenderTarget.Texture.GenerateMipmaps;

        RenderTarget.Texture.GenerateMipmaps = false;

        renderer.SetRenderTarget(RenderTarget, 0);
        renderer.Render(scene, cameraPX);

        renderer.SetRenderTarget(RenderTarget, 1);
        renderer.Render(scene, cameraNX);

        renderer.SetRenderTarget(RenderTarget, 2);
        renderer.Render(scene, cameraPY);

        renderer.SetRenderTarget(RenderTarget, 3);
        renderer.Render(scene, cameraNY);

        renderer.SetRenderTarget(RenderTarget, 4);
        renderer.Render(scene, cameraPZ);

        RenderTarget.Texture.GenerateMipmaps = generateMipmaps;

        renderer.SetRenderTarget(RenderTarget, 5);
        renderer.Render(scene, cameraNZ);

        renderer.SetRenderTarget(currentRenderTarget);

        //renderer.xr.enabled = currentXrEnabled;
    }
}