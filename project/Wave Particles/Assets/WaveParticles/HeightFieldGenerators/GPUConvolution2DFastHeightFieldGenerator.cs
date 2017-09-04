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
public class GPUConvolution2DFastHeightFieldGenerator : HeightFieldGenerator
{

    private ExtendedHeightField.HeightFieldInfo heightFieldInfo;
    private ExtendedHeightField pointMap;
    private ParticleContainer waveParticles;

    private Texture2D kernel;

    private Texture2D convolvedTexture;
    private RenderTexture shaderTexture;
    private Material convolutionMaterial;

    override public void Initialise(ExtendedHeightField.HeightFieldInfo hf, ParticleContainer wp)
    {
        heightFieldInfo = hf;
        pointMap = new ExtendedHeightField(hf.Width, hf.Height, hf.HoriRes, hf.VertRes);

        convolvedTexture = new Texture2D(heightFieldInfo.HoriRes, heightFieldInfo.VertRes, TextureFormat.RGBAFloat, false);
        convolvedTexture.anisoLevel = 1;
        convolvedTexture.filterMode = FilterMode.Point;
        convolvedTexture.wrapMode = TextureWrapMode.Clamp;
        convolvedTexture.name = "Convolved Texture";

        shaderTexture = new RenderTexture(heightFieldInfo.HoriRes, heightFieldInfo.VertRes, 24, RenderTextureFormat.ARGBFloat);
        shaderTexture.antiAliasing = 1;
        shaderTexture.anisoLevel = 0;
        shaderTexture.autoGenerateMips = false;
        shaderTexture.wrapMode = TextureWrapMode.Clamp;
        shaderTexture.filterMode = FilterMode.Point;


        int kernelWidth = Mathf.CeilToInt((WaveParticle.RADIUS / heightFieldInfo.Width) * heightFieldInfo.HoriRes);
        int  kernelHeight = Mathf.CeilToInt((WaveParticle.RADIUS / heightFieldInfo.Height) * heightFieldInfo.VertRes);
        Color[] kernelArray = Convolution2DFastHeightFieldGenerator.creatKernel(kernelHeight, kernelWidth, heightFieldInfo);

        kernel = new Texture2D(kernelWidth, kernelHeight, TextureFormat.RGBAFloat, false);
        kernel.SetPixels(kernelArray);
        kernel.Apply();

        convolutionMaterial = new Material(Shader.Find("Unlit/2DFunction"));
        convolutionMaterial.SetTexture(Shader.PropertyToID("_KernelTex"), kernel);
        convolutionMaterial.SetFloat(Shader.PropertyToID("_Width"), heightFieldInfo.Width);
        convolutionMaterial.SetFloat(Shader.PropertyToID("_Height"), heightFieldInfo.Height);
        convolutionMaterial.SetInt(Shader.PropertyToID("_HoriRes"), heightFieldInfo.HoriRes);
        convolutionMaterial.SetInt(Shader.PropertyToID("_VertRes"), heightFieldInfo.VertRes);
        convolutionMaterial.SetFloat(Shader.PropertyToID("_ParticleRadii"), WaveParticle.RADIUS);
        convolutionMaterial.SetFloat(Shader.PropertyToID("_KernelWidth"), kernelWidth);
        convolutionMaterial.SetFloat(Shader.PropertyToID("_KernelHeight"), kernelHeight);

        waveParticles = wp;
    }

    override public Texture[] getTextures()
    {
        Texture[] result = new Texture[] {
            convolvedTexture, pointMap.textureHeightMap
        };
        return result;
    }

    private void convolveWaveParticles()
    {
        if (!shaderTexture.IsCreated())
        {
            shaderTexture.Create();
        }

        // Set the texture to the active one so that it's values can be read back out to the pointMapTexture
        RenderTexture.active = shaderTexture;

        GL.Clear(true, true, Color.black);

        Graphics.Blit(pointMap.textureHeightMap, shaderTexture, convolutionMaterial);

        // Read back values from the render texture
        convolvedTexture.ReadPixels(new Rect(0, 0, convolvedTexture.width, convolvedTexture.height), 0, 0, false);
        convolvedTexture.Apply();

        RenderTexture.active = null;
        shaderTexture.Release();
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
        convolveWaveParticles();
        extendedHeightField.UpdateTexture(convolvedTexture);
        Profiler.EndSample();
    }

    override public string getName()
    {
        return "GPU 2D Convoluton Heightfield Generator";
    }
}
