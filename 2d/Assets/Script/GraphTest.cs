using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class GraphTest : MonoBehaviour
{
    public UiGraphNode nodePrefab;
    public List<UiGraphNode> uiNodes;

    public Transform uiNodeRoot;

    private Graph graph;

    private void Start()
    {
        int[,] map = new int[5, 5]
        {
            { 1, -1, 1, 1, 1 },
            { 1, -1, 1, 1, 1 },
            { 1, -1, 1, 1, 1 },
            { 1, -1, 1, 1, 1 },
            { 1, 1, 1, 1, 1 },
        };
        graph = new Graph();
        graph.Init(map);
        InitUiNodes(graph);
    }

    private void InitUiNodes(Graph graph)
    {
        foreach (var node in graph.nodes)
        {
            var uiNode = Instantiate(nodePrefab, uiNodeRoot);
            uiNode.SetNode(node);
            uiNode.Reset();
            uiNodes.Add(uiNode);
        }
    }

    public int startIndex;
    public int endIndex;

    [ContextMenu("Search")]
    public void Search()
    {
        var search = new GraphSearch();
        search.Init(graph);
        search.DFS(graph.nodes[startIndex]);
        ResetUiNodes();

        for (int i = 0; i < search.path.Count; ++i)
        {
            var node = search.path[i];
            var color = Color.Lerp(Color.red, Color.green, (float)i / search.path.Count - 1);
            uiNodes[node.id].SetColor(color);
            uiNodes[node.id].SetText($"Id: {node.id}\nWeight: {node.weight}\nPath: {i}");
        }

    }

    private void ResetUiNodes()
    {
        foreach (var uiNode in uiNodes)
        {
            uiNode.Reset();
        }

    }
}
