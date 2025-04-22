using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class TileDatabase : MonoBehaviour
{
    [System.Serializable]
    public class TileEntry
    {
        [Tooltip("Unique name for the tile type")] public string tileName;
        [Tooltip("Assign the prefab containing a TileCornerGizmo")] public GameObject prefab;
        [HideInInspector] public List<CornerConfig> rotations = new List<CornerConfig>();
    }

    [System.Serializable]
    public class CornerConfig
    {
        public int rotationDegrees;
        public int[] corners; // TL, TR, BR, BL
    }

    [Header("Tile Types")] public List<TileEntry> tiles = new List<TileEntry>();

    /// <summary>
    /// Reads each prefab's TileCornerGizmo to get corner heights,
    /// then generates 0°, 90°, 180°, and 270° rotations.
    /// </summary>
    public void GenerateAllRotationsFromGizmos()
    {
#if UNITY_EDITOR
        foreach (var entry in tiles)
        {
            entry.rotations.Clear();

            if (entry.prefab == null)
            {
                Debug.LogWarning($"Tile '{entry.tileName}' has no prefab assigned.");
                continue;
            }

            // Load the prefab asset contents
            string assetPath = AssetDatabase.GetAssetPath(entry.prefab);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(assetPath);
            var gizmo = prefabRoot.GetComponent<TileCornerGizmo>();

            if (gizmo == null)
            {
                Debug.LogError($"Prefab '{entry.prefab.name}' missing TileCornerGizmo component.");
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                continue;
            }

            // Read corner heights from the gizmo as integers
            int[] baseCorners = new int[] {
                Mathf.RoundToInt(gizmo.topLeft),
                Mathf.RoundToInt(gizmo.topRight),
                Mathf.RoundToInt(gizmo.bottomRight),
                Mathf.RoundToInt(gizmo.bottomLeft)
            };
            PrefabUtility.UnloadPrefabContents(prefabRoot);

            // Generate rotated corner configurations
            int[] current = (int[])baseCorners.Clone();
            for (int i = 0; i < 4; i++)
            {
                entry.rotations.Add(new CornerConfig {
                    rotationDegrees = i * 90,
                    corners = (int[])current.Clone()
                });
                // Rotate clockwise: TL <- BL, TR <- TL, BR <- TR, BL <- BR
                current = new int[] { current[3], current[0], current[1], current[2] };
            }
        }
#else
        Debug.LogError("GenerateAllRotationsFromGizmos can only run in the Unity Editor.");
#endif
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TileDatabase))]
public class TileDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var db = (TileDatabase)target;
        EditorGUILayout.Space();
        if (GUILayout.Button("Generate Rotations from Gizmos"))
        {
            db.GenerateAllRotationsFromGizmos();
            EditorUtility.SetDirty(db);
        }
    }
}
#endif

