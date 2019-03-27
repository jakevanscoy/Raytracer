using System;
using System.IO;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using ObjLoader;


namespace Raytracing {
    
    using Vector = Vector<float>;

    public class OBJParser {

        public static ComplexObject LoadObjFile(string filename) {
            var factory = new ObjLoader.Loader.Loaders.ObjLoaderFactory();
            var loader = factory.Create();
            var fs = File.OpenRead(filename);
            var result = loader.Load(fs);
            List<Triangle> objTris = new List<Triangle>();
            var m = new BasicMaterial(Rgba32.Gray);
            foreach(var g in result.Groups) {
                foreach(var f in g.Faces) {
                    Vector[] verts = new Vector[f.Count];
                    for(var fv = 0; fv < f.Count; fv++) {
                        var vx = result.Vertices[f[fv].VertexIndex-1].X;
                        var vy = result.Vertices[f[fv].VertexIndex-1].Y;
                        var vz = result.Vertices[f[fv].VertexIndex-1].Z;
                        Vector vert = Vector.Build.DenseOfArray(new float[] {vx, vy, vz});
                        verts[fv] = vert;
                    }
                    var t = new Triangle(verts, m);
                    objTris.Add(t);
                }
            }
            ComplexObject obj = new ComplexObject(objTris, m);
            return obj;
        }
    }
}