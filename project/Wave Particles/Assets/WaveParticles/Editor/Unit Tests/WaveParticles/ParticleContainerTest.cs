using UnityEngine;
using UnityEditor;
using NUnit.Framework;

public class ParticleContainerTest
{

    [Test]
    public void TestParticleAdditions()
    {
        var pc = new CPUParticleContainer();
        pc.Initialise(30000, 0.0001f);
        pc.addParticle(WaveParticle.createWaveParticle(new Vector2(0, 0), new Vector2(1, 0), 1f, Mathf.PI / 2, 0));
        int count = 0;
        foreach (var wp in pc)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(1));
        //Assert.That(count, Is.EqualTo(pc.numActiveParticles));
    }

    [Test]
    public void TestParticleAdditionsComplex()
    {
        var pc = new CPUParticleContainer();
        pc.Initialise(30000, 0.0001f);
        for (int i = 0; i < 29999; i++)
        {
            pc.addParticle(WaveParticle.createWaveParticle(new Vector2(0, 0), new Vector2(1, 0), 1f, Mathf.PI / 2, 0));
        }
        int count = 0;
        foreach (var wp in pc)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(29999));
       // Assert.That(count, Is.EqualTo(pc.numActiveParticles));
    }


    [Test]
    public void TestParticleAdditionsMegaComplex()
    {
        var pc = new CPUParticleContainer();
        pc.Initialise(30000, 0.0001f);
        for (int i = 0; i < 1000000; i++)
        {
            pc.addParticle(WaveParticle.createWaveParticle(new Vector2(0, 0), new Vector2(1, 0), 1f, Mathf.PI / 2, 0));
        }
        int count = 0;
        foreach (var wp in pc)
        {
            count++;
        }
        Assert.That(count, Is.EqualTo(30000));
       // Assert.That(count, Is.EqualTo(pc.numActiveParticles));
    }

    [Test]
    public void TestSubdivisions()
    {
        var pc = new CPUParticleContainer();
        pc.Initialise(30000, 0.0001f);
        for (int i = 0; i < 1000000; i++)
        {
            pc.addParticle(WaveParticle.createWaveParticle(new Vector2(0, 0), new Vector2(0.5f, 0.5f), 1f, Mathf.PI / 2, 0));
        }
    }
}
