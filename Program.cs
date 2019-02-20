using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace Raytracing {
    using Vector = Vector<float>;
    class Raytracer {
        public Camera camera {get; set;}
        public World world {get; set;}
        
        // default constructor
        public Raytracer(int width = 800, int height = 800) {
            // initialize default world and Object3Ds
            world = new World(width, height);
            LightSource l1 = new LightSource(Vector.Build.DenseOfArray(new float[] {0.0f, -5.0f, -3.0f}));
            world.AddLightSource(l1);
            // initialize camera
            Vector cameraCenter = Vector.Build.DenseOfArray(new float[] { -0.3f, -0.3f, -3.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] { -0.0f, -0.0f,  0.0f });
            this.camera = new Camera(cameraCenter, cameraLookAt, cameraUp, world);

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world, camera);
            
            // sphere 0
            Vector s0Center = Vector.Build.DenseOfArray(new float[] { -0.33f,  -0.2f,  -1.75f });
            float s0Radius = 0.15f;
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.White };
            float[] s0coefficients = new float[] { 1.0f, 0.2f };
            PhongMaterial s0PhongMaterial = new PhongMaterial(illuminationModel, s0colors, s0coefficients, 1.0f);
            Sphere sphere0 = new Sphere(s0Center, s0Radius, s0PhongMaterial);
            
            // sphere 1
            Vector s1Center = Vector.Build.DenseOfArray(new float[] { -0.1f,  -0.05f,  -1.0f });            
            float s1Radius = 0.15f;
            Rgba32[] s1colors = new Rgba32[] { Rgba32.White, Rgba32.Blue };
            float[] s1coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial s1PhongMaterial = new PhongMaterial(illuminationModel, s1colors, s1coefficients, 1.0f);
            Sphere sphere1 = new Sphere(s1Center, s1Radius, s1PhongMaterial);

            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.5f,  0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f });
            Rgba32[] p0colors = new Rgba32[] { Rgba32.Red, Rgba32.Yellow };
            float[] p0coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial p0PhongMaterial = new PhongMaterial(illuminationModel, p0colors, p0coefficients, 1.0f);

            Plane plane0 = new Plane(p0Center, p0Normal, 2.0f, 10.0f, p0PhongMaterial);
            
            world.AddObject(sphere0);
            world.AddObject(sphere1);
            world.AddObject(plane0);
        
        }

        public Image<Rgba32> Render(string fileName = "") {
            Image<Rgba32> image = new Image<Rgba32>(world.width, world.height);
            float whRatio = (float)world.width / (float)world.height;
            
            // screen coordinates
            float[] S = {
                -1.0f, -1.0f / whRatio, // x0, y0
                    1.0f,  1.0f / whRatio  // x1, y1
            };
            // variables to increment x and y screen coordinates
            float x_inc = (S[2] - S[0]) / image.Width;
            float y_inc = (S[3] - S[1]) / image.Height;

            // Parallel.For loop takes in a min, max, and a delegate/lambda function to execute
            Parallel.For(0, image.Width, x => { 
                    float x_s = S[0] + (x_inc * x);
                    for(int y = 0; y < image.Height; y++) {
                        float y_s = S[1] + (y_inc * y);
                        var color = camera.CastRay(world, x_s, y_s);
                        image[x, y] = color;
                    }
                }
            );
            if(fileName != "") {
                Console.WriteLine("Saving image to " + fileName);
                image.Save(fileName);
            }
            return image;
            
        }
    }
    class Program
    {
        static void Main(string[] args)
        { 
            Console.WriteLine("Initializing Raytracer...");   
           
            int width  = 400, 
                height = 400;
            Raytracer raytracer = new Raytracer(width, height);
            var gifEncode = new GifEncoder();
            Console.WriteLine("Rendering Image...");   
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using(FileStream fs = File.Create("out.gif")) {
                for(var i = 0; i < 24; i++) {
                    raytracer.world.lights[0].position[2] += 0.20f;
                    var image = raytracer.Render("out/img_"+i+".png");
                    System.Console.WriteLine("\r");
                    // gifEncode.Encode<Rgba32>(image, fs);
                }
                fs.Close();
            }
            watch.Stop();
            var time = watch.Elapsed.TotalSeconds;
            Console.WriteLine("Done!"); 
            Console.WriteLine(String.Format("Rendered in: {0:n}s", time));
        }
    }
}
