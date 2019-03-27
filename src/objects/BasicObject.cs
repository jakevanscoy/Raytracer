using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing {

    using Vector = Vector<float>;
    /// The base class for all of our 3D Objects
    public abstract class BasicObject {
        public int objID {get; set;}
        public Vector center {get; set;}

        public Matrix<float> GetTranslateMatrix(float tx, float ty, float tz) {
            var matrix = Matrix<float>.Build.DenseIdentity(4,4);
            matrix[0, 2] = tx;
            matrix[1, 2] = ty;
            matrix[2, 2] = tz;
            return matrix;
        }

        public virtual void Translate(float tx, float ty, float tz) {
            // var matrix = GetTranslateMatrix(tx, ty, tz);
            center[0] += tx;
            center[1] += ty;
            center[2] += tz;
        }

        public Matrix<float> GetRotateXMatrix(float theta) {
            var cosT = (float)Math.Cos(theta);
            var sinT = (float)Math.Sin(theta);
            var rx = new float[,] {
                { 1.0f, 0.0f,  0.0f },
                { 0.0f, cosT, -sinT },
                { 0.0f, sinT,  cosT } 
            };
            var matrix = Matrix<float>.Build.DenseOfArray(rx);  
            return matrix;
        }

        public Matrix<float> GetRotateYMatrix(float theta) {
            var cosT = (float)Math.Cos(theta);
            var sinT = (float)Math.Sin(theta);
            var ry = new float[,] {
                {  cosT, 0.0f, sinT },
                {  0.0f, 1.0f, 0.0f },
                { -sinT, 0.0f, cosT } 
            };
            var matrix = Matrix<float>.Build.DenseOfArray(ry);  
            return matrix;
        }

        public Matrix<float> GetRotateZMatrix(float theta) {
            var cosT = (float)Math.Cos(theta);
            var sinT = (float)Math.Sin(theta);
            var ry = new float[,] {
                { cosT, -sinT, 0.0f },
                { sinT,  cosT, 0.0f },
                { 0.0f,  0.0f, 1.0f } 
            };
            var matrix = Matrix<float>.Build.DenseOfArray(ry);  
            return matrix;
        }

        public virtual void RotateX(float theta) {
            var matrix = GetRotateXMatrix(theta); 
            center = matrix * center;          
        }

        public virtual void RotateY(float theta) {
            var matrix = GetRotateYMatrix(theta);  
            center = matrix * center;       
        }

        public virtual void RotateZ(float theta) {
            var matrix = GetRotateZMatrix(theta);
            center = matrix * center;       
        }

        public Matrix<float> GetScaleMatrix(float sx, float sy, float sz) {
            var matrix = Matrix<float>.Build.DiagonalOfDiagonalArray(new float[]{sx, sy, sz});
            return matrix;
        }

        public virtual void Scale(float sx, float sy, float sz) {
            var matrix = GetScaleMatrix(sx, sy, sz);
            center = matrix * center;
        }

    }
}