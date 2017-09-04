using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleWater : MonoBehaviour {

    WaveParticleSystem waveParticles;
    Texture particleTexture = new Texture();

    public float particleSpeed = 1f;
    public float particleRadius = 2f;
    public int numParticles = 70000;
    public int horRes = 100;
    public int vertRes = 100;
    public float height = 4f;
    public float width = 4f;
    public float waveParticleKillThreshold = 0.001f;

    // Use this for initialization
    void Start () {
        waveParticles = new WaveParticleSystem(particleSpeed, particleRadius, numParticles, horRes, vertRes, height, width, waveParticleKillThreshold);
        waveParticles.SetSplatImplementation(WaveParticleSystem.SPLAT_GPU);
        waveParticles.SetConvolutionImplementation(WaveParticleSystem.CONV_GPU_2D);
	}
	
	// Update is called once per frame
	void Update () {

	}

    void FixedUpdate()
    {
        Vector2 relativeTexturePosition = new Vector2(0f, 0f);
        Texture2D heightMapTexture = waveParticles.getHeigthMapTexture(relativeTexturePosition);
        print("Hello World");
    }
}
