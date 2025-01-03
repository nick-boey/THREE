namespace THREE;

[Serializable]
public class GlitchPass : Pass
{
    private float curF;
    private bool goWild;
    private ShaderMaterial material;
    private Random random = new();
    private int randX;
    private GLUniforms uniforms;

    public GlitchPass(float? dt_size = null)
    {
        var shader = new DigitalGlitch();

        uniforms = UniformsUtils.CloneUniforms(shader.Uniforms);

        if (dt_size == null) dt_size = 64;
        if (dt_size != null && dt_size.Value == 0) dt_size = 64;
        (uniforms["tDisp"] as GLUniform)["value"] = GenerateHeightmap(dt_size.Value);

        material = new ShaderMaterial
        {
            Uniforms = uniforms,
            VertexShader = shader.VertexShader,
            FragmentShader = shader.FragmentShader
        };

        fullScreenQuad = new FullScreenQuad(material);

        goWild = false;
        curF = 0;
        GenerateTrigger();
    }

    public override void Render(GLRenderer renderer, GLRenderTarget writeBuffer, GLRenderTarget readBuffer,
        float? deltaTime = null, bool? maskActive = null)
    {
        (uniforms["tDiffuse"] as GLUniform)["value"] = readBuffer.Texture;
        (uniforms["seed"] as GLUniform)["value"] = (float)random.NextDouble(); //default seeding
        (uniforms["byp"] as GLUniform)["value"] = 0;

        if (curF % randX == 0 || goWild)
        {
            (uniforms["amount"] as GLUniform)["value"] = (float)random.NextDouble() / 30;
            (uniforms["angle"] as GLUniform)["value"] = MathUtils.NextFloat((float)-Math.PI, (float)Math.PI);
            (uniforms["seed_x"] as GLUniform)["value"] = MathUtils.NextFloat(-1, 1);
            (uniforms["seed_y"] as GLUniform)["value"] = MathUtils.NextFloat(-1, 1);
            (uniforms["distortion_x"] as GLUniform)["value"] = MathUtils.NextFloat(0, 1);
            (uniforms["distortion_y"] as GLUniform)["value"] = MathUtils.NextFloat(0, 1);
            curF = 0;
            GenerateTrigger();
        }
        else if (curF % randX < randX / 5)
        {
            (uniforms["amount"] as GLUniform)["value"] = (float)random.NextDouble() / 90;
            (uniforms["angle"] as GLUniform)["value"] = MathUtils.NextFloat(-(float)Math.PI, (float)Math.PI);
            (uniforms["distortion_x"] as GLUniform)["value"] = MathUtils.NextFloat(0, 1);
            (uniforms["distortion_y"] as GLUniform)["value"] = MathUtils.NextFloat(0, 1);
            (uniforms["seed_x"] as GLUniform)["value"] = MathUtils.NextFloat(-0.3f, 0.3f);
            (uniforms["seed_y"] as GLUniform)["value"] = MathUtils.NextFloat(-0.3f, 0.3f);
        }
        else if (goWild == false)
        {
            (uniforms["byp"] as GLUniform)["value"] = 1;
        }

        curF++;

        if (RenderToScreen)
        {
            renderer.SetRenderTarget(null);
            fullScreenQuad.Render(renderer);
        }
        else
        {
            renderer.SetRenderTarget(writeBuffer);
            if (Clear) renderer.Clear();
            fullScreenQuad.Render(renderer);
        }
    }

    public override void SetSize(float width, float height)
    {
    }

    private void GenerateTrigger()
    {
        randX = random.Next(120, 240);
    }

    private DataTexture GenerateHeightmap(float dt_size)
    {
        var data_arr = new byte[(int)(dt_size * dt_size * 3)];
        var length = dt_size * dt_size;

        for (var i = 0; i < length; i++)
        {
            var val = (byte)(random.NextDouble() * 255);
            data_arr[i * 3 + 0] = val;
            data_arr[i * 3 + 1] = val;
            data_arr[i * 3 + 2] = val;
        }
        //Bitmap bitmap = new Bitmap((int)dt_size, (int)dt_size, PixelFormat.Format32bppArgb);
        //BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, (int)dt_size, (int)dt_size), System.Drawing.Imaging.ImageLockMode.WriteOnly, bitmap.PixelFormat);
        //IntPtr iptr = bitmapData.Scan0;

        //Marshal.Copy(iptr, data_arr, 0, data_arr.Length);

        //bitmap.UnlockBits(bitmapData);
        var bitmap = data_arr.ToSKBitMap((int)dt_size, (int)dt_size);
        return new DataTexture(bitmap, (int)dt_size, (int)dt_size, Constants.RGBAFormat, Constants.ByteType);
    }
}