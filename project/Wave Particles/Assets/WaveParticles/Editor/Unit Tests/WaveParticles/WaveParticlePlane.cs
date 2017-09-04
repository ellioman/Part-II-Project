using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class WaveParticlesTest
{

    /// <summary>
    /// Test that the wave particle visualisers are working as expected.
    /// </summary>
    [Test]
    public void VisualiserTest()
    {
        //Create new game object and attach an instance of the WaveParticles class
        var gameObject = new GameObject();
        WaveParticlePlane waveParticlesPlane = gameObject.AddComponent<WaveParticlePlane>();
        waveParticlesPlane.waveParticleKillThreshold = 0f;
    }
}
