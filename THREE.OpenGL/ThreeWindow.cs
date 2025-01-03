using System.Collections.Concurrent;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using ErrorCode = OpenTK.Windowing.GraphicsLibraryFramework.ErrorCode;
using static OpenTK.Windowing.GraphicsLibraryFramework.GLFWCallbacks;


namespace THREE;

public readonly struct FramebufferResizeEventArgs
{
    /// <summary>
    ///     Gets the new framebuffer size.
    /// </summary>
    public Vector2i Size { get; }

    /// <summary>
    ///     Gets the new framebuffer width.
    /// </summary>
    public int Width => Size.X;

    /// <summary>
    ///     Gets the new framebuffer height.
    /// </summary>
    public int Height => Size.Y;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FramebufferResizeEventArgs" /> struct.
    /// </summary>
    /// <param name="size">the new framebuffer size.</param>
    public FramebufferResizeEventArgs(Vector2i size)
    {
        Size = size;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="FramebufferResizeEventArgs" /> struct.
    /// </summary>
    /// <param name="width">The new framebuffer width.</param>
    /// <param name="height">The new framebuffer height.</param>
    public FramebufferResizeEventArgs(int width, int height)
        : this(new Vector2i(width, height))
    {
    }
}

public unsafe class ThreeWindow : IThreeWindow, IDisposable
{
    private static readonly ConcurrentQueue<ExceptionDispatchInfo> _callbackExceptions = new();
    private CharCallback _charCallback;
    private CursorEnterCallback _cursorEnterCallback;
    private CursorPosCallback _cursorPosCallback;
    private DropCallback _dropCallback;
    private ErrorCallback _errorCallback;
    private FramebufferSizeCallback _framebufferSizeCallback;
    private JoystickCallback _joystickCallback;
    private KeyCallback _keyCallback;

    private OpenTK.Mathematics.Vector2 _lastReportedMousePos;
    private MouseButtonCallback _mouseButtonCallback;
    private ScrollCallback _scrollCallback;

    private string _title;
    private WindowCloseCallback _windowCloseCallback;
    private WindowFocusCallback _windowFocusCallback;
    private WindowIconifyCallback _windowIconifyCallback;
    private WindowMaximizeCallback _windowMaximizeCallback;
    private WindowPosCallback _windowPosCallback;
    private WindowRefreshCallback _windowRefreshCallback;
    private WindowSizeCallback _windowSizeCallback;
    private WindowState _windowState = WindowState.Normal;

    public ThreeWindow(int width, int height, string title)
    {
        PrepareContext();
        Width = width;
        Height = height;
        Title = title;
        windowPtr = CreateWindow(width, height, title);
    }

    public Vector2i Size
    {
        get
        {
            GLFW.GetWindowFrameSize(windowPtr, out var left, out var top, out var right, out var bottom);
            GLFW.GetWindowSize(windowPtr, out var width, out var height);
            return (width + left + right, height + top + bottom);
        }
        set
        {
            GLFW.GetWindowFrameSize(windowPtr, out var left, out var top, out var right, out var bottom);
            var val = value.X - left - right;
            var val2 = value.Y - top - bottom;
            val = Math.Max(val, 0);
            val2 = Math.Max(val2, 0);
            GLFW.SetWindowSize(windowPtr, val, val2);
        }
    }

    public Vector2i Location
    {
        get
        {
            GLFW.GetWindowFrameSize(windowPtr, out var left, out var top, out _, out _);
            GLFW.GetWindowPos(windowPtr, out var x, out var y);
            return (x - left, y - top);
        }
        set
        {
            GLFW.GetWindowFrameSize(windowPtr, out var left, out var top, out _, out _);
            GLFW.SetWindowPos(windowPtr, value.X + left, value.Y + top);
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            GLFW.SetWindowTitle(windowPtr, value);
            _title = value;
        }
    }

    public bool IsFocused { get; private set; }

    public Window* windowPtr { get; set; }

    public Box2i Bounds
    {
        get => new(Location, Location + Size);
        set
        {
            GLFW.GetWindowFrameSize(windowPtr, out var left, out var top, out var right, out var bottom);
            var num = left + right;
            var num2 = top + bottom;
            GLFW.SetWindowSize(windowPtr, value.Size.X - num, value.Size.Y - num2);
            GLFW.SetWindowPos(windowPtr, value.Min.X + left, value.Min.Y + top);
        }
    }

    public int Width { get; set; }
    public int Height { get; set; }
    public IGraphicsContext Context { get; set; }

    public float AspectRatio
    {
        get
        {
            if (Height == 0) return 1;
            return (float)Width / Height;
        }
    }

    public void MakeCurrent()
    {
        GLFW.MakeContextCurrent(windowPtr);
    }

    public void SwapBuffers()
    {
        GLFW.SwapBuffers(windowPtr);
    }

    public void PollEvents()
    {
        GLFW.PollEvents();
    }

    ~ThreeWindow()
    {
        Dispose(true);
    }

    private void PrepareContext()
    {
        GLFW.WindowHint(WindowHintClientApi.ClientApi, ClientApi.OpenGlApi);
        GLFW.WindowHint(WindowHintInt.ContextVersionMajor, 3);
        GLFW.WindowHint(WindowHintInt.ContextVersionMinor, 3);
        GLFW.WindowHint(WindowHintOpenGlProfile.OpenGlProfile, OpenGlProfile.Core);
        GLFW.WindowHint(WindowHintBool.DoubleBuffer, true);
        GLFW.WindowHint(WindowHintBool.Decorated, true);
    }

    private static void InitializeGlBindings()
    {
        Assembly assembly;
        GLFWBindingsContext provider;
        try
        {
            assembly = Assembly.Load("OpenTK.Graphics");
        }
        catch
        {
            return;
        }

        provider = new GLFWBindingsContext();

        void LoadBindings(string typeNamespace)
        {
            var type = assembly.GetType("OpenTK.Graphics." + typeNamespace + ".GL");
            if (!(type == null))
            {
                var method = type.GetMethod("LoadBindings");
                if (method == null)
                    throw new MissingMethodException(
                        "OpenTK tried to auto-load the OpenGL bindings. We found the OpenTK.Graphics." + typeNamespace +
                        ".GL class, but we could not find the 'LoadBindings' method. If you are trying to run a trimmed assembly please add a [DynamicDependency()] attribute to your program, or set NativeWindowSettings.AutoLoadBindings = false and load the OpenGL bindings manually.");

                method?.Invoke(null, new object[1] { provider });
            }
        }

        LoadBindings("ES11");
        LoadBindings("ES20");
        LoadBindings("ES30");
        LoadBindings("OpenGL");
        LoadBindings("OpenGL4");
    }

    public Vector2 GetMousePosition()
    {
        GLFW.GetCursorPos(windowPtr, out var x, out var y);
        return new Vector2((float)x, (float)y);
    }

    private Window* CreateWindow(int width, int height, string title)
    {
        GLFWProvider.EnsureInitialized();
        // Create window, make the OpenGL context current on the thread, and import graphics functions
        var window = GLFW.CreateWindow(width, height, title, null, (Window*)IntPtr.Zero);
        var primaryMonitor = GLFW.GetPrimaryMonitor();
        GLFW.GetMonitorWorkarea(primaryMonitor, out var sx, out var sy, out var swidth, out var sheight);
        var x = (swidth - width) / 2;
        var y = (sheight - height) / 2;
        GLFW.SetWindowPos(window, x, y);
        GLFW.MakeContextCurrent(window);
        InitializeGlBindings();
        RegisterWindowCallbacks(window);

        GLFW.GetCursorPos(window, out var xPos, out var yPos);
        _lastReportedMousePos = new OpenTK.Mathematics.Vector2((float)xPos, (float)yPos);
        IsFocused = GLFW.GetWindowAttrib(window, WindowAttributeGetBool.Focused);
        return window;
    }

    public virtual void RenderFrame()
    {
        GLFW.PollEvents();
        GLFW.SwapBuffers(windowPtr);
    }

    public void Run()
    {
        OnLoad();
        OnResize(new ResizeEventArgs(Width, Height));

        while (!GLFW.WindowShouldClose(windowPtr)) RenderFrame();
        //ImGui.NewFrame();
        //ImGui.ShowDemoWindow();
        //ImGui.Render();
        //imGuiManager.ImGui_ImplOpenGL3_RenderDrawData(ImGui.GetDrawData());
        //imGuiManager.ImDraw();
        //GLFW.SwapBuffers(windowPtr);
    }


    public virtual void OnLoad()
    {
        Context = new GLFWGraphicsContext(windowPtr);
    }

    private WindowState GetWindowStateFromGLFW()
    {
        if (GLFW.GetWindowAttrib(windowPtr, WindowAttributeGetBool.Iconified)) return WindowState.Minimized;

        if (GLFW.GetWindowAttrib(windowPtr, WindowAttributeGetBool.Maximized)) return WindowState.Maximized;

        if (GLFW.GetWindowMonitor(windowPtr) != null) return WindowState.Fullscreen;

        return WindowState.Normal;
    }

    private void RegisterWindowCallbacks(Window* window)
    {
        _errorCallback = WindowErrorCallback;
        _windowPosCallback = WindowPosCallback;
        _windowSizeCallback = WindowSizeCallback;
        _framebufferSizeCallback = FramebufferSizeCallback;
        _windowCloseCallback = WindowCloseCallback;
        _windowRefreshCallback = WindowRefreshCallback;
        _windowFocusCallback = WindowFocusCallback;
        _windowIconifyCallback = WindowIconifyCallback;
        _windowMaximizeCallback = WindowMaximizeCallback;
        _mouseButtonCallback = MouseButtonCallback;
        _cursorPosCallback = CursorPosCallback;
        _cursorEnterCallback = CursorEnterCallback;
        _scrollCallback = ScrollCallback;
        _keyCallback = KeyCallback;
        _charCallback = CharCallback;
        _dropCallback = DropCallback;

        GLFW.SetWindowPosCallback(window, _windowPosCallback);
        GLFW.SetWindowSizeCallback(window, _windowSizeCallback);
        GLFW.SetFramebufferSizeCallback(window, _framebufferSizeCallback);
        GLFW.SetWindowCloseCallback(window, _windowCloseCallback);
        GLFW.SetWindowRefreshCallback(window, _windowRefreshCallback);
        GLFW.SetWindowFocusCallback(window, _windowFocusCallback);
        GLFW.SetWindowIconifyCallback(window, _windowIconifyCallback);
        GLFW.SetWindowMaximizeCallback(window, _windowMaximizeCallback);
        GLFW.SetMouseButtonCallback(window, _mouseButtonCallback);
        GLFW.SetCursorPosCallback(window, _cursorPosCallback);
        GLFW.SetCursorEnterCallback(window, _cursorEnterCallback);
        GLFW.SetScrollCallback(window, _scrollCallback);
        GLFW.SetKeyCallback(window, _keyCallback);
        GLFW.SetCharCallback(window, _charCallback);
        GLFW.SetDropCallback(window, _dropCallback);

        //_framebufferSizeCallback = WindowFramebufferSizeCallback;
        //_cursorPosCallback = CursorPosCallbakc;
        //_mouseButtonCallback = MouseButtonCallback;
        //GLFW.SetErrorCallback(_errorCallback);
        //GLFW.SetFramebufferSizeCallback(window,_framebufferSizeCallback);
        //GLFW.SetCursorPosCallback(window,_cursorPosCallback);
        //GLFW.SetMouseButtonCallback(window,_mouseButtonCallback);
        //GLFW.SetScrollCallback(window,_scrollCallback);
    }

    protected virtual void OnMove(WindowPositionEventArgs e)
    {
        Move?.Invoke(e);
    }

    private void WindowPosCallback(Window* window, int x, int y)
    {
        try
        {
            OnMove(new WindowPositionEventArgs(x, y));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    private void WindowSizeCallback(Window* window, int width, int height)
    {
        try
        {
            OnResize(new ResizeEventArgs(width, height));
        }
        catch (Exception e)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(e));
        }
    }

    protected virtual void OnClosing(CancelEventArgs e)
    {
        Closing?.Invoke(e);
    }

    private void WindowCloseCallback(Window* window)
    {
        try
        {
            var cancelEventArgs = new CancelEventArgs();
            OnClosing(cancelEventArgs);
            if (cancelEventArgs.Cancel) GLFW.SetWindowShouldClose(windowPtr, false);
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnRefresh()
    {
        Refresh?.Invoke();
    }

    private void WindowRefreshCallback(Window* window)
    {
        try
        {
            OnRefresh();
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnFocusedChanged(FocusedChangedEventArgs e)
    {
        FocusedChanged?.Invoke(e);
        IsFocused = e.IsFocused;
    }

    private void WindowFocusCallback(Window* window, bool focused)
    {
        try
        {
            OnFocusedChanged(new FocusedChangedEventArgs(focused));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnMinimized(MinimizedEventArgs e)
    {
        _windowState = e.IsMinimized ? WindowState.Minimized : GetWindowStateFromGLFW();
        Minimized?.Invoke(e);
    }

    private void WindowIconifyCallback(Window* window, bool iconified)
    {
        try
        {
            OnMinimized(new MinimizedEventArgs(iconified));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnMaximized(MaximizedEventArgs e)
    {
        _windowState = e.IsMaximized ? WindowState.Maximized : GetWindowStateFromGLFW();
        Maximized?.Invoke(e);
    }

    private void WindowMaximizeCallback(Window* window, bool maximized)
    {
        try
        {
            OnMaximized(new MaximizedEventArgs(maximized));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    private void WindowErrorCallback(ErrorCode code, string message)
    {
        Debug.WriteLine(code + "," + message);
    }

    private void FramebufferSizeCallback(Window* window, int width, int height)
    {
        Width = width;
        Height = height;
        OnResize(new ResizeEventArgs(width, height));
    }

    private void CursorPosCallback(Window* window, double posX, double posY)
    {
        try
        {
            var vector = new OpenTK.Mathematics.Vector2((float)posX, (float)posY);
            var delta = vector - _lastReportedMousePos;
            _lastReportedMousePos = vector;
            OnMouseMove(new MouseMoveEventArgs(vector, delta));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void MouseButtonCallback(Window* window, MouseButton button, InputAction action,
        KeyModifiers modes)
    {
        try
        {
            var e = new MouseButtonEventArgs(button, action, modes);
            if (action == InputAction.Release)
                //MouseState[button] = false;
                OnMouseUp(e);
            else
                //MouseState[button] = true;
                OnMouseDown(e);
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnMouseEnter()
    {
        MouseEnter?.Invoke();
    }

    protected virtual void OnMouseLeave()
    {
        MouseLeave?.Invoke();
    }

    private void CursorEnterCallback(Window* window, bool entered)
    {
        try
        {
            if (entered)
                OnMouseEnter();
            else
                OnMouseLeave();
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    private void ScrollCallback(Window* window, double offsetX, double offsetY)
    {
        try
        {
            OnMouseWheel(new MouseWheelEventArgs((float)offsetX, (float)offsetY));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnKeyDown(KeyboardKeyEventArgs e)
    {
        KeyDown?.Invoke(e);
    }

    protected virtual void OnKeyUp(KeyboardKeyEventArgs e)
    {
        KeyUp?.Invoke(e);
    }

    private void KeyCallback(Window* window, Keys key, int scancode, InputAction action, KeyModifiers mods)
    {
        try
        {
            var args = new KeyboardKeyEventArgs(key, scancode, mods, action == InputAction.Repeat);
            if (action == InputAction.Release)
                OnKeyUp(args);
            else
                OnKeyDown(args);
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnTextInput(TextInputEventArgs e)
    {
        TextInput?.Invoke(e);
    }

    private void CharCallback(Window* window, uint codepoint)
    {
        try
        {
            OnTextInput(new TextInputEventArgs((int)codepoint));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnFileDrop(FileDropEventArgs e)
    {
        FileDrop?.Invoke(e);
    }

    private void DropCallback(Window* window, int count, byte** paths)
    {
        try
        {
            var array = new string[count];
            for (var i = 0; i < count; i++) array[i] = Marshal.PtrToStringUTF8((IntPtr)paths[i]);

            OnFileDrop(new FileDropEventArgs(array));
        }
        catch (Exception source)
        {
            _callbackExceptions.Enqueue(ExceptionDispatchInfo.Capture(source));
        }
    }

    protected virtual void OnMouseDown(MouseButtonEventArgs args)
    {
        MouseDown?.Invoke(args);
    }

    protected virtual void OnMouseUp(MouseButtonEventArgs args)
    {
        MouseUp?.Invoke(args);
    }

    protected virtual void OnMouseWheel(MouseWheelEventArgs args)
    {
        MouseWheel?.Invoke(args);
    }

    protected virtual void OnMouseMove(MouseMoveEventArgs args)
    {
        MouseMove?.Invoke(args);
    }

    protected virtual void OnResize(ResizeEventArgs clientSize)
    {
        SizeChanged?.Invoke(clientSize);
    }

    #region Mouse Action

    public event Action<WindowPositionEventArgs> Move;
    public event Action<ResizeEventArgs> SizeChanged;
    public event Action<FramebufferResizeEventArgs> FramebufferResize;
    public event Action Refresh;
    public event Action<CancelEventArgs> Closing;
    public event Action<MinimizedEventArgs> Minimized;
    public event Action<MaximizedEventArgs> Maximized;
    public event Action<JoystickEventArgs> JoystickConnected;
    public event Action<FocusedChangedEventArgs> FocusedChanged;
    public event Action<KeyboardKeyEventArgs> KeyDown;
    public event Action<TextInputEventArgs> TextInput;
    public event Action<KeyboardKeyEventArgs> KeyUp;
    public event Action MouseLeave;
    public event Action MouseEnter;
    public event Action<MouseButtonEventArgs> MouseDown;
    public event Action<MouseButtonEventArgs> MouseUp;
    public event Action<MouseMoveEventArgs> MouseMove;
    public event Action<MouseWheelEventArgs> MouseWheel;
    public event Action<FileDropEventArgs> FileDrop;

    #endregion

    #region Dispose

    private bool _disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (!GLFWProvider.IsOnMainThread)
                throw new GLFWException(
                    "You can only dispose windows on the main thread. The window needs to be disposed as it cannot safely be disposed in the finalizer.");

            GLFW.DestroyWindow(windowPtr);
            _disposedValue = true;
        }
    }

    public virtual void OnDispose()
    {
    }

    public void Dispose()
    {
        OnDispose();
        Dispose(true);
    }

    #endregion
}