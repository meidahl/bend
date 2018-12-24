using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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

        Animation anim = GetComponent<Animation>();


        SkinnedMeshRenderer rend = GetComponent<SkinnedMeshRenderer>();
        Mesh mesh = new Mesh();

        // Generate Mesh
        MeshGenerator.MeshData meshData = MeshGenerator.generateMesh(field, 0, meshInverted);
        mesh.vertices = meshData.vertices;
        mesh.triangles = meshData.triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();

        // assign bone weights to mesh
        BoneWeight[] weights = new BoneWeight[4];
        weights[0].boneIndex0 = 0;
        weights[0].weight0 = 1;
        weights[1].boneIndex0 = 0;
        weights[1].weight0 = 1;
        weights[2].boneIndex0 = 1;
        weights[2].weight0 = 1;
        weights[3].boneIndex0 = 1;
        weights[3].weight0 = 1;

        BoneWeight[] boneWeights = new BoneWeight[mesh.vertices.Length];
        for(int i = 0; i < boneWeights.Length; i++) {
            if(i < boneWeights.Length / 2) {
                boneWeights[i].boneIndex0 = 0;
            } else {
                boneWeights[i].boneIndex0 = 1;
            }
            boneWeights[i].weight0 = 1;
        }
        mesh.boneWeights = boneWeights;

        // Create Bone Transforms and Bind poses
        // One bone at the bottom and one at the top

        Transform[] bones = new Transform[2];
        Matrix4x4[] bindPoses = new Matrix4x4[2];
        bones[0] = new GameObject("Lower").transform;
        bones[0].parent = transform;
        // Set the position relative to the parent
        bones[0].localRotation = Quaternion.identity;
        bones[0].localPosition = Vector3.zero;
        // The bind pose is bone's inverse transformation matrix
        // In this case the matrix we also make this matrix relative to the root
        // So that we can move the root game object around freely
        bindPoses[0] = bones[0].worldToLocalMatrix * transform.localToWorldMatrix;


        bones[1] = new GameObject("Upper").transform;
        bones[1].parent = transform;
        // Set the position relative to the parent
        bones[1].localRotation = Quaternion.identity;
        bones[1].localPosition = new Vector3(0, 5, 0);
        // The bind pose is bone's inverse transformation matrix
        // In this case the matrix we also make this matrix relative to the root
        // So that we can move the root game object around freely
        bindPoses[1] = bones[1].worldToLocalMatrix * transform.localToWorldMatrix;

        // bindPoses was created earlier and was updated with the required matrix.
        // The bindPoses array will now be assigned to the bindposes in the Mesh.
        mesh.bindposes = bindPoses;
            
        // Assign bones and bind poses
        rend.bones = bones;
        rend.sharedMesh = mesh;

        // Assign a simple waving animation to the bottom bone
        AnimationCurve curve = new AnimationCurve();
        curve.keys = new Keyframe[] { new Keyframe(0, 0, 0, 0), new Keyframe(1, 3, 0, 0), new Keyframe(2, 0.0F, 0, 0) };

        // Create the clip with the curve
        AnimationClip clip = new AnimationClip();
        clip.SetCurve("Lower", typeof(Transform), "m_LocalPosition.z", curve);
        clip.legacy = true;

        rend.sharedMesh = mesh;

        // Add and play the clip
        clip.wrapMode = WrapMode.Loop;
        anim.AddClip(clip, "test");
        anim.Play("test");

    //StartCoroutine(MeshGenerator.updateMesh(field, renderer.sharedMesh, 0, meshInverted));

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
                    if (stepDistance(center, x, y, z) <= headDiameter / 2) head[x, y, z] = 1;
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
                    arm[x, y, z] = 1;
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
                    leg[x, y, z] = 1;
                }
            }
        }
        return leg;
    }


    private int stepDistance(Vector3Int center, int x, int y, int z) {
        return Mathf.Abs(x - center.x) + Mathf.Abs(y - center.y) + Mathf.Abs(z - center.z);
    }


    private void insertArray(int[,,] field, int[,,] part, Vector3Int pos) {
        for (int z = pos.z; z < pos.z + part.GetLength(2) && z < field.GetLength(2); z++) {
            for (int y = pos.y; y < pos.y + part.GetLength(1) && y < field.GetLength(1); y++) {
                for (int x = pos.x; x < pos.x + part.GetLength(0) && x < field.GetLength(0); x++) {
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
