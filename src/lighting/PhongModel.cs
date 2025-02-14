using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;


namespace Raytracing
{

    using Vector = Vector<float>;

    public class Ray
    {
        public Vector origin { get; set; }
        public Vector direction { get; set; }
        public Vector i_dir { get; set; }
        public int[] sign { get; set; }
        public Ray(Vector O, Vector D)
        {
            origin = O;
            direction = D;
            i_dir = 1.0f / direction;
            sign = new int[] {
                i_dir[0] < 0 ? 1 : 0,
                i_dir[1] < 0 ? 1 : 0,
                i_dir[2] < 0 ? 1 : 0,
            };
        }

        public Ray Reverse()
        {
            return new Ray(this.origin, -this.direction);
        }

    }

    public class PhongIlluminationModel
    {

        public World world { get; private set; }
        public PhongIlluminationModel(World w)
        {
            world = w;
        }

        //////
        // Phong Illumination Model: 
        //
        // L = ka * La                     <--- ambient
        //   + kd * [Li * Od  * (Si . N)] <--- diffuse 
        //   + ks * [Li * Os  * (Ri . V) ^ ke)] <--- specular
        //
        // L - Final returned radiance (color value)
        // ka - ambient coefficient
        // La - world ambient radiance 
        // kd - diffuse coefficient
        // ke - specular exponent
        // [] - summation of all the light sources (i)
        // Li - radiance of light i
        // Si - angle of incidence of light i
        // Ri - angle of reflectance of light i
        // V  - viewing angle 
        //////
        public Rgba32 Illuminate(Ray ray, Vector isect, Vector normal, PhongMaterial material, Shape3D obj, bool KD = false)
        {
            // initialize Light (zeros)
            Vector L = Vector.Build.Dense(3);
            // add ambient lighting             
            Vector La = world.ambientLight.ToVector();
            float ka = world.ambientCoefficient;
            L += (La * ka);
            // add up diffuse/specular/reflection lighting for each light
            Vector Ld = Vector.Build.Dense(3);
            Vector Ls = Vector.Build.Dense(3);
            Vector Lr = Vector.Build.Dense(3);

            // object/material diffuse/specular colors
            Vector Od = material.diffuseColor.ToVector();
            Vector Os = material.specularColor.ToVector();

            // viewing direction
            Vector V = -ray.direction.Normalize();
            // shift intersection to avoid self-collision
            Vector lIntersection = isect + (normal * 0.001f);

            foreach (LightSource Li in world.GetLightSources())
            {
                // shadow ray
                Vector Sdir = (Li.center - lIntersection).Normalize();
                float l_d = Math.Abs((Li.center - lIntersection).Length());
                Ray S = new Ray(lIntersection, Sdir);
                // reflected ray
                Vector Rdir = Extensions.Reflected(Sdir, normal).Normalize();
                // Ray R = new Ray(lIntersection, Rdir);
                // check for shadow ray -> other object intersection
                bool shaded = false;
                float shade = 0.0f;
                if (KD)
                {
                    var s_shape = world.TraceRayKD(S, out var i, out var n);
                    if (s_shape != null)
                    {
                        var s_d = Math.Abs((isect - i[0]).Length());
                        if (s_d < l_d)
                        {
                            shaded = true;
                            shade = 1.0f - Extensions.Clamp(s_shape.material.kTransmission, 0.0f, 1.0f);
                        }

                    }
                }
                else
                {
                    foreach (Shape3D o_obj in world.objects)
                    {
                        if (!o_obj.Equals(obj))
                            if (o_obj.Intersect(S, out var I, out var N))
                            {
                                shaded = true;
                                shade = 1.0f - Extensions.Clamp(o_obj.material.kTransmission, 0.0f, 1.0f);
                            }
                    }
                }
                if (!shaded || shade < 1.0f)
                {
                    // diffuse
                    Vector LiOd = Li.color.ToVector().Multiply(Od).Clamp(0.0f, 1.0f);
                    float dist = (float)Distance.Euclidean(Li.center, lIntersection);
                    float attenuation = Li.strength / (dist * dist);
                    // System.Console.WriteLine(attenuation);
                    LiOd *= attenuation;
                    float SdotN = Sdir.DotProduct(normal).Clamp(0.0f, 1.0f);
                    Ld += (SdotN * LiOd);
                    Ld -= shade;
                    Ld.Clamp(0.0f, 1.0f);
                    // specular
                    Vector LiOs = Li.color.ToVector().Multiply(Os).Clamp(0.0f, 1.0f);
                    LiOs *= attenuation;
                    float RdotV = ((float)Math.Pow(Rdir.DotProduct(V), material.specularExponent)).Clamp(0.0f, 1.0f);
                    Ls += (RdotV * LiOs);
                    Ls -= shade;
                    Ls.Clamp(0.0f, 1.0f);
                }
            }
            Ld *= material.kDiffuse;
            Ls *= material.kSpecular;
            L += (Ld + Ls);
            return L.Clamp(0.0f, 1.0f).ToColor();
        }
    }



}