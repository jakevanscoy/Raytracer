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
            Vector rayO = I; // (I + N * 0.0001f).Normalize();
            Vector rayD = (S.direction - (2 * S.direction.DotProduct(N) * N)).Normalize();
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
        // L = ka * La                <--- ambient
        //   + kd * Ei(Li * (Si . N)) <--- diffuse 
        //   + ks * (Ei(Ri . V) ^ ke) <--- specular
        //
        // L - Final returned radiance (color value)
        // ka - ambient coefficient
        // La - world ambient radiance 
        // kd - diffuse coefficient
        // Ei - summation of all the light sources (i)
        // Li - radiance of light i
        // Si - angle of incidence of light i
        // Ri - angle of reflectance of light i
        // V  - viewing angle 
        //////
        public Rgba32 Illuminate(Vector intersectionPoint, Vector normal, PhongMaterial material) {
            // initial Light (zeros)
            Vector L = Vector.Build.DenseOfArray(new float[]{0.0f, 0.0f, 0.0f});
            // add ambient lighting             
            Vector La = world.ambientLight.ToVector();
            float ka = world.ambientCoefficient;
            L += (La * ka);

            // add up diffuse/specular lighting for each light
            Vector Ld = Vector.Build.DenseOfArray(new float[]{0.0f, 0.0f, 0.0f});
            Vector Ls = Vector.Build.DenseOfArray(new float[]{0.0f, 0.0f, 0.0f});

            // object/material diffuse/specular colors
            Vector Od = material.diffuseColor.ToVector();
            Vector Os = material.specularColor.ToVector();

            foreach(LightSource Li in world.GetLightSources()) {
                // shadow ray
                Vector Sdir = (Li.position - intersectionPoint).Normalize();
                // Vector Sdir = (intersectionPoint - Li.position).Normalize();
                Ray S = new Ray(intersectionPoint, Sdir);
                // viewing direction
                Vector V = (intersectionPoint - camera.center).Normalize();
                // Vector V = (intersectionPoint - camera.center).Normalize();
                // reflected ray
                Ray R = Ray.Reflect(S, normal, intersectionPoint);
    
                bool shaded = false;
                foreach(Object3D o in world.objects) {
                    shaded = shaded? true : o.Intersect(S, out var I, out var N);
                }
                if(!shaded) {
                    // diffuse
                    Vector LiOd = Li.color.ToVector().Multiply(Od);
                    float SdotN = Math.Max(S.direction.DotProduct(normal), 0.0f);
                    Ld += (SdotN * LiOd);
                    // specular
                    Vector LiOs = Li.color.ToVector().Multiply(Os);
                    float RdotV = (float)Math.Pow(Math.Max(R.direction.DotProduct(V), 0.0f), material.specularExponent);
                    Ls += (RdotV * LiOs);
                }
            }
            Ld *= material.kDiffuse;
            Ls *= material.kSpecular;
            L += Ld;
            L += Ls;

            return L.ToColor();
        }
    }



}