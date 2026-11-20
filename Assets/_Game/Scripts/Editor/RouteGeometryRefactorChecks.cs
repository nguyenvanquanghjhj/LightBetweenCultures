using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LightBetweenCultures.Editor
{
    // Explicit, read-only regression for the endpoint helper refactor.
    // Does not construct GameBootstrap, load scenes, write saves, or build players.
    public static class RouteGeometryRefactorChecks
    {
        [Serializable] private sealed class Result
        {
            public string unity, status, error;
            public int cities, pairs, assertions;
        }

        public static void Run()
        {
            var result = new Result { unity = Application.unityVersion, status = "FAILED" };
            var argument = Environment.GetCommandLineArgs().FirstOrDefault(a => a.StartsWith("--route-refactor-output="));
            if (argument == null) throw new ArgumentException("Provide --route-refactor-output=<new result file>");
            var output = argument.Substring("--route-refactor-output=".Length);
            if (File.Exists(output)) throw new IOException("Preserve existing evidence: " + output);
            try
            {
                foreach (var city in ContentDatabase.Load().cities)
                {
                    var data = GameData.Load(city.id);
                    for (var from = 0; from < data.nodes.Length; from++)
                    for (var to = 0; to < data.nodes.Length; to++)
                    {
                        // Frozen behavior of the original legacy geometry lookup:
                        // first matching edge wins, regardless of routing permissions.
                        var edge = data.edges.FirstOrDefault(e =>
                            (e.from == from && e.to == to) || (e.to == from && e.from == to));
                        var expectedMode = edge != null && edge.travel == "ferry" ? "ferry" : "walk";
                        Require(CityRouteGeometry.Travel(data, from, to) == expectedMode, city.id + ": travel", result);
                        var actual = CityRouteGeometry.Points(data, from, to);
                        if (edge == null)
                        {
                            Require(actual == null, city.id + ": missing edge", result);
                        }
                        else
                        {
                            var expected = new List<Vector2> { Arrival(data.nodes[edge.from]) };
                            if (edge.bends != null) expected.AddRange(edge.bends);
                            expected.Add(Arrival(data.nodes[edge.to]));
                            if (edge.from != from) expected.Reverse();
                            Require(actual != null && actual.SequenceEqual(expected), city.id + ": oriented points", result);
                        }
                        result.pairs++;
                    }
                    result.cities++;
                }
                CheckFirstMatch(result);
                result.status = "PASS";
            }
            catch (Exception error)
            {
                result.error = error.ToString();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
            File.WriteAllText(output, JsonUtility.ToJson(result, true));
            if (result.status != "PASS") throw new InvalidOperationException(result.error);
            Debug.Log("ROUTE REFACTOR CHECKS: " + JsonUtility.ToJson(result));
        }

        private static Vector2 Arrival(NodeData node)
            => node.hasArrivalPosition ? node.arrivalPosition : node.position;

        private static void Require(bool condition, string label, Result result)
        {
            if (!condition) throw new InvalidOperationException(label);
            result.assertions++;
        }

        private static void CheckFirstMatch(Result result)
        {
            var data = new GameData
            {
                nodes = new[]
                {
                    new NodeData { position = Vector2.zero },
                    new NodeData { position = Vector2.one, hasArrivalPosition = true, arrivalPosition = Vector2.up }
                },
                edges = new[]
                {
                    new EdgeData { from = 0, to = 1, travel = "ferry", bidirectional = false, bends = new[] { Vector2.right } },
                    new EdgeData { from = 0, to = 1, travel = "walk" }
                }
            };
            Require(CityRouteGeometry.Travel(data, 1, 0) == "ferry", "Reverse legacy lookup preserves first match", result);
            Require(CityRouteGeometry.Points(data, 1, 0).SequenceEqual(new[] { Vector2.up, Vector2.right, Vector2.zero }),
                "Reverse lookup preserves arrival anchor, bends and orientation", result);
            Require(CityRouteGeometry.Points(data, 0, 0) == null, "Missing self edge remains absent", result);
        }
    }
}
