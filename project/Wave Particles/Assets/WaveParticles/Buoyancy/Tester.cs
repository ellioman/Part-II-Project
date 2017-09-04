using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{

    RenderTexture boxTexture;
    Texture2D boxTexture2D;
    // TODO: find out why new was wanted
    new Camera camera;
    Shader depthShader;

    void Start()
    {
        boxTexture = new RenderTexture(5, 5, 24, RenderTextureFormat.ARGBFloat);
        boxTexture2D = new Texture2D(5, 5, TextureFormat.RGBAFloat, false);
        GameObject GO = new GameObject("PhysicsCamera");
        camera = GO.AddComponent<Camera>();
        camera.backgroundColor = Color.black;
        camera.enabled = false;
        camera.cullingMask = 1 << 8;
        camera.orthographic = true;
        depthShader = Shader.Find("WaveParticles/DepthShader");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!boxTexture.IsCreated())
        {
            boxTexture.Create();
        }
        Floater[] f = FindObjectsOfType<Floater>();
        // Floater floater = f[0];
        foreach (var floater in f)
        {
            var layer = floater.gameObject.layer;
            floater.gameObject.layer = 8;
            camera.targetTexture = boxTexture;
            camera.transform.position = new Vector3(floater.transform.position.x, 0, floater.transform.position.z);
            float rand = Random.Range(0, 360);
            camera.transform.eulerAngles = new Vector3(90, 0, rand);
            camera.orthographicSize = 1f;
            Vector3 floaterVelocity = floater.getVelocity();
            //camera.tag = "ege";
            //floater.tag = "ege";
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;

            floater.getMaterial().SetFloat("_WaveParticles_Velocity_x", floaterVelocity.x);
            floater.getMaterial().SetFloat("_WaveParticles_Velocity_y", floaterVelocity.y);
            floater.getMaterial().SetFloat("_WaveParticles_Velocity_z", floaterVelocity.z);

            camera.RenderWithShader(depthShader, "");
            floater.gameObject.layer = layer;

            camera.targetTexture = null;

            // TODO: work out a good (relative?) Threshold velocity for this.
            if (floaterVelocity.sqrMagnitude > 0.1f)
            {

                RenderTexture.active = boxTexture;
                boxTexture2D.ReadPixels(new Rect(0, 0, boxTexture.width, boxTexture.height), 0, 0);
                boxTexture2D.Apply();
                WaveParticlePlane wpp = FindObjectOfType<WaveParticlePlane>();

                Color[] values = boxTexture2D.GetPixels();
                float unitx = 1f / (float)boxTexture2D.width;
                float unity = 1f / (float)boxTexture2D.height;
                float area = unitx * unity;
                for (int row = 0; row < boxTexture2D.height; row++)
                {
                    for (int col = 0; col < boxTexture2D.width; col++)
                    {
                        int index = (row * boxTexture2D.width) + col;

                        // TODO: Make this particle go to nearest silhoutte point! And then have velocity and wavefront be formed correctly
                        Vector2 position = new Vector2(unitx * col + 3.5f + floater.transform.position.x, unity * row + 3.5f + floater.transform.position.z);
                        if (Mathf.Abs(values[index].r) > 0.01) wpp.AddParticle(position, new Vector2(1f, 0f), values[index].r * area, Mathf.PI * 2);
                        if (Mathf.Abs(values[index].b) > 0.01) wpp.AddParticle(position, new Vector2(1f, 0f), values[index].b * area * -1f, Mathf.PI * 2);
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        Texture texture = boxTexture;
        int width = texture.width * 4;
        int height = texture.height * 4;
        GUI.DrawTexture(new Rect(0, 0, width, height), texture);
        GUI.Label(new Rect(5, height, width, 50), texture.name);
    }
}
