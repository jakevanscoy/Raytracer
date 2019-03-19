using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using Newtonsoft.Json;

namespace Raytracing {
    
    using Vector = Vector<float>;

    public class SceneFactory {
        public static World GetDefaultWorld(int width, int height) {

            World world = new World(width, height);
            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] { 0.0f, -2.0f, -2.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 7.5f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] { -0.3f, -0.3f, -3.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras[0] = new Camera(cameraPos, cameraLookAt, cameraUp, world);

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            
            // sphere 0
            Vector s0Center = Vector.Build.DenseOfArray(new float[] { -0.33f,  -0.2f,  -1.75f });
            float s0Radius = 0.15f;
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.Red };
            float[] s0coefficients = new float[] { 1.0f, 0.5f };
            PhongMaterial s0PhongMaterial = new PhongMaterial(illuminationModel, s0colors, s0coefficients, 5.0f);
            Sphere sphere0 = new Sphere(s0Center, s0Radius, s0PhongMaterial);
            
            // sphere 1
            Vector s1Center = Vector.Build.DenseOfArray(new float[] { -0.1f,  -0.05f,  -1.0f });            
            float s1Radius = 0.15f;
            Rgba32[] s1colors = new Rgba32[] { Rgba32.Red, Rgba32.Blue };
            float[] s1coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial s1PhongMaterial = new PhongMaterial(illuminationModel, s1colors, s1coefficients, 1.0f);
            CheckerboardMaterial checkerMaterial = new CheckerboardMaterial(s1PhongMaterial, s0PhongMaterial, 1.0f);
            Sphere sphere1 = new Sphere(s1Center, s1Radius, checkerMaterial);

            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.5f,  0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] { 0.0f, -1.0f,  0.0f });
            Rgba32[] p0colors = new Rgba32[] { Rgba32.Red, Rgba32.Blue };
            float[] p0coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial p0PhongMaterial = new PhongMaterial(illuminationModel, p0colors, p0coefficients, 5.0f);
            Plane plane0 = new Plane(p0Center, p0Normal, 2.0f, 10.0f, checkerMaterial);

            // plane1 (background)
            Vector p1Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, 5.0f });
            Vector p1Normal = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, -1.0f });
            Rgba32[] p1colors = new Rgba32[] { Rgba32.Purple, Rgba32.White };
            float[] p1coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial p1PhongMaterial = new PhongMaterial(illuminationModel, p1colors, p1coefficients, 0.001f);
            Plane plane1 = new Plane(p1Center, p1Normal, 5.0f, 5.0f, p0PhongMaterial);
            
            world.AddObject(sphere0);
            world.AddObject(sphere1);
            world.AddObject(plane0);
            world.AddObject(plane1);

            return world;
        }

        public static World GetWorldFromFile(string filename) {
            
            using(FileStream fs = File.OpenRead(filename)) {
                var fbytes = new byte[fs.Length];
                fs.Read(fbytes, 0, (int)fs.Length);
                JsonConvert.DeserializeObject(fbytes.ToString());
            }

            return new World(0, 0);
        }

    }

}