using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Text;

namespace Raytracing
{

    using Vector = Vector<float>;
    public class Node
    {
        public PartitionPlane partition;
        public Voxel bounds;
        public Node left;
        public Node right;

        public Node(PartitionPlane P, Voxel V, Node L = null, Node R = null)
        {
            partition = P;
            bounds = V;
            left = L;
            right = R;
        }

        public override string ToString()
        {
            return "IN: Split Axis: " + partition.axis + "\n";
        }

    }


    public class LeafNode : Node
    {
        public List<Shape3D> shapes;
        public LeafNode(List<Shape3D> L, Voxel V) : base(null, V, null, null)
        {
            shapes = L;
        }

        public override string ToString()
        {
            return "LN: #Obj: " + shapes.Count + "\n";
        }

    }

    public class PartitionPlane
    {
        public Vector center;
        public Vector normal;
        public int axis;

        public PartitionPlane(Vector c, int d)
        {
            center = c;
            normal = Vector.Build.Dense(3);
            normal[d] = -1;
            axis = d;
        }

        public List<Shape3D>[] Partition(List<Shape3D> L, Voxel[] splitV)
        {
            var left = new List<Shape3D>();
            var right = new List<Shape3D>();
            var lV = splitV[0];
            var rV = splitV[1];
            foreach (Shape3D s in L)
            {
                var placed = false;
                if (lV.InBounds(s))
                {
                    left.Add(s);
                    placed = true;
                }
                if (rV.InBounds(s))
                {
                    right.Add(s);
                    placed = true;
                }
                if (!placed)
                    KDTree.nObj++;
            }
            return new List<Shape3D>[] { left, right };
        }

        public bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal)
        {
            intersection = new Vector[1];
            normal = new Vector[] { this.normal, -this.normal };
            float denom1 = this.normal.DotProduct(ray.direction.Normalize());
            float denom2 = (-this.normal).DotProduct(ray.direction.Normalize());
            if ((float)Math.Abs(denom1) > 0.00001f)
            {
                Vector rayO_center = center - ray.origin;
                float d = rayO_center.DotProduct(this.normal) / denom1;
                intersection[0] = ray.origin + (ray.direction.Normalize() * d);
                return true;
            }
            return false;
        }

    }

    public class Voxel
    {
        public Vector center;
        public Vector size { get; private set; }
        public Vector max;
        public Vector min;
        public Material material;
        public Plane[] planes = new Plane[6];
        public List<Vector> vertices;
        public PartitionPlane split;
        public float sa;

        public Voxel(Vector c, Vector s)
        {
            center = c;
            size = s;
            max = center + size / 2;
            min = center - size / 2;
            material = new BasicMaterial(Rgba32.Black);
            MakeVertices();
            sa = GetSurfaceArea();
            // MakePlanes();
        }

        public Voxel(Vector[] bounds)
        {
            center = (bounds[0] + bounds[1]) / 2.0f;
            max = bounds[0];
            min = bounds[1];
            size = max - min;
            material = new BasicMaterial(Rgba32.Black);
            MakeVertices();
            sa = GetSurfaceArea();
            // MakePlanes();
        }

        public Voxel(Vector c, Vector s, Material m)
        {
            center = c;
            size = s;
            max = center + size / 2;
            min = center - size / 2;
            material = m;
            sa = GetSurfaceArea();
            MakePlanes();
            MakeVertices();
        }

        private void MakeVertices()
        {
            // max z
            var v0 = Vector.Build.DenseOfVector(max);
            var v1 = Vector.Build.DenseOfArray(new float[] { min[0], max[1], max[2] });
            var v2 = Vector.Build.DenseOfArray(new float[] { min[0], min[1], max[2] });
            var v3 = Vector.Build.DenseOfArray(new float[] { max[0], min[1], max[2] });
            // min z
            var v4 = Vector.Build.DenseOfVector(min);
            var v5 = Vector.Build.DenseOfArray(new float[] { min[0], max[1], min[2] });
            var v6 = Vector.Build.DenseOfArray(new float[] { max[0], max[1], min[2] });
            var v7 = Vector.Build.DenseOfArray(new float[] { max[0], min[1], min[2] });
            vertices = new List<Vector>();
            vertices.AddRange(new Vector[] { v0, v1, v2, v3, v4, v5, v6, v7 });
        }

        private void MakePlanes()
        {
            var max_x_c = Vector.Build.DenseOfVector(center);
            var max_y_c = Vector.Build.DenseOfVector(center);
            var max_z_c = Vector.Build.DenseOfVector(center);
            var min_x_c = Vector.Build.DenseOfVector(center);
            var min_y_c = Vector.Build.DenseOfVector(center);
            var min_z_c = Vector.Build.DenseOfVector(center);

            max_x_c[0] += (size[0] / 2);
            max_y_c[1] += (size[1] / 2);
            max_z_c[2] += (size[2] / 2);
            min_x_c[0] -= (size[0] / 2);
            min_y_c[1] -= (size[1] / 2);
            min_z_c[2] -= (size[2] / 2);

            var max_x = new Plane(max_x_c, Vector.Build.DenseOfArray(new float[] { 1.0f, 0.0f, 0.0f }), size[1], size[2], material);
            var min_x = new Plane(min_x_c, Vector.Build.DenseOfArray(new float[] { -1.0f, 0.0f, 0.0f }), size[1], size[2], material);
            var max_y = new Plane(max_y_c, Vector.Build.DenseOfArray(new float[] { 0.0f, 1.0f, 0.0f }), size[0], size[2], material);
            var min_y = new Plane(min_y_c, Vector.Build.DenseOfArray(new float[] { 0.0f, -1.0f, 0.0f }), size[0], size[2], material);
            var max_z = new Plane(max_z_c, Vector.Build.DenseOfArray(new float[] { 0.0f, 0.0f, 1.0f }), size[0], size[1], material);
            var min_z = new Plane(min_z_c, Vector.Build.DenseOfArray(new float[] { 0.0f, 0.0f, -1.0f }), size[0], size[1], material);

            planes = new Plane[] {
                max_x, min_x,
                max_y, min_y,
                max_z, min_z,
            };
        }

        private float GetSurfaceArea()
        {
            return 2 * (size[0] * size[2] + // width * length
                        size[1] * size[2] + // height * length
                        size[1] * size[0]); // height * width
        }

        public Voxel[] Split(PartitionPlane P)
        {

            int cd = P.axis;

            float maxBound = max[cd];
            float minBound = min[cd];

            float c1d, c2d;
            c1d = P.center[cd] + ((maxBound - P.center[cd]) / 2);
            c2d = P.center[cd] + ((minBound - P.center[cd]) / 2);

            Vector c1 = Vector.Build.DenseOfVector(center);
            Vector c2 = Vector.Build.DenseOfVector(center);
            c1[cd] = c1d;
            c2[cd] = c2d;

            Vector s1 = Vector.Build.DenseOfVector(size);
            Vector s2 = Vector.Build.DenseOfVector(size);
            s1[cd] = Math.Abs(maxBound - P.center[cd]);
            s2[cd] = Math.Abs(minBound - P.center[cd]);

            Voxel v1 = new Voxel(c1, s1);
            Voxel v2 = new Voxel(c2, s2);

            return new Voxel[] { v1, v2 };
        }

        public bool InBounds(Shape3D s)
        {
            if (s is Triangle)
            {
                return InBounds(s as Triangle);
            }
            else if (s.AABB != null)
            {
                return InBounds(s.AABB);
            }
            else
            {
                System.Console.WriteLine("no AABB");
                return false;
            }
            // else if (s is Sphere)
            // {
            //     return InBounds(s as Sphere);
            // }
            // else if (s is Plane)
            // {
            //     return InBounds(s as Plane);
            // }

            // else return false;
        }

        public bool InBounds(Sphere s)
        {
            float d = 0;
            for (int i = 0; i < 3; i++)
            {
                float emin = s.center[i] - min[i];
                float emax = s.center[i] - max[i];
                if (emin < 0)
                {
                    d += (emin * emin);
                    if (emin < -s.radius)
                        return false;
                }
                else if (emax > 0)
                {
                    d += (emax * emax);
                    if (emax > s.radius)
                        return false;
                }
            }
            return d < s.radius2;
        }


        public bool InBounds(Triangle t)
        {
            float tmin, tmax, bmin, bmax;

            var b_norms = new Vector[] {
                Vector.Build.DenseOfArray(new float[] {1.0f, 0, 0}),
                Vector.Build.DenseOfArray(new float[] {0, 1.0f, 0}),
                Vector.Build.DenseOfArray(new float[] {0, 0, 1.0f}),
            };

            for (int i = 0; i < 3; i++)
            {
                Vector n = b_norms[i];
                Extensions.Project(t.vertices, n, out tmin, out tmax);
                if (tmax < min[i] || tmin > max[i])
                {
                    return false;
                }
            }

            float t_off = t.normal0.DotProduct(t.vertex0);
            Extensions.Project(vertices, t.normal0, out bmin, out bmax);
            if (bmax < t_off || bmin > t_off)
            {
                return false;
            }

            var t_edges = new Vector[] {
                t.vertex0 - t.vertex1,
                t.vertex1 - t.vertex2,
                t.vertex2 - t.vertex0,
            };

            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var axis = t_edges[i].CrossProduct(b_norms[j]);
                    Extensions.Project(vertices, axis, out bmin, out bmax);
                    Extensions.Project(vertices, axis, out tmin, out tmax);
                    if (bmax < tmin || bmin > tmax)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public bool InBounds(Vector point)
        {
            var dx = Math.Abs(point[0] - center[0]);
            var dy = Math.Abs(point[1] - center[1]);
            var dz = Math.Abs(point[2] - center[2]);
            return ((dx <= size[0]) &&
                    (dy <= size[1]) &&
                    (dz <= size[2]));
        }

        public bool InBounds(Voxel other)
        {
            for (int i = 0; i < 3; i++)
            {
                if (min[i] > other.max[i] || other.min[i] > max[i])
                    return false;
            }
            return true;
        }

        public void ISect(Ray ray, out float minimum, out float maximum)
        {
            float t1 = (min[0] - ray.origin[0]) * ray.i_dir[0];
            float t2 = (max[0] - ray.origin[0]) * ray.i_dir[0];
            float tmin = Math.Min(t1, t2);
            float tmax = Math.Min(t2, t1);
            for (int i = 1; i < 3; i++)
            {
                t1 = (min[i] - ray.origin[i]) * ray.i_dir[i];
                t2 = (max[i] - ray.origin[i]) * ray.i_dir[i];
                tmin = Math.Max(tmin, Math.Min(t1, t2));
                tmax = Math.Min(tmax, Math.Max(t1, t2));
            }
            minimum = Math.Min(tmin, tmax);
            maximum = Math.Max(tmax, tmin);
            maximum *= 1.00000024f;
            // return tmax > Math.Max(tmin, 0.0);
        }

        public bool Intersect(Ray ray, out float minimum, out float maximum)
        {
            var entry = Vector.Build.Dense(3);
            var exit = Vector.Build.Dense(3);
            var bounds = new Vector[] { min, max };
            minimum = float.MinValue;
            maximum = float.MaxValue;

            for (int i = 0; i < 3; i++)
            {
                float dmin = (bounds[0][i] - ray.origin[i]) * ray.i_dir[i];
                float dmax = (bounds[1][i] - ray.origin[i]) * ray.i_dir[i];
                if (dmin > dmax)
                {
                    var swp = dmin;
                    dmin = dmax;
                    dmax = swp;
                }
                entry[i] = dmin;
                exit[i] = dmax;
                if (i > 0)
                {
                    if (minimum > exit[i] || entry[i] > maximum)
                        return false;
                    minimum = Math.Max(minimum, dmin);
                    maximum = Math.Min(maximum, dmax);
                }
                else
                {
                    minimum = dmin;
                    maximum = dmax;
                }
            }
            return true;
        }
    }

    public class KDTree
    {
        public static int nObj = 0;
        public static List<Shape3D> objects;

        public static Node GetTree(List<Shape3D> L, Voxel V)
        {
            objects = L;
            return GetNode(L, V, 0);
        }

        public static Node GetNode(List<Shape3D> L, Voxel V, int depth)
        {
            if (Terminate(L, V) || depth > 5)
            {
                // nObj += L.Count;
                return new LeafNode(L, V);
            }
            // compute most efficient partition plane
            var P = ComputePartitionPlane(L, V, depth % 3);
            var splitV = V.Split(P);
            var splitL = P.Partition(L, splitV);
            return new Node(P, V,
                GetNode(splitL[0], splitV[0], depth + 1),
                GetNode(splitL[1], splitV[1], depth + 1));
        }

        private static bool Terminate(List<Shape3D> L, Voxel V)
        {
            if (L.Count < 10)
            {
                return true;
            }
            if (V.sa < 0.0001f)
            {
                return true;
            }
            return false;
        }

        private static PartitionPlane ComputePartitionPlane(List<Shape3D> L, Voxel V, int splitAxis)
        {
            L.Sort((a, b) =>
            {
                return (int)(a.center[splitAxis] - b.center[splitAxis]);
            });
            int i_m = (L.Count - 1) / 2;
            var split = new PartitionPlane(L[i_m].center, splitAxis);
            return split;
        }

        public static bool IntersectObjects(List<Shape3D> L, Ray ray, ref Shape3D hitObject, ref Vector intersect, ref Vector normal)
        {
            // run intersection test
            Shape3D closestObj = null;
            float? closestD = null;
            if (hitObject != null)
            {
                closestObj = hitObject;
                closestD = Math.Abs((ray.origin - intersect).Length());
            }
            foreach (Shape3D obj in L)
            {
                // if (obj.AABB.Intersect(ray, out var min, out var max))
                // {
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
                // }
            }
            if (closestObj != null)
                hitObject = closestObj;
            return hitObject != null;
        }

        public static bool Traverse(Ray ray, Node node, ref Shape3D hitObject, ref Vector intersect, ref Vector normal, int depth = 0)
        {
            // System.Console.WriteLine("Traversing Depth: " + depth);
            if (node is LeafNode)
            {
                var ln = node as LeafNode;
                // if(ln.bounds.Intersect(ray, out var min, out var max))
                IntersectObjects(ln.shapes, ray, ref hitObject, ref intersect, ref normal);
            }
            else
            {
                // traverse
                var ax = node.partition.axis;
                if (node.bounds.Intersect(ray, out var ad, out var bd))
                {
                    var s = float.MaxValue;
                    if (node.partition.Intersect(ray, out var splitIntersect, out var n))
                        s = splitIntersect[0][ax];
                    float a, b;
                    a = ray.origin[ax] + (ray.direction[ax] * ad);
                    b = ray.origin[ax] + (ray.direction[ax] * bd);
                    if (a <= s)
                    {
                        if (b < s)
                        {
                            Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1);
                        }
                        else
                        {
                            if (b == s)
                            {
                                Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1);
                                Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1);
                            }
                            else
                            {
                                Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1);
                                Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1);
                            }
                        }
                    }
                    else
                    {
                        if (b > s)
                        {
                            Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1);
                        }
                        else
                        {
                            Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1);
                            Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1);
                        }
                    }
                }
            }
            return hitObject != null;
        }

        public static string PrintNode(Node node, int depth = 0)
        {
            var result = "";
            for (int i = 0; i < depth; i++)
                result += "    ";
            if (node is LeafNode)
            {
                return result + node.ToString() + "\n";
            }
            else
            {
                return result + node.ToString() + PrintNode(node.left, depth + 1) + PrintNode(node.right, depth + 1);
            }
        }

    }
}