# Unity_PingPongPattern

<div style="display: flex; justify-content: space-between;">
<img src="https://github.com/XJINE/Unity_PingPongPattern/blob/main/Screenshot01.gif" style="width: 100px; height: 100px;">
<img src="https://github.com/XJINE/Unity_PingPongPattern/blob/main/Screenshot02.gif" style="width: 100px; height: 100px;">
<img src="https://github.com/XJINE/Unity_PingPongPattern/blob/main/Screenshot03.gif" style="width: 100px; height: 100px;">
<img src="https://github.com/XJINE/Unity_PingPongPattern/blob/main/Screenshot04.gif" style="width: 100px; height: 100px;">
</div>

"Ping Pong Pattern" is a useful pattern for representing spatial areas, such as national borders or habitats.
The area continuously changes due to seeds bouncing like a "Ping-Pong" inside it.

<img src="https://github.com/XJINE/Unity_PingPongPattern/blob/main/Screenshot.png" width="100%" height="auto" />

## Discussion

### Reflection

```csharp
float2 fakeNormal = normalize(seed.coord * texSize - sampleCoord + randomNormal);
seed.velocity = reflect(seed.velocity, fakeNormal);
```

I considered using the vector between the seed and the hit pixel as the normal vector.
(Of course, a bit of randomness is added to that.)

This approach produces poor results. It tends to be very stable, with little variation.

```csharp
float2 randomNormal = normalize(float2(random(seed.velocity * RandomSeed) - 0.5,
                                       random(seed.coord    * RandomSeed) - 0.5));
seed.velocity = reflect(seed.velocity, randomNormal);
```

To make the changes more dynamic, the reflections should be completely random.

If stable results are required, it is better to use something like Voronoi.

### Distinction

As you can see, the result is more organic than expected.

What sets it apart from typical noise like patterns is its ability to maintain continuity, even when the number of seeds or parameters changes.

However, this algorithm has no periodicity like typical noise, nor reproducibility.

It is only known that seeds with the same parameters create similar areas.
The sizes and movement speeds of each area tend to yield similar results, but the overall result cannot be controlled.