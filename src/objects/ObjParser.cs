using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
 


namespace Raytracing {
    
    using Vector = Vector<float>;

    struct Face {
        public int v1, v2, v3;
        public int vt1, vt2, vt3;
        public int vn1, vn2, vn3;
        public bool tMapped;
        public bool nMapped;
    }

    public class OBJParser {
        public static ComplexObject ParseObjFile(string filename) {
            using (FileStream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (BufferedStream bs = new BufferedStream(fs))
            using (StreamReader sr = new StreamReader(bs))
            {       
                List<Vector> v = new List<Vector>();
                List<Vector> vt = new List<Vector>();
                List<Vector> vn = new List<Vector>();
                List<Face> faces = new List<Face>(); 
                string line;
                while((line = sr.ReadLine()) != null) {
                    var l_split = line.Split(" ");
                    // System.Console.WriteLine(line);
                    switch(l_split[0]) {
                        case "v":
                            var vert = ParseVector(l_split);
                            v.Add(vert);
                            break;
                        case "vt":
                            var uv = ParseVector(l_split);
                            vt.Add(uv);
                            break;
                        case "vn":
                            var norm = ParseVector(l_split);
                            vn.Add(norm);
                            break;
                        case "f":
                            var face = ParseFace(l_split);
                            faces.Add(face);
                            break;
                    }
                }
                List<Triangle> obj_tris = new List<Triangle>();
                BasicMaterial defaultMat = new BasicMaterial(Rgba32.Gray);
                for(int t = 0; t < faces.Count; t++) {
                    Face f = faces[t];
                    Vector[] verts = new Vector[3];
                    Vector[] uvs = new Vector[3];
                    Vector[] norms = new Vector[3];
                    verts[0] = v[f.v1-1];
                    verts[1] = v[f.v2-1];
                    verts[2] = v[f.v3-1];
                    if(f.tMapped) {
                        uvs[0] = vt[f.vt1-1];
                        uvs[1] = vt[f.vt2-1];
                        uvs[2] = vt[f.vt3-1];
                    }
                    if(f.nMapped) {
                        norms[0] = vn[f.vn1-1];
                        norms[1] = vn[f.vn2-1];
                        norms[2] = vn[f.vn3-1];
                    }
                    Triangle triangle;
                    if(f.tMapped && f.nMapped) {
                        triangle = new Triangle(verts, norms, uvs, defaultMat);
                    } else if(f.tMapped) {
                        triangle = new Triangle(verts, uvs, defaultMat);
                    } else {
                        triangle = new Triangle(verts, defaultMat);
                    }
                    obj_tris.Add(triangle);
                }
                ComplexObject result = new ComplexObject(obj_tris, defaultMat);
                return result;
            }
        }

        private static Vector ParseVector(string[] l_split) {
            var result = Vector.Build.Dense(l_split.Length-1);
            for(int i = 1; i < l_split.Length; i++) {
                result[i-1] = float.Parse(l_split[i]);
            }
            return result;
        }

        private static Face ParseFace(string[] l_split) {
            // List<int[]> face = new List<int[]>();
            Face face = new Face();
            var v1_split = l_split[1].Split("/");
            var v2_split = l_split[2].Split("/");
            var v3_split = l_split[3].Split("/");

            switch(v1_split.Length) {
                case 3: {
                    face.tMapped = int.TryParse(v1_split[1], out var vt1);
                    face.nMapped = int.TryParse(v1_split[2], out var vn1);
                    if(face.nMapped) {
                        face.vn1 = vn1;
                        face.vn2 = int.Parse(v2_split[2]);
                        face.vn3 = int.Parse(v3_split[2]);
                    }
                    if(face.tMapped) {
                        face.vt1 = vt1;
                        face.vt2 = int.Parse(v2_split[1]);
                        face.vt3 = int.Parse(v3_split[1]);
                    }
                    face.v1 = int.Parse(v1_split[0]);
                    face.v2 = int.Parse(v2_split[0]);
                    face.v2 = int.Parse(v2_split[0]);
                    break;
                } case 2: {
                    face.tMapped = int.TryParse(v1_split[1], out var vt1);
                    // face.nMapped = int.TryParse(v1_split[2], out var vn1);
                    if(face.tMapped) {
                        face.vt1 = int.Parse(v1_split[1]);
                        face.vt2 = int.Parse(v2_split[1]);
                        face.vt3 = int.Parse(v3_split[1]);
                    }
                    face.v1 = int.Parse(v1_split[0]);
                    face.v2 = int.Parse(v2_split[0]);
                    face.v3 = int.Parse(v3_split[0]);
                    break;
                } default: {
                    face.v1 = int.Parse(v1_split[0]);
                    face.v2 = int.Parse(v2_split[0]);
                    face.v3 = int.Parse(v3_split[0]);
                    break;        
                }        
            }
            return face;
        }
    }

}