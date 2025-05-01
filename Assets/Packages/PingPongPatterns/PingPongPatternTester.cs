using UnityEngine;
using UnityEngine.Rendering;

namespace PingPongPatterns {
public class PingPongPatternTester : MonoBehaviour
{
    #region Field

    private PingPongPattern _pingPongPattern;

    public Vector2Int textureSize     = new (128, 128);
    public int        stepCountInit   = 1000;
    public float      deltaTimeInit   = 1 / 300f;
    public int        stepCountUpdate = 10;
    public bool       hideDebug;

    [System.Serializable]
    public class RandomSeedParameter
    {
        public int     seedCount   = 5;
        public Vector2 speedRange  = new (1f, 10f);
        public Vector2 radiusRange = new (1f,  5f);
    }
    public RandomSeedParameter randomSeedParameter;

    public PingPongPattern.Seed[] seeds;

    #endregion Field

    #region Method

    private void Start()
    {
        _pingPongPattern = FindObjectOfType<PingPongPattern>();
        Initialize();
    }

    private void Update()
    {
        for(var i = 0; i < stepCountUpdate; i++)
        {
            _pingPongPattern.Step(Time.deltaTime, true);
        }
    }

    private void OnGUI()
    {
        if (_pingPongPattern == null || !_pingPongPattern.Initialized)
        {
            return;
        }

        GUI.DrawTexture(new Rect(0, 0, _pingPongPattern.PatternTexture.width,
                                       _pingPongPattern.PatternTexture.height),
                                       _pingPongPattern.PatternTexture);

        if (!hideDebug)
        {
            GUI.DrawTexture(new Rect(0, 0, _pingPongPattern.DebugTexture.width,
                                           _pingPongPattern.DebugTexture.height),
                                           _pingPongPattern.DebugTexture);
        }
    }

    [ContextMenu(nameof(Initialize))]
    public void Initialize()
    {
        if (_pingPongPattern == null) { return; }

        _pingPongPattern.Initialize(seeds, textureSize);

        for(var i = 0; i < stepCountInit; i++)
        {
            _pingPongPattern.Step(deltaTimeInit, true);
        }
    }

    [ContextMenu(nameof(GenerateRandomSeeds))]
    public void GenerateRandomSeeds()
    {
        seeds = GenerateRandomSeeds(randomSeedParameter);
    }

    public PingPongPattern.Seed[] GenerateRandomSeeds(RandomSeedParameter randomSeedParameter)
    {
        var tempSeeds = new PingPongPattern.Seed[randomSeedParameter.seedCount];

        for (var i = 0; i < tempSeeds.Length; i++)
        {
            tempSeeds[i] = GenerateRandomSeed(randomSeedParameter);
        }

        return tempSeeds;
    }

    public PingPongPattern.Seed GenerateRandomSeed(RandomSeedParameter randomSeedParameter)
    {
        return new PingPongPattern.Seed()
        {
            radius   = Random.Range(randomSeedParameter.radiusRange.x, randomSeedParameter.radiusRange.y),
            coord    = new Vector2(Random.value, Random.value),
            velocity = Random.Range(randomSeedParameter.speedRange.x, randomSeedParameter.speedRange.y) * Random.insideUnitCircle.normalized,
            color    = Random.ColorHSV(),
        };
    }

    [ContextMenu(nameof(SetupSeeds))]
    public void SetupSeeds()
    {
        if (_pingPongPattern == null) { return; }

        _pingPongPattern.SetupSeeds(seeds);
    }

    [ContextMenu(nameof(ReadbackSeeds))]
    public void ReadbackSeeds()
    {
        if (!_pingPongPattern.Initialized) { return; }
    
        AsyncGPUReadback.Request(_pingPongPattern.SeedBufferCompute, request =>
        {
            if (request.hasError) { return; }
            seeds = request.GetData<PingPongPattern.Seed>().ToArray();
        });
    }

    #endregion Method
}}