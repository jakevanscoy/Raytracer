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
                if (lV.InBounds(s))
                {
                    left.Add(s);
                }
                if (rV.InBounds(s))
                {
                    right.Add(s);
                }
            }
            return new List<Shape3D>[] { left, right };
        }

        public bool Intersect(Ray ray, out Vector[] intersection, out Vector[] normal)
        {
            intersection = new Vector[1];
            normal = new Vector[] { this.normal };
            float denom = this.normal.DotProduct(ray.direction.Normalize());
            if ((float)Math.Abs(denom) > 0.00001f)
            {
                Vector rayO_center = center - ray.origin;
                float d = rayO_center.DotProduct(this.normal) / denom;
                intersection[0] = ray.origin + (ray.direction.Normalize() * d);
                return (d >= 0.0f);
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
        public PartitionPlane split;
        public float sa;
        public Voxel(Vector c, Vector s)
        {
            center = c;
            size = s;
            max = center + size / 2;
            min = center - size / 2;
            material = new BasicMaterial(Rgba32.Black);
            sa = GetSurfaceArea();
            MakePlanes();
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

            var max_x = new Plane(max_x_c, Vector.Build.DenseOfArray(new float[] { 1.0f, 0.0f, 0.0f }), size[2], size[1], material);
            var min_x = new Plane(min_x_c, Vector.Build.DenseOfArray(new float[] { -1.0f, 0.0f, 0.0f }), size[2], size[1], material);
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
            if (s is Sphere)
            {
                return InBounds(s as Sphere);
            }
            else if (s is Plane)
            {
                return InBounds(s as Plane);
            }
            else if (s is Triangle)
            {
                return InBounds(s as Triangle);
            }
            else return false;
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

        public bool InBounds(Plane p)
        {
            bool center = false,
                p0 = false,
                p1 = false,
                p2 = false, 
                p3 = false;
            for(int d = 0; d < 3; d++) {
                if(p.center[d] < max[d] && p.center[d] > min[d]) {
                    center = true;
                } else {
                    center = false;
                }
                if(p.p0[d] < max[d] && p.p0[d] > min[d]) {
                    p0 = true;
                } else {
                    p0 = false;
                }
                if(p.p1[d] < max[d] && p.p1[d] > min[d]) {
                    p1 = true;
                } else {
                    p1 = false;
                }
                if(p.p2[d] < max[d] && p.p2[d] > min[d]) {
                    p2 = true;
                } else {
                    p2 = false;
                }
                if(p.p3[d] < max[d] && p.p3[d] > min[d]) {
                    p3 = true;
                } else {
                    p3 = false;
                }
            }
            return(center || p0 || p1 || p2 || p3);
        }

        public bool InBounds(Triangle t)
        {
            bool p0 = false,
                 p1 = false,
                 p2 = false;
            for(int d = 0; d < 3; d++) {
                if(t.vertex0[d] < max[d] && t.vertex1[d] > min[d]) {
                    p0 = true;
                } else {
                    p0 = false;
                }
                if(t.vertex1[d] < max[d] && t.vertex1[d] > min[d]) {
                    p1 = true;
                } else {
                    p1 = false;
                }
                if(t.vertex2[d] < max[d] && t.vertex2[d] > min[d]) {
                    p2 = true;
                } else {
                    p2 = false;
                }
            }
            return (p0 || p1 || p2);
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

        public bool Intersect(Voxel other)
        {
            for (int i = 0; i < 3; i++)
            {
                if (min[i] > other.max[i] || other.min[i] > max[i])
                    return false;
            }
            return true;
        }

        public bool Intersect(Ray ray, out Vector entry, out Vector exit)
        {
            entry = null;
            exit = null;
            var enD = float.MaxValue;
            var exD = float.MinValue;
            var intersects = new Dictionary<Vector, float>();
            // System.Console.WriteLine(size);
            for (int i = 0; i < 6; i++)
            {
                // System.Console.WriteLine(planes[i].center);
                if (planes[i].Intersect(ray, out var inter, out var n))
                {
                    var d = (float)Math.Abs((inter[0] - ray.origin).Length());
                    // intersects.Add(inter[0], d);
                    if (d < enD)
                    {
                        enD = d;
                        entry = inter[0];
                    }
                    if (d > exD)
                    {
                        exD = d;
                        exit = inter[0];
                    }
                }
            }
            // if((entry == null || exit == null)) {
            //     System.Console.WriteLine(entry);
            //     System.Console.WriteLine(exit);
            // }
            // System.Console.WriteLine((entry != null && exit != null));
            return (entry != null || exit != null);
        }
    }

    public class KDTree
    {
        public static int nh = 0;
        public static List<Shape3D> objects;
        public static Node GetTree(List<Shape3D> L, Voxel V)
        {
            objects = L;
            return GetNode(L, V, 0);
        }
        public static Node GetNode(List<Shape3D> L, Voxel V, int depth)
        {
            if (Terminate(L, V) || depth > 7)
            {
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
            if (L.Count < 5)
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

        public static bool IntersectObjects(List<Shape3D> objects, Ray ray, ref Shape3D hitObject, ref Vector intersect, ref Vector normal)
        {
            // run intersection test
            Shape3D closestObj = null;
            float? closestD = null;
            if (hitObject != null)
            {
                closestObj = hitObject;
                closestD = Math.Abs((ray.origin - intersect).Length());
            }
            // intersect = null;
            // normal = null;
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
                // System.Console.WriteLine("Node: " + ln.shapes.Count);
                return IntersectObjects(ln.shapes, ray, ref hitObject, ref intersect, ref normal);
            }
            else
            {
                // traverse
                var ax = node.partition.axis;
                if (node.bounds.Intersect(ray, out var enter, out var exit))
                {
                    var s = float.MaxValue;
                    if (node.partition.Intersect(ray, out var splitIntersect, out var n))
                        s = splitIntersect[0][ax];
                    float a, b;
                    if(enter == null) a = float.MaxValue;
                    else a = enter[ax];
                    if(exit == null) b = float.MaxValue;
                    else b = exit[ax];
                    if (a <= s)
                    {
                        if (b < s)
                        {
                            return Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1);
                        }
                        else
                        {
                            if (b == s)
                            {
                                return Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1);
                                // Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth+1);
                            }
                            else
                            {
                                if(Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1)) {
                                    return true;
                                }
                                if(Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1)) {
                                    return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (b > s)
                        {
                            return Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1);
                        }
                        else
                        {
                            if(Traverse(ray, node.left, ref hitObject, ref intersect, ref normal, depth + 1)) {
                                return true;
                            }
                            if(Traverse(ray, node.right, ref hitObject, ref intersect, ref normal, depth + 1)) {
                                return true;
                            }
                        }
                    }
                }
            }
            if (hitObject != null)
            {
                return true;
            }
            else
            {
                return false;
            }
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