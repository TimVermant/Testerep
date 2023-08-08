using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Nodes;
using UnityEngine.Events;

public class RoadSystem : MonoBehaviour
{
    public enum RoadType
    {
        RoadNormal = 0,
        RoadInclined = 1,
    }

    public UnityEvent OnPathCalculationCompleted { get; private set; } = new UnityEvent();

    [SerializeField] private List<GameObject> _cornerPieces = new List<GameObject>();
    [SerializeField] private List<GameObject> _roadVisuals = new List<GameObject>();
    [SerializeField] private GameObject _cornersParent;
    private List<string> _roadTags = new List<string> { "RoadNormal", "RoadInclined" };




    private List<Connection> RoadsVisited { get; set; }
    private List<Connection> CurrentPath { get; set; }

    private Terrain _terrain;
    private BuildingManager _buildingManager;
    private bool _isConnected = false;

    private string _mainHallString = "MainHall";

    private void Awake()
    {
        _terrain = FindObjectOfType<Terrain>();
        _buildingManager = FindObjectOfType<BuildingManager>();
        CurrentPath = new List<Connection>();
        RoadsVisited = new List<Connection>();
    }


    public void UpdateBuildingEfficiencies()
    {
        bool buildingUpdated = false;
        foreach (Building building in _buildingManager.Buildings)
        {
            if (building.CompareTag(_mainHallString))
            {
                continue;
            }
            if (building.BuildingNodes.Count > 0)
            {
                foreach (Node node in building.BuildingNodes)
                {
                    if (IsConnected(node))
                    {
                        buildingUpdated = true;
                        // Only update when it needs to be updated
                        if (building.BuildingEfficiency == 0.0f)
                        {
                            building.BuildingEfficiency = 1.0f;
                        }
                        break;
                    }

                }
            }
            if (!buildingUpdated)
            {

                building.BuildingEfficiency = 0.0f;
            }

        }
        OnPathCalculationCompleted.Invoke();
    }


    public void UpdateConnectionVisuals(Connection connection)
    {
        UpdateNeighbouringCorners(connection);

        UpdateConnectionVisuals();

    }

    public void UpdateConnectionVisuals()
    {

        foreach (Connection roadConnection in _buildingManager.Connections)
        {
            if (roadConnection.IsRoad && roadConnection.CornerPieceA != null && roadConnection.CornerPieceB != null)
            {
                UpdateRoadVisuals(roadConnection);
                UpdateCornerPrefabs(roadConnection);
            }
        }
    }

    private void UpdateNeighbouringCorners(Connection connection)
    {
        List<Connection> neighbouringConnections = _terrain.GetNeighbouringConnections(connection);
        foreach (Connection neighbour in neighbouringConnections)
        {
            UpdateCorners(connection, neighbour);
        }

        UpdateCornerPrefabs(connection);

    }

    private void UpdateRoadVisuals(Connection connection)
    {

        float buildingYPos = connection.Building.transform.position.y;

        List<Connection> neighbouringConnections = _terrain.GetNeighbouringConnections(connection);
        foreach (Connection neighbour in neighbouringConnections)
        {
            if (neighbour.IsRoad)
            {
                float neighbourBuildingYPos = neighbour.Building.transform.position.y;

                // Height difference
                if (!buildingYPos.Equals(neighbourBuildingYPos))
                {

                    UpdateRoadVisualsTo(GetLowerRoad(connection, neighbour), RoadType.RoadInclined);
                    UpdateRoadVisualsTo(GetHigherRoad(connection, neighbour), RoadType.RoadNormal);

                }
                // No height difference
                else if (buildingYPos.Equals(neighbourBuildingYPos))
                {

                    UpdateRoadVisualsTo(connection, RoadType.RoadNormal);
                    UpdateRoadVisualsTo(neighbour, RoadType.RoadNormal);

                }
            }

        }

    }

    private Connection GetLowerRoad(Connection connection1, Connection connection2)
    {
        Connection lowestConnection = connection1;

        if (lowestConnection.Building.transform.position.y > connection2.Building.transform.position.y)
        {
            lowestConnection = connection2;
        }
        return lowestConnection;
    }


    private Connection GetHigherRoad(Connection connection1, Connection connection2)
    {
        Connection highestConnection = connection1;

        if (highestConnection.Building.transform.position.y < connection2.Building.transform.position.y)
        {
            highestConnection = connection2;
        }
        return highestConnection;
    }

    private void UpdateRoadVisualsTo(Connection connection, RoadType type)
    {
        if (connection.CornerPieceA == null || connection.CornerPieceB == null)
        {
            return;
        }

        Building building = connection.Building.GetComponent<Building>();

        if (type == RoadType.RoadInclined && !HasIncline(connection) ||
        (type == RoadType.RoadNormal && HasIncline(connection)))
        {
            return;
        }



        int roadVisualsIndex = (int)type;
        GameObject newVisuals = _roadVisuals[roadVisualsIndex];
        Transform visualTransform = building.Visuals.transform;
        Quaternion rotation = visualTransform.rotation;
        CornerPiece piece = connection.GetHighestCornerpiece();
        if (piece != null)
        {


            Vector3 pointAVec = connection.CornerPieceA.Position;
            Vector3 pointBVec = connection.CornerPieceB.Position;

            Vector3 vectorToCorner = new();
            if (piece.Position == pointAVec)
            {
                vectorToCorner = pointAVec - pointBVec;
            }
            else
            {
                vectorToCorner = pointBVec - pointAVec;
            }
            Vector3.Normalize(vectorToCorner);
            if (visualTransform.right == vectorToCorner)
            {
                rotation.SetLookRotation(-visualTransform.forward);
                building.Visuals.transform.rotation = rotation;
            }
        }
        GameObject visuals = _roadVisuals[roadVisualsIndex];


        building.GetComponentInChildren<StraightToStair>().ShouldBecomeStair(type == RoadType.RoadInclined);


        // building.Visuals.transform.Find("OutlinedVisuals").GetComponent<MeshFilter>().mesh = visuals.GetComponent<MeshFilter>().sharedMesh;
        // building.Visuals.transform.Find("OutlinedVisuals").GetComponent<MeshRenderer>().material = visuals.GetComponent<MeshRenderer>().sharedMaterial;


    }




    private bool HasIncline(Connection connection)
    {

        float posYMain = connection.Building.transform.position.y;
        List<Connection> neighbouringConnections = _terrain.GetNeighbouringConnections(connection);
        foreach (Connection neighbouringConnection in neighbouringConnections)
        {
            if (neighbouringConnection == null || neighbouringConnection.Building == null)
            {
                continue;
            }

            float posYNeighbour = neighbouringConnection.Building.transform.position.y;
            if (neighbouringConnection.IsRoad && !posYMain.Equals(posYNeighbour))
            {

                return true;
            }
        }
        return false;
    }

    private CornerPiece GetCommonCornerPiece(Connection connection1, Connection connection2)
    {
        CornerPiece piece = connection1.CornerPieceA;
        if (piece != connection2.CornerPieceA || piece != connection2.CornerPieceB)
        {
            piece = connection1.CornerPieceB;
        }
        return piece;
    }



    public bool IsConnected(Node node)
    {


        _isConnected = false;
        foreach (Connection connection in node.Connections)
        {
            if (connection == null || !connection.IsRoad)
            {
                continue;
            }
            RoadsVisited.Clear();
            CurrentPath.Clear();
            CurrentPath.Add(connection);
            RoadsVisited.Add(connection);
            CheckPath(connection);
            if (_isConnected)
            {
                break;
            }
        }

        return _isConnected;
    }


    public bool IsConnected(Connection connection)
    {

        _isConnected = false;
        if (connection == null)
        {
            return false;
        }
        RoadsVisited.Clear();
        CurrentPath.Clear();
        CurrentPath.Add(connection);
        RoadsVisited.Add(connection);
        CheckPath(connection);
        return _isConnected;

    }

    private void UpdateCorners(Connection connection, Connection neighbour)
    {

        // Corner A - A
        if (connection.PointA == neighbour.PointA)
        {

            if (connection.CornerPieceA == null)
            {
                if (neighbour.CornerPieceA == null)
                {
                    connection.CornerPieceA = new CornerPiece();
                    connection.CornerPieceA.AddConnection(connection);
                    neighbour.CornerPieceA = connection.CornerPieceA;

                }
                else
                {
                    connection.CornerPieceA = neighbour.CornerPieceA;
                }
            }
            connection.CornerPieceA.AddConnection(neighbour);

        }
        // Corner A - B
        else if (connection.PointA == neighbour.PointB)
        {
            if (connection.CornerPieceA == null)
            {
                if (neighbour.CornerPieceB == null)
                {
                    connection.CornerPieceA = new CornerPiece();
                    connection.CornerPieceA.AddConnection(connection);
                    neighbour.CornerPieceB = connection.CornerPieceA;

                }
                else
                {
                    connection.CornerPieceA = neighbour.CornerPieceB;

                }
            }
            connection.CornerPieceA.AddConnection(neighbour);

        }

        // Corner B - A
        if (connection.PointB == neighbour.PointA)
        {
            if (connection.CornerPieceB == null)
            {
                if (neighbour.CornerPieceA == null)
                {
                    connection.CornerPieceB = new CornerPiece();
                    connection.CornerPieceB.AddConnection(connection);
                    neighbour.CornerPieceA = connection.CornerPieceB;
                }
                else
                {
                    connection.CornerPieceB = neighbour.CornerPieceA;
                }
            }
            connection.CornerPieceB.AddConnection(neighbour);

        }
        // Corner B - B
        else if (connection.PointB == neighbour.PointB)
        {
            if (connection.CornerPieceB == null)
            {
                if (neighbour.CornerPieceB == null)
                {
                    connection.CornerPieceB = new CornerPiece();
                    connection.CornerPieceB.AddConnection(connection);
                    neighbour.CornerPieceB = connection.CornerPieceB;
                }
                else
                {
                    connection.CornerPieceB = neighbour.CornerPieceB;
                }
            }
            connection.CornerPieceB.AddConnection(neighbour);

        }

        if (connection.CornerPieceA != null)
        {
            connection.CornerPieceA.UpdateCornerObject();
        }
        if (connection.CornerPieceB != null)
        {
            connection.CornerPieceB.UpdateCornerObject();
        }
    }

    private void UpdateCornerPrefabs(Connection connection)
    {
        if (connection == null)
        {
            return;
        }

        CornerPiece cornerPieceA = connection.CornerPieceA;
        UpdateCornerPrefab(connection, connection.CornerPieceA);
        UpdateCornerPrefab(connection, connection.CornerPieceB);
    }

    private void UpdateCornerPrefab(Connection connection, CornerPiece cornerPiece)
    {
        if (cornerPiece == null)
        {
            return;
        }

        GameObject cornerPrefab = null;
        int roadAmount = cornerPiece.GetAmountOfRoads();
        cornerPrefab = GetCornerPrefab(roadAmount);
        Destroy(cornerPiece.CurrentCornerPiece);
        cornerPiece.CurrentCornerPiece = null;
        if (cornerPrefab != null)
        {
            Vector3 position = cornerPiece.Position;
            position.y = connection.GetHighestPositionCorner(cornerPiece);
            cornerPiece.CurrentCornerPiece = Instantiate(cornerPrefab, position, Quaternion.identity);
            cornerPiece.CurrentCornerPiece.transform.parent = _cornersParent.transform;
            cornerPiece.SetCornerPieceRotation();
            cornerPiece.UpdateCornerObject();
        }
    }

    private GameObject GetCornerPrefab(int roadAmount)
    {
        if (roadAmount > 1 && roadAmount <= 3)
        {
            return _cornerPieces[roadAmount - 2];
        }

        return null;

    }

    private void CheckPath(Connection road)
    {
        if (_isConnected)
        {
            return;
        }

        // If this road has a mainhall we stop checking
        if (road != null && HasMainHall(road))
        {
            _isConnected = true;
            return;
        }

        Connection newRoad = null;

        // Loop over the neighbouring connections and add the first road to the path
        foreach (Connection conn in _terrain.GetNeighbouringConnections(road))
        {
            if (conn != null && !RoadsVisited.Contains(conn))
            {
                if (conn.IsRoad)
                {
                    CurrentPath.Add(conn);
                    RoadsVisited.Add(conn);
                    newRoad = conn;
                    break;
                }
            }
        }

        if (newRoad != null)
        {
            // Recursively calls itself until it either finds no roads or finds a mainhall attached
            CheckPath(newRoad);
        }
        else
        {
            // Tries to go back and find a new path
            newRoad = FindNewPath();
            if (newRoad != null)
            {
                CheckPath(newRoad);
            }
        }

    }

    // Loops through my currently traversed path and finds a new path
    private Connection FindNewPath()
    {

        for (int i = CurrentPath.Count - 1; i > 0; --i)
        {
            Connection newConnection = null;
            Connection oldConnection = CurrentPath[i];
            if (oldConnection != null)
            {
                newConnection = GetAvailableRoad(oldConnection);
            }

            if (newConnection != null)
            {
                CurrentPath.RemoveAt(i);
                return newConnection;
            }
            CurrentPath.RemoveAt(i);

        }
        return null;
    }

    private Connection GetAvailableRoad(Connection connection)
    {
        foreach (Connection conn in _terrain.GetNeighbouringConnections(connection))
        {
            if (conn != null && !RoadsVisited.Contains(conn))
            {
                if (conn.IsRoad)
                {
                    return conn;
                }
            }
        }
        return null;
    }

    private bool HasMainHall(Connection connection)
    {


        GameObject buildingA = connection.NodeA.Building;
        GameObject buildingB = connection.NodeB.Building;
        if (buildingA != null && buildingA.CompareTag(_mainHallString))
        {
            return true;
        }
        if (buildingB != null && buildingB.CompareTag(_mainHallString))
        {
            return true;
        }
        return false;
    }

}
