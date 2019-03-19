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
    public class Raytracer {
        public Camera camera {get; set;}
        public World world {get; set;}
        public Shape3D[] animationObjects;

        // default constructor
        public Raytracer(int width = 800, int height = 800) {
            // initialize default world and Object3Ds
            world = SceneFactory.GetDefaultWorld(width, height);
            camera = world.cameras[0];
        }

        public Image<Rgba32> Render(string fileName = "", int samples = 1) {
            Image<Rgba32> image = new Image<Rgba32>(world.width, world.height);
            float whRatio = (float)world.width / (float)world.height;
            // screen coordinates
            float[] S = {
                -1.0f, -1.0f / whRatio, // x0, y0
                1.0f,  1.0f / whRatio  // x1, y1
            };
            // variables to increment x and y screen coordinates
            float x_inc = (S[2] - S[0]) / image.Width;
            float y_inc = (S[3] - S[1]) / image.Height;
            // Parallel.For loop takes in a min, max, and a delegate/lambda function to execute
            Parallel.For(0, image.Width, x => { 
                    float x_s = S[0] + (x_inc * x);
                    for(int y = 0; y < image.Height; y++) {
                        float y_s = S[1] + (y_inc * y);
                        Rgba32 color;
                        float rT = 0, gT = 0, bT = 0;
                        for(int s = 0; s < samples; s++){
                            color = camera.CastRay(world, x_s, y_s);
                            rT += (float)color.R / 255f;
                            gT += (float)color.G / 255f;
                            bT += (float)color.B / 255f;
                        }
                        float rF = rT / samples, gF = gT / samples, bF = bT / samples;
                        Vector fColorVec = Vector.Build.DenseOfArray(new float[] {rF, gF, bF});
                        var fColor = fColorVec.ToColor(); //new Rgba32(rF, gF, bF, 1.0f);
                        image[x, y] = fColor;
                    }
                }
            );
            if(fileName != "") {
                image.Save(fileName);
            }
            return image;
        }

        public void RenderAnimation(string filename="out", int frames = 24, float length = 1.0f) {
            // init progress bar
            char[] pstyles = new char[] {'>', '|'};
            string message = "Rendering " + frames + " " + world.width + "x" + world.height + " frames...";
            var rpb = new ProgressBar(1, frames, 55, pstyles, message, "Frame");

            LightSource l1 = world.lights[0];

            // set up frame and animation step intervals
            var interval = (int)((length / (float)frames) * 100);
            Vector cp = Vector.Build.DenseOfVector(camera.position);
            Vector cl = Vector.Build.DenseOfVector(camera.lookAt);
            Vector lp = Vector.Build.DenseOfVector(l1.position);
            Vector tr1 = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f, -0.03f });
            Vector tr2 = Vector.Build.DenseOfArray(new float[] { 0.0f,  0.0f,  0.03f });

            Animator a = new Animator(cp, tr1, Animator.add);
            Animator b = new Animator(cl, tr2, Animator.add);
            Animator c = new Animator(lp, tr2, Animator.add);

            var frameWatch = System.Diagnostics.Stopwatch.StartNew();
            var masterImage = Render(samples:2);
            frameWatch.Stop();
            var time = frameWatch.Elapsed;
            var est = time.Multiply(frames - 1);
            rpb.PrintProgressEstTime(1, est);
            float ascale = length/frames;
            for(var f = 1; f <= frames; f++) {
                frameWatch = System.Diagnostics.Stopwatch.StartNew();
                var imageTmp = Render(samples:1);
                imageTmp.Frames[0].MetaData.FrameDelay = interval;
                a.Animate(f * ascale);
                b.Animate(f * ascale);

                camera.position = a.target;
                camera.lookAt = b.target;
                // l1.position = c.target;
                // add current image frame to master image frames
                masterImage.Frames.AddFrame(imageTmp.Frames[0]);
                frameWatch.Stop();
                time = frameWatch.Elapsed;
                est = time.Multiply(frames - (f));
                rpb.PrintProgressEstTime(f, est);
            }
            bool saved = false;
            int attempts = 0;
            while(!saved) {
                saved = SaveGif(masterImage, filename, attempts++);
            }
        }
        public bool SaveGif(Image<Rgba32> image, string filename, int attempts) {
            try {
                string fname = filename;
                if(attempts != 0)
                    fname += "(" + attempts + ")";
                var outputStream = File.Open(fname+".gif", FileMode.OpenOrCreate);
                var gifEncoder = new SixLabors.ImageSharp.Formats.Gif.GifEncoder();
                image.Save(outputStream, gifEncoder);
                outputStream.Close();
                return true;
            } catch (IOException e) {
                System.Console.WriteLine("Image saving failed.");
                System.Console.WriteLine(e);
                return false;
            }
        }
    }
}