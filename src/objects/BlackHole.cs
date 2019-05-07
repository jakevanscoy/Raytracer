using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;



namespace Raytracing
{

    using Vector = Vector<float>;
    public class BlackHole : Sphere
    {
        public static float c = 299792458f;
        public static float G;
        public float M;

        public Sphere eventHorizon;

        // Schwarzschild Radius
        private float sradius;
        public float Sradius
        {
            get
            {
                return sradius;
            }
            set
            {
                sradius = value;
                eventHorizon.radius = value;
                eventHorizon.radius2 = value * value;
                // this.radius = value * 2;
                // this.radius2 = radius * radius;
            }
        }

        public BlackHole(Vector center, float r, float sr) : base(center, r)
        {
            sradius = sr;
            eventHorizon = new Sphere(center, sr, new BasicMaterial(Rgba32.Black));
        }
    }

    public class LenseMaterial : TransmissiveMaterial
    {
        public LenseMaterial(PhongIlluminationModel model) : base(model) { }
    }

    public class BlackHoleMaterial : BasicMaterial
    {
        private BlackHole blackHole;
        private World world;
        public BlackHoleMaterial(BlackHole bh, World w)
        {
            world = w;
            blackHole = bh;
        }

        public Rgba32 Intersect(Ray ray, Vector<float> intersection, Vector<float> normal, int depth, bool KD = false)
        {
            // calculate deflection angle based on Schwarzschild radius
            float d = (float)Math.Abs((blackHole.center - intersection).Length());
            var deflectionAngle = 2f * (blackHole.Sradius / d);
            deflectionAngle *= (float)(Math.PI / 180f);
            System.Console.WriteLine(deflectionAngle);
            // axis of rotation = unit vector perpendicular to both ray direction and normal vector
            var axis = normal.CrossProduct(ray.direction).Normalize();
            var rotationMatrix = blackHole.GetRotationMatrixAboutAxis(axis, deflectionAngle);

            // transform ray direction vector
            var nOrg = intersection + (normal * 0.001f);
            var rDir4 = ray.direction.GetVector4();
            var nDir4 = rotationMatrix * rDir4;
            var nDir = nDir4.SubVector(0, 3);
            // System.Console.WriteLine("Ray direction: " + ray.direction);
            // System.Console.WriteLine("New direction: " + nDir.Normalize());
            // cast new Ray
            var nRay = new Ray(nOrg, nDir.Normalize());
            Rgba32 color;
            if (KD)
            {
                world.SpawnRayKD(nRay, out color, depth + 1);
            }
            else
            {
                world.SpawnRay(nRay, out color, depth + 1);
            }
            // System.Console.WriteLine(color);
            return color;
        }

    }



}