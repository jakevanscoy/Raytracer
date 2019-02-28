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
            if(args.Length > 1) {
                try {
                    width = int.Parse(args[0]);
                    height = int.Parse(args[1]);
                    if(args.Length > 2) {
                        frames = int.Parse(args[2]);
                    }
                } catch {
                    width = 1200;
                    height = 800;
                    System.Console.WriteLine("Error parsing command line args");
                    System.Console.WriteLine("correct format 'dotnet run [<width> <height> [<frames>]]'");
                    System.Console.WriteLine("...continuing using default width/height...");
                }
            }
            Raytracer raytracer = new Raytracer(width, height);
            Console.WriteLine("Rendering Images...");
            var watch = System.Diagnostics.Stopwatch.StartNew();
            if(frames > 1) {
                raytracer.RenderGif(frames:frames, axis:2, length:3.0f, start: -3.75f, end: 3.75f);
            } else {
                raytracer.Render("outpng.png");
            }
            watch.Stop();
            var time = watch.Elapsed;
            Console.WriteLine("Done!"); 
            Console.WriteLine("Rendered in: " + time.PrettyPrint());
        }
    }
}
