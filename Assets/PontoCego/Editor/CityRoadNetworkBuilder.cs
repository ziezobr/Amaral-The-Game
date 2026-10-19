using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PontoCego.Editor
{
    public static partial class UrbanSceneBuilder
    {
        // StreetConnectionsBuilder keeps its furniture pass but must not recreate uncut sidewalks.
        static bool canonicalRoadNetworkReady;
        const float RoadHalfWidth = 3.5f, SidewalkOuterHalfWidth = 5f, RoadSnap = 100f;
        sealed class RoadLine
        {
            public bool horizontal;
            public int fixedAt;
            public float min, max;
            public readonly List<float> cuts = new List<float>();
            public Vector2 At(float value) => horizontal ? new Vector2(value, fixedAt / RoadSnap) : new Vector2(fixedAt / RoadSnap, value);
            public Rect Area(float halfWidth) => horizontal
                ? Rect.MinMaxRect(min - halfWidth, fixedAt / RoadSnap - halfWidth, max + halfWidth, fixedAt / RoadSnap + halfWidth)
                : Rect.MinMaxRect(fixedAt / RoadSnap - halfWidth, min - halfWidth, fixedAt / RoadSnap + halfWidth, max + halfWidth);
        }
        sealed class RoadNode
        {
            public Vector2 point;
            public readonly List<Vector2> neighbors = new List<Vector2>();
            public bool IsCornerOrJunction
            {
                get
                {
                    if (neighbors.Count > 2) return true;
                    if (neighbors.Count < 2) return false;
                    Vector2 a = neighbors[0] - point, b = neighbors[1] - point;
                    return Mathf.Abs(a.x * b.y - a.y * b.x) > .001f;
                }
            }
        }

        /// <summary>
        /// One canonical, orthogonal road graph and one set of surfaces. All original mission
        /// coordinates survive. Parallel overlaps are merged before intersections are split.
        /// Sidewalks are the geometric difference between the road shoulder union and asphalt union.
        /// This method creates visuals only: existing collision, mission and NPC objects stay intact.
        /// </summary>
        static void RebuildCityRoadNetwork(WorldHub world)
        {
            canonicalRoadNetworkReady = false;
            if (!world || world.roadStarts == null || world.roadEnds == null || world.roadStarts.Length != world.roadEnds.Length)
                throw new InvalidOperationException("Cannot rebuild a missing or inconsistent city road network.");
            var lines = MergeRoadIntervals(world.roadStarts, world.roadEnds);
            if (lines.Count == 0) throw new InvalidOperationException("The city requires at least one road.");
            foreach (var line in lines) { line.cuts.Add(line.min); line.cuts.Add(line.max); }
            for (int i = 0; i < lines.Count; i++) for (int j = i + 1; j < lines.Count; j++)
            {
                if (lines[i].horizontal == lines[j].horizontal) continue;
                var horizontal = lines[i].horizontal ? lines[i] : lines[j];
                var vertical = lines[i].horizontal ? lines[j] : lines[i];
                float x = vertical.fixedAt / RoadSnap, y = horizontal.fixedAt / RoadSnap;
                if (x < horizontal.min - .005f || x > horizontal.max + .005f || y < vertical.min - .005f || y > vertical.max + .005f) continue;
                horizontal.cuts.Add(x); vertical.cuts.Add(y);
            }
            var starts = new List<Vector2>(); var ends = new List<Vector2>();
            var nodes = new Dictionary<Vector2Int, RoadNode>();
            foreach (var line in lines)
            {
                line.cuts.Sort();
                float previous = line.cuts[0];
                for (int i = 1; i < line.cuts.Count; i++)
                {
                    float next = line.cuts[i]; if (next - previous < .005f) continue;
                    Vector2 a = line.At(previous), b = line.At(next);
                    starts.Add(a); ends.Add(b); AddRoadNeighbor(nodes, a, b); AddRoadNeighbor(nodes, b, a);
                    previous = next;
                }
            }
            // Validate the replacement before removing any old geometry.
            if (starts.Count == 0) throw new InvalidOperationException("The canonical city graph has no usable edges.");
            foreach (var p in world.roadStarts) if (!OnCanonicalRoad(p, lines))
                throw new InvalidOperationException("A mission road endpoint would be lost: " + p);
            foreach (var p in world.roadEnds) if (!OnCanonicalRoad(p, lines))
                throw new InvalidOperationException("A mission road endpoint would be lost: " + p);

            RemovePreviousRoadVisuals(world.gameObject.scene);
            var root = new GameObject("Rua de ligação / rede canônica");
            var sidewalk = FullRoadTile(Procedural("Pavimento urbano / canônico", 32, (x, y) =>
            {
                float grain = ((x * 13 + y * 7) % 9) * .0017f;
                bool joint = x % 16 == 0 || y % 16 == 0;
                return new Color(.48f + grain - (joint ? .035f : 0), .47f + grain - (joint ? .035f : 0), .43f + grain - (joint ? .035f : 0));
            }), "Calçada contínua / sprite completo");
            if (!cityAsphalt)
                cityAsphalt = FullRoadTile(Procedural("Asfalto urbano / canônico", 64, (x, y) =>
                {
                    float grain = ((x * 73856093 ^ y * 19349663) & 31) / 3100f;
                    return new Color(.16f + grain, .18f + grain, .19f + grain);
                }), "Asfalto contínuo / sprite completo");
            var curb = Procedural("Meio-fio / canônico", 4, (x, y) => new Color(.65f, .63f, .55f));
            var yellow = Procedural("Pintura central / canônica", 4, (x, y) => new Color(.91f, .71f, .27f));
            var white = Procedural("Travessia / pintura canônica", 4, (x, y) => new Color(.89f, .88f, .79f));
            var asphaltAreas = new List<Rect>();
            foreach (var line in lines) asphaltAreas.Add(line.Area(RoadHalfWidth));
            foreach (var area in asphaltAreas) RoadSurface("Asfalto / rede canônica", cityAsphalt, area, -9000, root.transform, true);

            int sidewalkCount = 0;
            foreach (var area in RoadShoulders(lines, asphaltAreas, SidewalkOuterHalfWidth))
            {
                RoadSurface("Calçada / rede canônica / " + sidewalkCount, sidewalk, area, -9001, root.transform, true);
                sidewalkCount++;
            }
            foreach (var area in RoadShoulders(lines, asphaltAreas, RoadHalfWidth + .14f))
                RoadSurface("Meio-fio / rede canônica", curb, area, -8997, root.transform, false);

            foreach (var line in lines)
            {
                float fixedAt = line.fixedAt / RoadSnap;
                // A global phase makes paint continuous across graph splits and rebuilt regions.
                for (float at = Mathf.Ceil((line.min + 1) / 4) * 4; at < line.max - 1; at += 4)
                {
                    Vector2 point = line.At(at); bool junction = false;
                    foreach (var node in nodes.Values)
                    {
                        if (!node.IsCornerOrJunction) continue;
                        float distance = Vector2.Distance(point, node.point);
                        if (distance < (node.neighbors.Count > 2 ? 8 : 4.5f)) { junction = true; break; }
                    }
                    if (junction) continue;
                    RoadSurface("Faixa da rua / rede canônica", yellow,
                        new Rect(point - (line.horizontal ? new Vector2(.8f, .06f) : new Vector2(.06f, .8f)),
                            line.horizontal ? new Vector2(1.6f, .12f) : new Vector2(.12f, 1.6f)), -8999, root.transform, false);
                }
            }
            var crossings = new HashSet<Vector3Int>(); int crosswalkCount = 0;
            foreach (var node in nodes.Values)
            {
                if (node.neighbors.Count < 3) continue;
                foreach (Vector2 neighbor in node.neighbors)
                {
                    Vector2 delta = neighbor - node.point;
                    if (delta.magnitude < 8.5f) continue;
                    Vector2 direction = delta.normalized, side = new Vector2(-direction.y, direction.x);
                    Vector2 center = node.point + direction * 6;
                    bool horizontal = Mathf.Abs(direction.x) > .5f;
                    var key = new Vector3Int(Mathf.RoundToInt(center.x * RoadSnap), Mathf.RoundToInt(center.y * RoadSnap), horizontal ? 0 : 1);
                    if (!crossings.Add(key)) continue;
                    for (float across = -2.7f; across <= 2.71f; across += .9f)
                    {
                        Vector2 p = center + side * across;
                        Vector2 size = horizontal ? new Vector2(1.7f, .45f) : new Vector2(.45f, 1.7f);
                        RoadSurface("Travessia / rede canônica / " + crosswalkCount, white, new Rect(p - size * .5f, size), -8998, root.transform, false);
                    }
                    crosswalkCount++;
                }
            }
            world.roadStarts = starts.ToArray(); world.roadEnds = ends.ToArray();
            cityRoadStarts = starts; cityRoadEnds = ends; canonicalRoadNetworkReady = true;
            Debug.Log("PONTO_CANONICAL_ROAD_NETWORK / lines=" + lines.Count + " edges=" + starts.Count + " sidewalks=" + sidewalkCount + " crosswalks=" + crosswalkCount);
        }

        static Sprite FullRoadTile(Sprite tile, string label)
        {
            var sprite = Sprite.Create(tile.texture, tile.rect, new Vector2(.5f, .5f), tile.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = label; AssetDatabase.AddObjectToAsset(sprite, bank); return sprite;
        }
        static void RoadSurface(string label, Sprite sprite, Rect area, int order, Transform parent, bool tiled)
        {
            if (area.width < .01f || area.height < .01f) return;
            var visual = Visual(label, sprite, area.center, order, parent);
            if (tiled) { visual.drawMode = SpriteDrawMode.Tiled; visual.size = area.size; }
            else visual.transform.localScale = new Vector3(area.width / sprite.bounds.size.x, area.height / sprite.bounds.size.y, 1);
        }
        static List<RoadLine> MergeRoadIntervals(Vector2[] starts, Vector2[] ends)
        {
            var groups = new Dictionary<Vector2Int, List<Vector2>>();
            for (int i = 0; i < starts.Length; i++)
            {
                Vector2 a = new Vector2(Mathf.Round(starts[i].x * RoadSnap) / RoadSnap, Mathf.Round(starts[i].y * RoadSnap) / RoadSnap);
                Vector2 b = new Vector2(Mathf.Round(ends[i].x * RoadSnap) / RoadSnap, Mathf.Round(ends[i].y * RoadSnap) / RoadSnap);
                if (Vector2.Distance(a, b) < .005f) continue;
                bool horizontal = Mathf.Abs(a.y - b.y) < .005f;
                if (!horizontal && Mathf.Abs(a.x - b.x) >= .005f) throw new InvalidOperationException("City roads must be orthogonal: " + a + " -> " + b);
                int fixedAt = Mathf.RoundToInt((horizontal ? a.y : a.x) * RoadSnap);
                var key = new Vector2Int(horizontal ? 0 : 1, fixedAt);
                if (!groups.TryGetValue(key, out var intervals)) { intervals = new List<Vector2>(); groups.Add(key, intervals); }
                float x = horizontal ? a.x : a.y, y = horizontal ? b.x : b.y;
                intervals.Add(new Vector2(Mathf.Min(x, y), Mathf.Max(x, y)));
            }
            var keys = new List<Vector2Int>(groups.Keys);
            keys.Sort((a, b) => a.x == b.x ? a.y.CompareTo(b.y) : a.x.CompareTo(b.x));
            var result = new List<RoadLine>();
            foreach (var key in keys)
            {
                var intervals = groups[key]; intervals.Sort((a, b) => a.x.CompareTo(b.x));
                float min = intervals[0].x, max = intervals[0].y;
                for (int i = 1; i <= intervals.Count; i++)
                {
                    if (i < intervals.Count && intervals[i].x <= max + .005f) { max = Mathf.Max(max, intervals[i].y); continue; }
                    result.Add(new RoadLine { horizontal = key.x == 0, fixedAt = key.y, min = min, max = max });
                    if (i < intervals.Count) { min = intervals[i].x; max = intervals[i].y; }
                }
            }
            return result;
        }
        static bool OnCanonicalRoad(Vector2 point, List<RoadLine> lines)
        {
            foreach (var line in lines)
            {
                float fixedPoint = line.horizontal ? point.y : point.x, along = line.horizontal ? point.x : point.y;
                if (Mathf.Abs(fixedPoint - line.fixedAt / RoadSnap) <= .011f && along >= line.min - .011f && along <= line.max + .011f) return true;
            }
            return false;
        }
        static void AddRoadNeighbor(Dictionary<Vector2Int, RoadNode> nodes, Vector2 at, Vector2 neighbor)
        {
            var key = new Vector2Int(Mathf.RoundToInt(at.x * RoadSnap), Mathf.RoundToInt(at.y * RoadSnap));
            if (!nodes.TryGetValue(key, out var node)) { node = new RoadNode { point = at }; nodes.Add(key, node); }
            if (!node.neighbors.Contains(neighbor)) node.neighbors.Add(neighbor);
        }
        static List<Rect> RoadShoulders(List<RoadLine> lines, List<Rect> asphalt, float outerHalfWidth)
        {
            var committed = new List<Rect>();
            foreach (var line in lines)
            {
                var pieces = new List<Rect> { line.Area(outerHalfWidth) };
                foreach (var road in asphalt) pieces = SubtractRoadRectangle(pieces, road);
                // Overlapping shoulders at corners are assigned once, avoiding texture doubling.
                foreach (var previous in committed) pieces = SubtractRoadRectangle(pieces, previous);
                committed.AddRange(pieces);
            }
            return committed;
        }
        static List<Rect> SubtractRoadRectangle(List<Rect> input, Rect mask)
        {
            var output = new List<Rect>();
            foreach (var source in input)
            {
                if (!source.Overlaps(mask)) { output.Add(source); continue; }
                float x0 = Mathf.Max(source.xMin, mask.xMin), x1 = Mathf.Min(source.xMax, mask.xMax);
                float y0 = Mathf.Max(source.yMin, mask.yMin), y1 = Mathf.Min(source.yMax, mask.yMax);
                void Add(float left, float bottom, float right, float top)
                {
                    if (right - left > .009f && top - bottom > .009f) output.Add(Rect.MinMaxRect(left, bottom, right, top));
                }
                Add(source.xMin, source.yMin, source.xMax, y0);
                Add(source.xMin, y1, source.xMax, source.yMax);
                Add(source.xMin, y0, x0, y1);
                Add(x1, y0, source.xMax, y1);
            }
            return output;
        }
        static void RemovePreviousRoadVisuals(UnityEngine.SceneManagement.Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                string name = root.name;
                bool replace = name == "Rua de ligação" || name == "Faixa da rua" || name == "Calçada contínua" ||
                    name == "Travessia / acesso à missão" || name == "Rua de ligação / rede canônica";
                if (!replace || root.GetComponentInChildren<Collider2D>(true) || root.GetComponentInChildren<Rigidbody2D>(true)) continue;
                UnityEngine.Object.DestroyImmediate(root);
            }
        }
    }
}
