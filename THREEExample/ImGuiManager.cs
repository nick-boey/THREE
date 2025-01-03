﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace THREEExample.ThreeImGui;

public class ImGuiManager : IDisposable
{
    // ImGui에서 화면 업데이트가 필요한 경우이 이벤트를 부르고 GLControl.Invalidate ()를 실행한다.
    public event EventHandler DrawRequested;

    //public static List<IImGuiDrawable> ImDrawList = new List<IImGuiDrawable>();
    public bool ImWantMouse => ImGui.GetIO().WantCaptureMouse;
    public bool ImWantKeyboard => ImGui.GetIO().WantCaptureKeyboard;

    private int vboHandle;
    private int vbaHandle;
    private int elementsHandle;
    private int attribLocationTex;
    private int attribLocationProjMtx;
    private int attribLocationVtxPos;
    private int attribLocationVtxUV;
    private int attribLocationVtxColor;
    private int shaderProgram;
    private int shader_vs;
    private int shader_fs;

    private int fontTexture;

    // C # sizeof는 얻을 수 없기 때문에 Marshal 분을 사용 
    private readonly int imDrawVertSize = Marshal.SizeOf(default(ImDrawVert));

    // 화면 크기 = GLControl 크기 
    private Vector2 displaySize;
    private bool show_demo_window = true;
    private bool show_another_window;

    private IntPtr koreanGlyph;

    private Vector3 clear_color = new(0.45f, 0.55f, 0.60f);
#if WSL
        public ImGuiManager(IThreeWindow glc)
#else
    public ImGuiManager(GLControl glc)
#endif
    {
        ImGui.CreateContext();
        ImGui.StyleColorsLight();
        var io = ImGui.GetIO();
        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors; // We can honor GetMouseCursor() values (optional)
        io.BackendFlags |=
            ImGuiBackendFlags.HasSetMousePos; // We can honor io.WantSetMousePos requests (optional, rarely used)
        io.BackendFlags |=
            ImGuiBackendFlags
                .RendererHasVtxOffset; // We can honor the ImDrawCmd::VtxOffset field, allowing for large meshes.
        //io.KeyMap[(int)ImGuiKey.Tab] = (int)System.Windows.Forms.Keys.Tab;
        //io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)System.Windows.Forms.Keys.Left;
        //io.KeyMap[(int)ImGuiKey.RightArrow] = (int)System.Windows.Forms.Keys.Right;
        //io.KeyMap[(int)ImGuiKey.UpArrow] = (int)System.Windows.Forms.Keys.Up;
        //io.KeyMap[(int)ImGuiKey.DownArrow] = (int)System.Windows.Forms.Keys.Down;
        //io.KeyMap[(int)ImGuiKey.PageUp] = (int)System.Windows.Forms.Keys.Prior;
        //io.KeyMap[(int)ImGuiKey.PageDown] = (int)System.Windows.Forms.Keys.Next;
        //io.KeyMap[(int)ImGuiKey.Home] = (int)System.Windows.Forms.Keys.Home;
        //io.KeyMap[(int)ImGuiKey.End] = (int)System.Windows.Forms.Keys.End;
        //io.KeyMap[(int)ImGuiKey.Insert] = (int)System.Windows.Forms.Keys.Insert;
        //io.KeyMap[(int)ImGuiKey.Delete] = (int)System.Windows.Forms.Keys.Delete;
        //io.KeyMap[(int)ImGuiKey.Backspace] = (int)System.Windows.Forms.Keys.Back;
        //io.KeyMap[(int)ImGuiKey.Space] = (int)System.Windows.Forms.Keys.Space;
        //io.KeyMap[(int)ImGuiKey.Enter] = (int)System.Windows.Forms.Keys.Return;
        //io.KeyMap[(int)ImGuiKey.Escape] = (int)System.Windows.Forms.Keys.Escape;
        //io.KeyMap[(int)ImGuiKey.A] = (int)System.Windows.Forms.Keys.A;
        //io.KeyMap[(int)ImGuiKey.C] = (int)System.Windows.Forms.Keys.C;
        //io.KeyMap[(int)ImGuiKey.V] = (int)System.Windows.Forms.Keys.V;
        //io.KeyMap[(int)ImGuiKey.X] = (int)System.Windows.Forms.Keys.X;
        //io.KeyMap[(int)ImGuiKey.Y] = (int)System.Windows.Forms.Keys.Y;
        //io.KeyMap[(int)ImGuiKey.Z] = (int)System.Windows.Forms.Keys.Z;

        displaySize.X = glc.Width;
        displaySize.Y = glc.Height;
        io.DisplaySize = displaySize;
        createDeviceObjects();
        createFontsTexture();


        setStyle();
        //WinForm Event
        addControlEvents(glc);
    }

    #region Initialize

#if WSL
        private void addControlEvents(IThreeWindow glc)
#else
    private void addControlEvents(GLControl glc)
#endif
    {
        glc.MouseDown += Glc_MouseDown;
        glc.MouseUp += Glc_MouseUp;
        glc.MouseMove += Glc_MouseMove;
        glc.MouseWheel += Glc_MouseWheel;
        //glc.KeyDown += Glc_KeyDown;
        //glc.KeyUp += Glc_KeyUp;
        glc.SizeChanged += Glc_SizeChanged;
    }


    private void createDeviceObjects()
    {
        int last_texture, last_array_buffer;
        last_texture = GL.GetInteger(GetPName.TextureBinding2D);
        last_array_buffer = GL.GetInteger(GetPName.ArrayBufferBinding);

        const string vertex_shader_glsl_440_core =
            @"#version 310 es
#define attribute in
#define varying out
#define texture2D texture
precision highp float; 
precision highp int; 
#define HIGH_PRECISION
layout (location = 0) in vec2 Position;
layout (location = 1) in vec2 UV;
layout (location = 2) in vec4 Color;
layout (location = 10) uniform mat4 ProjMtx;
out vec2 Frag_UV;
out vec4 Frag_Color;
void main()
{
  Frag_UV = UV;
  Frag_Color = Color;
  gl_Position = ProjMtx * vec4(Position.xy,0,1);
}";
        const string fragment_shader_glsl_440_core =
            @"#version 310 es
#define attribute in
#define varying out
#define texture2D texture
precision highp float; 
precision highp int; 
#define HIGH_PRECISION
in vec2 Frag_UV;
in vec4 Frag_Color;
layout (location = 20) uniform sampler2D Texture;
layout (location = 0) out vec4 Out_Color;
void main()
{
    Out_Color = Frag_Color * texture(Texture, Frag_UV.st);
}";

        shader_vs = GL.CreateShader(ShaderType.VertexShader);
        GL.ShaderSource(shader_vs, vertex_shader_glsl_440_core);
        GL.CompileShader(shader_vs);
        var info = GL.GetShaderInfoLog(shader_vs);
        if (!string.IsNullOrWhiteSpace(info))
            Debug.WriteLine($"GL.CompileShader [VertexShader] had info log: {info}");

        shader_fs = GL.CreateShader(ShaderType.FragmentShader);
        GL.ShaderSource(shader_fs, fragment_shader_glsl_440_core);
        GL.CompileShader(shader_fs);
        info = GL.GetShaderInfoLog(shader_fs);
        if (!string.IsNullOrWhiteSpace(info))
            Debug.WriteLine($"GL.CompileShader [VertexShader] had info log: {info}");
        shaderProgram = GL.CreateProgram();
        GL.AttachShader(shaderProgram, shader_vs);
        GL.AttachShader(shaderProgram, shader_fs);
        GL.LinkProgram(shaderProgram);
        info = GL.GetProgramInfoLog(shaderProgram);
        if (!string.IsNullOrWhiteSpace(info))
            Debug.WriteLine($"GL.LinkProgram had info log: {info}");


        attribLocationTex = 20; //glGetUniformLocation(g_ShaderHandle, "Texture");
        attribLocationProjMtx = 10; //glGetUniformLocation(g_ShaderHandle, "ProjMtx");
        attribLocationVtxPos = 0; // glGetAttribLocation(g_ShaderHandle, "Position");
        attribLocationVtxUV = 1; // glGetAttribLocation(g_ShaderHandle, "UV");
        attribLocationVtxColor = 2; //= glGetAttribLocation(g_ShaderHandle, "Color");
        vboHandle = GL.GenBuffer(); //SetupRenderState
        vbaHandle = GL.GenVertexArray(); //SetupRenderState
        elementsHandle = GL.GenBuffer(); //SetupRenderState

        // Restore modified GL state
        GL.BindTexture(TextureTarget.Texture2D, last_texture);
        GL.BindBuffer(BufferTarget.ArrayBuffer, last_array_buffer);
    }

    private bool createFontsTexture()
    {
        // Build texture atlas
        unsafe
        {
            var io = ImGui.GetIO();
            //Font setup
            var config = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig());
            // fill with data
            config.OversampleH = 2; // 가로 방향의 오버 샘플링 고화질되는 것 같다
            config.OversampleV = 1;
            config.RasterizerMultiply = 1.2f; // 1보다 확대 굵고된다. imGui 글꼴 그리기 앤티가 걸려 엷게 때문에 이것으로 해결
            config.FontNo = 2; // ttc (ttf가 여러 모인 녀석) 파일의 경우이 번호로 폰트를 지정할 수있다. 이 경우 MS UIGothic을 지정 
            config.PixelSnapH = true; // 선이 진하게되면 좋지만 효과 불명 

            //// 샘플 코드 
            //// font = io.Fonts.AddFontFromFileTTF (@ "c : \ windows \ fonts \ msgothic.ttc "12.0f, config, io.Fonts.GetGlyphRangesJapanese ()); 
            //// imgui에서 일본어가"? "가 될 경우의 대처를 적용 (https://qiita.com/benikabocha/items/a25571c1b059eaf952de) 
            //// 다음 클래스를 만드는 
            //// public static readonly ushort [] glyphRangesJapanese = new ushort [] { 
            //// 0x0020, 0x007E, 0x00A2, 0x00A3, 0x00A7 ....}; 
            //IntPtr koreanGlyph = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(ushort)) * FontGlyphs.glyphRangesJapanese.Length);
            //// Copy ()의 인수에 ushort []가 없기 때문에 다음 캐스트로 억지로 통과 
            //Marshal.Copy((short[])(object)FontGlyphs.glyphRangesJapanese, 0, japaneseGlyph, FontGlyphs.glyphRangesJapanese.Length);
            //font = io.Fonts.AddFontFromFileTTF(@"c:\windows\fonts\msgothic.ttc", 12.0f, config, japaneseGlyph);
            ////imgui内部でメモリを直接使用しているらしく、Freeすると落ちる
            ////Marshal.FreeCoTaskMem(ptr);
            config.Destroy();

            byte* pixels;
            int width, height;
            io.Fonts.GetTexDataAsRGBA32(out pixels, out width,
                out height); // Load as RGBA 32-bits (75% of the memory is wasted, but default font is so small) because it is more likely to be compatible with user's existing shaders. If your ImTextureId represent a higher-level concept than just a GL texture id, consider calling GetTexDataAsAlpha8() instead to save on GPU memory.

            // Upload texture to graphics system
            int last_texture;
            GL.GetInteger(GetPName.TextureBinding2D, out last_texture);

            fontTexture = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, fontTexture);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter,
                (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int)TextureMagFilter.Linear);
            //# ifdef GL_UNPACK_ROW_LENGTH
            //        GL.PixelStore(PixelStoreParameter.PackRowLength, 0);
            //#endif
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba,
                PixelType.UnsignedByte, (IntPtr)pixels);

            // Store our identifier
            io.Fonts.TexID = fontTexture;

            // Restore state
            GL.BindTexture(TextureTarget.Texture2D, last_texture);
        }

        return true;
    }

    private void setStyle()
    {
        ImGui.StyleColorsDark();
        var style = ImGui.GetStyle();
        style.Colors[(int)ImGuiCol.WindowBg].W = 0.78f;
        style.Colors[(int)ImGuiCol.FrameBg].W = 0.71f;
        style.Colors[(int)ImGuiCol.ChildBg].W = 0.78f;


        //style.WindowPadding = new System.Numerics.Vector2(5f, 5f);
        //style.FramePadding = new System.Numerics.Vector2(3f, 2f);
        //style.ItemSpacing = new System.Numerics.Vector2(4f, 4f);
        //style.WindowRounding = 4f;
        //style.TabRounding = 2f;
        //style.Colors[(int)ImGuiCol.WindowBg] = new System.Numerics.Vector4(0.94f, 0.94f, 0.94f, 0.78f);
        //style.Colors[(int)ImGuiCol.FrameBg] = new System.Numerics.Vector4(1.00f, 1.00f, 1.00f, 0.71f);
        //style.Colors[(int)ImGuiCol.ChildBg] = ImGui.ColorConvertU32ToFloat4(0x0f000000);
    }

    #endregion Initialize

    #region Control events

#if WSL
        private void Glc_MouseMove(MouseMoveEventArgs e)
        {
            System.Numerics.Vector2 mousePos = new System.Numerics.Vector2(e.X, e.Y);
            var io = ImGui.GetIO();
            io.MousePos = mousePos;
            DrawRequested?.Invoke(this, null);           
        }

        private void Glc_MouseUp(MouseButtonEventArgs e)
        {
            var io = ImGui.GetIO();
            int button = 0;
            button = getButtonNo(e, button);
            io.MouseDown[button] = false;
            DrawRequested?.Invoke(this, null);
        }

        private void Glc_MouseDown(MouseButtonEventArgs args)
        {
            var io = ImGui.GetIO();
            int button = 0;
            
            button = getButtonNo(args, button);
            io.MouseDown[button] = true;
            DrawRequested?.Invoke(this, null);
        }

        private void Glc_MouseWheel(MouseWheelEventArgs e)
        {
            var io = ImGui.GetIO();
            if (!io.WantCaptureMouse)
                return;
            io.MouseWheel += e.OffsetY / 120.0f;
            DrawRequested?.Invoke(this, null);
        }

        //private void Glc_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    var io = ImGui.GetIO();
        //    if (e.KeyValue < 256)
        //    {
        //        io.KeysDown[e.KeyValue] = true;

        //        //io.AddInputCharacter((uint)e.KeyValue);
        //    }
        //    io.KeyAlt = e.Alt;
        //    io.KeyCtrl = e.Control;
        //    io.KeyShift = e.Shift;
        //    DrawRequested?.Invoke(this, null);
        //}

        private void Glc_CharInputed(object sender, char e)
        {
            var io = ImGui.GetIO();
            io.AddInputCharacter(e);
        }
        //private void Glc_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        //{
        //    var io = ImGui.GetIO();
        //    if (e.KeyValue < 256)
        //        io.KeysDown[e.KeyValue] = false;
        //    io.KeyAlt = e.Alt;
        //    io.KeyCtrl = e.Control;
        //    io.KeyShift = e.Shift;
        //    DrawRequested?.Invoke(this, null);
        //}
        private void Glc_SizeChanged(ResizeEventArgs e)
        {
            var io = ImGui.GetIO();
            displaySize.X = e.Width;
            displaySize.Y = e.Height;
            io.DisplaySize = displaySize;
        }



        private static int getButtonNo(MouseButtonEventArgs e, int button)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    button = 1;
                    break;
                case MouseButton.Middle:
                    button = 2;
                    break;
                case MouseButton.Button4:
                    button = 3;
                    break;               
                default:
                    break;
            }

            return button;
        }

#else
    private static int getButtonNo(MouseEventArgs e, int button)
    {
        switch (e.Button)
        {
            case MouseButtons.Right:
                button = 1;
                break;
            case MouseButtons.Middle:
                button = 2;
                break;
            case MouseButtons.XButton1:
                button = 3;
                break;
            case MouseButtons.XButton2:
                button = 4;
                break;
            case MouseButtons.Left:
            case MouseButtons.None:
            default:
                break;
        }

        return button;
    }

    private void Glc_MouseMove(object sender, MouseEventArgs e)
    {
        var mousePos = new Vector2(e.X, e.Y);
        var io = ImGui.GetIO();
        io.MousePos = mousePos;
        DrawRequested?.Invoke(this, null);
    }

    private void Glc_MouseUp(object sender, MouseEventArgs e)
    {
        var io = ImGui.GetIO();
        var button = 0;
        button = getButtonNo(e, button);
        io.MouseDown[button] = false;
        DrawRequested?.Invoke(this, null);
    }

    private void Glc_MouseDown(object sender, MouseEventArgs e)
    {
        var io = ImGui.GetIO();
        var button = 0;
        button = getButtonNo(e, button);
        io.MouseDown[button] = true;
        DrawRequested?.Invoke(this, null);
    }

    private void Glc_MouseWheel(object sender, MouseEventArgs e)
    {
        var io = ImGui.GetIO();
        if (!io.WantCaptureMouse)
            return;
        io.MouseWheel += e.Delta / 120.0f;
        DrawRequested?.Invoke(this, null);
    }

    private void Glc_SizeChanged(object sender, EventArgs e)
    {
        var io = ImGui.GetIO();
        displaySize.X = ((Control)sender).Width;
        displaySize.Y = ((Control)sender).Height;
        io.DisplaySize = displaySize;
    }
#endif

    #endregion Control events

    #region Draw call

    private void ImGui_ImplOpenGL3_SetupRenderState(ImDrawDataPtr draw_data, uint vertex_array_object)
    {
        // Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, polygon fill
        GL.Enable(EnableCap.Blend);
        GL.BlendEquation(BlendEquationMode.FuncAdd);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Disable(EnableCap.CullFace);
        GL.Disable(EnableCap.DepthTest);
        GL.Enable(EnableCap.ScissorTest);
        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

        // Setup viewport, orthographic projection matrix
        // Our visible imgui space lies from draw_data->DisplayPos (top left) to draw_data->DisplayPos+data_data->DisplaySize (bottom right). DisplayPos is (0,0) for single viewport apps.
        //glViewport(0, 0, (GLsizei)fb_width, (GLsizei)fb_height);
        var L = draw_data.DisplayPos.X;
        var R = draw_data.DisplayPos.X + draw_data.DisplaySize.X;
        var T = draw_data.DisplayPos.Y;
        var B = draw_data.DisplayPos.Y + draw_data.DisplaySize.Y;
        var ortho_projection = new Matrix4(
            2.0f / (R - L), 0.0f, 0.0f, 0.0f,
            0.0f, 2.0f / (T - B), 0.0f, 0.0f,
            0.0f, 0.0f, -1.0f, 0.0f,
            (R + L) / (L - R), (T + B) / (B - T), 0.0f, 1.0f
        );

        GL.UseProgram(shaderProgram);
        GL.Uniform1(attribLocationTex, 0);
        GL.UniformMatrix4(attribLocationProjMtx, false, ref ortho_projection);
        GL.BindSampler(0,
            0); // We use combined texture/sampler state. Applications using GL 3.3 may set that otherwise.

        // Bind vertex/index buffers and setup attributes for ImDrawVert
        GL.BindVertexArray(vbaHandle);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vboHandle);
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementsHandle);
        GL.EnableVertexAttribArray(attribLocationVtxPos);
        GL.EnableVertexAttribArray(attribLocationVtxUV);
        GL.EnableVertexAttribArray(attribLocationVtxColor);

        GL.VertexAttribPointer(attribLocationVtxPos, 2, VertexAttribPointerType.Float, false, imDrawVertSize, 0);
        GL.VertexAttribPointer(attribLocationVtxUV, 2, VertexAttribPointerType.Float, false, imDrawVertSize, 8);
        GL.VertexAttribPointer(attribLocationVtxColor, 4, VertexAttribPointerType.UnsignedByte, true, imDrawVertSize,
            16);
    }

    public void ImGui_ImplOpenGL3_RenderDrawData(ImDrawDataPtr draw_data)
    {
        // Backup GL state
        var last_active_texture = GL.GetInteger(GetPName.ActiveTexture);
        var last_program = GL.GetInteger(GetPName.CurrentProgram);
        var last_texture = GL.GetInteger(GetPName.TextureBinding2D);
        var last_sampler = GL.GetInteger(GetPName.SamplerBinding);
        var last_array_buffer = GL.GetInteger(GetPName.ColorArrayBufferBinding);
        var last_polygon_mode = new int[2];
        GL.GetInteger(GetPName.PolygonMode, last_polygon_mode);
        //int[] last_viewport = new int[4]; GL.GetInteger(GetPName.Viewport, last_viewport);
        var last_scissor_box = new int[4];
        GL.GetInteger(GetPName.ScissorBox, last_scissor_box);
        var last_blend_src_rgb = GL.GetInteger(GetPName.BlendSrcRgb);
        var last_blend_dst_rgb = GL.GetInteger(GetPName.BlendDstRgb);
        var last_blend_src_alpha = GL.GetInteger(GetPName.BlendSrcAlpha);
        var last_blend_dst_alpha = GL.GetInteger(GetPName.BlendDstAlpha);
        var last_blend_equation_rgb = GL.GetInteger(GetPName.BlendEquationRgb);
        var last_blend_equation_alpha = GL.GetInteger(GetPName.BlendEquationAlpha);
        var last_enable_blend = GL.IsEnabled(EnableCap.Blend);
        var last_enable_cull_face = GL.IsEnabled(EnableCap.CullFace);
        var last_enable_depth_test = GL.IsEnabled(EnableCap.DepthTest);
        var last_enable_scissor_test = GL.IsEnabled(EnableCap.ScissorTest);
        var clip_origin_lower_left = true;
        GL.ActiveTexture(TextureUnit.Texture0);


        // Setup desired GL state
        // Recreate the VAO every time (this is to easily allow multiple GL contexts to be rendered to. VAO are not shared among GL contexts)
        // The renderer would actually work without any VAO bound, but then our VertexAttrib calls would overwrite the default one currently bound.
        uint vertex_array_object = 0;
        ImGui_ImplOpenGL3_SetupRenderState(draw_data, vertex_array_object);

        // Will project scissor/clipping rectangles into framebuffer space
        var clip_off = draw_data.DisplayPos; // (0,0) unless using multi-viewports
        var clip_scale = draw_data.FramebufferScale; // (1,1) unless using retina display which are often (2,2)

        // Render command lists
        for (var n = 0; n < draw_data.CmdListsCount; n++)
        {
            var cmd_list = draw_data.CmdLists[n];


            GL.BufferData(BufferTarget.ArrayBuffer, cmd_list.VtxBuffer.Size * imDrawVertSize, cmd_list.VtxBuffer.Data,
                BufferUsageHint.StreamDraw);
            GL.BufferData(BufferTarget.ElementArrayBuffer, cmd_list.IdxBuffer.Size * sizeof(ushort),
                cmd_list.IdxBuffer.Data, BufferUsageHint.StreamDraw);

            for (var cmd_i = 0; cmd_i < cmd_list.CmdBuffer.Size; cmd_i++)
            {
                var pcmd = cmd_list.CmdBuffer[cmd_i];
                if (pcmd.UserCallback != IntPtr.Zero)
                {
                    // User callback, registered via ImDrawList::AddCallback()
                    // (ImDrawCallback_ResetRenderState is a special callback value used by the user to request the renderer to reset render state.)
                    //if (pcmd.UserCallback == ImGui. ImDrawCallback_ResetRenderState)
                    //  ImGui_ImplOpenGL3_SetupRenderState(draw_data, fb_width, fb_height, vertex_array_object);
                    //else
                    //pcmd->UserCallback(cmd_list, pcmd);
                    Debug.WriteLine("UserCallback" + pcmd.UserCallback);
                }
                else
                {
                    // Project scissor/clipping rectangles into framebuffer space
                    var clip_rect = new Vector4();
                    clip_rect.X = (pcmd.ClipRect.X - clip_off.X) * clip_scale.X;
                    clip_rect.Y = (pcmd.ClipRect.Y - clip_off.Y) * clip_scale.Y;
                    clip_rect.Z = (pcmd.ClipRect.Z - clip_off.X) * clip_scale.X;
                    clip_rect.W = (pcmd.ClipRect.W - clip_off.Y) * clip_scale.Y;

                    if (clip_rect.X < displaySize.X && clip_rect.Y < displaySize.Y && clip_rect.Z >= 0.0f &&
                        clip_rect.W >= 0.0f)
                    {
                        // Apply scissor/clipping rectangle
                        if (clip_origin_lower_left)
                            GL.Scissor((int)clip_rect.X, (int)(displaySize.Y - clip_rect.W),
                                (int)(clip_rect.Z - clip_rect.X), (int)(clip_rect.W - clip_rect.Y));
                        else

                            GL.Scissor((int)clip_rect.X, (int)clip_rect.Y, (int)clip_rect.Z,
                                (int)clip_rect.W); // Support for GL 4.5 rarely used glClipControl(GL_UPPER_LEFT)

                        // Bind texture, Draw
                        GL.BindTexture(TextureTarget.Texture2D, pcmd.TextureId.ToInt32());

                        GL.DrawElementsBaseVertex(PrimitiveType.Triangles, (int)pcmd.ElemCount,
                            DrawElementsType.UnsignedShort, new IntPtr(pcmd.IdxOffset * sizeof(ushort)),
                            (int)pcmd.VtxOffset);
                        //If glDrawElementsBaseVertex not supported
                        //GL.DrawElements(BeginMode.Triangles, pcmd.ElemCount, sizeof(ImDrawIdx) == 2 ? GL_UNSIGNED_SHORT : GL_UNSIGNED_INT, (void*)(intptr_t)(pcmd->IdxOffset * sizeof(ImDrawIdx)));
                    }
                }
            }
        }


        // Restore modified GL state
        GL.UseProgram(last_program);
        GL.BindTexture(TextureTarget.Texture2D, last_texture);
        GL.BindSampler(0, last_sampler);
        GL.ActiveTexture((TextureUnit)last_active_texture);
        GL.BindBuffer(BufferTarget.ArrayBuffer, last_array_buffer);
        GL.BlendEquationSeparate((BlendEquationMode)last_blend_equation_rgb,
            (BlendEquationMode)last_blend_equation_alpha);
        GL.BlendFuncSeparate((BlendingFactorSrc)last_blend_src_rgb, (BlendingFactorDest)last_blend_dst_rgb,
            (BlendingFactorSrc)last_blend_src_alpha, (BlendingFactorDest)last_blend_dst_alpha);
        if (last_enable_blend) GL.Enable(EnableCap.Blend);
        else GL.Disable(EnableCap.Blend);
        if (last_enable_cull_face) GL.Enable(EnableCap.CullFace);
        else GL.Disable(EnableCap.CullFace);
        if (last_enable_depth_test) GL.Enable(EnableCap.DepthTest);
        else GL.Disable(EnableCap.DepthTest);
        if (last_enable_scissor_test) GL.Enable(EnableCap.ScissorTest);
        else GL.Disable(EnableCap.ScissorTest);
        GL.PolygonMode(MaterialFace.FrontAndBack, (PolygonMode)last_polygon_mode[0]);
        //GL.Viewport(last_viewport[0], last_viewport[1], last_viewport[2], last_viewport[3]);
        GL.Scissor(last_scissor_box[0], last_scissor_box[1], last_scissor_box[2], last_scissor_box[3]);
        GL.DisableVertexAttribArray(attribLocationVtxPos);
        GL.DisableVertexAttribArray(attribLocationVtxUV);
        GL.DisableVertexAttribArray(attribLocationVtxColor);
    }

    #endregion Draw call


    #region sample code

    private float f;
    private int counter;
    private ImFontPtr font;

    public void ImDraw()
    {
        ImGui.NewFrame();
        // 1. Show the big demo window (Most of the sample code is in ImGui::ShowDemoWindow()! You can browse its code to learn more about Dear ImGui!).
        if (show_demo_window)
            ImGui.ShowDemoWindow();
        // 2. Show a simple window that we create ourselves. We use a Begin/End pair to created a named window.
        {
            ImGui.Begin("Hello, world!"); // Create a window called "Hello, world!" and append into it.
            //ImGui.PushFont(font);

            ImGui.Text("This is some useful text."); // Display some text (you can use a format strings too)
            ImGui.Checkbox("Demo Window", ref show_demo_window); // Edit bools storing our window open/close state
            ImGui.Checkbox("Another Window", ref show_another_window);

            ImGui.SliderFloat("float", ref f, 0.0f, 1.0f); // Edit 1 float using a slider from 0.0f to 1.0f
            ImGui.ColorEdit3("clear color", ref clear_color); // Edit 3 floats representing a color

            if (ImGui.Button(
                    "Button")) // Buttons return true when clicked (most widgets return true when edited/activated)
                counter++;
            ImGui.SameLine();
            ImGui.Text($"counter = {counter}");

            ImGui.Text(
                $"Application average {1000.0f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate} FPS)");
            //ImGui.PopFont();
            ImGui.End();
        }

        // 3. Show another simple window.
        if (show_another_window)
        {
            ImGui.Begin("Another Window",
                ref show_another_window); // Pass a pointer to our bool variable (the window will have a closing button that will clear the bool when clicked)
            ImGui.Text("Hello from another window!");
            if (ImGui.Button("Close Me"))
                show_another_window = false;
            ImGui.End();
        }

        // Rendering
        ImGui.Render();
        // OpenGL 화면 지우기 등은 상위 모듈에서 실시 
        //GL.ClearColor(glc.BackColor);
        //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());

        // OpenGL의 Swap 상위 모듈에서 실시 
        //glc.SwapBuffers();
    }

    #endregion sample code


    #region Destroy

    private void destroyDeviceObjects()
    {
        GL.DeleteVertexArray(vbaHandle);
        GL.DeleteBuffer(vboHandle);
        GL.DeleteBuffer(elementsHandle);
        GL.DetachShader(shaderProgram, shader_vs);
        GL.DetachShader(shaderProgram, shader_fs);
        GL.DeleteProgram(shaderProgram);
    }

    private void destroyFontsTexture()
    {
        var io = ImGui.GetIO();
        GL.DeleteTexture(fontTexture);
        //if(io.Fonts.)
        io.Fonts.TexID = IntPtr.Zero;
        fontTexture = 0;
    }


    public void Dispose()
    {
        destroyFontsTexture();
        destroyDeviceObjects();
        ImGui.DestroyContext();
    }

    #endregion
}