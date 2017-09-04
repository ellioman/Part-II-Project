using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// This class it the main controller of all things WaveParticles. At the start of the frame, it initialises itself with various factors.
/// </summary>
public class ClickWater : MonoBehaviour
{
    [SerializeField]
    private int _numWaveParticles = 500000;
    // The amplitude that below which, Wave Particles are killed
    [SerializeField]
    private float _waveParticleKillThreshold = 0.001f;
    [SerializeField]
    private HeightFieldGeneratorSelector.Choice _selectedHeightFieldGenerator = HeightFieldGeneratorSelector.Choice.NONE;
    [SerializeField]
    private bool _useGpuForVertices = false;


    // TODO: Sort this out
    [SerializeField]
    public StringBoolPair[] enabledDisplayTextures = new StringBoolPair[0];

    private ParticleContainer waveParticles = new GPUParticleContainer();
    private ExtendedHeightField extendedHeightField = new ExtendedHeightField(0, 0, 0, 0);
    private HeightFieldGenerator heightFieldGenerator = new EmptyHeightFieldGenerator();

    private Material renderMaterial;
    private bool _hasStarted = false;
    private int currentFrame = 0;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;
    private bool needFreshMesh = true;
    private Mesh _mesh;

    void Start()
    {
        renderMaterial = GetComponent<Renderer>().material;
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
                rend.material = renderMaterial;
                renderMaterial.SetTexture(Shader.PropertyToID("_MainTex"), extendedHeightField.textureHeightMap);
                renderMaterial.SetFloat(Shader.PropertyToID("_UnitX"), extendedHeightField.UnitX / extendedHeightField.Width);
                renderMaterial.SetFloat(Shader.PropertyToID("_UnitY"), extendedHeightField.UnitY / extendedHeightField.Height);
                renderMaterial.SetInt(Shader.PropertyToID("_VertexEnabled"), _useGpuForVertices ? 1 : 0);
            }
        }

        _hasStarted = true;
    }

    /// <summary>
    /// Whether of not the system has started
    /// </summary>
    public bool hasStarted
    {
        get
        {
            return _hasStarted;
        }
    }

    public int numWaveParticles
    {
        get { return _numWaveParticles; }
        set { if (!hasStarted) _numWaveParticles = value; }
    }

    public float waveParticleKillThreshold
    {
        get { return _waveParticleKillThreshold; }
        set { _waveParticleKillThreshold = value; if (hasStarted) waveParticles.setWaveParticleKillThreshold(value); }
    }



    ///
    /// Code to allow the HeightFieldGenerator implementation to be selected.
    ///
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
                    renderMaterial.SetTexture(Shader.PropertyToID("_FieldTex"), extendedHeightField.textureHeightMap);
                }
            }
        }
    }


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
                renderMaterial.SetInt(Shader.PropertyToID("_VertexEnabled"), _useGpuForVertices ? 1 : 0);
            }
        }
    }



    public void OnDestroy()
    {
        waveParticles.OnDestroy();
    }

    void FixedUpdate()
    {
        currentFrame = (currentFrame + 1) % WaveParticle.FRAME_CYCLE_LENGTH;
        IterateWaveParticles();
        ComputeObjectForces();
        IterateObjects();
        GenerateWaveParticles();
        heightFieldGenerator.GenerateHeightField(currentFrame, extendedHeightField);
        needFreshMesh = true;
    }

    //// Update is called once per frame (setup the mesh for drawing the wave particle-generated waves)
    void Update()
    {
        // Load the new vertices into the Mesh
        if (!_useGpuForVertices)
        {
            GenerateMeshFromHeightMap(extendedHeightField, vertices, uv, triangles, _mesh);

            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = renderMaterial;
            }
        }
        else
        {
            Renderer rend = GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material = renderMaterial;
            }
        }

        renderMaterial.SetTexture(Shader.PropertyToID("_FieldTex"), extendedHeightField.textureHeightMap);
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
        waveParticles.calculateSubdivisions(currentFrame);
        waveParticles.calculateReflections(currentFrame);
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

    //void OnGUI()
    //{
    //    Texture[] textures = heightFieldGenerator.getTextures();

    //    int countIndex = 0;
    //    for (int i = 0; i < textures.Length; i++)
    //    {
    //        bool textureEnabled = enabledDisplayTextures[i].second;
    //        if (textureEnabled)
    //        {
    //            Texture texture = textures[i];
    //            texture.filterMode = FilterMode.Point;
    //            int width = texture.width * 2;
    //            int height = texture.height * 2;
    //            GUI.DrawTexture(new Rect(width * countIndex, 0, width, height), texture);
    //            GUI.Label(new Rect((width * countIndex) + 5, height, width, 50), texture.name);
    //            countIndex++;
    //        }
    //    }
    //}
}