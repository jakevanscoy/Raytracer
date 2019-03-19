using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
 

namespace Raytracing {
    
    using Vector = Vector<float>;

    public class LightSource {
        public Rgba32 color { get; private set; }
        public Vector position { get; set; }

        public float strength { get; private set; }
        public LightSource(Vector pos) {
            position = pos;
            color = Rgba32.White;
            strength = 1.0f;
        }
        public LightSource(Vector pos, Rgba32 col) {
            position = pos;
            color = col;
            strength = 1.0f;
        }
        public LightSource(Vector pos, Rgba32 col, float str) {
            position = pos;
            color = col;
            strength = str;
        }
    }

    public class Ray {
        public Vector origin {get; set;}
        public Vector direction {get; set;}
        public Ray(Vector O, Vector D) {
            origin = O;
            direction = D;
        }

        public Ray Reverse() {
            return new Ray(this.origin, -this.direction);
        }
    
    }


    public class PhongIlluminationModel {

        public World world { get; private set; }
        public Camera camera { get; private set; }
        public PhongIlluminationModel(World w) {
            world = w;
            camera = w.cameras[0];
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
        public Rgba32 Illuminate(Ray ray, Vector intersectionPoint, Vector normal, PhongMaterial material, Shape3D obj) {
            // initialize Light (zeros)
            Vector L = Vector.Build.Dense(3);
            // add ambient lighting             
            Vector La = world.ambientLight.ToVector();
            float ka = world.ambientCoefficient;
            L += (La * ka);



            // add up diffuse/specular lighting for each light
            Vector Ld = Vector.Build.Dense(3);
            Vector Ls = Vector.Build.Dense(3);

            // object/material diffuse/specular colors
            Vector Od = material.diffuseColor.ToVector();
            Vector Os = material.specularColor.ToVector();

            // viewing direction
            Vector V = -ray.direction.Normalize();
            // shift intersection to avoid self-collision
            Vector lIntersection = intersectionPoint + (normal * 0.0001f);
            foreach(LightSource Li in world.GetLightSources()) {
                // shadow ray
                Vector Sdir = (Li.position - lIntersection).Normalize();
                Ray S = new Ray(lIntersection, Sdir);
                // reflected ray
                Vector Rdir = Extensions.Reflected(Sdir, normal).Normalize();
                Ray R = new Ray(lIntersection, Rdir);
                // check for shadow ray -> other object intersection
                bool shaded = false;
                foreach(Shape3D o_obj in world.objects) {
                    if(!o_obj.Equals(obj))
                        if(o_obj.Intersect(S, out var I, out var N)) 
                            shaded = true;
                }

                if(!shaded) {
                    // diffuse
                    Vector LiOd = Li.color.ToVector().Multiply(Od).Clamp(0.0f, 1.0f);
                    float dist = (float)Distance.Euclidean(Li.position, lIntersection);

                    float attenuation = Li.strength/(dist*dist);
                    // System.Console.WriteLine(attenuation);
                    LiOd *= attenuation;
                    float SdotN = Sdir.DotProduct(normal).Clamp(0.0f, 1.0f);
                    Ld += (SdotN * LiOd);
                    // specular
                    Vector LiOs = Li.color.ToVector().Multiply(Os).Clamp(0.0f, 1.0f);
                    LiOs *= attenuation;
                    float RdotV = ((float)Math.Pow(Rdir.DotProduct(V), material.specularExponent)).Clamp(0.0f, 1.0f);
                    Ls += (RdotV * LiOs);
                }
            }
            Ld *= material.kDiffuse;
            Ls *= material.kSpecular;
            L += Ld;
            L += Ls;
            return L.Clamp(0.0f, 1.0f).ToColor();
        }
    }



}