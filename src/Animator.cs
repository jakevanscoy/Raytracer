using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
 

namespace Raytracing {
    using Vector = Vector<float>;
    public class Animator {
        public delegate Vector VectorTransformer (Vector v, Vector transform, float t);

        public static VectorTransformer add = (v, tr, t) => {
            return (v + (tr * t));
        };

        public static VectorTransformer sub = (v, tr, t) => {
            return (v - (tr * t));
        };

        public static Vector AnimateVector(Vector v, Vector transform, float t, VectorTransformer transformFunction) {
            return transformFunction(v, transform, t);
        }
    }
}