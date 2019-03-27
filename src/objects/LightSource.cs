using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
 


namespace Raytracing {
    
    using Vector = Vector<float>;

    public class LightSource : BasicObject {
        public Rgba32 color { get; private set; }

        public float strength { get; set; }
        public LightSource(Vector pos) {
            center = pos;
            color = Rgba32.White;
            strength = 1.0f;
        }
        public LightSource(Vector pos, Rgba32 col) {
            center = pos;
            color = col;
            strength = 1.0f;
        }
        public LightSource(Vector pos, Rgba32 col, float str) {
            center = pos;
            color = col;
            strength = str;
        }

    }
}