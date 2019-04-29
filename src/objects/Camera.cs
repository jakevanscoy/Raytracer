using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing {

    using Vector = Vector<float>;
    public class Camera : BasicObject {
        public Vector lookAt {get; set;}
        public Vector up {get; set;}
        public World world {get; private set;}

        // transforms world coordinates to camera coordinates
        public Matrix<float> viewTransform {get; private set;}

        public Camera(Vector pos, Vector lookAt, Vector up, World world) {
            this.center = pos;
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
            Vector rayOrigin = this.center;
            Vector currentLookAt = Vector.Build.DenseOfArray(
                new float[] { lookAt[0] + x_s, lookAt[1] + y_s, lookAt[2] }
            );
            Vector rayDirection = Extensions.Normalize(currentLookAt - this.center);
            Ray ray = new Ray(rayOrigin, rayDirection);
            // spawn ray in world, returning a color
            world.SpawnRayKD(ray, out var resultColor, 0);
            return resultColor;
        }

    }
}