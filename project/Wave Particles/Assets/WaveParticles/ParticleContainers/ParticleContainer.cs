using System.Collections.Generic;

public interface ParticleContainer
{
    void Initialise(int numParticles, float waveParticleKillThreshold);

    void addParticle(WaveParticle particle);

    void calculateSubdivisions(int currentFrame);

    void calculateReflections(int currentFrame);

    void setPointMap(int currentFrame, ref ExtendedHeightField pointMap);


    void setWaveParticleKillThreshold(float waveParticleKillThreshold);

    IEnumerator<WaveParticle> GetEnumerator();
    void OnDestroy();
}
