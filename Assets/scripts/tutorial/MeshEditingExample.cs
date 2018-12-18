using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VertexSculpt : MonoBehaviour {

    /*
    public float radius = 1f;
    public float sculptSpeed;
    public float maxSculptDist;
    public enum FallOff { Gauss, Linear, Needle };
    public FallOff fallOff;
    public bool isEnabled;
    public bool autoEnable;

    private Vector3[] normals;
    private Vector3[] verts;
    private Ray ray;
    private Vector3 mPos;
    private float sphereRadius;
    private MeshFilter unappliedMesh;

    public void SetEnabled() {
        SetEnabled(true);
    }
    public void SetEnabled(bool enable) {
        sphereRadius = transform.GetComponent<CreatePlanet>().planetRadius;
        verts = GetComponent<MeshFilter>().mesh.vertices;
        normals = GetComponent<MeshFilter>().mesh.normals;
        isEnabled = true;
    }

    // Update is called once per frame
    void Update() {
        if (autoEnable) { SetEnabled(); autoEnable = false; }
        if (isEnabled && !PlanetController.instance.isMoving &&
            (PlanetController.instance.GetCamZoomPercent() < 1f)) {
            if (Input.GetMouseButton(0) || Input.GetMouseButton(1)) {
                // See if we're reversing the sculpt
                bool raiseLand = Input.GetMouseButton(0);

                // Raycast to see if we hit the sphere
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit)) {
                    MeshFilter mFilter = hit.collider.GetComponent<MeshFilter>();
                    if (mFilter) {
                        // Don't update collider every frame in realtime, wait till finished
                        // mesh morphing

                        if (mFilter != unappliedMesh) {
                            ApplyMeshCollider();
                            unappliedMesh = mFilter;
                        }

                        // Deform Mesh
                        float _sculptSpeed = raiseLand ? sculptSpeed : -sculptSpeed;
                        Vector3 relativePoint = mFilter.transform.InverseTransformPoint(hit.point);
                        MorphMesh(mFilter.mesh, relativePoint, _sculptSpeed, radius, raiseLand);

                    }
                }
            }

            if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) {

                ApplyMeshCollider();
                return;
                // Very likely not needed =)
                //mesh.Optimize();
            }
        }
    }

    private void MorphMesh(Mesh mesh, Vector3 position, float power, float inRadius, bool isRaising) {
        float sqrRadius = radius * radius;

        // Calculate averaged normal of all surrounding verts
        Vector3 avgNormal = Vector3.zero;

        for (int i = 0; i < verts.Length; i++) {
            float distFromCenter = Vector3.Distance(verts[i], transform.position);
            float sqrMagnitude = (verts[i] - position).sqrMagnitude;

            // Drop out if not in radius
            if (sqrMagnitude > sqrRadius) { continue; }

            // Stop if reached height
            if (isRaising)
                if (distFromCenter >= sphereRadius + maxSculptDist) { return; }

            if (!isRaising)
                if (distFromCenter <= sphereRadius - maxSculptDist) { return; }

            float distance = Mathf.Sqrt(sqrMagnitude);
            float falloff = LinearFalloff(distance, inRadius);
            avgNormal += falloff * normals[i];
        }
        avgNormal = avgNormal.normalized;

        // Deform verts along avg normal
        for (int i = 0; i < verts.Length; i++) {

            float sqrMagnitude = (verts[i] - position).sqrMagnitude;
            // Drop out if not in radius
            if (sqrMagnitude > sqrRadius) { continue; }
            float distance = Mathf.Sqrt(sqrMagnitude);
            float falloff = 0f;
            switch (fallOff) {
                case FallOff.Gauss:
                    falloff = GausFalloff(distance, inRadius);

                    break;
                case FallOff.Needle:
                    falloff = NeedleFalloff(distance, inRadius);
                    break;
                case FallOff.Linear:
                    falloff = LinearFalloff(distance, inRadius);
                    break;
                default:
                    break;
            }

            verts[i] += avgNormal * falloff * power;

            GetComponent<CreatePlanet>().SetVHeight(i);
        }
        GetComponent<CreatePlanet>().SetVertexColors();
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private float LinearFalloff(float dist, float inRadius) {
        return Mathf.Clamp01(1 - dist / inRadius);
    }

    private float GausFalloff(float dist, float inRadius) {
        return Mathf.Clamp01(Mathf.Pow(360.0f, -Mathf.Pow(dist / inRadius, 2.5f) - 0.01f));
    }

    private float NeedleFalloff(float dist, float inRadius) {
        return -(dist * dist) / (inRadius * inRadius) + 1.0f;
    }

    private void ApplyMeshCollider() {
        if (unappliedMesh && unappliedMesh.GetComponent<MeshCollider>()) {
            unappliedMesh.GetComponent<MeshCollider>().sharedMesh = unappliedMesh.mesh;
        }
        unappliedMesh = null;
    }
    */
}