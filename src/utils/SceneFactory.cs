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

            World world = new World(width, height, Rgba32.Firebrick);
            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f, -3.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 4.0f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] { -0.0f, -0.3f, -5.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            
            // sphere 0
            Vector s0Center = Vector.Build.DenseOfArray(new float[] { -0.0f,  -0.05f,  -1.75f });
            float s0Radius = 0.15f;
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.White };
            float[] s0coefficients = new float[] { 0.0f, 0.0f };
            PhongMaterial s0PhongMaterial = new PhongMaterial(illuminationModel, s0colors, s0coefficients, 7.0f);
            s0PhongMaterial.kTransmission = 0.8f;
            Sphere sphere0 = new Sphere(s0Center, s0Radius, s0PhongMaterial);
            
            // sphere 1
            Vector s1Center = Vector.Build.DenseOfArray(new float[] { -0.0f,  -0.05f,  -1.0f });            
            float s1Radius = 0.15f;
            Rgba32[] s1colors = new Rgba32[] { Rgba32.Silver, Rgba32.White };
            float[] s1coefficients = new float[] { 0.1f, 0.1f };
            PhongMaterial s1PhongMaterial = new PhongMaterial(illuminationModel, s1colors, s1coefficients, 10.0f);
            CheckerboardMaterial s1checkerMaterial = new CheckerboardMaterial(s1PhongMaterial, s0PhongMaterial, 0.1f);
            s1PhongMaterial.kReflection = 0.7f;
            Sphere sphere1 = new Sphere(s1Center, s1Radius, s1PhongMaterial);

            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.2f,  0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] { 0.0f, -1.0f,  0.0f });
            Rgba32[] p01colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] p01coefficients = new float[] { 0.9f, 0.3f };
            Rgba32[] p02colors = new Rgba32[] { Rgba32.Yellow, Rgba32.White };
            float[] p02coefficients = new float[] { 0.9f, 0.3f };
            PhongMaterial pm0 = new PhongMaterial(illuminationModel, p01colors, p01coefficients, 10.0f);
            PhongMaterial pm1 = new PhongMaterial(illuminationModel, p02colors, p02coefficients, 10.0f);
            CheckerboardMaterial p0checkerMaterial = new CheckerboardMaterial(pm0, pm1, 0.005f);
            p0checkerMaterial.kReflection = 0.0f;
            Plane plane0 = new Plane(p0Center, p0Normal, 5.0f, 10.0f, p0checkerMaterial);

            // sphere 1
            Vector s2Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f,  0.0f });    
            float s2Radius = 50f;
            Sphere sphere2 = new Sphere(s2Center, s2Radius, p0checkerMaterial);

            sphere2.Translate(0f, s2Radius + 0.15f, 0f);

            world.AddObject(sphere0);
            world.AddObject(sphere1);
            // world.AddObject(sphere2);
            world.AddObject(plane0);
            return world;
        }

        public static World GetVoxelTestWorld(int width, int height) {
            World world = new World(width, height, Rgba32.Firebrick);
            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] {  0.0f, 0.0f, -2.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 4.0f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] { -0.0f, -0.3f, -5.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
           
            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.2f,  0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] { 0.0f, -1.0f,  0.0f });
            Rgba32[] p01colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] p01coefficients = new float[] { 0.9f, 0.3f };
            Rgba32[] p02colors = new Rgba32[] { Rgba32.Yellow, Rgba32.White };
            float[] p02coefficients = new float[] { 0.9f, 0.3f };
            PhongMaterial pm0 = new PhongMaterial(illuminationModel, p01colors, p01coefficients, 10.0f);
            PhongMaterial pm1 = new PhongMaterial(illuminationModel, p02colors, p02coefficients, 10.0f);
            CheckerboardMaterial p0checkerMaterial = new CheckerboardMaterial(pm0, pm1, 0.005f);
            p0checkerMaterial.kReflection = 0.0f;
            Plane plane0 = new Plane(p0Center, p0Normal, 5.0f, 10.0f, p0checkerMaterial);


            Voxel v = new Voxel(Vector.Build.Dense(3),
                                Vector.Build.DenseOfArray(new float[] { 1.0f,  1.0f,  1.0f }),
                                TransmissiveMaterial.GetTransmissiveMaterial(illuminationModel));

            // foreach(var p in v.planes) {
            //     world.AddObject(p);
            // }

            for(int i = 0; i < 6; i++) {
                world.AddObject(v.planes[i]);
            }
 
            // world.AddObject(plane0);
            return world;
        }

        public static World GetManyBallWorld(int width, int height) {
            var world = GetDefaultWorld(width, height);
            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            Vector lightPos = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f, 2.0f});
            // LightSource l2 = new LightSource(lightPos, Rgba32.White, 4.0f);
            // world.AddLightSource(l2);
            var random = new Random();
            var ballMat = new BasicMaterial(Rgba32.Blue);
            for(int i = 0; i < 10; i++) {
                for(int j = 0; j < 10; j++) {
                    var c = Vector.Build.DenseOfArray(new float[] { -0.2f + (0.05f * i), -0.3f, -1.0f - (0.05f * j) });
                    // c[1] += (float)(random.NextDouble() * 0.05); 
                    // c[2] += (float)(random.NextDouble() * 0.05); 
                    Sphere ball = new Sphere(c, 0.01f, PhongMaterial.Red(illuminationModel));
                    // c[0] += 0.1f;
                    world.AddObject(ball);
                }
            }
            return world;
        }
        public static World GetBunnyWorld(int width, int height) {
            World world = new World(width, height);

            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] { 0.0f, -5.0f, -2.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 95.5f);
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

            var complex = OBJParser.LoadObjFile("./data/bunny.obj");
            System.Console.WriteLine(complex.shapes.Count);
            complex.Scale(0.4f, 0.4f, 0.4f);
            complex.material = m1;
            world.AddObject(complex);
            return world;

        }
    }
}