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
            world = SceneFactory.GetDefaultWorld(width, height);
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
                ToneReproduction(ref image, 1000.0f, true);
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

            // BlackHole bh = null;
            // foreach (Shape3D obj in world.objects)
            // {
            //     if (obj is BlackHole)
            //     {
            //         bh = obj as BlackHole;
            //     }
            // }
            var interval = (int)((length / (float)frames) * 100);
            var lookStep = 40f / frames;
            var srStep = 0.3f / frames;
            var frameWatch = System.Diagnostics.Stopwatch.StartNew();
            var masterImage = Render(samples: 1);
            masterImage.Frames[0].MetaData.FrameDelay = interval;
            frameWatch.Stop();
            var time = frameWatch.Elapsed;
            var est = time.Multiply(frames - 1);
            rpb.PrintProgressEstTime(1, est);
            var ldm = 0.1f;
            var ldm_max = 10f;
            var step = ldm_max - ldm / frames;
            for (var f = 1; f <= frames; f++)
            {
                // if (f < frames / 4)
                // {
                //     bh.center[0] -= lookStep;
                //     world.cameras[0].lookAt[0] -= lookStep / 3.5f;
                // }
                // else if (f < ((frames / 4) * 3))
                // {
                //     bh.center[0] += lookStep;
                //     world.cameras[0].lookAt[0] += lookStep / 3.5f;
                // }
                // else
                // {
                //     bh.center[0] -= lookStep;
                //     world.cameras[0].lookAt[0] -= lookStep / 3.5f;
                // }
                frameWatch = System.Diagnostics.Stopwatch.StartNew();
                var imageTmp = Render(samples: 1);
                ToneReproduction(ref imageTmp, ldm, true);
                if (f <= frames / 3)
                {
                    ldm += step;
                }
                else if (f <= ((frames / 3) * 2))
                {
                    ldm -= step;
                }
                else
                {
                    ldm += step;
                }
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
            }
            bool saved = false;
            int attempts = 0;
            while (!saved)
            {
                saved = SaveGif(masterImage, filename, attempts++);
            }
        }

        public void ToneReproduction(ref Image<Rgba32> image, float LdMax, bool rein)
        {
            var absColors = new Vector[image.Width, image.Height];
            var sum = 0f;
            var c = image.Width * image.Height;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    var cVec = image[x, y].ToVector();
                    sum += (float)Math.Abs(Math.Log(Math.Abs(cVec.Length()) + 0.001));
                    absColors[x, y] = cVec;
                }
            }
            var log_avg = sum / c;
            if (rein)
            {
                var a = 0.18f;
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        var cVec = image[x, y].ToVector();
                        var sVec = a / log_avg * cVec;
                        var rVec = sVec / (1 + sVec);
                        var target = rVec * LdMax;
                        // System.Console.WriteLine(target);
                        image[x, y] = target.ToColor();
                    }
                }
            }
            else
            {
                var la_pow = (float)Math.Pow(log_avg, 0.4);
                var ld_pow = (float)Math.Pow(LdMax / 2, 0.4);
                var scale = (float)Math.Pow((1.219f + ld_pow) / (1.219 + la_pow), 2.5);
                System.Console.WriteLine(LdMax);
                System.Console.WriteLine(log_avg);
                System.Console.WriteLine(ld_pow);
                System.Console.WriteLine(scale);
                for (int x = 0; x < image.Width; x++)
                {
                    for (int y = 0; y < image.Height; y++)
                    {
                        var target = absColors[x, y] * scale;
                        image[x, y] = target.ToColor();
                    }
                }
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