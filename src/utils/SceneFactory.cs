using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing {
    
    using Vector = Vector<float>;

    public class SceneFactory {
        public static World GetDefaultWorld(int width, int height) {

            World world = new World(width, height);
            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] { -0.0f, -1.0f, -5.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 7.5f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] { -0.3f, -0.3f, -3.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            
            // sphere 0
            Vector s0Center = Vector.Build.DenseOfArray(new float[] { -0.33f,  -0.2f,  -1.75f });
            float s0Radius = 0.15f;
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.White };
            float[] s0coefficients = new float[] { 1.0f, 0.5f };
            PhongMaterial s0PhongMaterial = new PhongMaterial(illuminationModel, s0colors, s0coefficients, 5.0f);
            Sphere sphere0 = new Sphere(s0Center, s0Radius, s0PhongMaterial);
            
            // sphere 1
            Vector s1Center = Vector.Build.DenseOfArray(new float[] { -0.1f,  -0.05f,  -1.0f });            
            float s1Radius = 0.15f;
            Rgba32[] s1colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] s1coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial s1PhongMaterial = new PhongMaterial(illuminationModel, s1colors, s1coefficients, 5.0f);
            CheckerboardMaterial s1checkerMaterial = new CheckerboardMaterial(s1PhongMaterial, s0PhongMaterial, 0.1f);
            Sphere sphere1 = new Sphere(s1Center, s1Radius, s1checkerMaterial);

            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.5f,  0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] { 0.0f, -1.0f,  0.0f });
            Rgba32[] p0colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] p0coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial p0PhongMaterial = new PhongMaterial(illuminationModel, p0colors, p0coefficients, 5.0f);
            Rgba32[] p01colors = new Rgba32[] { Rgba32.Yellow, Rgba32.White };
            float[] p01coefficients = new float[] { 1.0f, 1.0f };
            PhongMaterial p01PhongMaterial = new PhongMaterial(illuminationModel, p01colors, p0coefficients, 5.0f);
            CheckerboardMaterial p0checkerMaterial = new CheckerboardMaterial(p0PhongMaterial, p01PhongMaterial, 0.1f);
            Plane plane0 = new Plane(p0Center, p0Normal, 5.0f, 10.0f, p0checkerMaterial);

            // // plane1 (background)
            // Vector p1Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, 5.0f });
            // Vector p1Normal = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, -1.0f });
            // Rgba32[] p1colors = new Rgba32[] { Rgba32.Black, Rgba32.White };
            // float[] p1coefficients = new float[] { 1.0f, 1.0f };
            // PhongMaterial p1PhongMaterial = new PhongMaterial(illuminationModel, p1colors, p1coefficients, 1.0f);
            // Plane plane1 = new Plane(p1Center, p1Normal, 10.0f, 10.0f, p1PhongMaterial);
            
            world.AddObject(sphere0);
            world.AddObject(sphere1);
            world.AddObject(plane0);
            // world.AddObject(plane1);
            // world.AddObject(complex);

            return world;
        }

        public static World GetGourdWorld(int width, int height) {
            World world = new World(width, height);

            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] { 0.0f, -5.0f, -2.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 70.5f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] { -0.3f, -0.3f, -10.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            var c1 = new Rgba32[] { Rgba32.White, Rgba32.White };
            var coeff1 = new float[] { 1.0f, 1.0f };
            var m1 = new PhongMaterial(illuminationModel, c1, coeff1, 5.0f);
            var c2 = new Rgba32[] { Rgba32.Yellow, Rgba32.Silver };
            var coeff2 = new float[] { 0.0f, 1.0f };
            var m2 = new PhongMaterial(illuminationModel, c2, coeff2, 0.1f);
            CheckerboardMaterial mC = new CheckerboardMaterial(m1, m2, 0.1f);

            var complex = OBJParser.LoadObjFile("./data/gourd.obj");
            complex.Scale(0.5f, 0.5f, 0.5f);
            complex.material = m1;
            world.AddObject(complex);
            return world;

        }


    }

}