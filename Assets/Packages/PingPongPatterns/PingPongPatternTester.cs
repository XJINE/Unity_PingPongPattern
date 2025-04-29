using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace PingPongPatterns {
public class PingPongPatternTester : MonoBehaviour
{
    #region Field

    private PingPongPattern _pingPongPattern;

    public Vector2Int textureSize     = new (128, 128);
    public int        stepCountInit   = 1000;
    public int        stepCountUpdate = 3;
    public bool       hideDebug;

    public int     seedCount   = 8;
    public Vector2 speedRange  = new (0.1f, 3f);
    public Vector2 radiusRange = new (1, 5);

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
        var deltaTime = 1 / 300f;

        if (_pingPongPattern == null) { return; }

        seeds = GenerateRandomSeeds(seedCount);

        _pingPongPattern.Initialize(seeds, textureSize);

        for(var i = 0; i < stepCountInit; i++)
        {
            _pingPongPattern.Step(deltaTime, true);
        }
    }

    private PingPongPattern.Seed[] GenerateRandomSeeds(int count)
    {
        var seeds = new PingPongPattern.Seed[count];

        for (var i = 0; i < count; i++)
        {
            seeds[i] = GenerateRandomSeed();
        }

        return seeds;
    }

    private PingPongPattern.Seed GenerateRandomSeed()
    {
        return new PingPongPattern.Seed()
        {
            radius   = Random.Range(radiusRange.x, radiusRange.y),
            coord    = new Vector2(Random.value, Random.value),
            velocity = Random.Range(speedRange.x, speedRange.y) * Random.insideUnitCircle.normalized,
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

    [ContextMenu(nameof(AddClone))]
    public void AddClone()
    {
        if (!_pingPongPattern.Initialized) { return; }
    
        AsyncGPUReadback.Request(_pingPongPattern.SeedBufferCompute, request =>
        {
            if (request.hasError) { return; }
    
            var seeds    = request.GetData<PingPongPattern.Seed>();
            var clone    = seeds[Random.Range(0, seeds.Length)];
            var newSeeds = new List<PingPongPattern.Seed>(seeds.ToArray())
            {
                clone
            };
    
            _pingPongPattern.SetupSeeds(newSeeds.ToArray());
        });
    }

    #endregion Method
}}