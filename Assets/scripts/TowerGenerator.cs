using UnityEngine;
using System.Collections;

public class TowerGenerator : MonoBehaviour {

    public Vector3Int mapSize;
    public Material mat;

    private int[,,] map;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private MeshCollider meshCollider;

    void Start() {
        mesh =  gameObject.AddComponent<MeshFilter>().mesh;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = mat;

        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;

        map = new int[mapSize.x, mapSize.y, mapSize.z];
        for (int z = 0; z < mapSize.z; z++) {
            for (int y = 0; y < mapSize.y; y++) {
                for (int x = 0; x < mapSize.x; x++) {
                    if (y <= 5) map[x, y, z] = 1;
                }
            }
        }

        StartCoroutine(MeshGenerator.updateMeshWithCollider(map, mesh, meshCollider, 0, true));
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            morph();
        }
    }

    private void morph() {
        for(int z = 0; z < mapSize.z; z++) {
            for(int y = 5; y < mapSize.y; y++) {
                for(int x = 0; x < mapSize.x; x++) {
                    if(map[x, y - 1, z] == 1) {
                        if (Random.Range(0, 100) < 10) map[x, y, z] = 1;
                    }
                }
            }
        }

        StartCoroutine(MeshGenerator.updateMeshWithCollider(map, mesh, meshCollider, 0, true));
    }

}
