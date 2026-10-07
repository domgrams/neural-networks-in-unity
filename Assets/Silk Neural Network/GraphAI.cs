using UnityEngine;
using System;
using System.Collections.Generic;

public class GraphAI : MonoBehaviour
{
    public int GraphSize;
    private int graphSize;


    public Graph graph;

    public int2 NodeIndices;
    public bool Disconnect;
    public bool Connect;

    public bool ValidAction = false;
    public bool Uniform;


    private const int maxGraphSize = 12;
    private const int minGraphSize = 1;

    public void OnValidate()
    {
        if (GraphSize > maxGraphSize) GraphSize = maxGraphSize;
        else if (GraphSize < minGraphSize) GraphSize = minGraphSize;
        
        
        if(GraphSize != graphSize)
        {
            graphSize = GraphSize;
            graph = new Graph(GraphSize);
            Uniform = graph.IsUniform();
        }
        if (Disconnect)
        {
            Disconnect = false;
            ValidAction = graph.Disconnect(NodeIndices);
            Uniform = graph.IsUniform();
        }
        if (Connect)
        {
            Connect = false;
            ValidAction = graph.Connect(NodeIndices);
            Uniform = graph.IsUniform();
        }

    }

    

    


}

[Serializable]
public struct int2
{
    public int i;
    public int j;
    public int2(int x, int y)
    {
        this.i = x;
        this.j = y;
    }

    public void Order()
    {
        if (j < i)
        {
            int temp = i;
            i = j;
            j = temp;
        }

    }
}

[Serializable]
public class Graph
{
    public int Size;
    public int2[] Nodes;
    
    public Graph(int size)
    {
        Size = size;
        Nodes = new int2[0];
    }

    public bool Possible(int2 indices)
    {
        if (indices.i >= Size || indices.j >= Size || indices.i < 0 || indices.j < 0)
        {
            return false;
        }
        if (indices.i == indices.j)
        {
            return false;
        }
        return true;
    }

    public bool IsUniform()
    {
        bool[] nodesMarked = new bool[Nodes.Length];

        void MarkAllNeighbours(int nodeIndex)
        {
            Debug.Log(nodeIndex);
            if (nodesMarked[nodeIndex])
            {
                return;
            }
            nodesMarked[nodeIndex] = true;

            int[] neighbouringIndices1 = GetNodeIndicesFromVertex(Nodes[nodeIndex].i);
            int[] neighbouringIndices2 = GetNodeIndicesFromVertex(Nodes[nodeIndex].j);

            for (int i = 0; i < neighbouringIndices1.Length; i++)
            {
                MarkAllNeighbours(neighbouringIndices1[i]);
            }
            for (int i = 0; i < neighbouringIndices2.Length; i++)
            {
                MarkAllNeighbours(neighbouringIndices2[i]);
            }

        }

        if(Nodes.Length == 0)
        {
            return Size <= 1;
        }

        MarkAllNeighbours(0);

        bool allMarked = true;
        for (int i = 0; i < nodesMarked.Length; i++)
        {
            allMarked = allMarked && nodesMarked[i];
        }

        if (!allMarked) return false;

        bool[] verticesMarked = new bool[Size];
        for (int i = 0; i < Nodes.Length; i++)
        {
            verticesMarked[Nodes[i].i] = true;
            verticesMarked[Nodes[i].j] = true;
        }

        allMarked = true;
        for (int i = 0; i < verticesMarked.Length; i++)
        {
            allMarked = allMarked && verticesMarked[i];
        }

        if (!allMarked) return false;


        return true;
    }

    public int[] GetNodeIndicesFromVertex(int v)
    {
        if (v >= Size || v < 0)
        {
            return new int[0];
        }
        List<int> vertices = new List<int>();
        int index = 0;
        foreach (int2 node in Nodes)
        {
            if (node.i == v || node.j == v)
            {
                vertices.Add(index);
            }
            index++;
        }
        return vertices.ToArray();
    }

    public int2[] GetNodesFromVertex(int v)
    {
        if (v >= Size || v < 0)
        {
            return new int2[0];
        }
        List<int2> vertices = new List<int2>();
        foreach (int2 node in Nodes)
        {
            if (node.i == v || node.j == v)
            {
                vertices.Add(node);
            }
        }
        return vertices.ToArray();
    }

    public int[] GetNeighbouringVertices(int v)
    {
        if (v >= Size || v < 0)
        {
            return new int[0];
        }
        List<int> vertices = new List<int>();
        foreach (int2 node in Nodes)
        {
            if (node.i == v)
            {
                vertices.Add(node.j);
            }
            if(node.j == v)
            {
                vertices.Add(node.j);
            }
        }
        return vertices.ToArray();
    }

    public bool Connected(int2 indices)
    {
        indices.Order();

        if (!Possible(indices)) return false;
        foreach (int2 node in Nodes)
        {
            if ((node.i == indices.i && node.j == indices.j))
            {
                return true;
            }
        }
        return false;
    }
    public int GetIndexOfNode(int2 indices)
    {
        indices.Order();

        if (!Possible(indices)) return -1;
        int index = 0;
        foreach (int2 node in Nodes)
        {
            if ((node.i == indices.i && node.j == indices.j))
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    public bool Connect(int2 indices)
    {
        indices.Order();
        if (!Possible(indices)) return false;
        if (Connected(indices)) return false;

        int2[] newNodes = new int2[Nodes.Length + 1];
        Nodes.CopyTo(newNodes, 0);
        newNodes[newNodes.Length - 1] = indices;
        
        Nodes = newNodes;
        return true;
    }

    public bool Disconnect(int2 indices)
    {
        indices.Order();
        if (!Possible(indices)) return false;
        int connectionIndex = GetIndexOfNode(indices);

        if (connectionIndex == -1) return false;

        int2[] newNodes = new int2[Nodes.Length - 1];
        for (int i = 0; i < connectionIndex; i++)
        {
            newNodes[i] = Nodes[i];
        }
        for (int i = connectionIndex; i < newNodes.Length; i++)
        {
            newNodes[i] = Nodes[i + 1];
        }
        Nodes = newNodes;
        return true;
    }
}