using UnityEngine;
using System.Collections.Generic;

public class Node : MonoBehaviour
{
    [Header("Gizmo Settings")]
    public float nodeRadius = 0.5f;
    public Color nodeColor = Color.green;
    public Color connectionColor = Color.white;
    
    [Header("Connections")]
    public List<Node> connectedNodes = new List<Node>();

    void OnValidate()
    {
        // Ensure bidirectional connections
        foreach (Node node in connectedNodes)
        {
            if (node != null && !node.connectedNodes.Contains(this))
            {
                node.connectedNodes.Add(this);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = nodeColor;
        Gizmos.DrawSphere(transform.position, nodeRadius);

        Gizmos.color = connectionColor;
        foreach (Node node in connectedNodes)
        {
            if (node != null)
            {
                Gizmos.DrawLine(transform.position, node.transform.position);
            }
        }
    }
}