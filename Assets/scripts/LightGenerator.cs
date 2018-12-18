using UnityEngine;
using System.Collections;

public class LightGenerator : MonoBehaviour {

    public Light light;
    public int diameter;
    private Mesh mesh;
    private MeshRenderer meshRenderer;


    private int[,,] field;

    void Start() {
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
        field = new int[diameter, diameter, diameter];

        Vector3Int center = new Vector3Int(diameter / 2, diameter / 2, diameter / 2);

        for (int z = 1; z < diameter - 1; z++) {
            for (int y = 1; y < diameter - 1; y++) {
                for (int x = 1; x < diameter - 1; x++) {
                    if (stepDistance(center, x, y, z) <= diameter / 2) field[x, y, z] = 1;
                }
            }
        }

                generateLight();
    }

    private int stepDistance(Vector3Int center, int x, int y, int z) {
        return Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y) + Mathf.Abs(z - center.z);
    }

    public void generateLight() {
        StartCoroutine(MeshGenerator.updateMesh(field, mesh, 0, true));
    }

    /*
    private void OnDrawGizmos() {
        if (field == null) return;
        for(int z = 0; z < field.GetLength(2); z++) {
            for(int y = 0; y < field.GetLength(1); y++) {
                for(int x = 0; x < field.GetLength(0); x++) {
                    if(field[x, y, z] == 0) {
                        Gizmos.color = Color.white;
                        //Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                    }
                    if (field[x, y, z] == 1) {
                        Gizmos.color = Color.black;
                        Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                    }
                }

            }
        }
    }
    */


}
