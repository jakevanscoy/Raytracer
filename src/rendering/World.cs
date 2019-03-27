using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace Raytracing {
    
    using Vector = Vector<float>;

    public class World {
        public List<Camera> cameras { get; private set; }
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
            cameras = new List<Camera>();
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
    
    
}