using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace Raytracing {
    
    using Vector = Vector<float>;

    public class World {
        public List<Shape3D> objects { get; private set; }
        public List<LightSource> lights { get; set; }
        public int width { get; private set; }
        public int height { get; private set; }
        public Rgba32 background { get; private set; }
        public Vector up { get; private set; }

        public Rgba32 ambientLight { get; set; }
        public float ambientCoefficient { get; set; }
        public World(int w, int h) {
            width = w;
            height = h;
            up = Vector.Build.DenseOfArray(new float[]{0.0f, 1.0f, 0.0f});
            objects = new List<Shape3D>();
            ambientLight = new Rgba32(1.0f, 1.0f, 1.0f);
            ambientCoefficient = 0.0f;
            background = new Rgba32(0.5f, 0.6f, 1.0f, 1.0f);
            lights = new List<LightSource>();
        }

        public World(int w, int h, Rgba32 bg_color) {
            width = w;
            height = h;
            up = Vector.Build.DenseOfArray(new float[]{0.0f, 1.0f, 0.0f});
            objects = new List<Shape3D>();
            ambientLight = new Rgba32(1.0f, 1.0f, 1.0f);
            ambientCoefficient = 1.0f;
            background = bg_color;
            lights = new List<LightSource>();
        }

        public List<LightSource> GetLightSources() {
            return lights;
        }

        public List<Shape3D> GetObjects() {
            return objects;
        }

        public void AddObject(Shape3D o) {
            o.objID = objects.Count;
            objects.Add(o);
        }

        public void AddLightSource(LightSource l) {
            lights.Add(l);
        }

        public bool SpawnRay(Ray ray, out Rgba32 color) {
            bool hit = false;
            float? closestD = null;
            color = background;
            foreach(Shape3D obj in objects) {
                if(obj.Intersect(ray, out var intersect, out var normal)) {
                    float dist = Math.Abs((ray.origin - intersect[0]).Length());
                    if(closestD == null || dist < closestD) {
                        color = obj.material.Intersect(ray, intersect[0], normal[0], obj);
                        closestD = dist;
                    }
                    hit = true;
                }
            }

            return hit;
        }
    }
    
    public class Camera {
        public Vector position {get; private set;}
        public Vector lookAt {get; private set;}
        public Vector up {get; private set;}
        public World world {get; private set;}

        // transforms world coordinates to camera coordinates
        public Matrix<float> viewTransform {get; private set;}

        public Camera(Vector pos, Vector lookAt, Vector up, World world) {
            this.position = pos;
            this.lookAt = lookAt;
            this.up = world.up;
            this.world = world;
            //viewTransform
            var M = Matrix<float>.Build;
            viewTransform = M.Dense(4, 4);
        }

        // casts a ray into World w, the direction of the ray is based on
        // screen coordinates x and y
        public Rgba32 CastRay(World w, float x_s, float y_s) {
            Vector rayOrigin = this.position;
            Vector currentLookAt = Vector.Build.DenseOfArray(
                new float[] { lookAt[0] + x_s, lookAt[1] + y_s, lookAt[2] }
            );
            Vector rayDirection = Extensions.Normalize(currentLookAt - this.position);
            Ray ray = new Ray(rayOrigin, rayDirection);
            // spawn ray in world, returning a color
            world.SpawnRay(ray, out var resultColor);
            return resultColor;
        }

    }
}