using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace Raytracing
{
    class Window
    {
        public Raytracer renderer { get; set; }
        public Window(Raytracer rt) {
            renderer = rt;
        }

        public void Loop() {
            while(true) {
                Gl.Viewport(0, 0, 800, 600);
                Gl.Clear(ClearBufferMask.ColorBufferBit);
                Gl.Begin(PrimitiveType.Triangles);
                Gl.Color3(1.0f, 0.0f, 0.0f); Gl.Vertex2(0.0f, 0.0f);
                Gl.Color3(0.0f, 1.0f, 0.0f); Gl.Vertex2(0.5f, 1.0f);
                Gl.Color3(0.0f, 0.0f, 1.0f); Gl.Vertex2(1.0f, 0.0f);
                Gl.End();
            }
        }
    }
}