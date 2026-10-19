using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PontoCego.Tests
{
    public sealed class RoadNetworkTests
    {
        [UnityTest] public IEnumerator CanonicalGraphHasNoDuplicateEdgesOrInteriorCollinearOverlaps()
        {
            GameSession.ExploreNeighborhood("01_Riberal"); yield return null; yield return null;
            var world = WorldHub.Instance;
            Assert.That(world.roadStarts.Length, Is.EqualTo(world.roadEnds.Length));
            var edges = new HashSet<string>();
            for (int i = 0; i < world.roadStarts.Length; i++)
            {
                Vector2 a = world.roadStarts[i], b = world.roadEnds[i];
                bool horizontal = Mathf.Abs(a.y - b.y) < .005f;
                Assert.That(horizontal || Mathf.Abs(a.x - b.x) < .005f, Is.True, "Road must stay orthogonal.");
                Assert.That(Vector2.Distance(a, b), Is.GreaterThan(.005f));
                string key = Mathf.RoundToInt(a.x * 100) + "," + Mathf.RoundToInt(a.y * 100) + ":" + Mathf.RoundToInt(b.x * 100) + "," + Mathf.RoundToInt(b.y * 100);
                Assert.That(edges.Add(key), Is.True, "Duplicate canonical edge " + key);
                for (int j = i + 1; j < world.roadStarts.Length; j++)
                {
                    Vector2 c = world.roadStarts[j], d = world.roadEnds[j];
                    bool otherHorizontal = Mathf.Abs(c.y - d.y) < .005f;
                    if (horizontal != otherHorizontal) continue;
                    float fixedA = horizontal ? a.y : a.x, fixedC = horizontal ? c.y : c.x;
                    if (Mathf.Abs(fixedA - fixedC) > .005f) continue;
                    float a0 = horizontal ? a.x : a.y, a1 = horizontal ? b.x : b.y;
                    float b0 = horizontal ? c.x : c.y, b1 = horizontal ? d.x : d.y;
                    float overlap = Mathf.Min(Mathf.Max(a0, a1), Mathf.Max(b0, b1)) - Mathf.Max(Mathf.Min(a0, a1), Mathf.Min(b0, b1));
                    Assert.That(overlap, Is.LessThanOrEqualTo(.011f), "Collinear asphalt edges overlap in the navigation graph.");
                }
            }
        }
        [UnityTest] public IEnumerator SidewalkSurfacesNeverCrossAnyAsphaltLane()
        {
            GameSession.ExploreNeighborhood("01_Riberal"); yield return null; yield return null;
            var world = WorldHub.Instance; int count = 0;
            foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>())
            {
                if (!renderer.name.StartsWith("Calçada / rede canônica /")) continue;
                count++;
                var bounds = renderer.bounds;
                var sidewalk = Rect.MinMaxRect(bounds.min.x, bounds.min.y, bounds.max.x, bounds.max.y);
                for (int i = 0; i < world.roadStarts.Length; i++)
                {
                    Vector2 a = world.roadStarts[i], b = world.roadEnds[i];
                    var asphalt = Rect.MinMaxRect(Mathf.Min(a.x, b.x) - 3.5f, Mathf.Min(a.y, b.y) - 3.5f, Mathf.Max(a.x, b.x) + 3.5f, Mathf.Max(a.y, b.y) + 3.5f);
                    float overlapX = Mathf.Min(sidewalk.xMax, asphalt.xMax) - Mathf.Max(sidewalk.xMin, asphalt.xMin);
                    float overlapY = Mathf.Min(sidewalk.yMax, asphalt.yMax) - Mathf.Max(sidewalk.yMin, asphalt.yMin);
                    Assert.That(overlapX <= .015f || overlapY <= .015f, Is.True, renderer.name + " covers a traffic lane.");
                }
                Assert.That(renderer.GetComponent<Collider2D>(), Is.Null, "Sidewalk art must not add invisible barriers.");
            }
            Assert.That(count, Is.GreaterThan(12), "Canonical road scenery was not included in the rebuilt world.");
        }
    }
}
