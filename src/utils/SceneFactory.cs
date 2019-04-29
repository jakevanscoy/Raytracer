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
            Vector lightPos = Vector.Build.DenseOfArray(new float[] {  0.0f, -0.4f, -5.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 20.0f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] { -0.0f, -0.3f, -5.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            
            // sphere 0
            Vector s0Center = Vector.Build.DenseOfArray(new float[] { -0.0f,  -0.05f,  -2.75f });
            float s0Radius = 0.15f;
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.White };
            float[] s0coefficients = new float[] { 0.0f, 0.0f };
            PhongMaterial s0PhongMaterial = new PhongMaterial(illuminationModel, s0colors, s0coefficients, 7.0f);
            s0PhongMaterial.kTransmission = 0.8f;
            Sphere sphere0 = new Sphere(s0Center, s0Radius, TransmissiveMaterial.GetTransmissiveMaterial(illuminationModel));
            
            // sphere 1
            Vector s1Center = Vector.Build.DenseOfArray(new float[] { -0.0f,  -0.1f,  -1.75f });            
            float s1Radius = 0.15f;
            Rgba32[] s1colors = new Rgba32[] { Rgba32.Silver, Rgba32.White };
            float[] s1coefficients = new float[] { 0.1f, 0.1f };
            PhongMaterial s1PhongMaterial = new PhongMaterial(illuminationModel, s1colors, s1coefficients, 10.0f);
            CheckerboardMaterial s1checkerMaterial = new CheckerboardMaterial(s1PhongMaterial, s0PhongMaterial, 0.1f);
            s1PhongMaterial.kReflection = 0.9f;
            Sphere sphere1 = new Sphere(s1Center, s1Radius, Mirror.GetMirror(illuminationModel));

            // plane0 (floor)
            Vector p0Center = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.2f,  0.0f });
            Vector p0Normal = Vector.Build.DenseOfArray(new float[] { 0.0f, -1.0f,  0.0f });
            Rgba32[] p01colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] p01coefficients = new float[] { 0.9f, 0.1f };
            Rgba32[] p02colors = new Rgba32[] { Rgba32.Yellow, Rgba32.White };
            float[] p02coefficients = new float[] { 0.9f, 0.1f };
            PhongMaterial pm0 = new PhongMaterial(illuminationModel, p01colors, p01coefficients, 10.0f);
            PhongMaterial pm1 = new PhongMaterial(illuminationModel, p02colors, p02coefficients, 10.0f);
            CheckerboardMaterial p0checkerMaterial = new CheckerboardMaterial(pm0, pm1, 0.12f);
            p0checkerMaterial.kReflection = 0.0f;
            Plane plane0 = new Plane(p0Center, p0Normal, 3.0f, 8.0f, p0checkerMaterial);


            var max_y_c = Vector.Build.DenseOfArray(new float[] {0.0f, -0.8f, 0.0f});
            var max_y_n = Vector.Build.DenseOfArray(new float[] {0.0f,  1.0f, 0.0f});
            Plane max_y = new Plane(max_y_c, max_y_n, 3.0f, 5.0f, p0checkerMaterial);

            world.AddObject(sphere0);
            world.AddObject(sphere1);
            world.AddObject(plane0);
            world.AddObject(max_y);

            return world;
        }

        public static World GetVoxelTestWorld(int width, int height) {
            // World world = GetDefaultWorld(width, height);
            World world = new World(width, height);
            // initialize light source 
            Vector lightPos = Vector.Build.DenseOfArray(new float[] { -3.0f, -3.0f, -3.0f});
            LightSource l1 = new LightSource(lightPos, Rgba32.White, 95.5f);
            world.AddLightSource(l1);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f, -3.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f, -1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  0.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            
            var c = Vector.Build.Dense(3);
            c[0] = 0.0f;
            c[1] = 0.0f;
            c[2] = 0.0f;
            var s = Vector.Build.DenseOfArray(new float[] {1.0f, 1.0f, 1.0f});
            var rmat = PhongMaterial.Red(illuminationModel);
            var bmat = PhongMaterial.Blue(illuminationModel);
            var gmat = PhongMaterial.Green(illuminationModel);

            var p = new PartitionPlane(c, 1);
            
            Voxel main = new Voxel(c, s);
            Voxel[] split = main.Split(p);
            Voxel left = new Voxel(split[0].center, split[0].size, rmat);
            Voxel right = new Voxel(split[1].center, split[1].size, bmat);
            for(int i = 0; i < 6; i++) {
                world.AddObject(left.planes[i]);
                world.AddObject(right.planes[i]);
            }

            var smax = new Sphere(main.max, 0.05f, gmat);
            var smin = new Sphere(main.min, 0.05f, gmat);
            world.AddObject(smax);
            world.AddObject(smin);
            return world;
        }


        public static World GetBoxWorld(int width, int height) {
            
            var world = GetDefaultWorld(width, height);
            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);

            Rgba32[] p01colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] p01coefficients = new float[] { 0.9f, 0.3f };
            Rgba32[] p02colors = new Rgba32[] { Rgba32.Yellow, Rgba32.White };
            float[] p02coefficients = new float[] { 0.9f, 0.3f };
            PhongMaterial pm0 = new PhongMaterial(illuminationModel, p01colors, p01coefficients, 10.0f);
            PhongMaterial pm1 = new PhongMaterial(illuminationModel, p02colors, p02coefficients, 10.0f);
            CheckerboardMaterial p0checkerMaterial = new CheckerboardMaterial(pm0, pm1, 0.05f);
            
            var max_y_c = Vector.Build.DenseOfArray(new float[] {0.0f, -2.0f, 0.0f});
            var max_y_n = Vector.Build.DenseOfArray(new float[] {0.0f,  -1.0f, 0.0f});
            Plane max_y = new Plane(max_y_c, max_y_n, 5.0f, 10.0f, p0checkerMaterial);

            var max_x_c = Vector.Build.DenseOfArray(new float[] {1.0f,  0.0f, 0.0f});
            var max_x_n = Vector.Build.DenseOfArray(new float[] {-1.0f,  0.0f, 0.0f});
            Plane max_x = new Plane(max_x_c, max_x_n, 5.0f, 10.0f, p0checkerMaterial);

            var min_x_c = Vector.Build.DenseOfArray(new float[] {-1.0f,  0.0f, 0.0f});
            var min_x_n = Vector.Build.DenseOfArray(new float[] { 1.0f,  0.0f, 0.0f});
            Plane min_x = new Plane(min_x_c, min_x_n, 5.0f, 10.0f, p0checkerMaterial);

            var min_z_c = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, -10.0f});
            var min_z_n = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.01f,  -1.0f});
            Plane min_z = new Plane(min_z_c, min_z_n, 5.0f, 10.0f, p0checkerMaterial);

            var max_z_c = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, 3.0f});
            var max_z_n = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.01f, 1.0f});
            Plane max_z = new Plane(min_z_c, max_z_n, 5.0f, 10.0f, p0checkerMaterial);



            return world;
        }

        public static World GetManyBallWorld(int width, int height) {
            var world = GetDefaultWorld(width, height);
            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);
            for(int i = 0; i < 40; i++) {
                for(int j = 0; j < 20; j++) {
                    var c = Vector.Build.DenseOfArray(new float[] { -0.6f + (0.03f * i), -0.5f + (0.03f * j), -1.75f });
                    Sphere ball = new Sphere(c, 0.01f, PhongMaterial.Red(illuminationModel));
                    world.AddObject(ball);
                }
            }
            return world;
        }
        public static World GetComplexWorld(int width, int height) {
            // World world = GetDefaultWorld(width, height);
            World world = new World(width, height);
            // initialize light source 
            // Vector lightPos = Vector.Build.DenseOfArray(new float[] { -1.0f, -1.0f, -3.0f});
            // LightSource l1 = new LightSource(lightPos, Rgba32.White, 95.5f);
            // world.AddLightSource(l1);

            Vector lightPos2 = Vector.Build.DenseOfArray(new float[] { 1.0f, -3.0f, -3.0f});
            LightSource l2 = new LightSource(lightPos2, Rgba32.White, 95.5f);
            world.AddLightSource(l2);

            // initialize camera
            Vector cameraPos    = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f, -10.0f });
            Vector cameraUp     = Vector.Build.DenseOfArray(new float[] {  0.0f,  1.0f,  0.0f  });
            Vector cameraLookAt = Vector.Build.DenseOfArray(new float[] {  0.0f,  0.0f,  -8.0f  });
            world.cameras.Add(new Camera(cameraPos, cameraLookAt, cameraUp, world));

            PhongIlluminationModel illuminationModel = new PhongIlluminationModel(world);

            var complex = OBJParser.LoadObjFile("./data/gourd.obj");
            complex.Translate(0.0f, -0.5f, 0.0f);
            System.Console.WriteLine(complex.shapes.Count);
            // complex.RotateZ(0.0f);
            complex.Scale(1.8f, 1.8f, 1.8f);
            var mat = PhongMaterial.Red(illuminationModel);
            mat.kSpecular = 0.01f;
            complex.SetMaterial(mat);
            world.AddObject(complex);
            return world;
        }
    }
}