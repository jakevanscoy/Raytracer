using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing {
    using Vector = Vector<float>;

    public class ProgressBar {
        
        private char[] styles = {'|'};

        private string message = "";
        private string units = "";
        private int start;
        private int length;
        private int stop;
        public ProgressBar(int _start, int _stop, int _length) {
            start = _start;
            stop = _stop;
            length = _length;
        }

        public ProgressBar(int _start, int _stop, int _length, char[] _styles) {
            start = _start;
            stop = _stop;
            length = _length;
            styles = _styles;
        }

        public ProgressBar(int _start, int _stop, int _length,
                        char[] _styles, string _message, string _units) {
            start = _start;
            stop = _stop;
            length = _length;
            styles = _styles;
            message = _message;
            units = _units;
        }

        public void PrintProgressBar(int current) {
            var normalProg = ((float)current / (float)stop) * length;
            var percent = (float)current / (float)stop;
            var s = styles[current%styles.Length];
            System.Console.Write("{");
            for(int c = 0; c < length; c++) {
                if(c < normalProg)
                    System.Console.Write(s);
                else 
                    System.Console.Write(' ');
            }
            System.Console.Write("}");  
            System.Console.WriteLine();
        }

        public void PrintProgressEstTime(int current, TimeSpan est) {
            System.Console.Clear();
            for(var n = 0; n < (System.Console.WindowHeight/2)-3; n++) {
                System.Console.WriteLine();
            }
            System.Console.WriteLine(message);
            System.Console.WriteLine(units + ": " + current + "/" + stop);
            System.Console.Write("Progress: ");
            var normalProg = ((float)current / (float)stop) * length;
            var percent = (float)current / (float)stop;
            var s = styles[current%styles.Length];
            System.Console.Write(String.Format(" {0:p}", percent));
            
            System.Console.WriteLine("\t ETA: " + est.PrettyPrint());
            PrintProgressBar(current);
            System.Console.WriteLine();
        }
    }
    public static class Extensions {
        
        public static Vector CrossProduct(this Vector left, Vector right) {
            if(left.Count != 3 || right.Count != 3) {
                string message = "Vectors must have a length of 3.";
                throw new Exception(message);
            }
            var result = Vector.Build.Dense(3);
                result[0] =  left[1] * right[2] - left[2] * right[1];
                result[1] = -left[0] * right[2] + left[2] * right[0]; 
                result[2] =  left[0] * right[1] - left[1] * right[0]; 
            return result;
        }

        // Normalizes a 3D Vector
        // returns: the given 3D vector normalized by its euclidean length
        public static Vector Normalize(this Vector v) {
            if(v.L2Norm() != 0) {
                for(var i = 0; i < 3; i++) {
                    v[i] = v[i] / (float)v.L2Norm();
                }
            }
            return v;
        }
        
        // Wrapper function for MathNet's quadratic solver
        // returns: a List<float> of the roots (0, 1, or 2 of them)
        public static List<float> SolveQuadratic(float a, float b, float c) {
            var tuple = FindRoots.Quadratic((double)c, (double)b, (double)a);
            var result = new List<float>();

            if(tuple.Item1.IsReal()){
                result.Add((float)tuple.Item1.Real);
            }
            if(tuple.Item2.IsReal()){
                result.Add((float)tuple.Item2.Real);
            }
            return result;
        }

        public static Vector ToVector(this Rgba32 color) {
            float R = ByteToFloat(color.R);
            float G = ByteToFloat(color.G);
            float B = ByteToFloat(color.B);
            return Vector.Build.DenseOfArray(new float[] {R, G, B});
        }

        public static Vector Multiply(this Vector vector, Vector other) {
            Vector result = Vector.Build.Dense(3);
            result[0] = vector[0] * other[0];
            result[1] = vector[1] * other[1];
            result[2] = vector[2] * other[2];
            return result;
        }
        public static float ByteToFloat(byte b) {
            return b / 255.0f;
        }

        public static Rgba32 ToColor(this Vector vector) {
            float R = Math.Max(vector[0], 0);
            float G = Math.Max(vector[1], 0);
            float B = Math.Max(vector[2], 0);
            float A = 1.0f;
    
            return new Rgba32(R, G, B, A);
        }

        public static string PrettyPrint(this TimeSpan ts) {
            string result = "";
            int days = (int)ts.TotalDays;
            int hours = (int)ts.TotalDays;
            int mins = (int)ts.TotalMinutes;
            double secs = Math.Round(ts.TotalSeconds - (mins * 60), 3);
            if(days > 0) result += days +" Days, ";
            if(hours > 0) result += hours +" Hours, ";
            if(mins > 0) result += mins +" Minutes, ";
            result += secs +" Seconds";
            return result;
        }

    }
}
