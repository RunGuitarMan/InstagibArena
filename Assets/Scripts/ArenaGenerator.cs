using UnityEngine;
using System.Collections.Generic;

public class ArenaGenerator : MonoBehaviour
{
    [Header("Arena Size")]
    public float arenaWidth = 100f;
    public float arenaLength = 100f;
    public float wallHeight = 50f; // Very high invisible walls

    [Header("Obstacles")]
    public int numberOfPillars = 12;
    public int numberOfPlatforms = 8;
    public int numberOfRamps = 6;
    public int numberOfTowers = 4;
    public int numberOfBridges = 3;

    [Header("Colors")]
    public Color floorColor = new Color(0.2f, 0.2f, 0.25f);
    public Color wallColor = new Color(0.15f, 0.15f, 0.2f);
    public Color obstacleColor = new Color(0.3f, 0.3f, 0.35f);
    public Color platformColor = new Color(0.25f, 0.3f, 0.35f);
    public Color accentColor = new Color(0.1f, 0.6f, 0.8f);

    [Header("Spawn Points")]
    public int numberOfSpawnPoints = 12;

    private List<Vector3> spawnPositions = new List<Vector3>();
    private List<Bounds> occupiedAreas = new List<Bounds>();

    void Awake()
    {
        GenerateArena();
    }

    public void GenerateArena()
    {
        CreateFloor();
        CreateInvisibleWalls();
        CreateCentralStructure();
        CreateTowers();
        CreatePlatforms();
        CreateBridges();
        CreatePillars();
        CreateRamps();
        CreateCoverWalls();
        CreateSpawnPoints();
        CreateLighting();
    }

    void CreateFloor()
    {
        // Main floor
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0, -0.5f, 0);
        floor.transform.localScale = new Vector3(arenaWidth, 1f, arenaLength);
        floor.isStatic = true;
        floor.GetComponent<Renderer>().material = CreateMaterial(floorColor);

        // Floor grid pattern
        CreateFloorPattern();

        // Outer floor rim (slightly lower)
        GameObject rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rim.name = "FloorRim";
        rim.transform.position = new Vector3(0, -1f, 0);
        rim.transform.localScale = new Vector3(arenaWidth + 10f, 1f, arenaLength + 10f);
        rim.isStatic = true;
        rim.GetComponent<Renderer>().material = CreateMaterial(wallColor);
    }

    void CreateFloorPattern()
    {
        Material lineMat = CreateMaterial(new Color(0.3f, 0.3f, 0.35f));
        Material accentMat = CreateMaterial(accentColor * 0.5f);

        // Grid lines
        float gridSpacing = 20f;
        for (float x = -arenaWidth / 2; x <= arenaWidth / 2; x += gridSpacing)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "GridLine";
            line.transform.position = new Vector3(x, 0.01f, 0);
            line.transform.localScale = new Vector3(0.15f, 0.02f, arenaLength);
            line.GetComponent<Renderer>().material = lineMat;
            line.GetComponent<Collider>().enabled = false;
        }

        for (float z = -arenaLength / 2; z <= arenaLength / 2; z += gridSpacing)
        {
            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "GridLine";
            line.transform.position = new Vector3(0, 0.01f, z);
            line.transform.localScale = new Vector3(arenaWidth, 0.02f, 0.15f);
            line.GetComponent<Renderer>().material = lineMat;
            line.GetComponent<Collider>().enabled = false;
        }

        // Center circle marker
        CreateCircleMarker(Vector3.up * 0.02f, 15f, accentMat);
    }

    void CreateCircleMarker(Vector3 position, float radius, Material mat)
    {
        int segments = 32;
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 360f / segments;
            float nextAngle = (i + 1) * 360f / segments;

            Vector3 start = position + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
            Vector3 end = position + new Vector3(Mathf.Cos(nextAngle * Mathf.Deg2Rad), 0, Mathf.Sin(nextAngle * Mathf.Deg2Rad)) * radius;

            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
            segment.name = "CircleSegment";
            segment.transform.position = (start + end) / 2f;
            segment.transform.localScale = new Vector3(0.3f, 0.02f, Vector3.Distance(start, end) + 0.1f);
            segment.transform.LookAt(end);
            segment.GetComponent<Renderer>().material = mat;
            segment.GetComponent<Collider>().enabled = false;
        }
    }

    void CreateInvisibleWalls()
    {
        float halfWidth = arenaWidth / 2f;
        float halfLength = arenaLength / 2f;
        float halfHeight = wallHeight / 2f;
        float wallThickness = 2f;

        // Create invisible walls - very tall so can't be jumped over
        CreateInvisibleWall(new Vector3(0, halfHeight, halfLength + wallThickness / 2f),
                           new Vector3(arenaWidth + wallThickness * 2, wallHeight, wallThickness));

        CreateInvisibleWall(new Vector3(0, halfHeight, -halfLength - wallThickness / 2f),
                           new Vector3(arenaWidth + wallThickness * 2, wallHeight, wallThickness));

        CreateInvisibleWall(new Vector3(halfWidth + wallThickness / 2f, halfHeight, 0),
                           new Vector3(wallThickness, wallHeight, arenaLength));

        CreateInvisibleWall(new Vector3(-halfWidth - wallThickness / 2f, halfHeight, 0),
                           new Vector3(wallThickness, wallHeight, arenaLength));

        // Visual border at ground level
        CreateVisualBorder();
    }

    void CreateInvisibleWall(Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "InvisibleWall";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.isStatic = true;

        // Make invisible
        Renderer renderer = wall.GetComponent<Renderer>();
        renderer.enabled = false;

        occupiedAreas.Add(new Bounds(position, scale));
    }

    void CreateVisualBorder()
    {
        float halfWidth = arenaWidth / 2f;
        float halfLength = arenaLength / 2f;
        float borderHeight = 1.5f;
        Material borderMat = CreateMaterial(accentColor);

        // Glowing border strips
        Vector3[] positions = {
            new Vector3(0, borderHeight / 2, halfLength),
            new Vector3(0, borderHeight / 2, -halfLength),
            new Vector3(halfWidth, borderHeight / 2, 0),
            new Vector3(-halfWidth, borderHeight / 2, 0)
        };

        Vector3[] scales = {
            new Vector3(arenaWidth, borderHeight, 0.3f),
            new Vector3(arenaWidth, borderHeight, 0.3f),
            new Vector3(0.3f, borderHeight, arenaLength),
            new Vector3(0.3f, borderHeight, arenaLength)
        };

        for (int i = 0; i < 4; i++)
        {
            GameObject border = GameObject.CreatePrimitive(PrimitiveType.Cube);
            border.name = "BorderStrip";
            border.transform.position = positions[i];
            border.transform.localScale = scales[i];
            border.isStatic = true;
            border.GetComponent<Renderer>().material = borderMat;
            border.GetComponent<Collider>().enabled = false;
        }
    }

    void CreateCentralStructure()
    {
        // Multi-level central platform
        float baseSize = 20f;

        // Level 1 - Ground level platform
        CreatePlatformBlock(Vector3.zero, baseSize, baseSize, 0.5f, platformColor);

        // Level 2 - Mid platform
        CreatePlatformBlock(new Vector3(0, 3f, 0), baseSize * 0.7f, baseSize * 0.7f, 0.5f, platformColor);

        // Level 3 - Top platform
        CreatePlatformBlock(new Vector3(0, 6f, 0), baseSize * 0.4f, baseSize * 0.4f, 0.5f, platformColor);

        // Ramps to access levels
        CreateRampTo(new Vector3(baseSize * 0.35f, 0, 0), 3f, 0);
        CreateRampTo(new Vector3(-baseSize * 0.35f, 0, 0), 3f, 180);
        CreateRampTo(new Vector3(0, 3f, baseSize * 0.25f), 3f, 90);
        CreateRampTo(new Vector3(0, 3f, -baseSize * 0.25f), 3f, -90);

        occupiedAreas.Add(new Bounds(new Vector3(0, 3f, 0), new Vector3(baseSize, 8f, baseSize)));

        spawnPositions.Add(new Vector3(0, 7f, 0)); // Top spawn
    }

    void CreatePlatformBlock(Vector3 position, float width, float length, float height, Color color)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "PlatformBlock";
        platform.transform.position = position;
        platform.transform.localScale = new Vector3(width, height, length);
        platform.isStatic = true;
        platform.GetComponent<Renderer>().material = CreateMaterial(color);
    }

    void CreateRampTo(Vector3 basePosition, float height, float rotationY)
    {
        float length = height * 2.5f;
        float width = 4f;

        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";
        ramp.transform.position = basePosition + Vector3.up * (height / 2f);
        ramp.transform.localScale = new Vector3(width, 0.3f, length);

        float angle = Mathf.Atan2(height, length) * Mathf.Rad2Deg;
        ramp.transform.rotation = Quaternion.Euler(-angle, rotationY, 0);
        ramp.isStatic = true;
        ramp.GetComponent<Renderer>().material = CreateMaterial(platformColor);
    }

    void CreateTowers()
    {
        // Corner towers
        float offset = arenaWidth * 0.35f;
        Vector3[] towerPositions = {
            new Vector3(offset, 0, offset),
            new Vector3(-offset, 0, offset),
            new Vector3(offset, 0, -offset),
            new Vector3(-offset, 0, -offset)
        };

        foreach (Vector3 pos in towerPositions)
        {
            CreateTower(pos);
        }
    }

    void CreateTower(Vector3 basePosition)
    {
        float towerSize = 8f;
        float level1Height = 4f;
        float level2Height = 8f;
        float level3Height = 12f;

        // Base
        CreatePlatformBlock(basePosition + Vector3.up * (level1Height / 2f), towerSize, towerSize, level1Height, obstacleColor);

        // Level 2 platform
        CreatePlatformBlock(basePosition + Vector3.up * level2Height, towerSize * 0.8f, towerSize * 0.8f, 0.5f, platformColor);

        // Level 3 platform (top)
        CreatePlatformBlock(basePosition + Vector3.up * level3Height, towerSize * 0.5f, towerSize * 0.5f, 0.5f, platformColor);

        // Pillars connecting levels
        float pillarOffset = towerSize * 0.3f;
        Vector3[] pillarOffsets = {
            new Vector3(pillarOffset, 0, pillarOffset),
            new Vector3(-pillarOffset, 0, pillarOffset),
            new Vector3(pillarOffset, 0, -pillarOffset),
            new Vector3(-pillarOffset, 0, -pillarOffset)
        };

        foreach (Vector3 offset in pillarOffsets)
        {
            // Lower pillars
            GameObject pillar1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar1.name = "TowerPillar";
            pillar1.transform.position = basePosition + offset + Vector3.up * (level1Height + (level2Height - level1Height) / 2f);
            pillar1.transform.localScale = new Vector3(0.8f, level2Height - level1Height, 0.8f);
            pillar1.isStatic = true;
            pillar1.GetComponent<Renderer>().material = CreateMaterial(obstacleColor);

            // Upper pillars
            GameObject pillar2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
            pillar2.name = "TowerPillar";
            pillar2.transform.position = basePosition + offset * 0.6f + Vector3.up * (level2Height + (level3Height - level2Height) / 2f);
            pillar2.transform.localScale = new Vector3(0.6f, level3Height - level2Height, 0.6f);
            pillar2.isStatic = true;
            pillar2.GetComponent<Renderer>().material = CreateMaterial(obstacleColor);
        }

        // Jump pads / ramps to access
        CreateRampTo(basePosition + new Vector3(towerSize * 0.5f, 0, 0), level1Height, 0);

        occupiedAreas.Add(new Bounds(basePosition + Vector3.up * 6f, new Vector3(towerSize + 4, 14f, towerSize + 4)));

        spawnPositions.Add(basePosition + Vector3.up * (level2Height + 1f));
        spawnPositions.Add(basePosition + Vector3.up * (level3Height + 1f));
    }

    void CreatePlatforms()
    {
        // Additional floating platforms at various heights
        for (int i = 0; i < numberOfPlatforms; i++)
        {
            Vector3 position = GetRandomFreePosition(10f, 10f);
            if (position != Vector3.zero)
            {
                float height = Random.Range(3f, 8f);
                float width = Random.Range(6f, 12f);
                float length = Random.Range(6f, 12f);

                CreateElevatedPlatform(position, width, length, height);
            }
        }
    }

    void CreateElevatedPlatform(Vector3 basePosition, float width, float length, float height)
    {
        // Main platform
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = "ElevatedPlatform";
        platform.transform.position = basePosition + Vector3.up * height;
        platform.transform.localScale = new Vector3(width, 0.5f, length);
        platform.isStatic = true;
        platform.GetComponent<Renderer>().material = CreateMaterial(platformColor);

        // Support structure
        GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
        support.name = "PlatformSupport";
        support.transform.position = basePosition + Vector3.up * (height / 2f);
        support.transform.localScale = new Vector3(width * 0.3f, height, length * 0.3f);
        support.isStatic = true;
        support.GetComponent<Renderer>().material = CreateMaterial(obstacleColor);

        occupiedAreas.Add(new Bounds(basePosition + Vector3.up * (height / 2), new Vector3(width, height, length)));

        spawnPositions.Add(basePosition + Vector3.up * (height + 1f));
    }

    void CreateBridges()
    {
        // Bridges connecting different areas
        float bridgeHeight = 6f;

        // Bridge across X axis
        CreateBridge(new Vector3(-arenaWidth * 0.25f, bridgeHeight, 0), new Vector3(arenaWidth * 0.25f, bridgeHeight, 0), 4f);

        // Diagonal bridges
        CreateBridge(new Vector3(-arenaWidth * 0.2f, bridgeHeight + 2, -arenaLength * 0.2f),
                     new Vector3(arenaWidth * 0.2f, bridgeHeight + 2, arenaLength * 0.2f), 3f);

        // Additional random bridges
        for (int i = 0; i < numberOfBridges - 2; i++)
        {
            Vector3 start = GetRandomFreePosition(5f, 5f);
            if (start != Vector3.zero)
            {
                Vector3 end = start + new Vector3(Random.Range(-20f, 20f), 0, Random.Range(-20f, 20f));
                end.x = Mathf.Clamp(end.x, -arenaWidth * 0.4f, arenaWidth * 0.4f);
                end.z = Mathf.Clamp(end.z, -arenaLength * 0.4f, arenaLength * 0.4f);

                float h = Random.Range(4f, 8f);
                CreateBridge(start + Vector3.up * h, end + Vector3.up * h, 3f);
            }
        }
    }

    void CreateBridge(Vector3 start, Vector3 end, float width)
    {
        Vector3 center = (start + end) / 2f;
        float length = Vector3.Distance(start, end);

        GameObject bridge = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bridge.name = "Bridge";
        bridge.transform.position = center;
        bridge.transform.localScale = new Vector3(width, 0.4f, length);
        bridge.transform.LookAt(end);
        bridge.isStatic = true;
        bridge.GetComponent<Renderer>().material = CreateMaterial(platformColor);

        // Railings
        CreateBridgeRailing(center, length, width, bridge.transform.rotation);

        // Supports
        CreateBridgeSupport(start);
        CreateBridgeSupport(end);

        spawnPositions.Add(center + Vector3.up * 1f);
    }

    void CreateBridgeRailing(Vector3 center, float length, float width, Quaternion rotation)
    {
        Material railMat = CreateMaterial(obstacleColor);

        for (int side = -1; side <= 1; side += 2)
        {
            GameObject rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rail.name = "BridgeRail";
            rail.transform.position = center + rotation * new Vector3(side * (width / 2f - 0.1f), 0.5f, 0);
            rail.transform.localScale = new Vector3(0.2f, 1f, length);
            rail.transform.rotation = rotation;
            rail.isStatic = true;
            rail.GetComponent<Renderer>().material = railMat;
        }
    }

    void CreateBridgeSupport(Vector3 position)
    {
        float height = position.y;

        GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
        support.name = "BridgeSupport";
        support.transform.position = new Vector3(position.x, height / 2f, position.z);
        support.transform.localScale = new Vector3(1.5f, height, 1.5f);
        support.isStatic = true;
        support.GetComponent<Renderer>().material = CreateMaterial(obstacleColor);
    }

    void CreatePillars()
    {
        for (int i = 0; i < numberOfPillars; i++)
        {
            Vector3 position = GetRandomFreePosition(3f, 3f);
            if (position != Vector3.zero)
            {
                CreatePillar(position);
            }
        }
    }

    void CreatePillar(Vector3 basePosition)
    {
        float height = Random.Range(4f, 10f);
        float radius = Random.Range(1f, 2.5f);

        GameObject pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pillar.name = "Pillar";
        pillar.transform.position = basePosition + Vector3.up * (height / 2f);
        pillar.transform.localScale = new Vector3(radius * 2, height / 2f, radius * 2);
        pillar.isStatic = true;
        pillar.GetComponent<Renderer>().material = CreateMaterial(obstacleColor);

        occupiedAreas.Add(new Bounds(pillar.transform.position, new Vector3(radius * 2, height, radius * 2)));
    }

    void CreateRamps()
    {
        for (int i = 0; i < numberOfRamps; i++)
        {
            Vector3 position = GetRandomFreePosition(8f, 12f);
            if (position != Vector3.zero)
            {
                CreateStandaloneRamp(position, Random.Range(0, 4) * 90f);
            }
        }
    }

    void CreateStandaloneRamp(Vector3 basePosition, float rotation)
    {
        float length = Random.Range(8f, 14f);
        float width = Random.Range(4f, 6f);
        float height = Random.Range(3f, 6f);

        GameObject ramp = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ramp.name = "Ramp";

        Vector3 rampCenter = basePosition + Quaternion.Euler(0, rotation, 0) * new Vector3(0, height / 2f, length / 4f);
        ramp.transform.position = rampCenter;
        ramp.transform.localScale = new Vector3(width, 0.4f, length);

        float angle = Mathf.Atan2(height, length) * Mathf.Rad2Deg;
        ramp.transform.rotation = Quaternion.Euler(-angle, rotation, 0);
        ramp.isStatic = true;
        ramp.GetComponent<Renderer>().material = CreateMaterial(platformColor);

        // Platform at top of ramp
        Vector3 topPos = basePosition + Quaternion.Euler(0, rotation, 0) * new Vector3(0, height, length / 2f);
        GameObject topPlatform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topPlatform.name = "RampTop";
        topPlatform.transform.position = topPos;
        topPlatform.transform.localScale = new Vector3(width, 0.5f, width);
        topPlatform.transform.rotation = Quaternion.Euler(0, rotation, 0);
        topPlatform.isStatic = true;
        topPlatform.GetComponent<Renderer>().material = CreateMaterial(platformColor);

        occupiedAreas.Add(new Bounds(basePosition + Vector3.up * (height / 2), new Vector3(width + 2, height + 2, length + 2)));

        spawnPositions.Add(topPos + Vector3.up * 1f);
    }

    void CreateCoverWalls()
    {
        // Low cover walls scattered around
        int numWalls = 10;

        for (int i = 0; i < numWalls; i++)
        {
            Vector3 position = GetRandomFreePosition(4f, 2f);
            if (position != Vector3.zero)
            {
                CreateCoverWall(position, Random.Range(0f, 180f));
            }
        }
    }

    void CreateCoverWall(Vector3 position, float rotation)
    {
        float height = Random.Range(1.5f, 2.5f);
        float width = Random.Range(4f, 8f);

        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "CoverWall";
        wall.transform.position = position + Vector3.up * (height / 2f);
        wall.transform.localScale = new Vector3(width, height, 0.5f);
        wall.transform.rotation = Quaternion.Euler(0, rotation, 0);
        wall.isStatic = true;
        wall.GetComponent<Renderer>().material = CreateMaterial(obstacleColor);

        occupiedAreas.Add(new Bounds(wall.transform.position, new Vector3(width, height, 2f)));
    }

    void CreateSpawnPoints()
    {
        // Corner spawns
        float offset = 8f;
        float halfWidth = arenaWidth / 2f - offset;
        float halfLength = arenaLength / 2f - offset;

        spawnPositions.Add(new Vector3(halfWidth, 1f, halfLength));
        spawnPositions.Add(new Vector3(-halfWidth, 1f, halfLength));
        spawnPositions.Add(new Vector3(halfWidth, 1f, -halfLength));
        spawnPositions.Add(new Vector3(-halfWidth, 1f, -halfLength));

        // Edge spawns
        spawnPositions.Add(new Vector3(0, 1f, halfLength));
        spawnPositions.Add(new Vector3(0, 1f, -halfLength));
        spawnPositions.Add(new Vector3(halfWidth, 1f, 0));
        spawnPositions.Add(new Vector3(-halfWidth, 1f, 0));

        // Additional random spawns
        int additionalSpawns = numberOfSpawnPoints - spawnPositions.Count;
        for (int i = 0; i < additionalSpawns; i++)
        {
            Vector3 pos = GetRandomFreePosition(3f, 3f);
            if (pos != Vector3.zero)
            {
                spawnPositions.Add(pos + Vector3.up);
            }
        }

        // Register spawn points
        SpawnManager spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
        {
            foreach (Vector3 pos in spawnPositions)
            {
                spawnManager.AddSpawnPoint(pos);
            }
        }
    }

    void CreateLighting()
    {
        // Main directional light
        GameObject lightObj = new GameObject("DirectionalLight");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(0.9f, 0.95f, 1f);
        light.intensity = 1.2f;
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // Ambient lights
        Color ambientColor = new Color(0.2f, 0.4f, 0.6f);
        float lightSpacing = arenaWidth / 3f;

        for (float x = -lightSpacing; x <= lightSpacing; x += lightSpacing)
        {
            for (float z = -lightSpacing; z <= lightSpacing; z += lightSpacing)
            {
                CreatePointLight(new Vector3(x, 12f, z), ambientColor, 25f, 0.4f);
            }
        }

        // Accent lights at towers
        float offset = arenaWidth * 0.35f;
        CreatePointLight(new Vector3(offset, 15f, offset), accentColor, 20f, 0.6f);
        CreatePointLight(new Vector3(-offset, 15f, offset), accentColor, 20f, 0.6f);
        CreatePointLight(new Vector3(offset, 15f, -offset), accentColor, 20f, 0.6f);
        CreatePointLight(new Vector3(-offset, 15f, -offset), accentColor, 20f, 0.6f);
    }

    void CreatePointLight(Vector3 position, Color color, float range, float intensity)
    {
        GameObject lightObj = new GameObject("PointLight");
        lightObj.transform.position = position;
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.range = range;
        light.intensity = intensity;
    }

    Vector3 GetRandomFreePosition(float clearanceX, float clearanceZ)
    {
        float halfWidth = arenaWidth / 2f - clearanceX - 5f;
        float halfLength = arenaLength / 2f - clearanceZ - 5f;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector3 testPos = new Vector3(
                Random.Range(-halfWidth, halfWidth),
                0f,
                Random.Range(-halfLength, halfLength)
            );

            bool isFree = true;
            Bounds testBounds = new Bounds(testPos, new Vector3(clearanceX * 2, 10f, clearanceZ * 2));

            foreach (Bounds occupied in occupiedAreas)
            {
                if (testBounds.Intersects(occupied))
                {
                    isFree = false;
                    break;
                }
            }

            // Keep center area more open
            if (testPos.magnitude < 15f)
            {
                isFree = false;
            }

            if (isFree)
            {
                return testPos;
            }
        }

        return Vector3.zero;
    }

    Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (shader == null)
            shader = Shader.Find("Standard");

        Material mat = new Material(shader);
        mat.SetColor("_BaseColor", color);
        mat.color = color;
        return mat;
    }
}
