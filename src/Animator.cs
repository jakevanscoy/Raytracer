using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
 

namespace Raytracing {
    using Vector = Vector<float>;
    public class Animator {
        
        public Vector target;
        public Vector animator;
        public VectorTransformer transformer;

        public Animator(Vector _target, Vector _animator, VectorTransformer _transformer) {
            target = _target;
            animator = _animator;
            transformer = _transformer;
        }

        public void Animate(float time) {
            target = transformer(target, animator, time);
        }

        public delegate Vector VectorTransformer (Vector v, Vector transform, float t);

        public static VectorTransformer add = (Vector v, Vector tr, float t) => {
            return (v + (tr * t));
        };

        public static VectorTransformer sub = (Vector v, Vector tr, float t) => {
            return (v - (tr * t));
        };
    }
}