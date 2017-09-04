using UnityEngine;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System;
using UnityEngine.Profiling;
using System.Collections;

public class CPUParticleContainer : ParticleContainer
{

    // Constant representing a 'Null_Handle' to a particle index.
    public const int NO_PARTICLE = -1;

    public WaveParticle[] mParticles;
    private int[] _eventIndices;

    private int mCurrentHead = 0;
    //Indexes into which waveParticle is due for a subdicision this step.

    // As we are counting steps using ushorts in the WaveParticles class, we should cycle back around after 
    // 'ushort.MaxValue' timesteps.
    private readonly int[] mSubdivisions = new int[WaveParticle.FRAME_CYCLE_LENGTH];
    private readonly int[] mReflections = new int[WaveParticle.FRAME_CYCLE_LENGTH];

    // The number of particles we wish to store
    private int _numParticles;
    private float _waveParticleKillThreshold;

    // TODO keep track of num particles
    private int mNumActiveParticles;

    public void Initialise(int numParticles, float waveParticleKillThreshold)
    {
        _numParticles = numParticles;
        _waveParticleKillThreshold = waveParticleKillThreshold;
        mParticles = new WaveParticle[numParticles];
        _eventIndices = new int[numParticles];
        for (int i = 0; i < mParticles.Length; i++)
        {
            mParticles[i] = WaveParticle.DEAD_PARTICLE;
        }
        for (int i = 0; i < _eventIndices.Length; i++)
        {
            _eventIndices[i] = NO_PARTICLE;
        }
        for (int i = 0; i < mSubdivisions.Length; i++)
        {
            mSubdivisions[i] = NO_PARTICLE;
        }
        for (int i = 0; i < mReflections.Length; i++)
        {
            mReflections[i] = NO_PARTICLE;
        }
    }

    public void setWaveParticleKillThreshold(float waveParticleKillThreshold)
    {
        _waveParticleKillThreshold = waveParticleKillThreshold;
    }

    public struct ParticleEnumerator : IEnumerator<WaveParticle>
    {
        private readonly int _numParticles;
        private readonly WaveParticle[] _particles;

        private int startIndex;
        private int index;

        public ParticleEnumerator(int currentHead, int numParticles, WaveParticle[] particles)
        {
            this._numParticles = numParticles;
            this._particles = particles;
            startIndex = (currentHead == 0) ? numParticles - 1 : currentHead - 1;
            index = startIndex;
        }

        public WaveParticle Current { get { return _particles[index]; } }

        object IEnumerator.Current { get { return Current; } }

        public void Reset()
        {
            index = startIndex;
        }

        public bool MoveNext()
        {
            index = (index == 0) ? _numParticles - 1 : index - 1;
            while(_particles[index].amplitude == 0)
            {
                if(index == startIndex)
                {
                    return false;
                }
                index = (index == 0) ? _numParticles - 1 : index - 1;
            }
            return index != startIndex;
        }

        public void Dispose()
        {

        }
    }

    public IEnumerator<WaveParticle> GetEnumerator()
    {
        Profiler.BeginSample("Get Enumerator");
        var result = new ParticleEnumerator(mCurrentHead, _numParticles, mParticles);
        Profiler.EndSample();
        return result;
    }

    //public IEnumerator<WaveParticle> GetEnumerator()
    //{
    //    Assert.IsTrue(mCurrentHead < _numParticles);
    //    int startIndex = (mCurrentHead == 0) ? _numParticles - 1 : mCurrentHead - 1;
    //    int index = startIndex;

    //    {
    //        Assert.IsTrue(index != NO_PARTICLE);
    //        if (mParticles[index].amplitude != 0)
    //        {
    //            WaveParticle particle = mParticles[index];
    //            yield return particle;
    //        }
    //        index = (index == 0) ? _numParticles - 1 : index - 1;
    //    }

    //    while (index != startIndex)
    //    {
    //        Assert.IsTrue(index != NO_PARTICLE);
    //        if (mParticles[index].amplitude != 0)
    //        {
    //            WaveParticle particle = mParticles[index];
    //            yield return particle;
    //        }
    //        index = (index == 0) ? _numParticles - 1 : index - 1;
    //    }
    //}

    public void addParticle(WaveParticle particle)
    {
        // TODO: clean up code!

        float timeToSubdivision = WaveParticle.RADIUS * 0.5f / (WaveParticle.PARTICLE_SPEED * particle.dispersionAngle);
        bool reflectBeforeSubdivision = false;

        {
            // TODO: Calculate Raycast
            Vector3 origin = new Vector3(particle.origin.x - 2f, 0f, particle.origin.y - 2f);
            Vector3 direction = new Vector3(particle.velocity.x, 0f, particle.velocity.y);
            RaycastHit hit;
            if (Physics.Raycast(origin, direction, out hit))
            {
                float timeToReflection = hit.distance / WaveParticle.PARTICLE_SPEED;

                if (timeToReflection < timeToSubdivision)
                {
                    reflectBeforeSubdivision = true;
                    /// reflection
                    /// 
                    int reflectionFrame = ((ushort)(Mathf.RoundToInt(timeToReflection / Time.fixedDeltaTime)));
                    reflectionFrame = (particle.startingFrame + reflectionFrame) % WaveParticle.FRAME_CYCLE_LENGTH;

                    int oldParticleIndex = mReflections[reflectionFrame];

                    int nextIndex = oldParticleIndex;
                    mReflections[reflectionFrame] = mCurrentHead;

                    mParticles[mCurrentHead] = particle;
                    _eventIndices[mCurrentHead] = nextIndex;
                }
                Debug.DrawLine(origin, hit.point);
            }
            else
            {
                Debug.DrawRay(origin, direction, Color.red);
            }
        }

        if (!reflectBeforeSubdivision)
        {
            int subdivisionFrame = ((ushort)(Mathf.RoundToInt(timeToSubdivision / Time.fixedDeltaTime)));
            subdivisionFrame = (particle.startingFrame + subdivisionFrame) % WaveParticle.FRAME_CYCLE_LENGTH;


            int oldParticleIndex = mSubdivisions[subdivisionFrame];
            int nextIndex = oldParticleIndex;
            mSubdivisions[subdivisionFrame] = mCurrentHead;

            mParticles[mCurrentHead] = particle;
            _eventIndices[mCurrentHead] = nextIndex;
        }

        mCurrentHead = (mCurrentHead + 1) % _numParticles;
    }

    public void calculateSubdivisions(int currentFrame)
    {
        int currentIndex = mSubdivisions[currentFrame];

        // the max uint value represents a non-existent wave particle
        while (currentIndex != NO_PARTICLE)
        {
            WaveParticle currentParticle = mParticles[currentIndex];


            if (Mathf.Abs(currentParticle.amplitude) > _waveParticleKillThreshold)
            {
                Vector2 origin = currentParticle.origin;
                Vector2 middleVelocity = currentParticle.velocity;
                float newDispersionAngle = currentParticle.dispersionAngle / 3;
                float newAmplitude = currentParticle.amplitude / 3;
                Vector2 leftVelocity = Quaternion.AngleAxis(newDispersionAngle * (180f / Mathf.PI), Vector3.forward) * middleVelocity;
                Vector2 rightVelocity = Quaternion.AngleAxis(newDispersionAngle * (-180f / Mathf.PI), Vector3.forward) * middleVelocity;

                addParticle(WaveParticle.createWaveParticleUnvalidated(origin, middleVelocity, newAmplitude, newDispersionAngle, currentParticle.startingFrame));
                addParticle(WaveParticle.createWaveParticleUnvalidated(origin, leftVelocity, newAmplitude, newDispersionAngle, currentParticle.startingFrame));
                addParticle(WaveParticle.createWaveParticleUnvalidated(origin, rightVelocity, newAmplitude, newDispersionAngle, currentParticle.startingFrame));
            }

            int nextIndex = _eventIndices[currentIndex];
            mParticles[currentIndex] = WaveParticle.DEAD_PARTICLE;
            _eventIndices[currentIndex] = NO_PARTICLE;
            currentIndex = nextIndex;
        }

        mSubdivisions[currentFrame] = NO_PARTICLE;
    }

    public void calculateReflections(int currentFrame)
    {
        // TODO
        int currentIndex = mReflections[currentFrame];

        // the max uint value represents a non-existent wave particle
        while (currentIndex != NO_PARTICLE)
        {
            {
                //Vector3 origin = new Vector3(currentParticle.origin.x - 2f, 0f, currentParticle.origin.y - 2f);
                //Vector3 dest = new Vector3(currentParticle.getPosition(currentFrame).x - 2f, 0f, currentParticle.getPosition(currentFrame).y - 2f);
                //Debug.DrawLine(origin, dest, Color.yellow);


            }

            int nextIndex = _eventIndices[currentIndex];
            mParticles[currentIndex] = WaveParticle.DEAD_PARTICLE;
            _eventIndices[currentIndex] = NO_PARTICLE;
            currentIndex = nextIndex;
        }

        mReflections[currentFrame] = NO_PARTICLE;
    }

    public void setPointMap(int currentFrame, ref ExtendedHeightField pointMap)
    {
        bool antiAliased = false;

        Profiler.BeginSample("Clear Point Map");
        pointMap.Clear();
        Profiler.EndSample();

        Profiler.BeginSample("Get Point Map Raw");
        var pointMapRaw = pointMap.heightMap;
        Profiler.EndSample();

        Profiler.BeginSample("Get Height Field Info");
        var heightFieldInfo = pointMap.heightFieldInfo;
        Profiler.EndSample();

        Profiler.BeginSample("For Each Wave Particle");
        foreach (var waveParticle in this)
        {
            Profiler.BeginSample("Splat Wave Particle");
            if (antiAliased)
            {
                Vector2 waveParticlePosition = waveParticle.getPosition(currentFrame);
                float xpos = (waveParticlePosition.x / heightFieldInfo.Width) * heightFieldInfo.HoriRes;
                float ypos = (waveParticlePosition.y / heightFieldInfo.Height) * heightFieldInfo.VertRes;

                int col0;
                int col1;
                int row0;
                int row1;
                {
                    int col = Mathf.RoundToInt(xpos);
                    int row = Mathf.RoundToInt(ypos);
                    col0 = col - 1;
                    row0 = row - 1;
                    col1 = col;
                    row1 = row;
                }

                float width0 = 0.5f + col1 - xpos;
                float width1 = 0.5f + xpos - col1;
                float height0 = 0.5f + row1 - ypos;
                float height1 = 0.5f + ypos - row1;

                bool col0InRange = col0 < heightFieldInfo.HoriRes && col0 > 0;
                bool col1InRange = col1 < heightFieldInfo.HoriRes && col1 > 0;

                bool row0InRange = row0 < heightFieldInfo.VertRes && row0 > 0;
                bool row1InRange = row1 < heightFieldInfo.VertRes && row1 > 0;

                if (col0InRange && row0InRange) pointMapRaw[(row0 * heightFieldInfo.HoriRes) + col0].g += waveParticle.amplitude * width0 * height0;
                if (col1InRange && row0InRange) pointMapRaw[(row0 * heightFieldInfo.HoriRes) + col1].g += waveParticle.amplitude * width1 * height0;
                if (col0InRange && row1InRange) pointMapRaw[(row1 * heightFieldInfo.HoriRes) + col0].g += waveParticle.amplitude * width0 * height1;
                if (col1InRange && row1InRange) pointMapRaw[(row1 * heightFieldInfo.HoriRes) + col1].g += waveParticle.amplitude * width1 * height1;
            }
            else
            {
                Vector2 waveParticlePosition = waveParticle.getPosition(currentFrame);
                int xPos = Mathf.RoundToInt((waveParticlePosition.x / heightFieldInfo.Width) * heightFieldInfo.HoriRes);
                int yPos = Mathf.RoundToInt((waveParticlePosition.y / heightFieldInfo.Height) * heightFieldInfo.VertRes);
                int index = (yPos * heightFieldInfo.HoriRes) + xPos;
                if (xPos < heightFieldInfo.HoriRes && xPos > 0 && yPos < heightFieldInfo.VertRes && yPos > 0)
                {
                    pointMapRaw[index].g += waveParticle.amplitude;
                }

            }
            Profiler.EndSample();
        }
        Profiler.EndSample();

        Profiler.BeginSample("Apply CPU Height Map");
        pointMap.ApplyCPUHeightMap();
        Profiler.EndSample();
    }

    public void OnDestroy()
    {

    }
}
