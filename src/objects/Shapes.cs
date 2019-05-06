using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing
{

    using Vector = Vector<float>;
    /// The base class for all of our 3D Objects
    public abstract class Shape3D : BasicObject
    {
        public Material material { get; set; }
        public abstract bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal);
        public virtual Vector GetTextureCoords(Vector intersection)
        {
            return Vector.Build.Dense(2);
        }
    }

    public class ComplexObject : Shape3D
    {
        public List<Shape3D> shapes { get; set; }
        public ComplexObject(List<Shape3D> s, Material m)
        {
            shapes = s;
            material = m;
        }

        public override void Scale(float sx, float sy, float sz)
        {
            // var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[]{sx, sy, sz});
            foreach (var t in shapes)
            {
                t.Scale(sx, sy, sz);
                // t.MakeAABB();
            }
        }

        public override void Translate(float tx, float ty, float tz)
        {
            // var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[]{sx, sy, sz});
            foreach (var t in shapes)
            {
                t.Translate(tx, ty, tz);
                // t.MakeAABB();
            }
        }

        public override void RotateX(float theta)
        {
            // var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[]{sx, sy, sz});
            foreach (var t in shapes)
            {
                t.RotateX(theta);
                // t.MakeAABB();
            }
        }
        public override void RotateY(float theta)
        {
            // var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[]{sx, sy, sz});
            foreach (var t in shapes)
            {
                t.RotateY(theta);
                // t.MakeAABB();
            }
        }
        public override void RotateZ(float theta)
        {
            // var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[]{sx, sy, sz});
            foreach (var t in shapes)
            {
                t.RotateZ(theta);
                // t.MakeAABB();
            }
        }

        public void SetMaterial(Material m)
        {
            foreach (var t in shapes)
            {
                t.material = m;
            }
        }

        public override bool Intersect(Ray ray, out Vector<float>[] intersection, out Vector<float>[] normal)
        {
            Shape3D closestT = null;
            intersection = new Vector[1];
            normal = new Vector[1];
            float closestD = float.MaxValue;
            int ti = 0;
            foreach (Shape3D t in shapes)
            {
                if (t.Intersect(ray, out var i, out var n))
                {
                    float dist = Math.Abs((ray.origin - i[0]).Length());
                    if (dist < closestD)
                    {
                        closestD = dist;
                        closestT = t;
                        intersection[0] = i[0];
                        normal[0] = n[0];
                    }
                }
                ti++;
            }
            return closestT != null;
        }

    }

    // Triangles are defined using 3 vertices: 
    //            v0
    //            /\
    //   edge01  /  \   edge02
    //          /    \
    //         /      \
    //        v1------v2
    //          edge12
    //
    public class Triangle : Shape3D
    {
        public Vector vertex0 { get; set; }
        public Vector vertex1 { get; set; }
        public Vector vertex2 { get; set; }
        public List<Vector> vertices { get; set; }
        public Vector uv0 { get; set; }
        public Vector uv1 { get; set; }
        public Vector uv2 { get; set; }
        public Vector normal0 { get; set; }
        public Vector normal1 { get; set; }
        public Vector normal2 { get; set; }

        private const float kEpsilon = 0.0000001f; // constant used in intersection method

        public Triangle(Vector v0, Vector v1, Vector v2, Material m)
        {
            vertex0 = v0;
            vertex1 = v1;
            vertex2 = v2;
            center = (v0 + v1 + v2) / 3;
            material = m;
            vertices = new List<Vector>(new Vector[] { vertex0, vertex1, vertex2 });
            normal0 = CalcNormal();
            MakeAABB();
        }

        public Triangle(Vector[] v, Vector[] vn, Vector[] vt, Material m)
        {
            vertex0 = v[0];
            vertex1 = v[1];
            vertex2 = v[2];
            vertices = new List<Vector>(new Vector[] { vertex0, vertex1, vertex2 });
            normal0 = vn[0];
            normal1 = vn[1];
            normal2 = vn[2];
            uv0 = vt[0];
            uv1 = vt[1];
            uv2 = vt[2];
            center = (vertex0 + vertex1 + vertex2) / 3;
            material = m;
            MakeAABB();
        }

        public Triangle(Vector[] v, Vector[] vt, Material m)
        {
            vertex0 = v[0];
            vertex1 = v[1];
            vertex2 = v[2];
            vertices = new List<Vector>(new Vector[] { vertex0, vertex1, vertex2 });
            uv0 = vt[0];
            uv1 = vt[1];
            uv2 = vt[2];
            normal0 = CalcNormal();
            center = (vertex0 + vertex1 + vertex2) / 3;
            material = m;
            MakeAABB();
        }

        public Triangle(Vector[] v, Material m)
        {
            vertex0 = v[0];
            vertex1 = v[1];
            vertex2 = v[2];
            vertices = new List<Vector>(new Vector[] { vertex0, vertex1, vertex2 });
            center = (vertex0 + vertex1 + vertex2) / 3;
            normal0 = CalcNormal();
            material = m;
            MakeAABB();
        }

        public override void MakeAABB()
        {
            var max = Vector.Build.DenseOfArray(new float[] { float.MinValue, float.MinValue, float.MinValue });
            var min = Vector.Build.DenseOfArray(new float[] { float.MaxValue, float.MaxValue, float.MaxValue });
            for (int i = 0; i < 3; i++)
            {
                min[i] = Math.Min(vertex0[i], min[i]);
                min[i] = Math.Min(vertex1[i], min[i]);
                min[i] = Math.Min(vertex2[i], min[i]);
                max[i] = Math.Max(vertex0[i], max[i]);
                max[i] = Math.Max(vertex1[i], max[i]);
                max[i] = Math.Max(vertex2[i], max[i]);
                if (min[i] > max[i])
                {
                    var tmp = min[i];
                    min[i] = max[i];
                    max[i] = tmp;
                }
            }
            AABB = new Voxel(new Vector[] { min, max });
        }
        public Vector CalcNormal()
        {
            var edge01 = vertex1 - vertex0;
            var edge02 = vertex2 - vertex0;
            var N = Vector.Build.Dense(3);
            N[0] = (edge01[1] * edge02[2]) - (edge01[2] * edge02[1]);
            N[1] = (edge01[2] * edge02[0]) - (edge01[0] * edge02[2]);
            N[2] = (edge01[0] * edge02[1]) - (edge01[1] * edge02[0]);
            return N.Normalize();
        }

        public new Vector GetTextureCoords(Vector intersect)
        {
            if (uv0 != null)
                return uv0;
            else
                return base.GetTextureCoords(intersect);
        }

        public override void Scale(float sx, float sy, float sz)
        {
            var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[] { sx, sy, sz });
            vertex0 = matrix * vertex0;
            vertex1 = matrix * vertex1;
            vertex2 = matrix * vertex2;
            center = (vertex0 + vertex1 + vertex2) / 3;
            MakeAABB();
        }

        public override void RotateZ(float theta)
        {
            var matrix = GetRotateZMatrix(theta);
            vertex0 = matrix * vertex0;
            vertex1 = matrix * vertex1;
            vertex2 = matrix * vertex2;
            center = (vertex0 + vertex1 + vertex2) / 3;
            CalcNormal();
            MakeAABB();
        }
        public override void RotateX(float theta)
        {
            var matrix = GetRotateXMatrix(theta);
            vertex0 = matrix * vertex0;
            vertex1 = matrix * vertex1;
            vertex2 = matrix * vertex2;
            center = (vertex0 + vertex1 + vertex2) / 3;
            CalcNormal();
            MakeAABB();
        }

        public override void RotateY(float theta)
        {
            var matrix = GetRotateYMatrix(theta);
            vertex0 = matrix * vertex0;
            vertex1 = matrix * vertex1;
            vertex2 = matrix * vertex2;
            center = (vertex0 + vertex1 + vertex2) / 3;
            CalcNormal();
            MakeAABB();
        }

        public override void Translate(float tx, float ty, float tz)
        {
            var matrix = GetTranslateMatrix(tx, ty, tz);
            var t = Vector.Build.DenseOfArray(new float[] { tx, ty, tz });
            vertex0 += t;
            vertex1 += t;
            vertex2 += t;
            // vertex0 = matrix * vertex0;
            // vertex0 = matrix * vertex1;
            // vertex0 = matrix * vertex2;
            // var v04 = vertex0.GetVector4();
            // var v14 = vertex1.GetVector4();
            // var v24 = vertex2.GetVector4();
            // v04 = matrix * v04;
            // v14 = matrix * v14;
            // v24 = matrix * v24;
            // vertex0 = v04.SubVector(0, 3);
            // vertex1 = v14.SubVector(0, 3);
            // vertex2 = v24.SubVector(0, 3);
            center = (vertex0 + vertex1 + vertex2) / 3;
            MakeAABB();
        }

        public override bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal)
        {
            // determinant, inverse determinant
            float det, idet;
            // point of intersection
            float x, y, z;
            Vector edge01, edge02, pvec, tvec, qvec;
            intersection = new Vector[] { Vector.Build.Dense(3) };
            normal = new Vector[] { Vector.Build.Dense(3) };
            edge01 = vertex1 - vertex0;
            edge02 = vertex2 - vertex0;
            normal[0] = this.normal0;
            pvec = ray.direction.CrossProduct(edge02);
            det = edge01.DotProduct(pvec);
            // no intersection if determinant is very small (or 0)
            if (det > -kEpsilon && det < kEpsilon)
                return false;

            idet = 1.0f / det;
            tvec = ray.origin - vertex0;
            y = tvec.DotProduct(pvec) * idet;
            if (y < 0 || y > 1)
                return false;

            qvec = tvec.CrossProduct(edge01);
            z = ray.direction.DotProduct(qvec) * idet;
            if (z < 0 || y + z > 1)
                return false;

            x = idet * edge02.DotProduct(qvec);
            // if(x > kEpsilon) {

            intersection[0][0] = x;
            intersection[0][1] = y;
            intersection[0][2] = z;
            return true;

            // } else {
            // return false;
            // }
        }
    }

    public class Sphere : Shape3D
    {
        public float radius { get; set; }
        public float radius2 { get; set; }
        public Sphere(Vector center, float radius, Material material)
        {
            this.center = center;
            this.radius = radius;
            this.radius2 = radius * radius;
            this.material = material;
            MakeAABB();
            // System.Console.WriteLine(this.material.color);
        }

        internal Sphere(Vector center, float radius)
        {
            this.center = center;
            this.radius = radius;
            this.radius2 = radius * radius;
        }
        public override void Scale(float sx, float sy, float sz)
        {
            var s = Vector.Build.DenseOfArray(new float[] { sx, sy, sz });
            center = center.Multiply(s);
            radius = radius * ((sx + sy + sz) / 3);
            MakeAABB();
        }

        public override void MakeAABB()
        {
            var s = Vector.Build.DenseOfArray(new float[] {
                radius * 2,
                radius * 2,
                radius * 2
            });
            AABB = new Voxel(center, s);
        }

        public override Vector GetTextureCoords(Vector intersection)
        {
            Vector d = (center - intersection).Normalize();
            float u = 0.5f + (float)(Math.Atan2(d[2], d[0]) / (2 * Math.PI));
            float v = 0.5f - (float)(Math.Asin(d[1]) / (Math.PI));
            return Vector.Build.DenseOfArray(new float[] { u, v });
        }

        /// Intersect (overrides base class)
        /// out argument - intersection 
        /// returns bool - true if intersecting sphere, else false 
        public override bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal)
        {
            intersection = new Vector[]{
                Vector.Build.Dense(3), // i0
                // Vector.Build.Dense(3), // i1
            };
            normal = new Vector[]{
                Vector.Build.Dense(3), // n0
                // Vector.Build.Dense(3), // n1
            };

            var ray_to_center = center - ray.origin;
            float tca = ray_to_center.DotProduct(ray.direction);
            // System.Console.WriteLine(tca);            
            // if(tca < 0.0f) return false; // sphere is behind camera

            float d2 = ray_to_center.DotProduct(ray_to_center) - (tca * tca);
            // System.Console.WriteLine(Math.Sqrt(d2));
            // float d2 = ray_to_center.DotProduct(ray_to_center) - tca * tca;
            if (d2 > (radius2)) return false; // NO INTERSECTION
            float t1c = (float)Math.Sqrt(radius2 - d2);
            float t0 = tca - t1c;
            float t1 = tca + t1c;

            var center_to_ray = ray.origin - center;
            float a = ray.direction.DotProduct(ray.direction);
            float b = ray.direction.DotProduct(2.0f * center_to_ray);
            float c = center_to_ray.DotProduct(center_to_ray) - (radius2);

            if (!Extensions.Quadratic(a, b, c, ref t0, ref t1))
                return false;

            if (t0 > t1)
            {
                // swap
                float tmp = t0;
                t0 = t1;
                t1 = tmp;
            }

            if (t0 < 0)
            {
                t0 = t1; // if t0 is negative, let's use t1 instead
                if (t0 < 0) return false; // both t0 and t1 are negative
            }

            intersection[0] = (ray.origin + (ray.direction * t0));
            Vector i0tmp = intersection[0] - center;
            normal[0] = i0tmp.Normalize();
            return true;
        }
    }

    public class Plane : Shape3D
    {

        public Vector normal { get; set; }
        public Vector min_bounds;
        public Vector max_bounds;
        public Vector p0 { get; set; }
        public Vector p1 { get; set; }
        public Vector p2 { get; set; }
        public Vector p3 { get; set; }

        /*      
            Plane constructor given:
                a center point/vector, a normal vector, 
                width (in world units), height (in world units),
                and material
            
                p0_________p1
            ^    |         | 
            |    |         |
            |    |    c  --------> n
            |    |         |
            |    |_________|   
           (V)   p2        p3
                
                (H)---------->

            to find p0-3, we need to find the horizontal and vertical 
            "projection vectors" (H, V) of the plane. 
            Using those, we can calculate p0-3 w.r.t. the center 
         */
        public Plane(Vector center, Vector normal, float width, float height, Material m)
        {
            this.center = center;
            this.normal = normal.Normalize();
            this.material = m;
            if (normal[1] == 0.0f)
            {
                normal[1] = 0.000001f;
            }
            // default world "up" vector
            Vector up = Vector.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f });
            // normal x up = 'horizontal' vector H of the plane
            Vector H = up.CrossProduct(normal).Normalize();
            // normal x H = 'vertical' vector V of the plane
            Vector V = normal.CrossProduct(H).Normalize();

            float max_x = float.MinValue, min_x = float.MaxValue;
            float max_y = float.MinValue, min_y = float.MaxValue;
            float max_z = float.MinValue, min_z = float.MaxValue;
            min_bounds = Vector.Build.DenseOfArray(new float[] { min_x, min_y, min_z });
            max_bounds = Vector.Build.DenseOfArray(new float[] { max_x, max_y, max_z });

            // calculate corner points based on the center, width, height, H, and V
            p0 = center + ((width / 2) * H) + ((height / 2) * V);
            p1 = center + ((width / 2) * H) - ((height / 2) * V);
            p2 = center - ((width / 2) * H) + ((height / 2) * V);
            p3 = center - ((width / 2) * H) - ((height / 2) * V);
            MakeBounds(p0);
            MakeBounds(p1);
            MakeBounds(p2);
            MakeBounds(p3);
            MakeAABB();
        }

        public Plane(Vector[] v, Material m)
        {
            p0 = v[0];
            p1 = v[1];
            p2 = v[2];
            p3 = v[3];
            MakeBounds(p0);
            MakeBounds(p1);
            MakeBounds(p2);
            MakeBounds(p3);
            center = (p0 + p1 + p2 + p3) / 4;
            normal = CalcNormal();
            material = m;
            MakeAABB();
        }
        public Vector CalcNormal()
        {
            var edge01 = p0 - p1;
            var edge02 = p0 - p2;
            var N = Vector.Build.Dense(3);
            N[0] = (edge01[1] * edge02[2]) - (edge01[2] * edge02[1]);
            N[1] = (edge01[2] * edge02[0]) - (edge01[0] * edge02[2]);
            N[2] = (edge01[0] * edge02[1]) - (edge01[1] * edge02[0]);
            return N.Normalize();
        }

        public override void MakeAABB()
        {
            var s = Vector.Build.DenseOfArray(new float[] {
                max_bounds[0] - min_bounds[0],
                max_bounds[1] - min_bounds[1],
                max_bounds[2] - min_bounds[2],
            });
            AABB = new Voxel(center, s);
        }

        public override void Scale(float sx, float sy, float sz)
        {
            var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[] { sx, sy, sz });
        }

        public override void Translate(float tx, float ty, float tz)
        {
            var matrix = GetTranslateMatrix(tx, ty, tz);
            var p04 = p0.GetVector4();
            var p14 = p1.GetVector4();
            var p24 = p2.GetVector4();
            var p34 = p3.GetVector4();
            p04 = matrix * p04;
            p14 = matrix * p14;
            p24 = matrix * p24;
            p34 = matrix * p34;
            p0 = p04.SubVector(0, 3);
            p1 = p14.SubVector(0, 3);
            p2 = p24.SubVector(0, 3);
            p3 = p34.SubVector(0, 3);
            MakeBounds(p0);
            MakeBounds(p1);
            MakeBounds(p2);
            MakeBounds(p3);
            center = (p0 + p1 + p2 + p3) / 4;
        }

        private void MakeBounds(Vector p)
        {
            if (p[0] < min_bounds[0]) min_bounds[0] = p[0];
            if (p[1] < min_bounds[1]) min_bounds[1] = p[1];
            if (p[2] < min_bounds[2]) min_bounds[2] = p[2];

            if (p[0] > max_bounds[0]) max_bounds[0] = p[0];
            if (p[1] > max_bounds[1]) max_bounds[1] = p[1];
            if (p[2] > max_bounds[2]) max_bounds[2] = p[2];
        }

        private bool InBounds(Vector intersect)
        {
            var i_min_epsilon = intersect + (0.0001f);
            var i_max_epsilon = intersect - (0.0001f);
            bool in_x = (i_min_epsilon[0] >= min_bounds[0]) && (i_max_epsilon[0] <= max_bounds[0]);
            bool in_y = (i_min_epsilon[1] >= min_bounds[1]) && (i_max_epsilon[1] <= max_bounds[1]);
            bool in_z = (i_min_epsilon[2] >= min_bounds[2]) && (i_max_epsilon[2] <= max_bounds[2]);
            return (in_x && in_y && in_z);
        }

        public override Vector GetTextureCoords(Vector intersection)
        {
            // default world "up" vector
            Vector up = Vector.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f });
            // normal x up = 'horizontal' vector H of the plane
            Vector H = up.CrossProduct(normal).Normalize();
            // normal x H = 'vertical' vector V of the plane
            Vector V = normal.CrossProduct(H).Normalize();

            var s = normal.DotProduct(intersection - min_bounds);
            // var t_1 = H.DotProduct(intersection - min_bounds);
            // var t_2 = V.DotProduct(intersection - min_bounds);
            var vP = min_bounds;
            var vQ = intersection;
            var vN = normal.Normalize();
            var vPQ = vP - vQ;
            var dot = vPQ.DotProduct(vN);
            var vQN = vN * dot;
            var vAns = vQ - vQN;

            Vector nI = (2 * (intersection - min_bounds) / (max_bounds - min_bounds)) - 1;
            var t_1 = H * (intersection - min_bounds);
            var t_2 = V * (intersection - min_bounds);
            float u = (nI[0] + 1) / 2;
            float v = (nI[1] + 1) / 2;
            float w = (nI[2] + 1) / 2;

            return Vector.Build.DenseOfArray(new float[] { t_1, t_2 });
        }

        public override bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal)
        {
            intersection = new Vector[1];
            normal = new Vector[] { this.normal };
            float denom = this.normal.DotProduct(ray.direction.Normalize());
            if ((float)Math.Abs(denom) > 0.00001f)
            {
                Vector rayO_center = center - ray.origin;
                float d = rayO_center.DotProduct(this.normal) / denom;
                intersection[0] = ray.origin + (ray.direction.Normalize() * d);
                return (d >= 0.0f && InBounds(intersection[0]));
            }
            return false;
        }
    }
}