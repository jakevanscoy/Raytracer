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
        public Vector position { get; private set; }
        public LightSource(Vector pos) {
            position = pos;
            color = Rgba32.White;
        }
        public LightSource(Vector pos, Rgba32 col) {
            position = pos;
            color = col;
        }
    }

    public class Ray {
        public Vector origin {get; set;}
        public Vector direction {get; set;}
        public Ray(Vector O, Vector D) {
            origin = O;
            direction = D;
        }
    
        // Perfect reflection:
        // R = S + 2a
        // R: reflected ray
        // S: ray from intersection point (I) -> source 
        // a: -(S . N) / len(N)^2
        // N: normal vector of an object at intersection point I
        public static Ray Reflect(Ray S, Vector N, Vector I) {
            Vector rayO = (I + N * 0.00001f);
            // Vector rayD = 2 * (N.DotProduct(S.direction) * (N - S.direction)).Normalize();
            Vector rayD = ((2 * S.direction.DotProduct(N) * N) - S.direction).Normalize();
            Ray R = new Ray(rayO, rayD);
            return R;
        }
    }


    public class PhongIlluminationModel {
        public World world { get; private set; }

        public Camera camera { get; private set; }
        public PhongIlluminationModel(World w, Camera c) {
            world = w;
            camera = c;
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
        // [] - summation of all the light sources (i)
        // Li - radiance of light i
        // Si - angle of incidence of light i
        // Ri - angle of reflectance of light i
        // V  - viewing angle 
        //////
        public Rgba32 Illuminate(Ray ray, Vector intersectionPoint, Vector normal, PhongMaterial material) {
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
            // local normal
            Vector lNormal = (V.DotProduct(normal) < 0.0f)? normal : -normal;
            Vector lIntersection = intersectionPoint - (lNormal * 0.0001f);

            foreach(LightSource Li in world.GetLightSources()) {
                // shadow ray
                Vector Sdir = (Li.position - lIntersection).Normalize();
                Ray S = new Ray(lIntersection, Sdir);
                // reflected ray
                Vector Rdir = Extensions.Reflected(Sdir, lNormal).Normalize();
                Ray R = new Ray(lIntersection, Rdir);
                // check for shadow ray -> other object intersection
                bool shaded = false;
                foreach(Object3D obj in world.objects) {
                    if(obj.Intersect(S, out var I, out var N)) {
                        shaded = true;
                    }
                }

                if(!shaded) {
                    // diffuse
                    Vector LiOd = Li.color.ToVector().Multiply(Od).Clamp(0.0f, 1.0f);
                    float SdotN = Sdir.DotProduct(normal).Clamp(0.0f, 1.0f);
                    Ld += (SdotN * LiOd);
                    // specular
                    Vector LiOs = Li.color.ToVector().Multiply(Os).Clamp(0.0f, 1.0f);
                    float RdotV = ((float)Math.Pow(V.DotProduct(Rdir), material.specularExponent)).Clamp(0.0f, 1.0f);
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