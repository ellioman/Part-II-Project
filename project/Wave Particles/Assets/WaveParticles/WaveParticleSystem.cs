
using System;
using UnityEngine;

public class WaveParticleSystem
{
    public enum SplatImplementation
    {
        GPU, CPU, CPU_DIRECT
    }
    public const SplatImplementation SPLAT_GPU = SplatImplementation.GPU;
    public const SplatImplementation SPLAT_CPU = SplatImplementation.CPU;
    public const SplatImplementation SPLAT_CPU_DIRECT = SplatImplementation.CPU_DIRECT;

    public enum ConvolutionImplementation
    {
        GPU_2D, CPU_2D, GPU_1D, CPU_1D
    }
    public const ConvolutionImplementation CONV_GPU_2D = ConvolutionImplementation.GPU_2D;
    public const ConvolutionImplementation CONV_CPU_2D = ConvolutionImplementation.CPU_2D;
    public const ConvolutionImplementation CONV_GPU_1D = ConvolutionImplementation.GPU_1D;
    public const ConvolutionImplementation CONV_CPU_1D = ConvolutionImplementation.CPU_1D;

    private float _particleSpeed;
    private float _particleRadius;
    private int _numParticles;

    private float _waveParticleKillThreshold;

    private int _currentFrame;
    private readonly int _frameCycleLength;

    private SplatImplementation _splatImplementationChoice = SPLAT_GPU;
    private ConvolutionImplementation _convolutionImplementationChoice = CONV_GPU_2D;

    private ParticleContainer _particleContainer;
    // TODO: Think of a new, better name for this class
    private HeightFieldGenerator _heightFieldGenerator;

    // TODO: This needs to be refactored, as we may want to have a list of these!
    // (For different height field locations etc.)
    private ExtendedHeightField _extendedHeightField;

    public WaveParticleSystem(float particleSpeed, float particleRadius, int maxNumParticles, int horRes, int vertRes, float height, float width, float waveParticleKillThreshold)
    {
        _particleSpeed = particleSpeed;
        _particleRadius = particleRadius;
        _numParticles = maxNumParticles;
        _waveParticleKillThreshold = waveParticleKillThreshold;
        _currentFrame = 0;
        // TODO: move all relvant code to do with this to here!
        _frameCycleLength = WaveParticle.FRAME_CYCLE_LENGTH;

        _extendedHeightField = new ExtendedHeightField(width, height, horRes, vertRes);
        _extendedHeightField.Clear();

        _particleContainer = SplatEnumToInstance(_splatImplementationChoice);
        _particleContainer.Initialise(maxNumParticles, waveParticleKillThreshold);
        _heightFieldGenerator = ConvEnumToInstance(_convolutionImplementationChoice);
        _heightFieldGenerator.Initialise(_extendedHeightField.heightFieldInfo, _particleContainer);
    }

    private static ParticleContainer SplatEnumToInstance(SplatImplementation implementation)
    {
        ParticleContainer particleContainer;
        switch (implementation)
        {
            case SPLAT_GPU:
                {
                    particleContainer = new GPUParticleContainer();
                }
                break;
            case SPLAT_CPU:
                {
                    particleContainer = new CPUParticleContainer();
                }
                break;
            case SPLAT_CPU_DIRECT:
                {
                    throw new NotImplementedException();
                }
            default:
                {
                    throw new NotImplementedException();
                }
        }
        return particleContainer;
    }

    public void SetSplatImplementation(SplatImplementation implementation)
    {
        if (implementation == _splatImplementationChoice)
        {
            return;
        }
        _particleContainer = SplatEnumToInstance(_splatImplementationChoice);
        _particleContainer.Initialise(_numParticles, _waveParticleKillThreshold);
    }

    private static HeightFieldGenerator ConvEnumToInstance(ConvolutionImplementation implementation)
    {
        HeightFieldGenerator heightFieldGenerator;
        switch (implementation)
        {
            case CONV_GPU_1D:
                {
                    throw new NotImplementedException();
                }
            case CONV_GPU_2D:
                {
                    heightFieldGenerator = new Convolution2DFastHeightFieldGenerator();
                }
                break;
            case CONV_CPU_1D:
                {
                    throw new NotImplementedException();
                }
            case CONV_CPU_2D:
                {
                    heightFieldGenerator = new GPUConvolution2DFastHeightFieldGenerator();
                }
                break;
            default:
                {
                    throw new NotImplementedException();
                }
        }
        return heightFieldGenerator;
    }

    public void SetConvolutionImplementation(ConvolutionImplementation implementation)
    {
        if (implementation == _convolutionImplementationChoice)
        {
            return;
        }
        _heightFieldGenerator = ConvEnumToInstance(_convolutionImplementationChoice);
        _heightFieldGenerator.Initialise(_extendedHeightField.heightFieldInfo, _particleContainer);
    }

    public Texture2D getHeigthMapTexture(Vector2 textureCentrePosition)
    {
        // TODO: take relative texture position into account
        _heightFieldGenerator.GenerateHeightField(_currentFrame, _extendedHeightField);
        _currentFrame = (_currentFrame + 1) % _frameCycleLength;
        return _extendedHeightField.textureHeightMap;
    }
}