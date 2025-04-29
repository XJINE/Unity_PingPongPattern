using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PingPongPatterns {
public class PingPongPattern : MonoBehaviour
{
    [System.Serializable]
    public struct Seed
    {
        public float   radius;
        public Vector2 coord;
        public Vector2 velocity;
        public Color   color;
    }

    #region Field

    [SerializeField] private ComputeShader pingPongPatternComputeShader;

    private bool _initialized;

    private RenderTexture _patternTexture;
    private RenderTexture _debugTexture;
    private Seed[]        _seedBuffer;
    private ComputeBuffer _seedBufferCompute;
    private int           _seedCount;

    private int _kernelIndex_Voronoi;
    private int _kernelIndex_UpdateSeeds;
    private int _kernelIndex_DrawSeeds;

    private Vector3Int _threadSize_Voronoi;
    private Vector3Int _threadSize_UpdateSeeds;
    private Vector3Int _threadSize_DrawSeeds;

    #endregion Field

    #region Property

    private static int PropertyID_PatternTexture { get; }
    private static int PropertyID_DebugTexture   { get; }
    private static int PropertyID_SeedBuffer     { get; }
    private static int PropertyID_DeltaTime      { get; }
    private static int PropertyID_RandomSeed     { get; }

    public bool          Initialized       => _initialized;
    public RenderTexture PatternTexture    => _patternTexture;
    public RenderTexture DebugTexture      => _debugTexture;
    public ComputeBuffer SeedBufferCompute => _seedBufferCompute;

    #endregion Property

    #region Constructor

    static PingPongPattern()
    {
        PropertyID_PatternTexture = Shader.PropertyToID("PatternTexture");
        PropertyID_DebugTexture   = Shader.PropertyToID("DebugTexture");
        PropertyID_SeedBuffer     = Shader.PropertyToID("SeedBuffer");
        PropertyID_DeltaTime      = Shader.PropertyToID("DeltaTime");
        PropertyID_RandomSeed     = Shader.PropertyToID("RandomSeed");
    }

    #endregion Constructor

    #region Method

    public void Step(float deltaTime, bool updateDebug = false)
    {
        if (!_initialized) { return; }

        pingPongPatternComputeShader.SetFloat(PropertyID_DeltaTime,  deltaTime);
        pingPongPatternComputeShader.SetFloat(PropertyID_RandomSeed, Random.value);
        pingPongPatternComputeShader.Dispatch(_kernelIndex_UpdateSeeds,
                                              _seedCount,
                                              _threadSize_UpdateSeeds.y,
                                              _threadSize_UpdateSeeds.z);

        if (!updateDebug) { return; }

        pingPongPatternComputeShader.Dispatch(_kernelIndex_DrawSeeds,
                                              Mathf.CeilToInt(_patternTexture.width  / (float)_threadSize_DrawSeeds.x),
                                              Mathf.CeilToInt(_patternTexture.height / (float)_threadSize_DrawSeeds.y),
                                              _threadSize_DrawSeeds.z);
    }

    private void OnDisable() 
    {
        Dispose();
    }

    public void Initialize(Seed[] seedBuffer, Vector2Int textureSize)
    {
        if (_initialized) { Dispose(); }
            _initialized = true;

        InitializeKernelInfo();
        SetupSeeds(seedBuffer);

        _patternTexture = new RenderTexture(textureSize.x, textureSize.y, 0)
        {
            graphicsFormat    = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat,
            enableRandomWrite = true
        };
        _patternTexture.Create();

        _debugTexture = new RenderTexture(_patternTexture.descriptor);
        _debugTexture.Create();

        pingPongPatternComputeShader.SetTexture(_kernelIndex_Voronoi,     PropertyID_PatternTexture, _patternTexture);
        pingPongPatternComputeShader.SetTexture(_kernelIndex_UpdateSeeds, PropertyID_PatternTexture, _patternTexture);
        pingPongPatternComputeShader.SetTexture(_kernelIndex_DrawSeeds,   PropertyID_DebugTexture,   _debugTexture);

        pingPongPatternComputeShader.Dispatch(_kernelIndex_Voronoi,
                                              Mathf.CeilToInt(textureSize.x / (float)_threadSize_Voronoi.x),
                                              Mathf.CeilToInt(textureSize.y / (float)_threadSize_Voronoi.y),
                                                                                     _threadSize_Voronoi.z);
    }

    public void SetupSeeds(Seed[] seedBuffer)
    {
        _seedBuffer = seedBuffer;
        _seedCount  = seedBuffer.Length;

        _seedBufferCompute?.Dispose();
        _seedBufferCompute = new ComputeBuffer(_seedCount, Marshal.SizeOf(typeof(Seed)));
        _seedBufferCompute.SetData(_seedBuffer);

        pingPongPatternComputeShader.SetBuffer(_kernelIndex_Voronoi,     PropertyID_SeedBuffer, _seedBufferCompute);
        pingPongPatternComputeShader.SetBuffer(_kernelIndex_UpdateSeeds, PropertyID_SeedBuffer, _seedBufferCompute);
        pingPongPatternComputeShader.SetBuffer(_kernelIndex_DrawSeeds,   PropertyID_SeedBuffer, _seedBufferCompute);
    }

    private void InitializeKernelInfo()
    {
        _kernelIndex_Voronoi = pingPongPatternComputeShader.FindKernel("Voronoi");
        pingPongPatternComputeShader.GetKernelThreadGroupSizes(_kernelIndex_Voronoi,
                                                               out var threadSizeX,
                                                               out var threadSizeY,
                                                               out var threadSizeZ);
        _threadSize_Voronoi = new Vector3Int((int)threadSizeX, (int)threadSizeY, (int)threadSizeZ);

        _kernelIndex_UpdateSeeds = pingPongPatternComputeShader.FindKernel("UpdateSeeds");
        pingPongPatternComputeShader.GetKernelThreadGroupSizes(_kernelIndex_UpdateSeeds,
                                                               out threadSizeX,
                                                               out threadSizeY,
                                                               out threadSizeZ);
        _threadSize_UpdateSeeds = new Vector3Int((int)threadSizeX, (int)threadSizeY, (int)threadSizeZ);

        _kernelIndex_DrawSeeds = pingPongPatternComputeShader.FindKernel("DrawSeeds");
        pingPongPatternComputeShader.GetKernelThreadGroupSizes(_kernelIndex_DrawSeeds,
                                                               out threadSizeX,
                                                               out threadSizeY,
                                                               out threadSizeZ);
        _threadSize_DrawSeeds = new Vector3Int((int)threadSizeX, (int)threadSizeY, (int)threadSizeZ);
    }

    private void Dispose()
    {
        Destroy(_patternTexture);
        Destroy(_debugTexture);
        _seedBufferCompute?.Dispose();

        _initialized       = false;
        _patternTexture    = null;
        _debugTexture      = null;
        _seedBufferCompute = null;
    }

    #endregion Method
}}