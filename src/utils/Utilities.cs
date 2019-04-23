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

        public void PrintProgressBarNoEst(int current) {
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
            System.Console.Write(String.Format(" {0:p} ", percent));
            PrintProgressBar(current);
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
            float vlen = v.Length();
            if(vlen > 0)
                return v / vlen;
            else
                return v;
        }

        public static float Length(this Vector v) {
            float sum = 0.0f;
            for(var i = 0; i < 3; i++) {
                sum += (v[i] * v[i]);
            }
            return (float)Math.Sqrt(sum);
        }
        public static bool Quadratic(float a, float b, float c, ref float r0, ref float r1) {
            float disc = b * b - 4 * a * c;
            if(disc < 0) return false;
            else if(disc == 0) r0 = r1 = -0.5f * b / a;
            else {
                float q = (b > 0) ?
                    -0.5f * (b + (float)Math.Sqrt(disc)) :
                    -0.5f * (b - (float)Math.Sqrt(disc));
                r0 = q / a;
                r1 = c / q;
            }
            if(r0 > r1) {
                float tmp = r0;
                r0 = r1;
                r1 = tmp;
            }
            return true;
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

        public static float Dot3(Vector v1, Vector v2) {
            return (v1[0] * v2[0]) + (v1[1] * v2[1]) + (v1[2] * v2[2]);
        }

        public static Vector Multiply(this Vector vector, Vector other) {
            var result = Vector.Build.Dense(3);
            result[0] = vector[0] * other[0];
            result[1] = vector[1] * other[1];
            result[2] = vector[2] * other[2];
            return result;
        }

        // public static Vector Reflect(Vector inVec, Vector mirrorVec) {
        //     float c = -Dot3(inVec, mirrorVec);
        //     var result = Vector.Build.Dense(3);
        //     result[0] = -(inVec[0] + (2 * mirrorVec[0] * c));
        //     result[1] = -(inVec[1] + (2 * mirrorVec[1] * c));
        //     result[2] = -(inVec[2] + (2 * mirrorVec[2] * c));
        //     return result;
        // }

        public static Vector Clamp(this Vector v, float min, float max) {
            v[0].Clamp(min, max);
            v[1].Clamp(min, max);
            v[2].Clamp(min, max);
            return v;
        }

        public static float Clamp(this float f, float min, float max) {
            return Math.Min(Math.Max(f, min), max);
        }

        public static Vector Reflected(Vector inVec, Vector nVec) {
            return (nVec * (2.0f * Dot3(inVec, nVec))) - inVec;
        }


        public static Vector GetRefraction(Vector dir, Vector normal, float in_index, float tr_index) {
            dir = dir.Normalize();
            normal = normal.Normalize();
            float nd = Dot3(normal, dir);
            float nd2 = nd * nd;
            float ni = in_index;
            float nt = tr_index;
            Vector norm = normal;
            if(nd > 0) {
                nd = -nd;
                // OUTSIDE
            } else {
                // INSIDE
                norm = -normal;
                ni = tr_index;
                nt = in_index;
            }
            float ni2 = ni * ni;
            float nt2 = nt * nt;
            float ind = ni / nt;
            float ind2 = ind * ind;
            float k = 1 - ((ind2 * (1 - nd2)));
            if(k < 0) {
                //TOTAL INTERNAL REFLECTION
                return Reflected(-dir, norm);
            } else {
                var left = (ni * (dir - norm * (nd)))/nt;
                return left + (norm * (float)Math.Sqrt(k));
            }
            // return k < 0 ? Reflected(dir, norm) : norm * dir + (norm * nd - (float)Math.Sqrt(k)) * n; 
            // var tmpLeft = (ni * (dir - norm * (nd))) / nt;
            // var tmpRight = norm * (float)Math.Sqrt(ni2 * ((1-nd2)/nt2));
            // return tmpLeft + tmpRight;
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

        public static Vector GetVector4(this Vector vector) {
            var v4 = Vector.Build.Dense(4);
            v4[0] = vector[0];
            v4[1] = vector[1];
            v4[2] = vector[2];
            v4[3] = 1.0f;
            return v4;
        }

        public static void SortObjects(int dimension, ref List<Shape3D> obj) {
            MergeSortObjects(dimension, ref obj, 0, obj.Count);
        }

        private static void MergeSortObjects(int d, ref List<Shape3D> obj, int left, int right) {
            if(right < left)
                return; 
            int middle = (left + right) / 2;
            MergeSortObjects(d, ref obj, left, middle);
            MergeSortObjects(d, ref obj, middle+1, right);
            MergeObjects(d, ref obj, left, middle, right);
        }

        private static void MergeObjects(int d, ref List<Shape3D> obj, int left, int middle, int right) {
            int n1 = middle - left + 1;
            int n2 = right - middle;
            var l_tmp = new List<Shape3D>();
            var r_tmp = new List<Shape3D>();
            int i = 0, j = 0, k = 0;

            for(i = 0; i < n1; i++) 
                l_tmp[i] = obj[left + i]; 
            for(j = 0; j < n2; j++) 
                r_tmp[j] = obj[middle + 1 + j]; 

            while(i < n1 && j < n2) {
                if(l_tmp[i].center[d] <= r_tmp[j].center[d]) {
                    obj[k] = l_tmp[i];
                    i++;
                } else {
                    obj[k] = r_tmp[j];
                    j++;
                }
                k++;
            }
            while(i < n1) {
                obj[k] = l_tmp[i];
                i++;
                k++;
            }
            while(j < n2) {
                obj[k] = r_tmp[j];
                j++;
                k++;
            }
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
