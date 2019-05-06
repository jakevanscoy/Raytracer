using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;



namespace Raytracing
{

    using Vector = Vector<float>;

    public abstract class Material
    {
        public float kReflection = 0.0f;
        public float kTransmission = 0.0f;
        public abstract Rgba32 Intersect(Ray ray, Vector intersection, Vector normal, Shape3D obj, bool KD = false);
    }

    public class BasicMaterial : Material
    {
        public Rgba32 color { get; private set; }

        public BasicMaterial()
        {
            color = Rgba32.Black;
        }
        public BasicMaterial(Rgba32 c)
        {
            color = c;
        }
        public override Rgba32 Intersect(Ray ray, Vector intersection, Vector normal, Shape3D obj, bool KD = false)
        {
            return this.color;
        }
    }

    public class PhongMaterial : Material
    {

        public PhongIlluminationModel lightingModel { get; private set; }

        // material color variables 
        public Rgba32 diffuseColor { get; set; }
        public Rgba32 specularColor { get; set; }

        // color coefficients
        public float kDiffuse { get; set; }
        public float kSpecular { get; set; }

        // specular highlight exponent
        public float specularExponent { get; set; }

        // color - array of 3 Rgb32 objects that set diffuse and specular color variables
        // coefficients - array of 3 float values that set diffuse and specular coefficients
        // ke - specular exponent
        public PhongMaterial(PhongIlluminationModel model, Rgba32[] color, float[] coefficents, float ke)
        {
            lightingModel = model;
            diffuseColor = color[0];
            specularColor = color[1];
            kDiffuse = coefficents[0];
            kSpecular = coefficents[1];
            specularExponent = ke;
            kReflection = 0.0f;
        }

        public PhongMaterial(PhongIlluminationModel model)
        {
            lightingModel = model;
            diffuseColor = Rgba32.Black;
            specularColor = Rgba32.White;
            kDiffuse = 1.0f;
            kSpecular = 1.0f;
            specularExponent = 1.0f;
            kReflection = 0.0f;
        }

        public static PhongMaterial Red(PhongIlluminationModel model)
        {
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Red, Rgba32.White };
            float[] s0coefficients = new float[] { 1.0f, 1.0f };
            return new PhongMaterial(model, s0colors, s0coefficients, 7.0f);
        }


        public static PhongMaterial Blue(PhongIlluminationModel model)
        {
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Blue, Rgba32.White };
            float[] s0coefficients = new float[] { 1.0f, 1.0f };
            return new PhongMaterial(model, s0colors, s0coefficients, 7.0f);
        }

        public static PhongMaterial Green(PhongIlluminationModel model)
        {
            Rgba32[] s0colors = new Rgba32[] { Rgba32.Green, Rgba32.White };
            float[] s0coefficients = new float[] { 1.0f, 1.0f };
            return new PhongMaterial(model, s0colors, s0coefficients, 7.0f);
        }

        public override Rgba32 Intersect(Ray ray, Vector intersection, Vector normal, Shape3D obj, bool KD = false)
        {
            return lightingModel.Illuminate(ray, intersection, normal.Normalize(), this, obj, KD);
        }
    }


    public class Mirror : PhongMaterial
    {

        private Mirror(PhongIlluminationModel model) : base(model)
        {
            diffuseColor = Rgba32.Silver;
            specularColor = Rgba32.White;
            kDiffuse = 0.001f;
            kSpecular = 0.001f;
            specularExponent = 0.01f;
            kReflection = 1.0f;
        }
        public static Mirror GetMirror(PhongIlluminationModel model)
        {
            Mirror m = new Mirror(model);
            return m;
        }
    }

    public class TransmissiveMaterial : PhongMaterial
    {
        internal TransmissiveMaterial(PhongIlluminationModel model) : base(model)
        {
            diffuseColor = Rgba32.Silver;
            specularColor = Rgba32.White;
            kDiffuse = 0.001f;
            kSpecular = 0.001f;
            specularExponent = 0.01f;
            kReflection = 0.0f;
            kTransmission = 0.9f;
        }
        public static TransmissiveMaterial GetTransmissiveMaterial(PhongIlluminationModel model)
        {
            TransmissiveMaterial tm = new TransmissiveMaterial(model);
            return tm;
        }
    }

    public class CheckerboardMaterial : Material
    {
        public Material material1 { get; set; }
        public Material material2 { get; set; }
        public float checksize { get; set; }

        public CheckerboardMaterial(Material m1, Material m2, float c)
        {
            material1 = m1;
            material2 = m2;
            checksize = c;
            kReflection = (m1.kReflection + m2.kReflection) / 2;
        }

        public Material GetMaterial(Shape3D obj, Vector<float> intersection)
        {
            var tex = obj.GetTextureCoords(intersection);
            float row = tex[1] / checksize;
            float col = tex[0] / checksize;
            if ((int)row % 2 == (int)col % 2)
            {
                return material1;
            }
            else
            {
                return material2;
            }
        }

        public override Rgba32 Intersect(Ray ray, Vector<float> intersection, Vector<float> normal, Shape3D obj, bool KD = false)
        {
            return GetMaterial(obj, intersection).Intersect(ray, intersection, normal, obj, KD);
        }
    }

}