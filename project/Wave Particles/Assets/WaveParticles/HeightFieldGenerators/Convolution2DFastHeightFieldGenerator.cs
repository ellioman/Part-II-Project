using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Profiling;

/// <summary>
/// This class generates heightfields from waveparticles using a single pass of a 2D convolution with a
/// kernel that approximates both the longitudinal and lattitudinal components of surface distortions
/// caused by wave particles.
/// 
/// This is done by creating a kernel that is the of a wave particles (just greater than a wave particle's radius),
/// and having it contain the needed values from which the extended height field can be generated.
/// 
/// TODO: provide a much better explenation of what is happening here!
/// </summary>
public class Convolution2DFastHeightFieldGenerator : HeightFieldGenerator
{

    private ExtendedHeightField.HeightFieldInfo heightFieldInfo;
    private ExtendedHeightField pointMap;
    private ParticleContainer waveParticles;


    // Kernel definitions
    private int kernelWidth;
    private int kernelHeight;
    private Color[] kernel;

    public static Color[] creatKernel(int kernelHeight, int kernelWidth, ExtendedHeightField.HeightFieldInfo heightFieldInfo)
    {
        Color[] kernel = new Color[kernelWidth * kernelHeight];

        // Create the kernel that is used in convolution.
        for (int y = 0; y < kernelHeight; y++)
        {
            for (int x = 0; x < kernelWidth; x++)
            {
                int index = (y * kernelWidth) + x;
                float abs_diff;

                float x_component = ((kernelWidth / 2) - x) * heightFieldInfo.UnitX;
                float y_component = ((kernelHeight / 2) - y) * heightFieldInfo.UnitY;
                {
                    abs_diff = Mathf.Sqrt((y_component * y_component) + (x_component * x_component));
                }
                if (abs_diff > WaveParticle.RADIUS)
                {
                    // Don't need to do rest of calculation, as these pixel fall outside of wave particles's radii.
                    kernel[index] = new Vector4(0, 0, 0, 1);
                }
                else
                {
                    float relativePixelDistance = (Mathf.PI * abs_diff) / WaveParticle.RADIUS;
                    float y_displacement_factor = 0.5f * (Mathf.Cos(relativePixelDistance) + 1);
                    Vector2 long_component = -Mathf.Sqrt(2) * y_displacement_factor * Mathf.Sin(relativePixelDistance) * new Vector2(x_component, y_component);
                    kernel[index] = new Color(long_component.x, y_displacement_factor, long_component.y, 1);
                }
            }
        }
        return kernel;
    }

    override public void Initialise(ExtendedHeightField.HeightFieldInfo hf, ParticleContainer wp)
    {
        heightFieldInfo = hf;
        pointMap = new ExtendedHeightField(hf.Width, hf.Height, hf.HoriRes, hf.VertRes);
        pointMap.textureHeightMap.name = "Point Map Texture";
        waveParticles = wp;

        kernelWidth = Mathf.CeilToInt((WaveParticle.RADIUS / heightFieldInfo.Width) * heightFieldInfo.HoriRes);
        kernelHeight = Mathf.CeilToInt((WaveParticle.RADIUS / heightFieldInfo.Height) * heightFieldInfo.VertRes);
        kernel = creatKernel(kernelWidth, kernelHeight, heightFieldInfo);
    }

    override public Texture[] getTextures()
    {
        return new Texture[0];
    }


    override public void GenerateHeightField(int currentFrame, ExtendedHeightField extendedHeightField)
    {
        Profiler.BeginSample("Clear Height Field");
        extendedHeightField.Clear();
        Profiler.EndSample();

        Profiler.BeginSample("Set Point Map");
        waveParticles.setPointMap(currentFrame, ref pointMap);
        Profiler.EndSample();

        Profiler.BeginSample("Convolve Wave Particles");
        for (int row = 0; row < heightFieldInfo.VertRes; row++)
        {
            for (int col = 0; col < heightFieldInfo.HoriRes; col++)
            {
                int index = row * heightFieldInfo.HoriRes + col;
                for (int y = 0; y < kernelHeight; y++)
                {
                    for (int x = 0; x < kernelWidth; x++)
                    {
                        int y_index = (y - (kernelHeight / 2)) + row;
                        int x_index = (x - (kernelWidth / 2)) + col;
                        if (y_index > 0 && y_index < heightFieldInfo.VertRes && x_index > 0 && x_index < heightFieldInfo.HoriRes)
                        {
                            int fresh_index = (y_index * heightFieldInfo.HoriRes) + (x_index);
                            extendedHeightField.heightMap[index] += (Color) (pointMap.heightMap[fresh_index] * kernel[(y * kernelWidth) + x]);
                        }
                    }
                }
            }
        }
        extendedHeightField.ApplyCPUHeightMap();
        Profiler.EndSample();

    }

    override public string getName()
    {
        return "CPU 2D Convoluton Heightfield Generator";
    }
}
