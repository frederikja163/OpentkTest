using System;
using OpenToolkit.Windowing.Desktop;
using OpenToolkit.Windowing.Common;
using OpenToolkit.Graphics.GL_VERSION_4_6;
using OpenToolkit.Input;
using AdvancedDLSupport;

namespace Tests
{
    class Program
    {
        static IGL _gl;
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
        #Version 330 core

        layout(location = 0) vec3 _pos;

        void main()
        {
            gl_position = vec4(_pos, 0);
        }
        ";

        static readonly string FragmentSource = @"
        #Version 330 core

        out vec4 color;

        void main()
        {
            color = vec4(0.8, 0.2, 0.4);
        }
        ";

        static void Main(string[] args)
        {
            _window = new GameWindow(GameWindowSettings.Default, NativeWindowSettings.Default);
            _window.Load += Load;
            _window.RenderFrame += Render;
            _window.Disposed += Dispose;
            _window.Resize += Resize;

            _window.Run();
        }
        private unsafe static void Load(object sender, EventArgs e)
        {
            _gl = new NativeLibraryBuilder(ImplementationOptions.SuppressSecurity |
                                               ImplementationOptions.GenerateDisposalChecks |
                                               ImplementationOptions.UseLazyBinding |
                                               ImplementationOptions.EnableOptimizations).ActivateInterface<IGL>(new OpenGLLibraryNameContainer().GetLibraryName());

            fixed (uint* vbo = &_vbo)
            {
                _gl.GenBuffers(1, vbo);
                _gl.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, _vbo);
                fixed (void* data = &VertexData[0])
                {
                    _gl.BufferData(BufferTargetARB.GL_ARRAY_BUFFER, (UIntPtr)(VertexData.Length * sizeof(float)), data, BufferUsageARB.GL_STATIC_DRAW);
                }
            }

            fixed (uint* ebo = &_ebo)
            {
                _gl.GenBuffers(1, ebo);
                _gl.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, _vbo);
                fixed (void* data = &IndexData[0])
                {
                    _gl.BufferData(BufferTargetARB.GL_ARRAY_BUFFER, (UIntPtr)(IndexData.Length * sizeof(uint)), data, BufferUsageARB.GL_STATIC_DRAW);
                }
            }

            fixed (uint* vao = &_vao)
            {
                _gl.GenBuffers(1, vao);
                _gl.BindVertexArray(_vao);
                _gl.BindBuffer(BufferTargetARB.GL_ARRAY_BUFFER, _vbo);
                _gl.BindBuffer(BufferTargetARB.GL_ELEMENT_ARRAY_BUFFER, _ebo);

                _gl.EnableVertexAttribArray(0);
                _gl.VertexAttribPointer(0, 3, VertexAttribPointerType.GL_FLOAT, true, 0, null);
            }

            uint vertex = GL.CreateShader(ShaderType.GL_VERTEX_SHADER);
            char[] charArray = VertexSource.ToCharArray();
            int length = VertexSource.Length;
            fixed (char* c = &charArray[0])
            {
                _gl.ShaderSource(vertex, 1, &c, &length);
            }

            uint fragment = GL.CreateShader(ShaderType.GL_FRAGMENT_SHADER);
            charArray = FragmentSource.ToCharArray();
            length = VertexSource.Length;
            fixed (char* c = &charArray[0])
            {
                _gl.ShaderSource(fragment, 1, &c, &length);
            }

            _program = GL.CreateProgram();
            _gl.AttachShader(_program, vertex);
            _gl.AttachShader(_program, fragment);
            _gl.LinkProgram(_program);
            _gl.DetachShader(_program, vertex);
            _gl.DeleteShader(vertex);
            _gl.DetachShader(_program, fragment);
            _gl.DeleteShader(fragment);
        }

        private unsafe static void Render(object sender, FrameEventArgs e)
        {
            _gl.Clear((uint)ClearBufferMask.GL_COLOR_BUFFER_BIT);

            _gl.BindVertexArray(_vao);
            _gl.UseProgram(_program);

            _gl.DrawElements(PrimitiveType.GL_TRIANGLES, (uint)IndexData.Length, DrawElementsType.GL_UNSIGNED_INT, null);

            _window.SwapBuffers();
        }

        private static void Resize(object sender, ResizeEventArgs e)
        {

        }

        private unsafe static void Dispose(object sender, EventArgs e)
        {
            fixed (uint* vbo = &_vbo)
            {
                _gl.DeleteBuffers(1, vbo);
            }
            fixed (uint* ebo = &_ebo)
            {
                _gl.DeleteBuffers(1, ebo);
            }
            fixed (uint* vao = &_vao)
            {
                _gl.DeleteBuffers(1, vao);
            }
            _gl.DeleteShader(_program);
        }

    }
}
