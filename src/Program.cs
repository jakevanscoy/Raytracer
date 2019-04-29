using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing {
    class Program
    {
        static void Main(string[] args)
        { 
            Console.WriteLine("Initializing Raytracer...");
            int width  = 1200, height = 800, frames = 24;
            float length = 1.0f;
            string fname = "out";
            if(args.Length > 1) {
                try {
                    width = int.Parse(args[0]);
                    height = int.Parse(args[1]);
                    if(args.Length > 2) {
                        frames = int.Parse(args[2]);
                    }
                    if(args.Length > 3) {
                        if(frames > 1)
                            length = float.Parse(args[3]);
                        else
                            fname = args[3];
                    }
                    if(args.Length > 4) {
                        fname = args[4];
                    }
                } catch {
                    foreach(string s in args) {
                        System.Console.WriteLine(s);
                    }
                    width = 600;
                    height = 400;
                    length = 1.0f;
                    frames = 24;
                    System.Console.WriteLine("Error parsing command line args");
                    System.Console.WriteLine("correct format 'dotnet run [<width> <height> [<frames>] [<output file name>]]'");
                    System.Console.WriteLine("...continuing using default width/height...");
                }
            }
            Raytracer raytracer = new Raytracer(width, height);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if(frames > 1) {
                raytracer.RenderAnimation(filename:fname, frames:frames, length:length);
            } else {
                System.Console.WriteLine("Rendering single image...");
                raytracer.Render(fname+".png", samples:1);
            }
            watch.Stop();
            var time = watch.Elapsed;
            Console.WriteLine("Done!"); 
            Console.WriteLine("Rendered in: " + time.PrettyPrint());
        }
    }
}
