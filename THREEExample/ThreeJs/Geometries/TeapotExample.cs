using System.Collections.Generic;
using ImGuiNET;
using THREE;
using Color = THREE.Color;

namespace THREEExample.Three.Geometries;

[Example("teapot", ExampleCategory.ThreeJs, "geometry")]
public class TeapotExample : Example
{
    private readonly int teapotSize = 400;

    private readonly int[] tessArray = { 2, 3, 4, 5, 6, 8, 10, 15, 20, 30, 40, 50 };
    private AmbientLight ambientLight;
    private bool body = true;
    private bool bottom = true;
    private Color diffuseColor;
    private bool fitLid;
    private MeshPhongMaterial flatMaterial;
    private MeshLambertMaterial gouraudMaterial;
    private float hue = 0.121f;

    private float ka = 0.17f;
    private float kd = 0.51f;
    private float ks = 0.2f;

    private float lhue = 0.04f;
    private bool lid = true;
    private Light light;
    private float lightness = 0.66f;
    private float llightness = 1.0f;
    private float lsaturation = 0.01f;

    private float lx = 0.32f;
    private float ly = 0.39f;
    private float lz = 0.7f;
    private Material[] materialArray;

    private Color materialColor;
    private bool metallic = true;
    private int newShading = 3;
    private int newTess = 7;
    private bool nonblinn;
    private MeshPhongMaterial phongMaterial;
    private MeshPhongMaterial reflectiveMaterial;

    private float saturation = 0.73f;
    private float shininess = 40.0f;
    private Color specularColor;
    private Mesh teapot;
    private CubeTexture textureCube;
    private MeshPhongMaterial texturedMaterial;
    private MeshBasicMaterial wireMaterial;

    public TeapotExample()
    {
        scene.Background = Color.Hex(0xAAAAAA);
    }

    public override void InitCamera()
    {
        camera = new PerspectiveCamera(45, glControl.AspectRatio, 1, 80000);
        camera.Position.Set(-600, 550, 1300);
    }

    public override void InitLighting()
    {
        // LIGHTS
        ambientLight = new AmbientLight(0x333333); // 0.2

        light = new DirectionalLight(0xFFFFFF, 1.0f);

        scene.Add(ambientLight);
        scene.Add(light);
    }

    public override void InitRenderer()
    {
        base.InitRenderer();
        renderer.SetClearColor(Color.Hex(0x000000));
    }

    public override void Init()
    {
        base.Init();

        InitGeometry();

        AddGuiControlsAction = SetupGui;
    }

    private void InitGeometry()
    {
        InitMaterial();


        CreateNewTeapot();

        diffuseColor.SetHSL(hue, saturation, lightness);
    }

    private void InitMaterial()
    {
        var textureMap = TextureLoader.Load("../../../../assets/textures/uv_grid_opengl.jpg");
        textureMap.WrapS = textureMap.WrapT = Constants.RepeatWrapping;
        textureMap.Anisotropy = 16;
        textureMap.Encoding = Constants.sRGBEncoding;

        var path = "../../../../assets/textures/cube/pisa/";
        var urls = new List<string>
        {
            path + "px.png", path + "nx.png",
            path + "py.png", path + "ny.png",
            path + "pz.png", path + "nz.png"
        };

        textureCube = CubeTextureLoader.Load(urls);
        textureCube.Encoding = Constants.sRGBEncoding;


        materialColor.SetRGB(1.0f, 1.0f, 1.0f);

        wireMaterial = new MeshBasicMaterial { Color = Color.Hex(0xFFFFFF), Wireframe = true };

        flatMaterial = new MeshPhongMaterial
            { Color = materialColor, Specular = Color.Hex(0x000000), FlatShading = true, Side = Constants.DoubleSide };

        gouraudMaterial = new MeshLambertMaterial { Color = materialColor, Side = Constants.DoubleSide };

        phongMaterial = new MeshPhongMaterial { Color = materialColor, Side = Constants.DoubleSide };

        texturedMaterial = new MeshPhongMaterial
            { Color = materialColor, Map = textureMap, Side = Constants.DoubleSide };

        reflectiveMaterial = new MeshPhongMaterial
            { Color = materialColor, EnvMap = textureCube, Side = Constants.DoubleSide };

        materialArray = new Material[6]
            { wireMaterial, flatMaterial, gouraudMaterial, phongMaterial, texturedMaterial, reflectiveMaterial };
    }

    private void CreateNewTeapot()
    {
        if (teapot != null)
        {
            teapot.Geometry.Dispose();
            scene.Remove(teapot);
        }

        var teapotGeometry = new TeapotBufferGeometry(teapotSize, tessArray[newTess],
            bottom,
            lid,
            body,
            fitLid,
            !nonblinn);

        teapot = new Mesh(teapotGeometry, materialArray[newShading]);

        if (newShading == 5)
            scene.Background = textureCube;
        else
            scene.Background = null;
        scene.Add(teapot);
    }


    private void SetupGui()
    {
        if (ImGui.TreeNode("Material control"))
        {
            ImGui.SliderFloat("shininess", ref shininess, 1.0f, 400.0f);

            ImGui.SliderFloat("diffuse strength", ref kd, 0.0f, 1.0f);

            ImGui.SliderFloat("specular strength", ref ks, 0.0f, 1.0f);

            ImGui.Checkbox("metallic", ref metallic);

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Material color"))
        {
            ImGui.SliderFloat("hue", ref hue, 0.0f, 1.0f);

            ImGui.SliderFloat("saturation", ref saturation, 0.0f, 1.0f);

            ImGui.SliderFloat("lightness", ref lightness, 0.0f, 1.0f);

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Lighting"))
        {
            ImGui.SliderFloat("lhue", ref lhue, 0.0f, 1.0f);

            ImGui.SliderFloat("lsaturation", ref lsaturation, 0.0f, 1.0f);

            ImGui.SliderFloat("llightness", ref llightness, 0.0f, 1.0f);

            ImGui.SliderFloat("ambient", ref ka, 0.0f, 1.0f);

            ImGui.TreePop();
        }

        if (ImGui.TreeNode("Light direction"))
        {
            ImGui.SliderFloat("lx", ref lx, -1.0f, 1.0f);

            ImGui.SliderFloat("ly", ref ly, -1.0f, 1.0f);

            ImGui.SliderFloat("lz", ref lz, -1.0f, 1.0f);

            ImGui.TreePop();
        }

        var recreateTeapot = false;
        if (ImGui.TreeNode("Tessellation control"))
        {
            if (ImGui.Combo("newTess", ref newTess, "2\03\04\05\06\08\010\015\020\030\040\050\0"))
                recreateTeapot = true;
            if (ImGui.Checkbox("display lid", ref lid)) recreateTeapot = true;
            if (ImGui.Checkbox("display body", ref body)) recreateTeapot = true;
            if (ImGui.Checkbox("display bottom", ref bottom)) recreateTeapot = true;
            if (ImGui.Checkbox("snug lid", ref fitLid)) recreateTeapot = true;
            if (ImGui.Checkbox("original scale", ref nonblinn)) recreateTeapot = true;
            if (ImGui.Combo("newShading", ref newShading, "wireframe\0flat\0smooth\0glossy\0textured\0reflective\0"))
                recreateTeapot = true;
            ImGui.TreePop();
        }

        if (recreateTeapot) CreateNewTeapot();
    }

    public override void Render()
    {
        // skybox is rendered separately, so that it is always behind the teapot.

        if (!imGuiManager.ImWantMouse)
            controls.Enabled = true;
        else
            controls.Enabled = false;

        controls.Update();

        phongMaterial.Shininess = shininess;
        texturedMaterial.Shininess = shininess;

        diffuseColor.SetHSL(hue, saturation, lightness);
        if (metallic)
            // make colors match to give a more metallic look
            specularColor.Copy(diffuseColor);
        else
            // more of a plastic look
            specularColor.SetRGB(1, 1, 1);

        diffuseColor.MultiplyScalar(kd);
        flatMaterial.Color = new Color().Copy(diffuseColor);
        gouraudMaterial.Color = new Color().Copy(diffuseColor);
        phongMaterial.Color = new Color().Copy(diffuseColor);
        texturedMaterial.Color = new Color().Copy(diffuseColor);

        specularColor.MultiplyScalar(ks);
        phongMaterial.Specular.Copy(specularColor);
        texturedMaterial.Specular.Copy(specularColor);

        // Ambient's actually controlled by the light for this demo
        ambientLight.Color.SetHSL(hue, saturation, lightness * ka);

        light.Position.Set(lx, ly, lz);
        light.Color.SetHSL(lhue, lsaturation, llightness);

        base.Render();
    }
}