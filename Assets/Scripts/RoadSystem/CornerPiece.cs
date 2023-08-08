using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Nodes;

public class CornerPiece
{
    public List<Connection> ConnectedEdges { get; } = new List<Connection>();
    public GameObject CurrentCornerPiece { get; set; } = null;
    public Vector3 Position
    {
        get
        {
            return GetCommonPoint();
        }
    }

    // This is necessary since it for some reason doesnt get stored properly on the connection
    public void UpdateCornerObject()
    {
        if(ConnectedEdges.Count != 3)
        {
            return;
        }
        foreach (Connection connection in ConnectedEdges)
        {
            if (!connection.IsRoad)
            {
                continue;
            }
            if (connection.CornerPieceA != null && connection.CornerPieceA.Position == Position)
            {
                connection.CornerPieceA.CurrentCornerPiece = CurrentCornerPiece;
            }
            else if (connection.CornerPieceB != null && connection.CornerPieceB.Position == Position)
            {
                connection.CornerPieceB.CurrentCornerPiece = CurrentCornerPiece;
            }
        }

    }



    public void AddConnection(Connection connection)
    {
        if (ConnectedEdges.Count >= 3)
        {
            return;
        }
        if (connection != null && !ConnectedEdges.Contains(connection))
        {
            ConnectedEdges.Add(connection);
        }
    }


    public int GetAmountOfRoads()
    {
        int roadCount = 0;
        foreach (Connection edge in ConnectedEdges)
        {
            if (edge.IsRoad)
            {
                roadCount++;
            }
        }
        return roadCount;
    }

    private List<Connection> GetConnectionRoads()
    {
        List<Connection> connections = new List<Connection>();
        foreach (Connection connection in ConnectedEdges)
        {
            if (connection.IsRoad)
            {
                connections.Add(connection);
            }
        }
        return connections;
    }

    public void SetCornerPieceRotation()
    {
        if (CurrentCornerPiece != null)
        {
            List<Connection> roadConnections = GetConnectionRoads();
            // Get correct node
            Node node = GetCommonNode();

            int sideIdx1 = -1;
            int sideIdx2 = -1;
            // Finds sideidx by looping over node connections
            for (int i = 0; i < node.Connections.Length; i++)
            {
                if (node.Connections[i] == null)
                {
                    continue;
                }

                if (node.Connections[i].IsRoad &&
                    HasOverlappingConnections(node.Connections[i], roadConnections))
                {
                    // Checks and sets the 2 sideIndices next to the corner
                    if (sideIdx1 == -1)
                    {
                        sideIdx1 = i;
                    }
                    else if (sideIdx2 == -1)
                    {
                        sideIdx2 = i;
                    }
                }
            }

            // Should normally never get called
            if (sideIdx1 == -1 || sideIdx2 == -1)
            {
                Debug.LogError("One of the side indices is invalid");
            }


            float angle = CalculateAngle(node, sideIdx1, sideIdx2, roadConnections.Count);
            Vector3 eulerAngles = Vector3.zero;
            eulerAngles.y = angle;
            CurrentCornerPiece.transform.eulerAngles = eulerAngles;

        }
    }

    private bool HasOverlappingConnections(Connection nodeConnection, List<Connection> roadConnections)
    {
        foreach (Connection connection in roadConnections)
        {
            if (connection == nodeConnection)
            {
                return true;
            }
        }
        return false;
    }

    private float CalculateAngle(Node node, int sideIdx1, int sideIdx2, int roadCount)
    {
        int cornerIdx = Mathf.Max(sideIdx1, sideIdx2);
        float hexagonAngle = 60.0f;
        int hexagonSides = 6;
        
        // Calculations differs if sideIdx == 0
        if ((sideIdx1 == 0 || sideIdx2 == 0) && cornerIdx == 5)
        {
            cornerIdx = 0;
        }
        float angle = 0.0f;
        if (roadCount == 2)
        {
            // 180 - (nrOfSides - cornerIdx) * angleBetweenCorners
            float startAngle = 180.0f;
            angle = startAngle - (hexagonSides - cornerIdx) * hexagonAngle;
        }
        else if (roadCount == 3)
        {
            float startAngle = 90.0f;
            // Flip when above 4
            if (cornerIdx >= 4)
            {
                cornerIdx = hexagonSides - cornerIdx;
            }

            angle = startAngle - (hexagonAngle * cornerIdx);
        }
        return angle;
    }


    private Node GetCommonNode()
    {


        Node nodeA = null, nodeB = null;

        foreach (Connection connection in ConnectedEdges)
        {
            if (!connection.IsRoad)
            {
                continue;
            }
            if (nodeA == null && nodeB == null)
            {
                nodeA = connection.NodeA;
                nodeB = connection.NodeB;
                continue;
            }
            else
            {

                if (nodeA == connection.NodeA || nodeA == connection.NodeB)
                {
                    return nodeA;
                }
                else if (nodeB == connection.NodeA || nodeB == connection.NodeB)
                {
                    return nodeB;
                }
            }

        }

        return null;
    }

    private Vector3 GetCommonPoint()
    {

        int currentConnection = 0;
        while (currentConnection < ConnectedEdges.Count)
        {
            Vector3 point1A = new Vector3
                (ConnectedEdges[currentConnection].PointA.x, 0
                , ConnectedEdges[currentConnection].PointA.y);
            Vector3 point1B = new Vector3
                (ConnectedEdges[currentConnection].PointB.x, 0
                , ConnectedEdges[currentConnection].PointB.y);

            int nextConnection = currentConnection + 1;
            if (nextConnection == ConnectedEdges.Count)
            {
                nextConnection = 0;
            }

            Vector3 point2A = new Vector3
        (ConnectedEdges[nextConnection].PointA.x, 0
        , ConnectedEdges[nextConnection].PointA.y);
            Vector3 point2B = new Vector3
                (ConnectedEdges[nextConnection].PointB.x, 0
                , ConnectedEdges[nextConnection].PointB.y);


            if (point1A == point2A || point1A == point2B)
            {
                return point1A;
            }
            else if (point1B == point2A || point1B == point2B)
            {
                return point1B;
            }

            currentConnection++;
        }


        return new();
    }



}
