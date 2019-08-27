using System;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Graphics.GL46;
using OpenToolkit.Input;

namespace Tests
{
    class Program
    {
        static GameWindow _window;
        static uint _vao, _vbo, _ebo, _program;
        static readonly float[] VertexData =
        {
             0.0f,  0.5f,  0.0f,
             0.0f, -0.5f,  0.0f,
             0.5f,  0.0f,  0.0f
        };
        static readonly uint[] IndexData =
        {
            0, 1, 2
        };

        static readonly string VertexSource = @"
        #version 330 core

        layout(location = 0) in vec3 _pos;
        out vec4 oPos;

        void main()
        {
            gl_Position = vec4(_pos, 1.0);
            oPos = vec4(_pos + vec3(0.5,0.5,0), 1.0);
        }
        ";

        static readonly string FragmentSource = @"
        #version 330 core

        in vec4 oPos;
        out vec4 color;

        void main()
        {
            color = oPos;//vec4(0.8, 0.8, 0.8, 1.0);
        }
        ";

        static void Main(string[] args)
        {
            var settings = new GameWindowSettings {IsSingleThreaded = true}; // multithreaded rendering not yet implemented
            _window = new GameWindow(settings, NativeWindowSettings.Default);
            _window.Load += Load;
            _window.RenderFrame += Render;
            _window.Disposed += Dispose;
            _window.Resize += Resize;

            _window.Run();
        }
        private static unsafe void Load(object sender, EventArgs e)
        {
            GL.GenVertexArrays(1, ref _vao);
            GL.BindVertexArray(_vao);

            GL.GenBuffers(1, ref _vbo);
            GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            fixed (void* data = VertexData)
            {
                GL.BufferData(BufferTargetARB.ArrayBuffer, (UIntPtr)(VertexData.Length * sizeof(float)), data, BufferUsageARB.StaticDraw);
            }
            
            GL.GenBuffers(1, ref _ebo);
            GL.BindBuffer(BufferTargetARB.ElementArrayBuffer, _ebo);
            fixed (void* data = IndexData)
            {
                GL.BufferData(BufferTargetARB.ElementArrayBuffer, (UIntPtr)(IndexData.Length * sizeof(uint)), data, BufferUsageARB.StaticDraw);
            }

            GL.EnableVertexAttribArray(0);

            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, null);


            GL.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);
            
            uint vertex = GL.CreateShader(ShaderType.VertexShader);
            CompileShader(vertex, VertexSource);
            uint fragment = GL.CreateShader(ShaderType.FragmentShader);
            CompileShader(fragment, FragmentSource);
            
            _program = GL.CreateProgram();
            GL.AttachShader(_program, vertex);
            GL.AttachShader(_program, fragment);
            GL.LinkProgram(_program);

            var status = 0;
            GL.GetProgram(_program, ProgramPropertyARB.LinkStatus, ref status);

            if (status != 1)
            {
                throw new Exception("shader linking failed");
            }

            GL.DetachShader(_program, vertex);
            GL.DeleteShader(vertex);
            GL.DetachShader(_program, fragment);
            GL.DeleteShader(fragment);
        }

        static unsafe void CompileShader(uint shader, string source)
        {
            var length = source.Length;
            byte[] sourceBytes = System.Text.Encoding.Default.GetBytes(source);
            fixed (byte* c = sourceBytes)
            {
                GL.ShaderSource(shader, 1, (char**)&c, &length);
            }
            GL.CompileShader(shader);
        }
        private static unsafe void Render(object sender, FrameEventArgs e)
        {
            GL.ClearColor(0.2f, 0.4f, 0.5f, 1f);
            GL.Clear((uint)ClearBufferMask.ColorBufferBit);

            GL.UseProgram(_program);

            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

            _window.SwapBuffers();
        }

        private static void Resize(object sender, ResizeEventArgs e)
        {
        }

        private static unsafe void Dispose(object sender, EventArgs e)
        {
            GL.DeleteBuffers(1,ref _vbo);
            GL.DeleteBuffers(1, ref _ebo);
            GL.DeleteBuffers(1, ref _vao);
            GL.DeleteShader(_program);
        }

    }
}
