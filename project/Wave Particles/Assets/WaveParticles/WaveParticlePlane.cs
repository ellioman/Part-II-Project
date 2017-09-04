using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

/// <summary>
/// This class it the main controller of all things WaveParticles. At the start of the frame, it initialises itself with various factors.
/// </summary>
public class WaveParticlePlane : MonoBehaviour
{
    private Material waterMaterial;

    private bool _hasStarted = false;
    public bool hasStarted { get { return _hasStarted; } }

    private int _numWaveParticles = 500000;
    public int numWaveParticles { get { return _numWaveParticles; } set { if (!hasStarted) _numWaveParticles = value; } }

    // That the amplitude of wave particles fall below this value, kill them
    [SerializeField]
    private float _waveParticleKillThreshold = 0.001f;
    public float waveParticleKillThreshold { get { return _waveParticleKillThreshold; } set { _waveParticleKillThreshold = value; if (hasStarted) waveParticles.setWaveParticleKillThreshold(value); } }

    private int currentFrame = 0;

    private ParticleContainer waveParticles = new GPUParticleContainer();
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    private ExtendedHeightField extendedHeightField = new ExtendedHeightField(0, 0, 0, 0);

    ///
    /// Code to allow the HeightFieldGenerator implementation to be selected.
    ///
    private HeightFieldGenerator heightFieldGenerator = new EmptyHeightFieldGenerator();
    [SerializeField]
    private HeightFieldGeneratorSelector.Choice _selectedHeightFieldGenerator = HeightFieldGeneratorSelector.Choice.NONE;
    [SerializeField]
    public StringBoolPair[] enabledDisplayTextures = new StringBoolPair[0];
    public HeightFieldGeneratorSelector.Choice selectedHeightFieldGenerator
    {
        get { return _selectedHeightFieldGenerator; }
        set
        {
            // If
            if (_selectedHeightFieldGenerator != value)
            {
                heightFieldGenerator = HeightFieldGeneratorSelector.CreateAndInitialise(value, extendedHeightField.heightFieldInfo, waveParticles);
                enabledDisplayTextures = heightFieldGenerator.getTexturesEnabled();
                _selectedHeightFieldGenerator = value;
                if (hasStarted)
                {
                    heightFieldGenerator.GenerateHeightField(currentFrame, extendedHeightField);
                    GenerateMeshFromHeightMap(extendedHeightField, vertices, uv, triangles, _mesh);
                    waterMaterial.SetTexture(Shader.PropertyToID("_FieldTex"), extendedHeightField.textureHeightMap);
                }
            }
        }
    }

    private bool needFreshMesh = true;

    [SerializeField]
    private bool _useGpuForVertices = false;
    public bool useGpuForVertices
    {
        get { return _useGpuForVertices; }
        set
        {
            _useGpuForVertices = value;
            if (hasStarted)
            {
                {
                    GenerateMeshFromHeightMap(extendedHeightField, vertices, uv, triangles, _mesh);
                }
                waterMaterial.SetInt(Shader.PropertyToID("_VertexEnabled"), _useGpuForVertices ? 1 : 0);
            }
        }
    }

    private Mesh _mesh;

    void Start()
    {
        waterMaterial = GetComponent<Renderer>().material;
        waveParticles.Initialise(numWaveParticles, waveParticleKillThreshold);
        {
            int horRes = 100;
            int vertRes = 100;
            float height = 8;
            float width = 8;
            extendedHeightField = new ExtendedHeightField(width, height, horRes, vertRes);
            extendedHeightField.Clear();
        }

        _mesh = GetComponent<MeshFilter>().mesh;
        vertices = new Vector3[extendedHeightField.HoriRes * extendedHeightField.VertRes];
        uv = new Vector2[extendedHeightField.HoriRes * extendedHeightField.VertRes];
        triangles = new int[(extendedHeightField.HoriRes - 1) * (extendedHeightField.VertRes - 1) * 6];

        heightFieldGenerator = HeightFieldGeneratorSelector.CreateAndInitialise(selectedHeightFieldGenerator, extendedHeightField.heightFieldInfo, waveParticles);


        // Initialise and set-up the mesh the wave particles will be rendered to
        {
            GenerateMeshFromHeightMap(extendedHeightField, vertices, uv, triangles, _mesh);
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = waterMaterial;
                waterMaterial.SetTexture(Shader.PropertyToID("_MainTex"), extendedHeightField.textureHeightMap);
                waterMaterial.SetFloat(Shader.PropertyToID("_UnitX"), extendedHeightField.UnitX);
                waterMaterial.SetFloat(Shader.PropertyToID("_UnitY"), extendedHeightField.UnitY);
                waterMaterial.SetFloat(Shader.PropertyToID("_HoriResInverse"), 1f / ((float) extendedHeightField.HoriRes));
                waterMaterial.SetFloat(Shader.PropertyToID("_VertResInverse"), 1f / ((float) extendedHeightField.VertRes));
                waterMaterial.SetInt(Shader.PropertyToID("_VertexEnabled"), _useGpuForVertices ? 1 : 0);
            }
        }

        _hasStarted = true;
    }

    public void OnDestroy()
    {
        waveParticles.OnDestroy();
    }

    void Update()
    {
        currentFrame = (currentFrame + 1) % WaveParticle.FRAME_CYCLE_LENGTH;

        Profiler.BeginSample("Iterate Wave Particles");
        IterateWaveParticles();
        Profiler.EndSample();

        Profiler.BeginSample("Compute Object Forces");
        ComputeObjectForces();
        Profiler.EndSample();

        Profiler.BeginSample("Iterate Objects");
        IterateObjects();
        Profiler.EndSample();

        Profiler.BeginSample("Generate Wave Particles");
        GenerateWaveParticles();
        Profiler.EndSample();

        Profiler.BeginSample("Generate Height Field");
        heightFieldGenerator.GenerateHeightField(currentFrame, extendedHeightField);
        Profiler.EndSample();

        needFreshMesh = true;
    //}

    ////// Update is called once per frame (setup the mesh for drawing the wave particle-generated waves)
    //void Update()
    //{
        // Load the new vertices into the Mesh
        if (!_useGpuForVertices)
        {
            GenerateMeshFromHeightMap(extendedHeightField, vertices, uv, triangles, _mesh);

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = waterMaterial;
            }
        }
        else
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = waterMaterial;
            }
        }

        waterMaterial.SetTexture(Shader.PropertyToID("_FieldTex"), extendedHeightField.textureHeightMap);
    }

    /// <summary>
    /// Move wave particles to new positions and handle subdivision and reflections events within the time step.
    /// 
    /// Use a timetable of events, as when waveparticles are generated, their exact subdivision and reflections times
    /// can be found. Before subdivision, check wave particle amplitude and if it is below a certain user-defined
    /// threshold, kill the wave particle instead of subdividing it.
    /// </summary>
    void IterateWaveParticles()
    {
        //Loop over event timetable for this frame, and update wave particles as neccesary.
        Profiler.BeginSample("Calculate Subdivisions");
        waveParticles.calculateSubdivisions(currentFrame);
        Profiler.EndSample();
        Profiler.BeginSample("Calculate Reflections");
        waveParticles.calculateReflections(currentFrame);
        Profiler.EndSample();
    }

    /// <summary>
    /// Computer static and dynamic forces generated by waves and apply them to objects.
    /// 
    /// 
    /// </summary>
    void ComputeObjectForces()
    {
        // Compute static and dynamic forces and apply them to objects
    }

    /// <summary>
    /// Handle Rigid-body simulation of water.
    /// </summary>
    void IterateObjects()
    {

    }


    /// <summary>
    /// Create new wave particles due to computed object motion.
    /// </summary>
    void GenerateWaveParticles()
    {
        // TODO: Fix the ripple generator.
        Floater[] f = FindObjectsOfType<Floater>();
        if (f.Length == 0)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                RaycastHit globalMousePosition;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out globalMousePosition))
                {
                    Vector3 localMousePosition = transform.InverseTransformPoint(globalMousePosition.point);
                    Vector2 clickLocation = new Vector2(localMousePosition.x + extendedHeightField.Width / 2f, localMousePosition.z + extendedHeightField.Height / 2f);


                    // Create a small ripple of particles
                    {
                        WaveParticle wp = WaveParticle.createWaveParticle(clickLocation, new Vector2(0.5f, 0.5f), 0.8f, Mathf.PI * 2, currentFrame);
                        waveParticles.addParticle(wp);
                    }
                }
            }
        }
    }

    public void AddParticle(Vector2 position, Vector2 velocity, float amplitude, float dispersionAngle)
    {
        if (amplitude > _waveParticleKillThreshold)
        {
            WaveParticle wp = WaveParticle.createWaveParticle(position, velocity, amplitude, dispersionAngle, currentFrame);
            waveParticles.addParticle(wp);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        collision.rigidbody.AddForce(new Vector3(0f, 1000f, 0f));
        print("There was a collision with " + collision.gameObject.name);
    }

    void GenerateMeshFromHeightMap(ExtendedHeightField heightMap, Vector3[] verticesOut, Vector2[] uvOut, int[] trianglesOut, Mesh mesh)
    {
        if (needFreshMesh)
        {
            if (!_useGpuForVertices)
            {
                for (int row = 0; row < heightMap.VertRes; row++)
                {
                    for (int col = 0; col < heightMap.HoriRes; col++)
                    {
                        int index = row * heightMap.HoriRes + col;
                        Vector4 displacement = heightMap.heightMap[index];

                        verticesOut[index].x = (col * heightMap.UnitX + displacement.x) - (0.5f * heightMap.Width);
                        verticesOut[index].y = displacement.y;
                        verticesOut[index].z = (row * heightMap.UnitY + displacement.z) - (0.5f * heightMap.Height);

                        uvOut[index].x = (col * heightMap.UnitX + displacement.x) / heightMap.Width;
                        uvOut[index].y = (row * heightMap.UnitY + displacement.z) / heightMap.Height;
                    }
                }
            }
            else
            {
                for (int row = 0; row < heightMap.VertRes; row++)
                {
                    for (int col = 0; col < heightMap.HoriRes; col++)
                    {
                        int index = row * heightMap.HoriRes + col;
                        verticesOut[index].x = (col * heightMap.UnitX) - (0.5f * heightMap.Width);
                        verticesOut[index].y = 0;
                        verticesOut[index].z = (row * heightMap.UnitY) - (0.5f * heightMap.Height);

                        uvOut[index].x = (col * heightMap.UnitX) / heightMap.Width;
                        uvOut[index].y = (row * heightMap.UnitY) / heightMap.Height;
                    }
                }
            }

            {
                int triangle_index = 0;
                for (int row = 0; row < heightMap.VertRes - 1; row++)
                {
                    for (int col = 0; col < heightMap.HoriRes - 1; col++)
                    {
                        int vertIndex = row * heightMap.HoriRes + col;
                        trianglesOut[triangle_index++] = vertIndex;
                        trianglesOut[triangle_index++] = vertIndex + heightMap.HoriRes;
                        trianglesOut[triangle_index++] = vertIndex + heightMap.HoriRes + 1;
                        trianglesOut[triangle_index++] = vertIndex;
                        trianglesOut[triangle_index++] = vertIndex + heightMap.HoriRes + 1;
                        trianglesOut[triangle_index++] = vertIndex + 1;
                    }
                }
            }
        }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    void OnGUI()
    {
        Texture[] textures = heightFieldGenerator.getTextures();

        int countIndex = 0;
        for (int i = 0; i < textures.Length; i++)
        {
            bool textureEnabled = enabledDisplayTextures[i].second;
            if (textureEnabled)
            {
                Texture texture = textures[i];
                texture.filterMode = FilterMode.Point;
                int width = texture.width * 2;
                int height = texture.height * 2;
                GUI.DrawTexture(new Rect(width * countIndex, 0, width, height), texture);
                GUI.Label(new Rect((width * countIndex) + 5, height, width, 50), texture.name);
                countIndex++;
            }
        }
    }
}