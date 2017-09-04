using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This implementation of the HeightFieldGenerator interface tries to create as accurate a
/// heightfield as-is possible from the data available.
/// 
/// However, it is slow. The runtime of heightfield generation is 
/// O( numWaveParticles * heightFieldSize )!
/// 
/// TODO: change algorithm to giev it a better running time!
/// TODO: refactor this code
/// </summary>
public class PreciseHeightFieldGenerator : HeightFieldGenerator
{
    private ExtendedHeightField.HeightFieldInfo heightFieldInfo;
    private ParticleContainer waveParticles;

    private int sectionWidth;
    private int sectionHeight;

    override public void Initialise(ExtendedHeightField.HeightFieldInfo heightFieldInfo, ParticleContainer waveParticles)
    {
        this.heightFieldInfo = heightFieldInfo;
        this.waveParticles = waveParticles;

        sectionWidth = Mathf.CeilToInt((WaveParticle.RADIUS / heightFieldInfo.Width) * heightFieldInfo.HoriRes);
        sectionHeight = Mathf.CeilToInt((WaveParticle.RADIUS / heightFieldInfo.Height) * heightFieldInfo.VertRes);
    }

    override public void GenerateHeightField(int currentFrame, ExtendedHeightField extendedHeightField)
    {
        extendedHeightField.Clear();

        foreach (WaveParticle waveParticle in waveParticles)
        {
            Vector2 waveParticlePosition = waveParticle.getPosition(currentFrame);
            int xPos = Mathf.RoundToInt((waveParticlePosition.x / heightFieldInfo.Width) * heightFieldInfo.HoriRes);
            int yPos = Mathf.RoundToInt((waveParticlePosition.y / heightFieldInfo.Height) * heightFieldInfo.VertRes);
            for (int y = 0; y < sectionWidth; y++)
            {
                for (int x = 0; x < sectionHeight; x++)
                {
                    int col = (x - (sectionWidth / 2)) + xPos;
                    int row = (y - (sectionHeight / 2)) + yPos;

                    if (row > 0 && row < heightFieldInfo.VertRes && col > 0 && col < heightFieldInfo.HoriRes)
                    {

                        int index = row * heightFieldInfo.HoriRes + col;
                        Vector2 position = new Vector2(heightFieldInfo.UnitX * col, heightFieldInfo.UnitY * row);
                        Vector2 diff = position - waveParticlePosition;
                        float abs_diff = diff.magnitude;
                        if (abs_diff > WaveParticle.RADIUS)
                        {
                            // Don't need to do rest of calculation based on rectangle function.
                            continue;
                        }
                        // Caclulate vertical displacement
                        float piOverRadius = Mathf.PI / WaveParticle.RADIUS;
                        float y_displacement = (waveParticle.amplitude * 0.5f) * (Mathf.Cos(abs_diff * piOverRadius) + 1);

                        Vector2 longitudinalComponent;
                        {
                            float dotproduct = Vector2.Dot(waveParticle.velocity.normalized, diff);
                            Vector2 Li = -Mathf.Sin(dotproduct * piOverRadius) * waveParticle.velocity.normalized;
                            longitudinalComponent = y_displacement * Li;
                        }
                        float x_displacement = longitudinalComponent.x;
                        float z_displacement = longitudinalComponent.y;
                        extendedHeightField.heightMap[index] += new Color(x_displacement, y_displacement, z_displacement, 1f);
                    }
                }
            }
        }
    }

    override public Texture[] getTextures()
    {
        return new Texture[0];
    }

    override public string getName()
    {
        return "Precise Heightfield Generator";
    }
}

