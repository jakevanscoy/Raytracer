using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using SixLabors.ImageSharp;
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
            LightSource l1 = new LightSource(Vector.Build.DenseOfArray(new float[] {0.0f, -2.0f, -5.0f}));
            world.AddLightSource(l1);
            // initialize camera
            Vector cameraCenter = Vector.Build.DenseOfArray(new float[] { -0.3f, -0.3f, -3.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] { -0.3f, -0.25f,  0.0f });
            this.camera = new Camera(cameraCenter, cameraLookAt, cameraUp, world);

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world, camera);
            
            // sphere 0
            Vector s0Center = Vector.Build.DenseOfArray(new float[] { -0.33f,  -0.2f,  -1.75f });
            float s0Radius = 0.15f;

            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.Red };
            float[] s0coefficients = new float[] { 1.0f, 1.0f };

            PhongMaterial s0PhongMaterial = new PhongMaterial(illuminationModel, s0colors, s0coefficients, 1.0f);
            Sphere sphere0 = new Sphere(s0Center, s0Radius, s0PhongMaterial);
            
            // sphere 1
            Vector s1Center = Vector.Build.DenseOfArray(new float[] { -0.1f,  -0.05f,  -1.0f });            
            float s1Radius = 0.15f;
            Rgba32[] s1colors = new Rgba32[] { Rgba32.Blue, Rgba32.Green };
            float[] s1coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial s1PhongMaterial = new PhongMaterial(illuminationModel, s1colors, s1coefficients, 1.0f);
            Sphere sphere1 = new Sphere(s1Center, s1Radius, s1PhongMaterial);

            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.5f, 0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] {  0.0f,  1.0f, 0.0f });
            var p0Colors = new Rgba32(0.4f, 0.4f, 0.4f);
            BasicMaterial p0BasicMaterial = new BasicMaterial(p0Colors);
            Plane plane0 = new Plane(p0Center, p0Normal, 2.0f, 10.0f, p0BasicMaterial);
            
            world.AddObject(sphere0);
            world.AddObject(sphere1);
            world.AddObject(plane0);

           
        
        }

        public void RenderImage() {
            using (Image<Rgb48> image = new Image<Rgb48>(world.width, world.height)) {
                float whRatio = (float)world.width / (float)world.height;
                
                // screen coordinates
                float[] S = {
                    -1.0f, -1.0f / whRatio + 0.25f, // x0, y0
                     1.0f,  1.0f / whRatio + 0.25f  // x1, y1
                };
                // variables to increment x and y screen coordinates 
                float x_inc = (S[2] - S[0]) / world.width;
                float y_inc = (S[3] - S[1]) / world.height;

                int i_x = 0; // pixel x-coordinate
                for(float x = S[0]; x < S[2]; x+= x_inc) {
                    int i_y = 0; // pixel y-coordinat
                    for(float y = S[1]; y < S[3]; y+= y_inc) {
                        if(i_x < image.Width && i_y < image.Height){
                            var color = camera.CastRay(world, x, y);
                            Rgb48 color48 = new Rgb48();
                            color.ToRgb48(ref color48);
                            image[i_x, i_y] = color48;
                        }
                        i_y++;
                    }
                    i_x++;
                }
                Console.WriteLine("Saving image to img.png");
                image.Save("img.png");
            }
        }

        public void ParallelRender() {
            using (Image<Rgba32> image = new Image<Rgba32>(world.width, world.height)) {
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
                Console.WriteLine("Saving image to img.png");
                image.Save("img.png");
            }
        }
    }
    class Program
    {
        static void Main(string[] args)
        { 
            Console.WriteLine("Initializing Raytracer...");   
           
            int width  = 200, 
                height = 200;
            Raytracer raytracer = new Raytracer(width, height);
            for(var i = -5.0f; i < 5.0f; i+=0.2f) {
                var newLight = new LightSource(Vector.Build.DenseOfArray(new float[]{0.0f, -5.0f, i}));
                raytracer.world.AddLightSource(newLight);
                Console.WriteLine("Rendering Image...");   
                var watch = System.Diagnostics.Stopwatch.StartNew();
                raytracer.ParallelRender();
                watch.Stop();
                var time = watch.Elapsed.TotalSeconds;
                Console.WriteLine("Done!"); 
                Console.WriteLine(String.Format("Rendered in: {0:n}s", time));
                raytracer.world.lights = new List<LightSource>();
            }
            // Console.WriteLine("Rendering Image...");   
            // var watch = System.Diagnostics.Stopwatch.StartNew();
            // raytracer.ParallelRender();
            // watch.Stop();
            // var time = watch.Elapsed.TotalSeconds;
            // Console.WriteLine("Done!"); 
            // Console.WriteLine(String.Format("Rendered in: {0:n}s", time));
        }
    }
}
