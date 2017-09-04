using UnityEngine;
using System.Collections;

public static class HeightFieldGeneratorSelector
{
    public enum Choice
    {
        GPU_CONVOLUTION_2D,
        CPU_CONVOLUTION_2D,
        PRECISE,
        NONE
    }

    public static HeightFieldGenerator CreateAndInitialise(Choice hfge, ExtendedHeightField.HeightFieldInfo heightFieldInfo, ParticleContainer waveParticles)
    {
        HeightFieldGenerator heightFieldGenerator;
        switch (hfge)
        {
            case Choice.GPU_CONVOLUTION_2D:
                {
                    heightFieldGenerator = new GPUConvolution2DFastHeightFieldGenerator();
                }
                break;
            case Choice.CPU_CONVOLUTION_2D:
                {
                    heightFieldGenerator = new Convolution2DFastHeightFieldGenerator();
                }
                break;
            case Choice.PRECISE:
                {
                    heightFieldGenerator = new PreciseHeightFieldGenerator();
                }
                break;
            case Choice.NONE:
                {
                    heightFieldGenerator = new EmptyHeightFieldGenerator();
                }
                break;
            default:
                {
                    // TODO: throw an appropriate exception!
                    throw new System.Exception();
                }
        }
        heightFieldGenerator.Initialise(heightFieldInfo, waveParticles);
        return heightFieldGenerator;
    }
}
