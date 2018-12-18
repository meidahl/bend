using UnityEngine;
using System.Collections;

public class CharacterGenerator : MonoBehaviour {

    public int headDiameter;

    public Vector3Int bodySize;
    //public Vector3Int bodyPos;

    public Vector3Int armSize;
    //public Vector3Int armPos;

    public Vector3Int legSize;
    //public Vector3Int legPos;

    public bool meshInverted;

    private int[,,] field;

    void Start() {

        /*
        * 1. Generate Head, Body, Arm, Leg
        * 2. Add them into the field together
        * 3. Connect the regions
        * 4. Generate colliders
        * 5. Generate mesh from the field
        * 
        * 
        */

        field = new int[bodySize.x + 2 * armSize.x + 2, bodySize.y + headDiameter + legSize.y + 2, headDiameter + 2];
        Debug.Log("Field Size: " + field.GetLength(0) + ", " + field.GetLength(1) + ", " + field.GetLength(2));

        Vector3Int bodyPos = new Vector3Int(armSize.x + 1, legSize.y + 1, field.GetLength(2) / 2);

        Vector3Int headPos = new Vector3Int(bodyPos.x, bodyPos.y + bodySize.y, 1);

        Vector3Int rightArmPos = new Vector3Int(bodyPos.x + bodySize.x, bodyPos.y + bodySize.y - armSize.y, bodyPos.z);
        Vector3Int leftArmPos = new Vector3Int(bodyPos.x - armSize.x, bodyPos.y + bodySize.y - armSize.y, bodyPos.z);

        Vector3Int rightLegPos = new Vector3Int(bodyPos.x + bodySize.x - legSize.x, bodyPos.y - legSize.y, bodyPos.z);
        Vector3Int leftLegPos = new Vector3Int(bodyPos.x, bodyPos.y - legSize.y, bodyPos.z);

        insertArray(field, generateBody(), bodyPos);

        insertArray(field, generateHead(), headPos);

        insertArray(field, generateArm(), rightArmPos);
        insertArray(field, generateArm(), leftArmPos);

        insertArray(field, generateLeg(), rightLegPos);
        insertArray(field, generateLeg(), leftLegPos);

        //TODO: ADD COLLIDERS

        StartCoroutine(MeshGenerator.updateMesh(field, GetComponent<MeshFilter>().mesh, 0, meshInverted));

    }


    private int[,,] generateBody() {
        int[,,] body = new int[bodySize.x, bodySize.y, bodySize.z];
        for (int z = 0; z < body.GetLength(2); z++) {
            for (int y = 0; y < body.GetLength(1); y++) {
                for (int x = 0; x < body.GetLength(0); x++) {
                    body[x, y, z] = 1;
                }
            }
        }
        return body;
    }

    private int[,,] generateHead() {
        int[,,] head = new int[headDiameter, headDiameter, headDiameter];

        Vector3Int center = new Vector3Int(headDiameter / 2, headDiameter / 2, headDiameter / 2);

        for (int z = 0; z < head.GetLength(2); z++) {
            for (int y = 0; y < head.GetLength(1); y++) {
                for (int x = 0; x < head.GetLength(0); x++) {
                    if (stepDistance(center, x, y, z) <= headDiameter / 2) head[x, y, z] = 2;
                }
            }
        }
        return head;
    }

    private int[,,] generateArm() {
        int[,,] arm = new int[armSize.x, armSize.y, armSize.z];
        for (int z = 0; z < arm.GetLength(2); z++) {
            for (int y = 0; y < arm.GetLength(1); y++) {
                for (int x = 0; x < arm.GetLength(0); x++) {
                    arm[x, y, z] = 3;
                }
            }
        }
        return arm;
    }

    private int[,,] generateLeg() {
        int[,,] leg = new int[legSize.x, legSize.y, legSize.z];
        for (int z = 0; z < leg.GetLength(2); z++) {
            for (int y = 0; y < leg.GetLength(1); y++) {
                for (int x = 0; x < leg.GetLength(0); x++) {
                    leg[x, y, z] = 4;
                }
            }
        }
        return leg;
    }


    private int stepDistance(Vector3Int center, int x, int y, int z) {
        return Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y) + Mathf.Abs(z - center.z);
    }


    private void insertArray(int[,,] field, int[,,] part, Vector3Int pos) {
        Debug.Log("Part Size: " + part.GetLength(0) + ", " + part.GetLength(1) + ", " + part.GetLength(2));

        for (int z = pos.z; z < pos.z + part.GetLength(2) && z < field.GetLength(2); z++) {
            for (int y = pos.y; y < pos.y + part.GetLength(1) && y < field.GetLength(1); y++) {
                for (int x = pos.x; x < pos.x + part.GetLength(0) && x < field.GetLength(0); x++) {
                    Debug.Log(( x - pos.x) + ", " + (y - pos.y) + ", " + (z - pos.z));
                    field[x, y, z] = part[x - pos.x, y - pos.y, z - pos.z];
                }
            }
        }
    }

    /*
    void OnDrawGizmos() {
        if (field != null) {
            for (int z = 0; z < field.GetLength(2); z++) {
                for (int y = 0; y < field.GetLength(1); y++) {
                    for (int x = 0; x < field.GetLength(0); x++) {
                        if (field[x, y, z] == 1) {
                            Gizmos.color = Color.black;
                            //Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                            Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                        }
                        if (field[x, y, z] == 2) {
                            Gizmos.color = Color.blue;
                            //Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                            Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                        }
                        if (field[x, y, z] == 3) {
                            Gizmos.color = Color.red;
                            //Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                            Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                        }
                        if (field[x, y, z] == 4) {
                            Gizmos.color = Color.green;
                            //Gizmos.DrawCube(new Vector3(x, y, z), Vector3.one);
                            Gizmos.DrawSphere(new Vector3(x, y, z), .25f);
                        }

                    }
                }
            }
        }
    }
    */
    

}
