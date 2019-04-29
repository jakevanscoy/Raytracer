using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace Raytracing
{

    using Vector = Vector<float>;
    public class Raytracer
    {
        public Camera camera { get; set; }
        public World world { get; set; }

        private static Random random = new Random();

        // default constructor
        public Raytracer(int width = 800, int height = 800)
        {
            // initialize default world and Object3Ds
            world = SceneFactory.GetManyBallWorld(width, height);
            System.Console.WriteLine("Building k-d tree...");
            world.MakeTree();
            System.Console.WriteLine(KDTree.PrintNode(world.tree));
            System.Console.WriteLine("Objects not in k-d tree: " + KDTree.nObj);
            camera = world.cameras[0];
        }

        public Image<Rgba32> Render(string fileName = "", int samples = 1)
        {
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
            Parallel.For(0, image.Width, x =>
            {
                float x_s = S[0] + (x_inc * x);
                for (int y = 0; y < image.Height; y++)
                {
                    float y_s = S[1] + (y_inc * y);
                    Rgba32 color;
                    float rT = 0, gT = 0, bT = 0;
                    for (int s = 0; s < samples; s++)
                    {
                        float rx_s = x_s + (float)((random.NextDouble() * (x_inc / 2)) - x_inc / 4);
                        float ry_s = y_s + (float)((random.NextDouble() * (y_inc / 2)) - y_inc / 4);
                        color = camera.CastRay(world, rx_s, ry_s);
                        // System.Console.WriteLine(rx_s + " " + ry_s);
                        rT += (float)color.R / 255f;
                        gT += (float)color.G / 255f;
                        bT += (float)color.B / 255f;
                    }
                    float rF = rT / samples, gF = gT / samples, bF = bT / samples;
                    Vector fColorVec = Vector.Build.DenseOfArray(new float[] { rF, gF, bF });
                    var fColor = fColorVec.ToColor(); //new Rgba32(rF, gF, bF, 1.0f);
                    image[x, y] = fColor;
                }
            }
            );
            if (fileName != "")
            {
                image.Save(fileName);
            }
            return image;
        }

        public Image<Rgba32> RenderWithProgress(string fileName = "", int samples = 1)
        {
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
            var frameWatch = System.Diagnostics.Stopwatch.StartNew();
            char[] pstyles = new char[] { '>', '|' };
            string message = "Rendering a single frame...";
            var rpb = new ProgressBar(0, world.width * world.height, 55, pstyles, message, "Pixels");
            rpb.PrintProgressEstTime(1, frameWatch.Elapsed);
            // Parallel.For loop takes in a min, max, and a delegate/lambda function to execute
            int pc = 0;
            Parallel.For(0, image.Width, x =>
            {
                float x_s = S[0] + (x_inc * x);
                for (int y = 0; y < image.Height; y++)
                {
                    float y_s = S[1] + (y_inc * y);
                    Rgba32 color;
                    float rT = 0, gT = 0, bT = 0;
                    for (int s = 0; s < samples; s++)
                    {
                        float rx_s = x_s + (float)((random.NextDouble() * (x_inc / 2)) - x_inc / 4);
                        float ry_s = y_s + (float)((random.NextDouble() * (y_inc / 2)) - y_inc / 4);
                        color = camera.CastRay(world, rx_s, ry_s);
                        rT += (float)color.R / 255f;
                        gT += (float)color.G / 255f;
                        bT += (float)color.B / 255f;
                    }
                    float rF = rT / samples, gF = gT / samples, bF = bT / samples;
                    Vector fColorVec = Vector.Build.DenseOfArray(new float[] { rF, gF, bF });
                    var fColor = fColorVec.ToColor(); //new Rgba32(rF, gF, bF, 1.0f);
                    image[x, y] = fColor;
                }
                lock (this)
                {
                    pc += image.Height;
                    rpb.PrintProgressBarNoEst(pc);
                }
            }
            );
            if (fileName != "")
            {
                image.Save(fileName);
            }
            return image;
        }

        public void RenderAnimation(string filename = "out", int frames = 24, float length = 1.0f)
        {
            // init progress bar
            char[] pstyles = new char[] { '>', '|' };
            string message = "Rendering " + frames + " " + world.width + "x" + world.height + " frames...";
            var rpb = new ProgressBar(1, frames, 55, pstyles, message, "Frame");

            // LightSource l1 = world.lights[0];
            // Plane y_max = (Plane)world.objects[3];
            // set up frame and animation step intervals
            var interval = (int)((length / (float)frames) * 100);
            var frameWatch = System.Diagnostics.Stopwatch.StartNew();
            var rstep = 4.0f / frames;
            world.cameras[0].Translate(rstep, 0, 0);
            var masterImage = Render(samples: 1);
            masterImage.Frames[0].MetaData.FrameDelay = interval;
            frameWatch.Stop();
            var time = frameWatch.Elapsed;
            var est = time.Multiply(frames - 1);
            rpb.PrintProgressEstTime(1, est);
            for (var f = 1; f <= frames; f++)
            {

                if (f < frames / 4)
                {
                    world.cameras[0].Translate(rstep, 0, 0);
                }
                else if (f < 1 + (frames / 4) * 3)
                {
                    world.cameras[0].Translate(-rstep, 0, 0);
                }
                else
                {
                    world.cameras[0].Translate(rstep, 0, 0);
                }

                if (f < frames / 4)
                {
                    world.cameras[0].lookAt[2] -= rstep;
                }
                else if (f < (frames / 2))
                {
                    world.cameras[0].lookAt[2] += rstep;
                }
                else if (f < 1 + (frames / 4) * 3)
                {
                    world.cameras[0].lookAt[2] -= rstep;
                }
                else
                {
                    world.cameras[0].lookAt[2] += rstep;
                }

                frameWatch = System.Diagnostics.Stopwatch.StartNew();
                var imageTmp = Render(samples: 1);
                // imageTmp.Frames[0].MetaData.FrameDelay = interval;
                lock (this)
                {
                    // add current image frame to master image frames
                    masterImage.Frames.AddFrame(imageTmp.Frames[0]);
                    masterImage.Frames[f].MetaData.FrameDelay = interval;
                }
                frameWatch.Stop();
                time = frameWatch.Elapsed;
                est = time.Multiply(frames - (f));
                rpb.PrintProgressEstTime(f, est);
                // y_max.Translate(0.01f, 0.01f, -0.1f);

            }
            bool saved = false;
            int attempts = 0;
            while (!saved)
            {
                saved = SaveGif(masterImage, filename, attempts++);
            }
        }
        public bool SaveGif(Image<Rgba32> image, string filename, int attempts)
        {
            try
            {
                string fname = filename;
                if (attempts != 0)
                    fname += "(" + attempts + ")";
                var outputStream = File.Open(fname + ".gif", FileMode.OpenOrCreate);
                var gifEncoder = new SixLabors.ImageSharp.Formats.Gif.GifEncoder();
                image.Save(outputStream, gifEncoder);
                outputStream.Close();
                return true;
            }
            catch (IOException e)
            {
                System.Console.WriteLine("Image saving failed.");
                System.Console.WriteLine(e);
                return false;
            }
        }
    }
}