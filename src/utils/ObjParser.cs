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
            List<Shape3D> objShapes = new List<Shape3D>();
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
                    Shape3D shape= null;
                    switch(f.Count){ 
                        case 3:
                            shape = new Triangle(verts, m);
                            break;
                        case 4:
                            shape = new Plane(verts, m);
                            break;
                        default:
                            shape = new Plane(verts, m);
                            break;
                    }
                    if(shape != null)
                        objShapes.Add(shape);
                }
            }
            ComplexObject obj = new ComplexObject(objShapes, m);
            return obj;
        }
    }
}