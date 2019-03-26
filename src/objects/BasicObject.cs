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
        public abstract void Scale(float sx, float sy, float sz);

        public Matrix<float> GetTranslateMatrix(float tx, float ty, float tz) {
            var matrix = Matrix<float>.Build.DiagonalIdentity(4,4);
            matrix[0, 3] = tx;
            matrix[1, 3] = ty;
            matrix[2, 3] = tz;
            return matrix;
        }

        public virtual void Translate(float tx, float ty, float tz) {
            var matrix = GetTranslateMatrix(tx, ty, tz);
            center = matrix * center;     
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
    
    }
}