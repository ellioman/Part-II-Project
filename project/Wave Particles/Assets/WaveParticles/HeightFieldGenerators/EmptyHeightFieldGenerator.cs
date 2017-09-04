using UnityEngine;
using System.Collections;
using System;

public class EmptyHeightFieldGenerator : HeightFieldGenerator {
    public override void GenerateHeightField(int currentFrame, ExtendedHeightField extendedHeightField)
    {
        return;
    }

    public override string getName()
    {
        return "Empty Height Field Generator";
    }

    public override Texture[] getTextures()
    {
        return new Texture[0];
    }

    public override void Initialise(ExtendedHeightField.HeightFieldInfo heightFieldInfo, ParticleContainer waveParticles)
    {
        return;
    }
}
