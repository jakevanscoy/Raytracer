using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace Raytracing
{

    using Vector = Vector<float>;

    public class World
    {
        public List<Camera> cameras { get; private set; }
        public List<Shape3D> objects { get; set; }
        public Node tree { get; private set; }
        public List<LightSource> lights { get; set; }
        public int width { get; private set; }
        public int height { get; private set; }
        public Rgba32 background { get; private set; }
        public Vector up { get; private set; }
        public Rgba32 ambientLight { get; set; }
        public float ambientCoefficient { get; set; }
        public float airKt = 1.0f;
        public World(int w, int h)
        {
            width = w;
            height = h;
            up = Vector.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f });
            ambientLight = new Rgba32(0.2f, 0.2f, 0.25f, 1.0f);
            ambientCoefficient = 0.1f;
            background = new Rgba32(0.9f, 0.6f, 1.0f, 1.0f);
            objects = new List<Shape3D>();
            lights = new List<LightSource>();
            cameras = new List<Camera>();
        }

        public World(int w, int h, Rgba32 bg_color)
        {
            width = w;
            height = h;
            up = Vector.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f });
            ambientLight = bg_color;
            ambientCoefficient = 0.01f;
            background = bg_color;
            objects = new List<Shape3D>();
            lights = new List<LightSource>();
            cameras = new List<Camera>();
        }

        public List<LightSource> GetLightSources()
        {
            return lights;
        }

        public List<Shape3D> GetObjects()
        {
            return objects;
        }

        public void AddObject(Shape3D o)
        {
            o.objID = objects.Count;
            objects.Add(o);
        }
        public void AddObject(ComplexObject co)
        {
            co.objID = objects.Count;
            foreach (Shape3D s in co.shapes)
            {
                s.objID = objects.Count + 1;
                objects.Add(s);
            }
        }

        public void AddLightSource(LightSource l)
        {
            lights.Add(l);
        }

        public void MakeTree()
        {
            var center = Vector.Build.Dense(3);
            var size = Vector.Build.DenseOfArray(new float[] { 5.0f, 5.0f, 5.0f });
            foreach (Shape3D s in objects)
            {
                var c_d = (center - s.center);
                for (int i = 0; i < 3; i++)
                {
                    if (Math.Abs(c_d[i]) > size[i] / 2)
                    {
                        size[i] = (float)Math.Abs(c_d[i] * 2);
                    }
                }
            }
            tree = KDTree.GetTree(objects, new Voxel(center, size));
        }

        public Shape3D TraceRay(Ray ray, out Vector intersect, out Vector normal)
        {
            Shape3D closestObj = null;
            float? closestD = null;
            intersect = null;
            normal = null;
            foreach (Shape3D obj in objects)
            {
                if (obj.Intersect(ray, out var i, out var n))
                {
                    float dist = Math.Abs((ray.origin - i[0]).Length());
                    if (closestD == null || dist < closestD)
                    {
                        closestD = dist;
                        closestObj = obj;
                        intersect = i[0];
                        normal = n[0];
                    }
                }
            }
            return closestObj;
        }

        public Shape3D TraceRayKD(Ray ray, out Vector intersect, out Vector normal)
        {
            Shape3D hitObj = null;
            intersect = null;
            normal = null;
            KDTree.Traverse(ray, tree, ref hitObj, ref intersect, ref normal);
            return hitObj;
        }

        public Vector ReflectAndRefract(Shape3D obj, Ray ray, Vector intersect, Vector normal, int depth)
        {
            // var cVec = obj.material.Intersect(ray, intersect, normal, obj).ToVector();
            var cVec = Vector.Build.Dense(3);
            var lIntersect = intersect + (normal * 0.001f);
            bool outside = Extensions.Dot3(ray.direction, normal) > 0.0f;
            if (obj.material.kReflection > 0f)
            {
                var reflect = Extensions.Reflected(-ray.direction, normal).Normalize();
                Ray r = new Ray(lIntersect, reflect);
                SpawnRay(r, out var reflectColor, depth + 1);
                cVec += (reflectColor.ToVector() * obj.material.kReflection);
            }
            if (obj.material.kTransmission > 0f)
            {
                var refract = Extensions.Refract(ray.direction, normal, airKt,
                    obj.material.kTransmission).Normalize();
                var rO = outside ? intersect + (normal * 0.001f) : intersect - (normal * 0.001f);
                Ray r = new Ray(rO, refract);
                SpawnRay(r, out var refractColor, depth + 1);
                cVec += (refractColor.ToVector() * obj.material.kTransmission);
            }
            return cVec;
        }

        public Vector ReflectAndRefractKD(Shape3D obj, Ray ray, Vector intersect, Vector normal, int depth)
        {
            // var cVec = obj.material.Intersect(ray, intersect, normal, obj, true).ToVector();
            var cVec = Vector.Build.Dense(3);
            var lIntersect = intersect + (normal * 0.001f);
            if (obj.material is LenseMaterial)
            {
                var bh = obj as BlackHole;
                var sw_radius = bh.sradius;
                var ray_to_center = obj.center - ray.origin;
                float tca = ray_to_center.DotProduct(ray.direction);
                var p = ray.origin + (ray.direction * tca);
                var d = (p - obj.center).Length();
                if (d < sw_radius)
                {
                    return cVec;
                }
                var a = 2 * sw_radius / d;
                // System.Console.WriteLine(sw_radius / d);
                var rO = intersect - (normal * 0.0001f);
                var mat = obj.GetRotationMatrixAboutAxis(normal.CrossProduct(ray.direction).Normalize(), a);
                var refract = (mat * ray.direction.GetVector4()).SubVector(0, 3).Normalize();
                var rDotN = refract.DotProduct(normal);
                Ray r = new Ray(rO, refract);
                SpawnRayKD(r, out var reflectColor, depth + 1);
                cVec += (reflectColor.ToVector());
                // var ior = obj.material.kTransmission;
                // float fresnel;
                // Extensions.Fresnel(ray.direction, normal, ior, out fresnel);
                // if (fresnel < 1)
                // {
                //     var n_fresnel = (fresnel) / 0.1f;
                //     var n1 = airKt;
                //     var n2 = ior;
                //     var a = ior * (float)Math.PI / 180f;

                //     // System.Console.WriteLine(n_fresnel);
                //     // cVec[0] = 1.0f - ior;
                //     var mat = obj.GetRotationMatrixAboutAxis(normal.CrossProduct(ray.direction).Normalize(), a);
                //     // var refract = Extensions.Refract(ray.direction, normal, airKt, n2).Normalize();
                //     var refract = (mat * ray.direction.GetVector4()).SubVector(0, 3).Normalize();
                //     Ray r = new Ray(rO, refract);
                //     SpawnRayKD(r, out var reflectColor, depth + 1);
                //     cVec += (reflectColor.ToVector() * n_fresnel);
                //     return cVec;
                // }
                // else
                // {
                //     var mat = obj.GetRotationMatrixAboutAxis(normal.CrossProduct(ray.direction).Normalize(), 0.1f);
                //     var refract = (mat * ray.direction.GetVector4()).SubVector(0, 3).Normalize();
                //     Ray r = new Ray(rO, refract);
                //     SpawnRayKD(r, out var reflectColor, depth + 1);
                //     cVec += (reflectColor.ToVector());
                //     return cVec;
                // }
            }
            if (obj.material.kReflection > 0f)
            {
                var reflect = Extensions.Reflected(-ray.direction, normal).Normalize();
                Ray r = new Ray(lIntersect, reflect);
                SpawnRayKD(r, out var reflectColor, depth + 1);
                cVec += (reflectColor.ToVector() * obj.material.kReflection);
            }
            if (obj.material.kTransmission > 0f)
            {
                bool outside = Extensions.Dot3(ray.direction, normal) > 0.0f;
                var n1 = airKt;
                var n2 = obj.material.kTransmission;
                var norm = normal.Clone();
                var refract = Extensions.Refract(ray.direction, norm, n1, n2).Normalize();
                var rO = outside ? intersect + (normal * 0.001f) : intersect - (normal * 0.001f);
                Ray r = new Ray(rO, refract);
                SpawnRayKD(r, out var refractColor, depth + 1);
                cVec += (refractColor.ToVector() * obj.material.kTransmission);
            }
            return cVec;
        }

        public bool SpawnRay(Ray ray, out Rgba32 color, int depth = 0)
        {
            if (depth > 4)
            {
                color = (ambientLight.ToVector() * ambientCoefficient).ToColor();//background;
                return false;
            }
            // color = (ambientLight.ToVector() * ambientCoefficient).ToColor();
            var cVec = Vector.Build.Dense(3);
            Shape3D obj = TraceRay(ray, out var intersect, out var normal);
            if (obj is BlackHole)
            {
                System.Console.WriteLine(obj);
                var bhm = obj.material as BlackHoleMaterial;
                cVec += bhm.Intersect(ray, intersect, normal, depth, true).ToVector();
                cVec += ReflectAndRefractKD(obj, ray, intersect, normal, depth);
                color = cVec.ToColor();
                return true;
            }
            if (obj != null)
            {
                cVec += obj.material.Intersect(ray, intersect, normal, obj).ToVector();
                cVec += ReflectAndRefract(obj, ray, intersect, normal, depth);
                color = cVec.ToColor();
                return true;
            }
            else
            {
                color = (ambientLight.ToVector() * ambientCoefficient).ToColor();//background;
                return false;
            }
        }

        public bool SpawnRayKD(Ray ray, out Rgba32 color, int depth = 0)
        {
            if (depth > 5)
            {
                color = (ambientLight.ToVector() * ambientCoefficient).ToColor();//background;
                return false;
            }
            var cVec = Vector.Build.Dense(3);
            Shape3D obj = TraceRayKD(ray, out var intersect, out var normal);
            if (obj != null)
            {
                cVec += obj.material.Intersect(ray, intersect, normal, obj, true).ToVector();
                cVec += ReflectAndRefractKD(obj, ray, intersect, normal, depth);
                color = cVec.ToColor();
                return true;
            }
            else
            {
                color = (ambientLight.ToVector() * ambientCoefficient).ToColor();//background;
                return false;
            }
        }

    }
}