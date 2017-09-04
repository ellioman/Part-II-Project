using UnityEngine;
using System.Collections;

/// <summary>
/// This interface is the interface to be used for classes which can generate a
/// heightfield from a set of wave particles.
/// </summary>
public abstract class HeightFieldGenerator
{
    public abstract void Initialise(ExtendedHeightField.HeightFieldInfo heightFieldInfo, ParticleContainer waveParticles);

    public abstract void GenerateHeightField(int currentFrame, ExtendedHeightField extendedHeightField);

    public abstract Texture[] getTextures();

    public abstract string getName();

    private StringBoolPair[] mEnabledDisplayTextures = null;

    public StringBoolPair[] getTexturesEnabled()
    {
        if (mEnabledDisplayTextures == null)
        {
            Texture[] textures = getTextures();
            mEnabledDisplayTextures = new StringBoolPair[textures.Length];
            for (int i = 0; i < textures.Length; i++)
            {
                mEnabledDisplayTextures[i] = new StringBoolPair(textures[i].name, false);
            }
        }

        return mEnabledDisplayTextures;
    }
}
