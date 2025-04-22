using UnityEngine;

[ExecuteInEditMode]
public class TileCornerGizmo : MonoBehaviour
{
    [Header("Corner heights (any integer levels)")]
    [Tooltip("Top-Left corner height")]
    public float topLeft = 1f;
    [Tooltip("Top-Right corner height")]
    public float topRight = 1f;
    [Tooltip("Bottom-Right corner height")]
    public float bottomRight = 1f;
    [Tooltip("Bottom-Left corner height")]
    public float bottomLeft = 1f;

    [Header("Gizmo settings")]
    [Tooltip("Radius of the corner indicator spheres")]
    public float gizmoRadius = 0.1f;

    void OnDrawGizmosSelected()
    {
        // Base position of tile (assumes tile spans 1 unit in X and Z)
        Vector3 center = transform.position;

        // Calculate local corner offsets
        Vector3 tlPos = center + new Vector3(-0.5f, topLeft, 0.5f);
        Vector3 trPos = center + new Vector3(0.5f, topRight, 0.5f);
        Vector3 brPos = center + new Vector3(0.5f, bottomRight, -0.5f);
        Vector3 blPos = center + new Vector3(-0.5f, bottomLeft, -0.5f);

        // Draw spheres at each corner
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(tlPos, gizmoRadius);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(trPos, gizmoRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(brPos, gizmoRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(blPos, gizmoRadius);

        // Draw lines between corners to outline the tile
        Gizmos.color = Color.white;
        Gizmos.DrawLine(tlPos, trPos);
        Gizmos.DrawLine(trPos, brPos);
        Gizmos.DrawLine(brPos, blPos);
        Gizmos.DrawLine(blPos, tlPos);
    }
}
