using System.Collections;
using UnityEngine;

public class Wire : MonoBehaviour
{
    public Transform target;
    private Transform replacement;
    private Transform parental;
    public float maxDistance;
    public float thickness;
    private Vector3 start;
    private Vector3 right;
    private Vector3 front;
    private Vector3 back;
    private Rigidbody targetRig;
    private bool retract;

    //Mesh Data
    public MeshFilter render;
    private Mesh mesh;
    private Vector3[] Verts;
    private Vector2[] Uvs;
    private readonly int[] Tris = new int[] { 0, 4, 1, 4, 5, 1, 1, 5, 2, 5, 6, 2, 2, 6, 3, 6, 7, 3, 3, 7, 0, 7, 4, 0, 0, 1, 3, 1, 2, 3, 4, 7, 5, 7, 6, 5 };

    private void Start()
    {
        target.localEulerAngles = Vector3.zero;
        target.localScale = Vector3.one * 0.001f;
        parental = transform.parent;
        start = transform.position;
        targetRig = target.GetComponent<Rigidbody>();
        retract = false;
        right = target.right * thickness;
        front = target.up * 0.09f;

        //Preset only if non-inherited mesh
        if (mesh == null) {
            back = target.up * -0.02f;
            mesh = new Mesh();
            Verts = new Vector3[8];
            Uvs = new Vector2[8];
            Vertex();
            Vertexes();
            GenerateMesh();
        } else {
            StartCoroutine(FrameDelay());
        }

        back = target.up * -0.09f;
    }

    private void FixedUpdate()
    {
        //Too far away
        if (Vector3.Distance(start, target.position) > maxDistance) {
            target.position = start;
            GetComponents();
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

    //Destroys and replaces the wire grab so the player lets go
    private void GetComponents()
    {
        replacement = Instantiate(target, start, Quaternion.identity);
        targetRig = replacement.GetComponent<Rigidbody>();
        replacement.name = target.name;
        replacement.parent = parental;

        //Transfer mesh
        Vertex();
        Vertexes();
        GenerateMesh();
        replacement.GetComponent<Wire>().GetMeshData(Verts, Uvs, mesh);

        Destroy(target.gameObject);
        target = replacement;
        replacement = null;
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

    //Return to goal
    public void Freeze(Vector3 point)
    {
        target.transform.position = point;
        GetComponents();
        ResetPhysics();

        //Disable collider for grabbing and update the puzzles 
        GetComponent<BoxCollider>().enabled = false;
        GetComponent<Puzzle>().completed = true;
        target.transform.position = point;
        target.transform.rotation = Quaternion.identity;
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
        parental.GetComponent<PuzzleSystem>().Restore(GetComponent<Puzzle>());
    }
}