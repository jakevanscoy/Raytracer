using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing {

    using Vector = Vector<float>;

   
    /// The base class for all of our 3D Objects
    public abstract class Object3D {
        public int objID {get; set;}
        public abstract bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal);
        public Material material {get; set;}
    }

    // Triangles are defined using 3 vertices
    public class Triangle : Object3D {
        public Vector vertex0 {get; set;}
        public Vector vertex1 {get; set;}
        public Vector vertex2 {get; set;}
        private const float kEpsilon = 0.0000001f; // constant used in intersection method

        public Triangle(Vector v0, Vector v1, Vector v2, Material m) {
            vertex0 = v0;
            vertex1 = v1;
            vertex2 = v2;
            material = m;
        }
        public override bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal) {
            // determinant, inverse determinant
            float det, idet;
            // point of intersection
            float x, y, z;

            Vector edge01, edge02, pvec, tvec, qvec;
            intersection = new Vector[] {
                Vector.Build.Dense(3)
            };
            normal = new Vector[] {
                Vector.Build.Dense(3)
            };
            edge01 = vertex1 - vertex0;
            edge02 = vertex2 - vertex0;
            pvec = ray.direction.CrossProduct(edge02);
            det = edge01.DotProduct(pvec);
            // no intersection if determinant is very small (or 0)
            if(det > -kEpsilon && det < kEpsilon)
                return false;

            idet = 1.0f / det;
            tvec = ray.origin - vertex0;
            y = tvec.DotProduct(pvec) * idet;
            if(y < 0 || y > 1)
                return false;

            qvec = tvec.CrossProduct(edge01);
            z = ray.direction.DotProduct(qvec) * idet;
            if(z < 0 || y + z > 1)
                return false;

            x = idet * edge02.DotProduct(qvec);
            if(x > kEpsilon) {
                intersection[0][0] = x;
                intersection[0][1] = y;
                intersection[0][2] = z;
                return true;
            } else {
                return false;
            }
        }
    }

    public class Plane : Object3D {
        public Vector center {get; set;}
        public Vector normal {get; set;}
        public Triangle t0 {get;}
        public Triangle t1 {get;}

        /*      
            Plane constructor given:
                a center point/vector, a normal vector, 
                width (in world units), height (in world units),
                and material
            

            planes are made of two constituent triangles t0 and t1,
            this makes intersection logic easy.

                p0________p1
            ^    |        /| 
            |    | t0   /  |
            |    |    c  --------> n
            |    |  /   t1 |
            |    |/________|   
           (V)   p2        p3
                
                (H)---------->

            to find p0-3, we need to find the horizontal and vertical 
            "projection vectors" (H, V) of the plane. 
            Using those, we can calculate p0-3 w.r.t. the center 
         */         
        public Plane(Vector center, Vector normal, float width, float height,  Material material) {
            this.center = center;
            this.normal = Extensions.Normalize(normal);
            this.material = material;

            // default world "up" vector
            Vector up = Vector.Build.DenseOfArray(new float[] {0.0f, 0.0f, -1.0f});
            // normal x up = 'horizontal' vector H of the plane
            Vector H = normal.CrossProduct(up);
            // normal x H = 'vertical' vector V of the plane
            Vector V = normal.CrossProduct(H);

            // calculate corner points based on the center, width, height, H, and V
            Vector p0 = center + ((width / 2) * H) + ((height / 2) * V);
            Vector p1 = center + ((width / 2) * H) - ((height / 2) * V);
            Vector p2 = center - ((width / 2) * H) + ((height / 2) * V);
            Vector p3 = center - ((width / 2) * H) - ((height / 2) * V);
        
            t0 = new Triangle(p0, p2, p1, material);
            t1 = new Triangle(p3, p1, p2, material);
        }

        public override bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal) {
            // check if the ray intersects either of our constituent triangles
            bool i0 = t0.Intersect(ray, out var intersection0, out var normal0);
            bool i1 = t1.Intersect(ray, out var intersection1, out var normal1);
            
            intersection = new Vector[2];
            normal = new Vector[2];
            if(i0) {
                intersection[0] = intersection0[0];
                normal[0] = normal0[0];
            }
            if(i0 && i1) {
                intersection[1] = intersection1[0];
                normal[1] = normal1[0];
            }
            if(!i0 && i1) {
                intersection[0] = intersection1[0];
                normal[0] = normal1[0];
            }
            normal[0] = this.normal;

            return (i0 || i1);
        }
    }

    public class Sphere : Object3D {
        public Vector center {get; private set;}
        public float radius {get; private set;}        
        public float radius2 {get; private set;}
        public Sphere(Vector center, float radius, Material material) {
            this.center = center;
            this.radius = radius;
            this.radius2 = radius * radius;
            this.material = material;
            // System.Console.WriteLine(this.material.color);
        }

        /// Intersect (overrides base class)
        /// out argument - intersection 
        /// returns bool - true if intersecting sphere, else false 
        public override bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal) {
            intersection = new Vector[]{
                Vector.Build.Dense(3), // i0
                Vector.Build.Dense(3), // i1
            };
            normal = new Vector[]{
                Vector.Build.Dense(3), // n0
                Vector.Build.Dense(3), // n1
            };
            var ray_to_center = center - ray.origin;
            float tca = ray_to_center.DotProduct(ray.direction);
            // System.Console.WriteLine(tca);            
            if(tca < 0) return false; // sphere is behind camera
            
            float d2 = ray_to_center.DotProduct(ray_to_center) - tca * tca;
            if(d2 > (radius2)) return false; // NO INTERSECTION
            float thc = (float)System.Math.Sqrt((radius2) - d2);
            
            float t0 = tca - thc;
            float t1 = tca + thc;

            if (t0 > t1) {
                // swap
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }

            var center_to_ray = ray.origin - center;
            float a = ray.direction.DotProduct(ray.direction);
            float b = 2 * ray.direction.DotProduct(center_to_ray);
            float c = center_to_ray.DotProduct(center_to_ray) - (radius2);

            if(Extensions.SolveQuadratic(a, b, c).Count == 0) return false;

            if (t0 < 0) {
                t0 = t1; // if t0 is negative, let's use t1 instead
                if (t0 < 0) return false; // both t0 and t1 are negative
            }

            intersection[0] = (ray.origin + ray.direction * t0);
            intersection[1] = (ray.origin + ray.direction * t1);

            Vector i0tmp = intersection[0] - center;
            Vector i1tmp = intersection[1] - center;
            normal[0] = i0tmp.Normalize();
            normal[1] = i1tmp.Normalize();
            
            return true;
        }
    }
}