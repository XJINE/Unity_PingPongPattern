# Unity_PingPongPattern

<table style="width: 100%; border: none;"><tr>
<td style="width: 25%;"><img src="https://raw.githubusercontent.com/XJINE/Unity_PingPongPattern/main/Screenshot01.gif"></td>
<td style="width: 25%;"><img src="https://raw.githubusercontent.com/XJINE/Unity_PingPongPattern/main/Screenshot02.gif"></td>
<td style="width: 25%;"><img src="https://raw.githubusercontent.com/XJINE/Unity_PingPongPattern/main/Screenshot03.gif"></td>
<td style="width: 25%;"><img src="https://raw.githubusercontent.com/XJINE/Unity_PingPongPattern/main/Screenshot04.gif"></td>
</tr></table>

"Ping Pong Pattern" is a useful pattern for representing spatial areas, such as national borders or habitats.
The area continuously changes due to seeds bouncing like a "Ping-Pong" inside it.

This isn't a completely original idea. You'll find plenty of others who have come up with similar algorithm.

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
