using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Use cellular atomata to generate a map 
public class MapGenerator : MonoBehaviour {

    /* Things to think about
     * - minimum height of a cavern
     * - minimum height of a tunnel
     * - tunnel connectedness
     * - mesh smoothing
     * - beginning, key, item, end rooms
     * - build mesh in seperate chunks, only update that chunk at a time
     * - different isovalues for corners after linear interpolation is implemented for mesh generation
     * - improve tunneling alg to ensure connectiveness of horseshoe regions
     */

    public Vector3Int mapSize;
    public int smoothingPasses;
    public int minimumRegionVolume;
    public bool invertedMesh;

    public int seed;
    public bool useRandomSeed;

    [Range(0, 100)]
    public int randomFillPercent;

    public LightGenerator lightGenerator;

    private int fillValue = 1;
    private int emptyValue = 0;

    private int[,,] map;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private MeshCollider meshCollider;

    private struct Region {
        public List<Vector3Int> spaces;
        public int value;
        public bool connected;

        // This may not return a value inside the region if it's similar to a horseshoe shape
        public Vector3Int findCenter() {
            Vector3 center = new Vector3Int();
            foreach (Vector3Int space in spaces) {
                center += space;
            }
            center /= spaces.Count;
            return new Vector3Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y), Mathf.RoundToInt(center.z));
        }

        public void setConnected(bool connected) {
            this.connected = connected;
        }

    }


    // Use this for initialization
    void Start() {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        if (useRandomSeed) seed = Time.time.GetHashCode();
        Random.InitState(seed);

        map = new int[mapSize.x, mapSize.y, mapSize.z];
        generateMap(smoothingPasses);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.G)) {
            generateMap(smoothingPasses);
        }
        if (Input.GetKeyDown(KeyCode.M)) {
            morphMap();
        }
    }

    public void generateMap(int smoothingPasses) {
        initMap();
        for(int i = 0; i < smoothingPasses; i++) smoothMap();
        List<Region> regions = findRegions();

        int nRegionsRemoved = 0;
        while (removeSmallRegions(regions) != 0) {
            nRegionsRemoved++;
            regions = findRegions();
        }

        addLights(regions);

        connectRegions(regions);

        Debug.Log("Regions: " + regions.Count + "  Pruned: " + nRegionsRemoved);
        Debug.Log("Regions After Connect: " + findRegions().Count);

        StartCoroutine(MeshGenerator.updateMeshWithCollider(map, mesh, meshCollider, 0, invertedMesh));
    }

    public int initCell(int x, int y, int z) {

        // Make Edges Solid
        if (x == 0  || z == 0 || x == mapSize.x - 1 || z == mapSize.z - 1 || y == 0 || y == mapSize.y - 1)
            return 1;
        
        if (Random.Range(0, 100) < randomFillPercent) return fillValue;
        return emptyValue;
    }

    public int smoothCell(int x, int y, int z) {

        int solids = 0;

        if (map[x, y, z] == 1) solids++;

        if (x - 1 < 0) solids++;
        else solids += map[x - 1, y, z];

        if (x + 1 > mapSize.x - 1) solids++;
        else solids += map[x + 1, y, z];

        
        if (y - 1 < 0) solids++;
        else solids += map[x, y - 1, z];

        if (y + 1 > mapSize.y - 1) solids++;
        else solids += map[x, y + 1, z];
           

        if (z - 1 < 0) solids++;
        else solids += map[x, y, z - 1];

        if (z + 1 > mapSize.z - 1) solids++;
        else solids += map[x, y, z + 1];

        if (solids >= 4) return fillValue;
        else return emptyValue;
        
    }

    public int morphCell(int x, int y, int z) {
        return map[x, y, z];
    }

    public void initMap() {
        for (int z = 0; z < mapSize.z; z++) {
            for (int y = 0; y < mapSize.y; y++) {
                for (int x = 0; x < mapSize.x; x++) {
                    map[x, y, z] = initCell(x, y, z);
                }
            }
        }
    }

    public void smoothMap() {
        for (int z = 0; z < mapSize.z; z++) {
            for (int y = 0; y < mapSize.y; y++) {
                for (int x = 0; x < mapSize.x; x++) {
                    map[x, y, z] = smoothCell(x, y, z);
                }
            }
        }
    }

    public void morphMap() {
        for (int z = 0; z < mapSize.z; z++) {
            for (int y = 0; y < mapSize.y; y++) {
                for (int x = 0; x < mapSize.x; x++) {
                    map[x, y, z] = morphCell(x, y, z);
                }
            }
        }
    }

    private List<Region> findRegions() {
        List<Region> regions = new List<Region>();

        int[,,] assigned = new int[map.GetLength(0), map.GetLength(1), map.GetLength(2)];

        for (int z = 0; z < mapSize.z; z++) {
            for (int y = 0; y < mapSize.y; y++) {
                for (int x = 0; x < mapSize.x; x++) {
                    if (assigned[x, y, z] > 0) continue;
                    Region region;
                    region.value = map[x, y, z];
                    region.spaces = new List<Vector3Int>();

                    Queue<Vector3Int> q = new Queue<Vector3Int>();
                    q.Enqueue(new Vector3Int(x, y, z));
                    region.spaces.Add(new Vector3Int(x, y, z));
                    assigned[x, y, z] = 1;
                    while (q.Count > 0) {
                        Vector3Int node = q.Dequeue();
                        Vector3Int up = new Vector3Int(node.x, node.y + 1, node.z);
                        Vector3Int down = new Vector3Int(node.x, node.y - 1, node.z);
                        Vector3Int right = new Vector3Int(node.x + 1, node.y, node.z);
                        Vector3Int left = new Vector3Int(node.x - 1, node.y, node.z);
                        Vector3Int forward = new Vector3Int(node.x, node.y, node.z + 1);
                        Vector3Int backward = new Vector3Int(node.x, node.y, node.z - 1);

                        if (withinBounds(up) && getMapValue(up) == region.value && assigned[up.x, up.y, up.z] == 0) {
                            assigned[up.x, up.y, up.z] = 1;
                            region.spaces.Add(up);
                            q.Enqueue(up);
                        }
                        if (withinBounds(down) && getMapValue(down) == region.value && assigned[down.x, down.y, down.z] == 0) {
                            assigned[down.x, down.y, down.z] = 1;
                            region.spaces.Add(down);
                            q.Enqueue(down);
                        }
                        if (withinBounds(right) && getMapValue(right) == region.value && assigned[right.x, right.y, right.z] == 0) {
                            assigned[right.x, right.y, right.z] = 1;
                            region.spaces.Add(right);
                            q.Enqueue(right);
                        }
                        if (withinBounds(left) && getMapValue(left) == region.value && assigned[left.x, left.y, left.z] == 0) {
                            assigned[left.x, left.y, left.z] = 1;
                            region.spaces.Add(left);
                            q.Enqueue(left);
                        }
                        if (withinBounds(forward) && getMapValue(forward) == region.value && assigned[forward.x, forward.y, forward.z] == 0) {
                            assigned[forward.x, forward.y, forward.z] = 1;
                            region.spaces.Add(forward);
                            q.Enqueue(forward);
                        }
                        if (withinBounds(backward) && getMapValue(backward) == region.value && assigned[backward.x, backward.y, backward.z] == 0) {
                            assigned[backward.x, backward.y, backward.z] = 1;
                            region.spaces.Add(backward);
                            q.Enqueue(backward);
                        }
                    }
                    region.connected = false;
                    regions.Add(region);
                }
            }
        }

        return regions;
    }

    // Remove regions that are too small
    private int removeSmallRegions(List<Region> regions) {

        int regionsRemoved = 0;

        foreach (Region region in regions) {
            if (region.spaces.Count < minimumRegionVolume) {
                regionsRemoved++;
                foreach (Vector3Int pos in region.spaces) {
                    if (region.value == emptyValue) setMapValue(pos, fillValue);
                    else if (region.value == fillValue) setMapValue(pos, emptyValue);
                    else {
                        Debug.Log("Non Empty, Non Fill Region is too small! Region Value: " + region.value);
                        setMapValue(pos, -1);
                    }
                }
            }
        }

        return regionsRemoved;
    }

    private void connectRegions(List<Region> regions) {

        List<Region> emptyRegions = new List<Region>();

        foreach (Region r in regions) {
            if (r.value == emptyValue) emptyRegions.Add(r);
        }

        for(int i = 0; i < emptyRegions.Count; i++) {
            if(!emptyRegions[i].connected) {
                int index = -1;
                float closestDistance = float.MaxValue;
                Vector3 center = emptyRegions[i].findCenter();
                for (int j = i + 1; j < emptyRegions.Count; j++) {
                    if (emptyRegions[j].connected) continue;
                    float d = Vector3.Distance(center, emptyRegions[j].findCenter());
                    if (d < closestDistance) index = j;

                }
                if(index != -1) {
                    emptyRegions[i].setConnected(true);
                    emptyRegions[index].setConnected(true);
                    buildTunnel(emptyRegions[i], emptyRegions[index]);
                }
            }
        }

    }

    private void buildTunnel(Region startRegion, Region endRegion) {
        Vector3 startCenter = startRegion.findCenter();
        Vector3 endCenter = endRegion.findCenter();
        Vector3Int start = new Vector3Int(Mathf.RoundToInt(startCenter.x), Mathf.RoundToInt(startCenter.y), Mathf.RoundToInt(startCenter.z));
        Vector3Int end = new Vector3Int(Mathf.RoundToInt(endCenter.x), Mathf.RoundToInt(endCenter.y), Mathf.RoundToInt(endCenter.z));

        Vector3Int currentPos = start;
        setMapValue(currentPos, emptyValue);

        for (int z = start.z; z != end.z;) {
            if (currentPos.z < end.z) z++;
            else z--;
            currentPos.z = z;
            setMapValue(currentPos, emptyValue);
        }

        for (int y = start.y; y != end.y;) {
            if (currentPos.y < end.y) y++;
            else y--;
            currentPos.y = y;
            setMapValue(currentPos, emptyValue);
        }

        for (int x = start.x; x != end.x;) {
            if (currentPos.x < end.x) x++;
            else x--;
            currentPos.x = x;
            setMapValue(currentPos, emptyValue);
        }

    }

    // TODO: make this breadth first search
    private Vector3Int findClosestGridPosToCenter(Vector3 center) {
        Vector3Int start = new Vector3Int(Mathf.RoundToInt(center.x), Mathf.RoundToInt(center.y), Mathf.RoundToInt(center.z));
        return start;
    }

    private void addLights(List<Region> regions) {
        foreach(Region r in regions) {
            if (r.value == emptyValue) {
                Vector3Int regionGridCenter = r.findCenter();
                Vector3 regionWorldCenter = new Vector3(
                    (regionGridCenter.x) * transform.lossyScale.x,
                    (regionGridCenter.y) * transform.lossyScale.y, 
                    (regionGridCenter.z) * transform.lossyScale.z);
                LightGenerator go = Instantiate(lightGenerator, regionWorldCenter, transform.rotation, transform);
            }
        }
    }

    private bool allConnected(List<Region> regions) {
        foreach(Region r in regions) {
            if (r.connected == false) return false;
        }
        return true;
    }

    private bool withinBounds(Vector3Int v) {
        if (v.x >= 0 && v.x < mapSize.x && v.y >= 0 && v.y < mapSize.y && v.z >= 0 && v.z < mapSize.z) return true;
        return false;
    }

    private int getMapValue(Vector3Int pos) {
        return map[pos.x, pos.y, pos.z];
    }

    private void setMapValue(Vector3Int pos, int val) {
        map[pos.x, pos.y, pos.z] = val;
    }

    public int[,,] getMap() {
        return map;
    }

    /*
    void OnDrawGizmos() {
        if (map != null) {
            for (int z = 0; z < mapSize.z; z++) {
                for (int y = 0; y < mapSize.y; y++) {
                    for (int x = 0; x < mapSize.x; x++) {
                        if (map[x, y, z] == fillValue) {
                            Gizmos.color = Color.black;
                            //Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                            //Gizmos.DrawSphere(v, .25f);
                        }
                        else {
                            Gizmos.color = Color.white;
                            //Gizmos.DrawCube(v, Vector3.one * .25f);
                            Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                        }
                    }
                }
            }
        }
    }
    */
    
  

}
