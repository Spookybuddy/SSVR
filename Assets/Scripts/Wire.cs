using System.Collections;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform target;
    private Transform replacement;
    public Transform parental;
    public float maxDistance;
    public float thickness;
    private Vector3 start;
    private Vector3 right;
    private Vector3 front;
    private Vector3 back;
    private Rigidbody targetRig;
    private bool retract;
    private bool connected;
    private readonly Vector3 scale = new Vector3(0.001f, 0.001f, 0.001f);

    //Mesh Data
    public MeshFilter render;
    private Mesh mesh;
    private Vector3[] Verts;
    private Vector2[] Uvs;
    private readonly int[] Tris = new int[] { 0, 4, 1, 4, 5, 1, 1, 5, 2, 5, 6, 2, 2, 6, 3, 6, 7, 3, 3, 7, 0, 7, 4, 0, 0, 1, 3, 1, 2, 3, 4, 7, 5, 7, 6, 5 };

    private void Start()
    {
        target.localEulerAngles = Vector3.zero;
        target.localScale = scale;
        start = target.transform.position;
        targetRig = target.GetComponent<Rigidbody>();
        retract = false;
        connected = false;
        right = target.right * thickness;
        front = target.up * 0.05f;

        back = target.up * -0.01f;
        mesh = new Mesh();
        Verts = new Vector3[8];
        Uvs = new Vector2[8];
        Vertex();
        Vertexes();
        GenerateMesh();

        back = target.up * -0.05f;
    }

    private void FixedUpdate()
    {
        if (!connected) {
            //Too far away
            if (Vector3.Distance(start, target.position) > maxDistance) {
                GetComponents(start);
                ResetPhysics();
                retract = true;
            }

            //Renable gravity once far enough away
            if (!targetRig.useGravity) {
                if (Vector3.Distance(start, target.position) > 0.1f) targetRig.useGravity = true;
            }

            //Velocity?
            if (targetRig.velocity.magnitude > 0.1f) {
                ResetPhysics();
                retract = true;
            }

            //Change from instant teleport to pull back for coolness sake
            if (retract) {
                target.transform.position = Vector3.MoveTowards(target.position, start, maxDistance * Time.deltaTime * 2);
                if (Vector3.Distance(start, target.position) < 0.1f) {
                    retract = false;
                    target.transform.position = start;
                }
            }

            //Update meshes
            if (targetRig.useGravity || retract) {
                Vertexes();
                GenerateMesh();
            }
        }
    }

    //Destroys and replaces the wire grab so the player lets go
    private void GetComponents(Vector3 spawn)
    {
        replacement = Instantiate(target, spawn, Quaternion.identity, parental);
        replacement.localScale = scale;
        targetRig = replacement.GetComponent<Rigidbody>();
        replacement.name = target.name;
        Destroy(target.gameObject);
        target = replacement;
        replacement = null;

        //Update mesh
        Vertex();
        Vertexes();
        GenerateMesh();

        StartCoroutine(FrameDelay());
    }

    //Reset physics to 0
    private void ResetPhysics()
    {
        targetRig.isKinematic = false;
        targetRig.useGravity = false;
        targetRig.velocity = Vector3.zero;
        targetRig.angularVelocity = Vector3.zero;
        target.rotation = Quaternion.identity;
    }

    //Once connected, update the puzzle system, and lock into place
    public void Freeze(Vector3 point)
    {
        target.transform.position = point;
        GetComponents(point);
        ResetPhysics();
        retract = false;
        connected = true;
        StartCoroutine(FrameDelayDone());
    }

    //Pass mesh from destroyed object
    public void GetMeshData(Vector3[] V, Vector2[] U, Mesh M)
    {
        mesh = M;
        Verts = V;
        Uvs = U;
        GenerateMesh();
    }

    //Creates a mesh using the given vertices
    private void GenerateMesh()
    {
        if (mesh == null) mesh = new Mesh();
        mesh.Clear();
        UVS();
        mesh.vertices = Verts;
        mesh.SetUVs(0, Uvs);
        mesh.triangles = Tris;
        mesh.RecalculateNormals();
        render.mesh = mesh;
    }

    //Set uvs to tile
    private void UVS()
    {
        for (int j = 0; j < 4; j++) {
            float D = Vector3.Distance(start, target.position) / 2;
            Uvs[j] = Vector2.zero;
            Uvs[j + 4] = new Vector2(D, 0);
        }
    }

    //Sets the vertices relative to start
    private void Vertex()
    {
        Vector3 up = Vector3.up * thickness;
        Verts[0] = start + up + front;
        Verts[1] = start + right + front;
        Verts[2] = start - up + front;
        Verts[3] = start - right + front;
    }

    //Sets the vertices relative to grab
    private void Vertexes()
    {
        Vector3 up = Vector3.up * thickness;
        Verts[4] = target.position + up + back;
        Verts[5] = target.position + right + back;
        Verts[6] = target.position - up + back;
        Verts[7] = target.position - right + back;
    }

    //Delayed update to puzzle manager
    private IEnumerator FrameDelay()
    {
        yield return new WaitForSeconds(Time.deltaTime);
        parental.GetComponent<PuzzleSystem>().Restore(target.GetComponent<Puzzle>());
    }

    private IEnumerator FrameDelayDone()
    {
        yield return new WaitForSeconds(Time.deltaTime * 2);
        parental.GetComponent<PuzzleSystem>().Restore(target.GetComponent<Puzzle>());
        target.GetComponent<BoxCollider>().enabled = false;
        target.GetComponent<Puzzle>().completed = true;
    }
}