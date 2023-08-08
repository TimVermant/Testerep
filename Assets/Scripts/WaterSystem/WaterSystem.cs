using Assets.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Assertions;
using Utilities;
using System.Linq;

public enum WaterDirection
{
    North = 0,
    West = 1,
    East = 2,
    None = 3
}

public class WaterSystem : MonoBehaviour
{
    [Header("Flood Information")]
    public WaterDirection _currentWaterDirection = WaterDirection.North;
    [Header("The amount of tiles that should be flooded when the direction changes")]
    public int _nrOfTilesToFlood = 5;

    public UnityEvent WaveDone;
    public bool _isFloodDone { get; private set; } = false;

    [Header("Wave Information")]
    [SerializeField] private int _smallWaveStrength = 7;
    [SerializeField] private int _mediumWaveStrength = 15;
    [SerializeField] private int _bigWaveStrength = 25;
    [SerializeField] private GameObject _waveParticle;
    [SerializeField] private float _waveRFXDistance = 10;

    [Header("Water Plane")]
    [SerializeField] private AnimationCurve _waterCurve;
    [SerializeField] private float _waterRaiseSpeed = 0.5f;

    [Header("Fields To Set")]
    [SerializeField] private GameObject _waterPlane;
    [SerializeField] private NodeManager _nodeManager;
    [SerializeField] private Material _waveWarningMaterial;
    [SerializeField] private BuildingManager _buildingManager;

    // References
#if UNITY_EDITOR
    private Basic_Inputsystem _inputSystem;
#endif
    private Terrain _terrain;
    private WaveWarning _waveWarning; // To update the ocean material on wave end
    private Testerep.Events.EventSystem _eventSystem;

    // Flooded Dike Root Object
    private GameObject _loweredDikes;

    // Water Level Information
    private float _heightPositionDifference = 0.25f;
    private List<Node> _tilesToRise = new List<Node>();
    private bool _wasWaterLevelRaised = false;
    private float _currentCurveLevel = 0f;

    // Flood Information
    private int _currentWave = 0;
    private int _currentHeightLevel = 0;
    private int _maxWaves = 0;
    private List<Connection> _dikesDamagedThisFlood = new List<Connection>();
    private WaterDirection _previousWaterDirection = WaterDirection.None;

    // Fake Flood Information
    private List<Node> _allNodesToBeFlooded = new List<Node>();

    // Wave Information
    private List<Node> _previousWave = new List<Node>();
    private List<Wave> _waves = new List<Wave>();
    private List<bool> _completedWaves = new List<bool>();
    private List<Node> _dangerTiles = new List<Node>();

    public void OnTutorialStart(Testerep.Tutorial.Tutorial tutorial)
    {
        if (tutorial._key.Equals("TutorialWater"))
        {
            StartFlood(EventType.SmallWave, WaterDirection.North);
        }
    }
    public void StartFlood(EventType type = EventType.Unknown, WaterDirection direction = WaterDirection.None)
    {
        if (direction == WaterDirection.None)
        {
            direction = _eventSystem.WaveEventsDirections.First().Value;
        }

        if (type == EventType.Unknown)
        {
            foreach (var eve in _eventSystem.EventQueue)
            {
                if (Testerep.Events.EventSystem.IsWaveEvent(eve.Value.type))
                {
                    type = eve.Value.type;
                    break;
                }
            }
        }

        // Reset flood information
        _currentWave = 0;
        _waves.Clear();
        _completedWaves.Clear();
        _isFloodDone = false;

        // Reset some wave information if the water was raised
        if (_wasWaterLevelRaised)
        {
            _previousWave.Clear();
            _wasWaterLevelRaised = false;
        }

        // Set the wave strength
        Assert.IsTrue(Testerep.Events.EventSystem.IsWaveEvent(type));
        switch (type)
        {
            case EventType.SmallWave:
                _maxWaves = _smallWaveStrength;
                break;
            case EventType.MediumWave:
                _maxWaves = _mediumWaveStrength;
                break;
            case EventType.BigWave:
                _maxWaves = _bigWaveStrength;
                break;
        }

        // Safety check
        Assert.IsTrue(direction != WaterDirection.None);

        // Set water direction
        _currentWaterDirection = direction;

        if (_previousWaterDirection != _currentWaterDirection)
        {
            _previousWave.Clear();
        }

        // Spawn waves
        for (int i = 0; i < _maxWaves; ++i)
        {
            Wave wave = new Wave(_currentWaterDirection, _nrOfTilesToFlood, _currentHeightLevel,
                _loweredDikes, _nodeManager, _terrain, _buildingManager, _dikesDamagedThisFlood);
            wave._onWaveHalfDone.AddListener(OnWaveHalfDone);
            wave._onWaveFailed.AddListener(OnWaveFailed);
            wave._onWaveDone.AddListener(OnWaveDone);
            _waves.Add(wave);
            _completedWaves.Add(false);
        }

        // Make sure previously damaged dikes are still getting flooded
        foreach (Connection conn in _dikesDamagedThisFlood)
        {
            if (conn == null || conn.dike == null || conn.GetHighestPositionNode().HeightLevel > _currentHeightLevel)
            {
                continue;
            }

            if (conn.NodeA.IsFlooded)
            {
                _previousWave.AddUnique(conn.NodeA);
            }

            if (conn.NodeB.IsFlooded)
            {
                _previousWave.AddUnique(conn.NodeB);
            }
        }
        _dikesDamagedThisFlood.Clear();

        // Start the flood
        _previousWave = new List<Node>(_waves[0].Flood(_previousWave));

        _previousWaterDirection = _currentWaterDirection;

        // Spawn our Wave RFX
        // SpawnWaveRFX();

        //Spawn SoundTrack
        //Play Sound waterSplash
        SoundManager sound = SoundManager.Instance;
        sound.PlaySoundEffect(sound.Clips["watertile"]);
    }

    public void ShowDangerTiles(EventType type)
    {
        // Commented out because it looks kinda wonky and we might not want it in game anymore
        //_dangerTiles = GetDangerTiles();
        //ShowDangerTiles();
    }

    public void HideDangerTiles(EventType type, WaterDirection direction)
    {
        //  HideDangerTiles();

    }


    public void ShowDangerTiles()
    {


        foreach (Node node in _dangerTiles)
        {
            foreach (Renderer renderer in node.GetComponents<Renderer>())
            {
                renderer.sharedMaterial = _waveWarningMaterial;
            }
        }
    }

    public void HideDangerTiles()
    {
        foreach (Node node in _dangerTiles)
        {
            node.ResetMaterial();
        }
        _dangerTiles.Clear();
    }

    public List<Node> GetDangerTiles()
    {
        WaterDirection direction = _eventSystem.WaveEventsDirections.First().Value;
        EventType type = EventType.Unknown;

        foreach (var eve in _eventSystem.EventQueue)
        {
            if (Testerep.Events.EventSystem.IsWaveEvent(eve.Value.type))
            {
                type = eve.Value.type;
                break;
            }
        }

        _allNodesToBeFlooded.Clear();
        List<Node> previousWave = new List<Node>(_previousWave);
        List<Connection> dikesDamagedThisFlood = new List<Connection>(_dikesDamagedThisFlood);

        // Reset some wave information if the water was raised
        if (_wasWaterLevelRaised)
        {
            previousWave.Clear();
        }

        // Set the wave strength
        Assert.IsTrue(Testerep.Events.EventSystem.IsWaveEvent(type));
        int nrOfWaves = 0;
        switch (type)
        {
            case EventType.SmallWave:
                nrOfWaves = _smallWaveStrength;
                break;
            case EventType.MediumWave:
                nrOfWaves = _mediumWaveStrength;
                break;
            case EventType.BigWave:
                nrOfWaves = _bigWaveStrength;
                break;
        }

        // Safety check
        Assert.IsTrue(direction != WaterDirection.None);

        if (_previousWaterDirection != direction)
        {
            previousWave.Clear();
        }

        // Make sure previously damaged dikes are still getting flooded
        foreach (Connection conn in dikesDamagedThisFlood)
        {
            if (conn == null || conn.dike == null || conn.GetHighestPositionNode().HeightLevel > _currentHeightLevel)
            {
                continue;
            }

            if (conn.NodeA.IsFlooded)
            {
                previousWave.AddUnique(conn.NodeA);
            }

            if (conn.NodeB.IsFlooded)
            {
                previousWave.AddUnique(conn.NodeB);
            }
        }
        dikesDamagedThisFlood.Clear();

        List<Wave> waves = new List<Wave>();
        for (int i = 0; i < nrOfWaves; ++i)
        {
            waves.Add(new Wave(direction, _nrOfTilesToFlood, _currentHeightLevel,
                _loweredDikes, _nodeManager, _terrain, _buildingManager, dikesDamagedThisFlood));
        }

        List<Node> nodesToReset = new List<Node>();
        List<Dike> dikesToReset = new List<Dike>();
        for (int i = 0; i < nrOfWaves; ++i)
        {
            var fakeFlood = waves[i].FakeFlood(previousWave);

            previousWave = new List<Node>(fakeFlood.Item1);

            nodesToReset.AddUniqueRange(fakeFlood.Item1);
            dikesToReset.AddUniqueRange(fakeFlood.Item2);
        }

        foreach (Node node in nodesToReset)
        {
            node.IsFlooded = false;

            //foreach (Renderer renderer in node.GetComponents<Renderer>())
            //{
            //    renderer.sharedMaterial = _waveWarningMaterial;
            //}
        }

        foreach (Dike dike in dikesToReset)
        {
            dike.RepairDike();
        }

        return nodesToReset;
    }
    private void Awake()
    {
#if UNITY_EDITOR
        _inputSystem = new Basic_Inputsystem();
        _inputSystem.Player.WaterSystemTest.performed += CreateWaves;
        _inputSystem.Player.WaterSystemTest1.performed += CreateWaves2;
#endif
        _terrain = FindObjectOfType<Terrain>();
        _waveWarning = FindObjectOfType<WaveWarning>();
        _eventSystem = FindObjectOfType<Testerep.Events.EventSystem>();

        FindObjectOfType<Testerep.Events.EventSystem>().WaveLaunched.AddListener(StartFlood);

        _terrain.OnNodesFilled.AddListener(OnNodesFilled);

       
    }
    private void Start()
    {
        _loweredDikes = new GameObject("LoweredDikes");
        _loweredDikes.transform.parent = GameObject.Find("Buildings").transform;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        _inputSystem.Enable();
    }
    private void OnDisable()
    {
        _inputSystem.Disable();
    }
#endif

    private void OnWaveHalfDone()
    {
        if (_currentWave < _completedWaves.Count)
        {
            _completedWaves[_currentWave++] = true;
        }

        if (_waves.Count > 0)
        {
            _waves.RemoveAt(0);

            if (_waves.Count > 0)
            {
                List<Node> nodes = new List<Node>(_previousWave);
                _previousWave.Clear();
                _previousWave = new List<Node>(_waves[0].Flood(nodes));
            }
        }
    }
    private void OnWaveFailed()
    {
        if (_currentWave < _completedWaves.Count)
        {
            _completedWaves[_currentWave++] = true;
        }

        if (_waves.Count > 0)
        {
            _waves.RemoveAt(0);

            if (_waves.Count > 0)
            {
                List<Node> nodes = new List<Node>(_previousWave);
                _previousWave.Clear();
                _previousWave = new List<Node>(_waves[0].Flood(nodes));
            }
        }
    }
    private void OnWaveDone()
    {
        if (GameLoop.CurrentGamestate == GameLoop.GameState.Defeat ||
            GameLoop.CurrentGamestate == GameLoop.GameState.Victory)
        {
            return;
        }
        HideDangerTiles();

        if (_waves.Count == 0 &&
            _completedWaves.All(val => val == true))
        {
            _isFloodDone = true;
            WaveDone.Invoke();
            // check if all tiles that can be flooded are flooded
            // if they are, raise the water level
            if (AreAllNodesFlooded())
            {
                StartCoroutine(RaiseWater());
            }
        }
    }
    private void CreateWaves(InputAction.CallbackContext ctx)
    {
        StartFlood(EventType.SmallWave, _currentWaterDirection);
    }
    private void CreateWaves2(InputAction.CallbackContext ctx)
    {
        GetDangerTiles();
        ShowDangerTiles();
    }
    private void OnNodesFilled()
    {
        foreach (Node node in _nodeManager.Nodes)
        {
            if (node.HeightLevel < 0)
            {
                node.IsFlooded = true;
            }
        }
    }
    private bool AreAllNodesFlooded()
    {
        List<Node> nodes = new List<Node>(_nodeManager.Nodes);

        nodes.RemoveAll(node => node.HeightLevel != _currentHeightLevel || node.IsFlooded);

        if (nodes.Count == 0)
        {
            return true;
        }

        if (AreAllNodesFlooded(nodes, out List<Node> exploredNodes))
        {
            _tilesToRise = new List<Node>(exploredNodes);

            return true;
        }

        return false;
    }
    private bool AreAllNodesFlooded(List<Node> nodes, out List<Node> exploredNodes)
    {
        exploredNodes = new List<Node>();
        foreach (Node root in nodes)
        {
            Queue<Node> queue = new Queue<Node>();

            exploredNodes.AddUnique(root);

            queue.Enqueue(root);

            while (queue.Count > 0)
            {
                Node node = queue.Dequeue();

                if (node.IsFlooded)
                {
                    return false;
                }

                foreach (Connection connection in node.Connections)
                {
                    if (connection == null || connection.dike != null)
                    {
                        continue;
                    }

                    if (connection.NodeA.IsFlooded || connection.NodeB.IsFlooded)
                    {
                        return false;
                    }

                    Node to = connection.NodeB;
                    if (node == to)
                    {
                        to = connection.NodeA;
                    }

                    if (to.HeightLevel == node.HeightLevel &&
                        !exploredNodes.Contains(to))
                    {
                        exploredNodes.AddUnique(to);
                        queue.Enqueue(to);
                    }
                }
            }
        }

        return true;
    }
    private IEnumerator RaiseWater()
    {
        ++_currentHeightLevel;
        _previousWave.Clear();
        _dikesDamagedThisFlood.Clear();
        _wasWaterLevelRaised = true;
        _currentCurveLevel = 0f;

        Vector3 originalWaterPos = new Vector3(
            _waterPlane.transform.position.x,
            _waterPlane.transform.position.y,
            _waterPlane.transform.position.z
            );
        List<Vector3> originalPositions = new List<Vector3>();
        List<List<Vector3?>> originalConnectionPositions = new();
        foreach (Node node in _tilesToRise)
        {
            originalPositions.Add(node.transform.position);

            List<Vector3?> connPositions = new();
            foreach (Connection conn in node.Connections)
            {
                if (conn == null || conn.Building == null ||
                    conn.Building.transform.position.y >= _currentHeightLevel * _heightPositionDifference)
                {
                    connPositions.Add(null);
                    continue;
                }

                connPositions.Add(conn.Building.transform.position);
            }

            originalConnectionPositions.Add(connPositions);
        }

        bool shouldContinue = true;
        while (shouldContinue)
        {
            _currentCurveLevel = Mathf.MoveTowards(_currentCurveLevel, 1f, Time.deltaTime * _waterRaiseSpeed);

            _waterPlane.transform.position = Vector3.Lerp(originalWaterPos, new Vector3
                (
                    originalWaterPos.x,
                    originalWaterPos.y + _heightPositionDifference,
                    originalWaterPos.z
                ), _waterCurve.Evaluate(_currentCurveLevel));

            if (Mathf.Abs(originalWaterPos.y + _heightPositionDifference - _waterPlane.transform.position.y) <= 0.000001f)
            {
                shouldContinue = false;
            
            }

            for (int i = 0; i < _tilesToRise.Count; ++i)
            {
                _tilesToRise[i].transform.position = Vector3.Lerp(originalPositions[i],
                    new Vector3(
                        originalPositions[i].x,
                        originalPositions[i].y + _heightPositionDifference,
                        originalPositions[i].z),
                    _waterCurve.Evaluate(_currentCurveLevel));

                for (int j = 0; j < _tilesToRise[i].Connections.Length; ++j)
                {
                    if (originalConnectionPositions[i][j] == null)
                    {
                        continue;
                    }

                    _tilesToRise[i].Connections[j].Building.transform.position = Vector3.Lerp(originalConnectionPositions[i][j].Value,
                        new Vector3(
                            originalConnectionPositions[i][j].Value.x,
                            originalConnectionPositions[i][j].Value.y + _heightPositionDifference,
                            originalConnectionPositions[i][j].Value.z),
                        _waterCurve.Evaluate(_currentCurveLevel));
                }
            }

            yield return null;
        }
        FindObjectOfType<RoadSystem>().UpdateConnectionVisuals();
        _tilesToRise.Clear();
    }
    // MOVE THIS FUNCTION TO WAVE
    private void SpawnWaveRFX()
    {
        float middle = 0;
        float minCoord = _currentWaterDirection == WaterDirection.West ? float.NegativeInfinity : float.PositiveInfinity;

        foreach (Node node in _previousWave)
        {
            switch (_currentWaterDirection)
            {
                case WaterDirection.North:
                    middle += node.transform.position.x;
                    if (node.transform.position.z < minCoord)
                    {
                        minCoord = node.transform.position.z;
                    }
                    break;
                case WaterDirection.West:
                    middle += node.transform.position.z;
                    if (node.transform.position.x > minCoord)
                    {
                        minCoord = node.transform.position.x;
                    }
                    break;
                case WaterDirection.East:
                    middle += node.transform.position.z;
                    if (node.transform.position.x < minCoord)
                    {
                        minCoord = node.transform.position.x;
                    }
                    break;
            }
        }

        middle /= _previousWave.Count;

        GameObject waveRFX = Instantiate(_waveParticle);

        switch (_currentWaterDirection)
        {
            case WaterDirection.North:
                waveRFX.transform.position = new Vector3(middle, 0f, minCoord - _waveRFXDistance);
                break;
            case WaterDirection.West:
                waveRFX.transform.position = new Vector3(minCoord + _waveRFXDistance, 0f, middle);
                waveRFX.transform.Rotate(Vector3.up, -90f);
                break;
            case WaterDirection.East:
                waveRFX.transform.position = new Vector3(minCoord - _waveRFXDistance, 0f, middle);
                waveRFX.transform.Rotate(Vector3.up, 90f);
                break;
        }
    }
}

public sealed class Wave
{
    public UnityEvent _onWaveHalfDone { get; private set; } = new UnityEvent();
    public UnityEvent _onWaveDone { get; private set; } = new UnityEvent();
    public UnityEvent _onWaveFailed { get; private set; } = new UnityEvent();

    private WaterDirection _waterDirection;
    private int _nrOfTilesInWave;
    private int _height;
    private GameObject _floodedDikesParent;
    private NodeManager _nodeManager;
    private Terrain _terrain;
    private BuildingManager _buildingManager;
    private List<Connection> _dikesDamagedThisFlood; // owned by WaterSystem

    public Wave(WaterDirection direction, int nrOfTilesInWave, int height, GameObject floodedDikesParent,
        NodeManager nodeManager, Terrain terrain, BuildingManager buildingManager, List<Connection> dikesDamagedThisFlood)
    {
        _waterDirection = direction;
        _nrOfTilesInWave = nrOfTilesInWave;
        _height = height;
        _floodedDikesParent = floodedDikesParent;
        _nodeManager = nodeManager;
        _terrain = terrain;
        _buildingManager = buildingManager;
        _dikesDamagedThisFlood = dikesDamagedThisFlood;
    }
    public List<Node> Flood(List<Node> previousWave)
    {
        // find all nodes about to be flooded
        List<Node> nodes = FindTilesToFlood(previousWave, _waterDirection, _nrOfTilesInWave, _height, _nodeManager);

        // stop if we can't find any
        if (nodes == null || nodes.Count == 0 || nodes.SequenceEqual(previousWave))
        {
            _onWaveFailed.Invoke();
            _onWaveDone.Invoke();
            return previousWave;
        }

        // actually flood the nodes
        List<GameObject> edgeBuildings = new List<GameObject>();
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            Node node = nodes[i];
            node.IsFlooded = true;

            foreach (Connection conn in node.Connections)
            {
                if (conn == null)
                {
                    continue;
                }

                if (conn.dike != null)
                {
                    if (conn.GetHighestPositionNode().HeightLevel > _height)
                    {
                        continue;
                    }

                    if (conn.NodeA.IsFlooded || conn.NodeB.IsFlooded)
                    {
                        if (!_dikesDamagedThisFlood.Contains(conn))
                        {
                            conn.dike.DamageDike();
                            _dikesDamagedThisFlood.Add(conn);
                        }
                    }

                    if (conn.NodeA.IsFlooded && conn.NodeB.IsFlooded)
                    {
                        GameObject visuals = conn.dike.gameObject.GetGameObjectVisuals();
                        visuals.transform.parent = _floodedDikesParent.transform;
                        visuals.transform.position = conn.dike.transform.position;
                        visuals.transform.rotation = conn.dike.transform.rotation;

                        edgeBuildings.Add(visuals);

                        DestroyDikeConnection(conn);
                    }
                }
                else if (conn.IsRoad)
                {
                    if (conn.GetHighestPositionNode().HeightLevel > _height)
                    {
                        continue;
                    }

                    if (conn.NodeA.IsFlooded && conn.NodeB.IsFlooded)
                    {
                        _buildingManager.DestroyEdgeBuilding(conn);
                    }
                }
            }

            if (node.Building)
            {
                FloodBuildingTile(node.Index, nodes);
            }
        }

        _terrain.AddSinkTask(new SinkTask(nodes, edgeBuildings, _height - 1, _onWaveHalfDone.Invoke, _onWaveDone.Invoke));

        return nodes;
    }
    public System.Tuple<List<Node>, List<Dike>> FakeFlood(List<Node> previousWave)
    {
        // find all nodes about to be flooded
        List<Node> nodes = FindTilesToFlood(previousWave, _waterDirection, _nrOfTilesInWave, _height, _nodeManager);
        List<Dike> edgeBuildings = new List<Dike>();

        // stop if we can't find any
        if (nodes == null || nodes.Count == 0 || nodes.SequenceEqual(previousWave))
        {
            return new(previousWave, edgeBuildings);
        }

        // actually flood the nodes
        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            Node node = nodes[i];
            node.IsFlooded = true;

            foreach (Connection conn in node.Connections)
            {
                if (conn == null)
                {
                    continue;
                }

                if (conn.dike != null)
                {
                    if (conn.GetHighestPositionNode().HeightLevel > _height)
                    {
                        continue;
                    }

                    if (conn.NodeA.IsFlooded || conn.NodeB.IsFlooded)
                    {
                        if (!_dikesDamagedThisFlood.Contains(conn))
                        {
                            conn.dike.DamageDike();
                            _dikesDamagedThisFlood.Add(conn);
                            edgeBuildings.Add(conn.dike);
                        }
                    }
                }
                else if (conn.IsRoad)
                {
                    if (conn.GetHighestPositionNode().HeightLevel > _height)
                    {
                        continue;
                    }
                }
            }
        }

        return new(nodes, edgeBuildings);
    }

    private void DestroyDikeConnection(Connection connection)
    {
        connection.dike = null;
        _buildingManager.DestroyEdgeBuilding(connection);
    }
    private void FloodBuildingTile(int index, List<Node> nodes)
    {
        Node node = _nodeManager.Nodes[index];

        GameObject buildingObject = node.Building;
        if (buildingObject == null)
        {
            return;
        }

        Building building = buildingObject.GetComponent<Building>();
        if (building == null)
        {
            return;
        }

        if (building.BuildingNodes.Count > 1)
        {
            for (int i = 1; i < building.BuildingNodes.Count; i++)
            {
                nodes.Add(building.BuildingNodes[i]);
            }
        }

        // _buildingManager.DestroyBuilding(building);
        _buildingManager.Buildings.Remove(building);
        building.OnDestroyedByFlood();
    }
    private List<Node> FindTilesToFlood(List<Node> previousWave, WaterDirection direction, int nrOfTilesInWave, int height, NodeManager nodeManager)
    {
        if (previousWave != null && previousWave.Count != 0)
        {
            return FloodNeighbours(previousWave);
        }
        else
        {
            List<Node> nodes = null;

            Assert.IsTrue(direction != WaterDirection.None);

            List<Node> nodesToFlood = new List<Node>();
            List<int> restrictedIndices = new List<int>();
            int nrNodesToFlood = nrOfTilesInWave;
            int rowColIndex = 0;
            int maxIterations = 30;
            int currentIteration = 0;
            while (nrNodesToFlood > restrictedIndices.Count)
            {
                ++currentIteration;
                if (currentIteration >= maxIterations)
                {
                    Debug.Log("Max iteration limit reached");
                    break;

                }
                switch (direction)
                {
                    case WaterDirection.North:
                        nodes = GetFirstNonEmptyRow(nodeManager, height, ref rowColIndex);
                        break;
                    case WaterDirection.East:
                        nodes = GetFirstNonEmptyColumn(nodeManager, height, ref rowColIndex);
                        break;
                    case WaterDirection.West:
                        nodes = GetLastNonEmptyColumn(nodeManager, height, ref rowColIndex);
                        break;
                }

                // if we've exhausted all nodes just stop
                if (nodes == null)
                {
                    break;
                }

                for (int i = 0; i < nodes.Count; ++i)
                {
                    if (direction == WaterDirection.North)
                    {
                        if (restrictedIndices.Contains(_terrain.GetCol(nodes[i].Index)))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (restrictedIndices.Contains(_terrain.GetRow(nodes[i].Index)))
                        {
                            continue;
                        }
                    }

                    if (CanTileBeFlooded(nodes[i], height))
                    {
                        if (CanTileBeFloodedFromDirection(nodes[i], direction))
                        {
                            if (nodesToFlood.Count > 0)
                            {
                                if (AreTilesFullyProtected(nodesToFlood[0], nodes[i]))
                                {
                                    if (direction == WaterDirection.North)
                                    {
                                        restrictedIndices.Add(_terrain.GetCol(nodes[i].Index));
                                    }
                                    else
                                    {
                                        restrictedIndices.Add(_terrain.GetRow(nodes[i].Index));
                                    }
                                    continue;
                                }
                            }
                            else
                            {
                                /* Find ANY flooded tile */
                                Node floodedNode = _nodeManager.Nodes.Find(node => node.HeightLevel >= _height - 2 && node.HeightLevel < _height);

                                if (!CanTileBeReached(floodedNode, nodes[i]))
                                {
                                    if (direction == WaterDirection.North)
                                    {
                                        restrictedIndices.Add(_terrain.GetCol(nodes[i].Index));
                                    }
                                    else
                                    {
                                        restrictedIndices.Add(_terrain.GetRow(nodes[i].Index));
                                    }
                                    continue;
                                }
                            }

                            nodesToFlood.Add(nodes[i]);
                            --nrNodesToFlood;

                            if (nrNodesToFlood == 0)
                            {
                                break;
                            }
                        }

                        if (direction == WaterDirection.North)
                        {
                            restrictedIndices.Add(_terrain.GetCol(nodes[i].Index));
                        }
                        else
                        {
                            restrictedIndices.Add(_terrain.GetRow(nodes[i].Index));
                        }
                    }
                    else if (nodes[i].HeightLevel > height)
                    {
                        if (direction == WaterDirection.North)
                        {
                            restrictedIndices.Add(_terrain.GetCol(nodes[i].Index));
                        }
                        else
                        {
                            restrictedIndices.Add(_terrain.GetRow(nodes[i].Index));
                        }
                    }
                }

                ++rowColIndex;
            }

            return nodesToFlood;
        }
    }

    private bool AreTilesFullyProtected(Node root, Node goal)
    {
        List<Node> exploredNodes = new List<Node>();
        Queue<Node> queue = new Queue<Node>();

        exploredNodes.AddUnique(root);

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();

            if (node == goal)
            {
                return false;
            }

            foreach (Connection connection in node.Connections)
            {
                if (connection == null || connection.dike != null)
                {
                    continue;
                }

                Node to = connection.NodeB;
                if (node == to)
                {
                    to = connection.NodeA;
                }

                if (to.HeightLevel == node.HeightLevel &&
                    !exploredNodes.Contains(to))
                {
                    exploredNodes.AddUnique(to);
                    queue.Enqueue(to);
                }
            }
        }

        return true;
    }
    private bool CanTileBeReached(Node root, Node goal)
    {
        List<Node> exploredNodes = new List<Node>();
        Queue<Node> queue = new Queue<Node>();

        exploredNodes.AddUnique(root);

        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            Node node = queue.Dequeue();

            if (node == goal)
            {
                return true;
            }

            foreach (Connection connection in node.Connections)
            {
                if (connection == null || connection.dike != null)
                {
                    continue;
                }

                Node to = connection.NodeB;
                if (node == to)
                {
                    to = connection.NodeA;
                }

                if (to.HeightLevel <= _height &&
                    !exploredNodes.Contains(to))
                {
                    exploredNodes.AddUnique(to);
                    queue.Enqueue(to);
                }
            }
        }

        return false;
    }

    private List<Node> FloodNeighbours(List<Node> previousWave)
    {
        List<Node> nodes = new List<Node>();
        List<Node> nodesAdjacentToDikes = new List<Node>();

        foreach (Node node in previousWave)
        {
            foreach (Connection connection in node.Connections)
            {
                if (connection == null)
                {
                    continue;
                }

                if (connection.dike != null)
                {
                    if (connection.GetHighestPositionNode().HeightLevel > _height)
                    {
                        continue;
                    }

                    if (!node.IsFlooded)
                    {
                        nodesAdjacentToDikes.AddUnique(node);
                    }

                    if (_dikesDamagedThisFlood.Contains(connection))
                    {
                        continue;
                    }

                    _dikesDamagedThisFlood.Add(connection);
                    if (!connection.dike.DamageDike())
                    {
                        continue;
                    }
                }

                if (CanTileBeFlooded(connection.NodeA, _height))
                {
                    nodes.AddUnique(connection.NodeA);
                }
                if (CanTileBeFlooded(connection.NodeB, _height))
                {
                    nodes.AddUnique(connection.NodeB);
                }
            }
        }

        if (nodes.Count == 0)
        {
            return previousWave;
        }

        nodes.AddRange(nodesAdjacentToDikes);

        return nodes;
    }

    private bool CanTileBeFlooded(Node node, int height)
    {
        return node.HeightLevel == height && !node.IsFlooded;
    }
    private bool CanTileBeFloodedFromDirection(Node node, WaterDirection direction)
    {
        Assert.IsTrue(direction != WaterDirection.None, "WaterDirection cannot be none");

        int min = 0;
        int max = 0;

        switch (direction)
        {
            case WaterDirection.North:
                min = 2;
                max = 3;
                break;
            case WaterDirection.East:
                max = 2;
                break;
            case WaterDirection.West:
                min = 3;
                max = 5;
                break;
        }

        for (int i = min; i <= max; ++i)
        {
            if (node.Connections[i] != null)
            {
                if (node.Connections[i]?.dike != null)
                {
                    // Check for dikes
                    if (!_dikesDamagedThisFlood.Contains(node.Connections[i])
                        && node.Connections[i].dike.WillDikeBeDestroyed())
                    {
                        return true;
                    }
                }
                // If there is no dike check for height
                else
                {
                    Node otherNode = node.Connections[i].NodeA;

                    if (otherNode == node)
                    {
                        otherNode = node.Connections[i].NodeB;
                    }

                    if (otherNode.HeightLevel <= _height)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
    private List<Node> GetFirstNonEmptyColumn(NodeManager nodeManager, int height, ref int start)
    {
        for (int i = start; i < nodeManager.NrOfCols; ++i)
        {
            List<Node> col = nodeManager.GetColumn(i);

            col.RemoveAll(node => node.HeightLevel != height || node.IsFlooded);

            if (col.Count > 0)
            {
                start = i;
                return col;
            }
        }

        return null;
    }
    private List<Node> GetLastNonEmptyColumn(NodeManager nodeManager, int height, ref int start)
    {
        for (int i = nodeManager.NrOfCols - 1 - start; i >= 0; --i)
        {
            List<Node> col = nodeManager.GetColumn(i);

            col.RemoveAll(node => node.HeightLevel != height || node.IsFlooded);

            if (col.Count > 0)
            {
                start = i;
                return col;
            }
        }

        return null;
    }
    private List<Node> GetFirstNonEmptyRow(NodeManager nodeManager, int height, ref int start)
    {
        for (int i = start; i < nodeManager.NrOfRows; ++i)
        {
            List<Node> row = nodeManager.GetRow(i);

            row.RemoveAll(node => node.HeightLevel != height || node.IsFlooded);

            if (row.Count > 0)
            {
                start = i;
                return row;
            }
        }

        return null;
    }
}