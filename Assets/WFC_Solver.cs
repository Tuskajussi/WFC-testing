using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WFC_Solver : MonoBehaviour
{
    Dictionary<string, Dictionary<string, int>> tileRules = new Dictionary<string, Dictionary<string, int>>();
    public GameObject[] tilePrefabs; // Array of tile prefabs
    public string[] tileTypes = { "Water", "Shore", "ShoreIB", "ShoreOB", "Grass", "Slope", "SlopeIB", "SlopeOB", "Hill" };
    public int mapSizeX; // Size of the tilemap in X direction
    public int mapSizeZ; // Size of the tilemap in Z direction
    private tileBlock[,] tileMap;
    List<int[]> changedTiles = new List<int[]>();
    //public GameObject tilePref;
    private void PopulateTileRules()
    {
        // Populating the tileRules dictionary with the rules from your data

        int[,] ruleValues = {
            { 1, 1, 1, 1, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 0 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 1 }
        };

        for (int i = 0; i < tileTypes.Length; i++)
        {
            Dictionary<string, int> neighbourRules = new Dictionary<string, int>();
            for (int j = 0; j < tileTypes.Length; j++)
            {
                neighbourRules.Add(tileTypes[j], ruleValues[i, j]);
            }
            tileRules.Add(tileTypes[i], neighbourRules);
        }
    }

    void Start()
    {
        PopulateTileRules();
        GenerateTilemap();
        Propagate();

    }

    string DebugAStringArray(string[] arr)
    {
        string toreturn = "";
        foreach (string possibility in arr)
        {
            toreturn = toreturn + possibility + " ";
        }
        return toreturn;
    }

    void GenerateTilemap()
    {
        tileMap = new tileBlock[mapSizeX, mapSizeZ];
        string[] tileWater = { "Water" };
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                tileMap[x, z] = new tileBlock(tileTypes, x, z);
            }
        }
        for(int x=0; x < mapSizeX; x++)
        {
            changedTiles.Add(tileMap[x, 0].ChangeRules(tileWater));
            changedTiles.Add(tileMap[x, mapSizeZ-1].ChangeRules(tileWater));
            changedTiles.Add(tileMap[0, x].ChangeRules(tileWater));
            changedTiles.Add(tileMap[mapSizeX-1, x].ChangeRules(tileWater));
        }
    }
    public string[] GetUniqueNeighborPossibilitiesWithAllRules(string[] currentRules, string[] newNeighborRules)
    {
        HashSet<string> uniquePossibilities = new HashSet<string>();

        foreach (var rule in currentRules)
        {
            if (tileRules.ContainsKey(rule))
            {
                Dictionary<string, int> neighborRules = tileRules[rule];
                foreach (var possibility in neighborRules.Keys)
                {
                    uniquePossibilities.Add(possibility);
                }
            }
        }

        List<string> validPossibilities = new List<string>();

        foreach (var possibility in uniquePossibilities)
        {
            if (Array.Exists(currentRules, rule => rule == possibility) || Array.Exists(newNeighborRules, rule => rule == possibility))
            {
                validPossibilities.Add(possibility);
            }
        }

        return validPossibilities.ToArray();
    }

    void Propagate()
    {
        int propagateAmount = 0;
        List<int[]> itemsToAdd = new List<int[]>();
        int debugint = 40;
        while(changedTiles.Count > 0 || itemsToAdd.Count > 0)
        {
            foreach (int[] item in changedTiles)
            {
                propagateAmount++;
                string[] newNeighbourRules = tileMap[item[0], item[1]].getPossibilities();
                // Check top neighbor
                if (item[1] > 0)
                {
                    tileBlock topBlock = tileMap[item[0], item[1] - 1];
                    string[] currentRules = topBlock.getPossibilities();
                    string[] newRules = GetUniqueNeighborPossibilitiesWithAllRules(currentRules, newNeighbourRules);
                    if(debugint > 0)
                    {
                        Debug.Log(item[0] + " " + item[1]);
                        Debug.Log(DebugAStringArray(currentRules));
                        Debug.Log(DebugAStringArray(newRules));
                        debugint--;
                    }
                    if(!topBlock.ArraysAreEqual(currentRules, newRules))
                    {
                        itemsToAdd.Add(tileMap[item[0], item[1] - 1].ChangeRules(newRules));
                    }
                }
                // Check bottom neighbor
                if (item[1] < tileMap.GetLength(1) - 1) // Check if not at the bottom edge
                {
                    tileBlock bottomBlock = tileMap[item[0], item[1] + 1];
                    string[] currentRules = bottomBlock.getPossibilities();
                    string[] newRules = GetUniqueNeighborPossibilitiesWithAllRules(currentRules, newNeighbourRules);
                    if (!bottomBlock.ArraysAreEqual(currentRules, newRules))
                    {
                        itemsToAdd.Add(tileMap[item[0], item[1] + 1].ChangeRules(newRules));
                    }
                }
                // Check right neighbor
                if (item[0] < tileMap.GetLength(0) - 1) // Check if not at the right edge
                {
                    tileBlock rightBlock = tileMap[item[0] + 1, item[1]];
                    string[] currentRules = rightBlock.getPossibilities();
                    string[] newRules = GetUniqueNeighborPossibilitiesWithAllRules(currentRules, newNeighbourRules);
                    if (!rightBlock.ArraysAreEqual(currentRules, newRules))
                    {
                        itemsToAdd.Add(tileMap[item[0] + 1, item[1]].ChangeRules(newRules));
                    }
                }
                // Check left neighbor
                if (item[0] > 0) // Check if not at the left edge
                {
                    tileBlock leftBlock = tileMap[item[0] - 1, item[1]];
                    string[] currentRules = leftBlock.getPossibilities();
                    string[] newRules = GetUniqueNeighborPossibilitiesWithAllRules(currentRules, newNeighbourRules);
                    if (!leftBlock.ArraysAreEqual(currentRules, newRules))
                    {
                        itemsToAdd.Add(tileMap[item[0] - 1, item[1]].ChangeRules(newRules));
                    }
                }
            }
            changedTiles.Clear();
            if(itemsToAdd.Count > 0)
            {
                changedTiles.AddRange(itemsToAdd);
                itemsToAdd.Clear();
            }
        }
        Debug.Log(propagateAmount);
    }

    /*void UpdateTileMapRules()
    {
        for (int x = 0; x < mapSizeX; x++)
        {
            for (int z = 0; z < mapSizeZ; z++)
            {
                tileBlock tile = tileMap[x, z];
                if (tile.hasChanged)
                {
                    List<string> neighborTileTypes = new();

                    // Check top neighbor
                    if (z > 0 && tileMap[x, z - 1].hasChanged)
                    {
                        neighborTileTypes.AddRange(tileMap[x, z - 1].getPossibilities());
                    }

                    // Check bottom neighbor
                    if (z < mapSizeZ - 1 && tileMap[x, z + 1].hasChanged)
                        neighborTileTypes.AddRange(tileMap[x, z + 1].getPossibilities());

                    // Check left neighbor
                    if (x > 0 && tileMap[x - 1, z].hasChanged)
                        neighborTileTypes.AddRange(tileMap[x - 1, z].getPossibilities());

                    // Check right neighbor
                    if (x < mapSizeX - 1 && tileMap[x + 1, z].hasChanged)
                        neighborTileTypes.AddRange(tileMap[x + 1, z].getPossibilities());

                    // Update rules of tile based on neighboring tiles
                    tile.ChangeRules(neighborTileTypes.ToArray());
                    tile.updateRules();
                }
            }
        }
    }*/
 
    // Method to get the tile types of neighboring tiles
    List<string> GetNeighborTileTypes(int x, int z)
    {
        List<string> neighborTileTypes = new List<string>();

        // Define the relative offsets to the neighboring tiles
        int[] offsetX = { -1, 0, 1, 0 };
        int[] offsetZ = { 0, 1, 0, -1 };

        // Loop through the neighboring tiles
        for (int i = 0; i < offsetX.Length; i++)
        {
            int neighborX = x + offsetX[i];
            int neighborZ = z + offsetZ[i];

            // Check if the neighboring coordinates are within the valid range
            if (neighborX >= 0 && neighborX < mapSizeX && neighborZ >= 0 && neighborZ < mapSizeZ)
            {
                // Get the tile type of the neighboring tile at the current position
                //string neighborTileType = GetTileType(neighborX, neighborZ);
                //neighborTileTypes.Add(neighborTileType);
            }
        }

        return neighborTileTypes;
    }

    // Method to select a random tile type based on the neighbor tile types and the tile rules
    string SelectRandomTileType(List<string> neighborTileTypes)
    {
        // Get the valid tile types based on the neighbor tile types and the tile rules
        List<string> validTileTypes = new List<string>();
        foreach (var kvp in tileRules)
        {
            string tileType = kvp.Key;
            Dictionary<string, int> allowedNeighbors = kvp.Value;
            bool isValid = true;
            foreach (var neighbor in neighborTileTypes)
            {
                if (!allowedNeighbors.ContainsKey(neighbor) || allowedNeighbors[neighbor] == 0)
                {
                    isValid = false;
                    break;
                }
            }
            if (isValid)
            {
                validTileTypes.Add(tileType);
            }
        }

        // Select a random tile type from the valid tile types
        if (validTileTypes.Count > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, validTileTypes.Count);
            return validTileTypes[randomIndex];
        }

        // If no valid tile types are found, randomize type
        Debug.Log("Randooom");
        return GetRandomTileType();
    }

    string GetRandomTileType()
    {
        if (tileTypes.Length > 0)
        {
            int randomIndex = UnityEngine.Random.Range(0, tileTypes.Length);
            return tileTypes[randomIndex];
        }
        else
        {
            // Return an empty string or throw an error, depending on your desired behavior
            return "Water"; 
        }
    }

    // Method to get the prefab of a tile type
    GameObject GetTilePrefab(string tileType)
    {
        // Loop through the tile prefabs and find the one that matches the tile type
        foreach (GameObject prefab in tilePrefabs)
        {
            if (prefab.name == tileType)
            {
                return prefab;
            }
        }
        // If no prefab is found, return null
        return null;
    }

    // Method to get the tile type at a specific position in the tilemap
    string[] GetTileType(int x, int z)
    {
        string[] error = { "Water" };
        // Check if the given position is within the bounds of the tilemap
        if (x >= 0 && x < mapSizeX && z >= 0 && z < mapSizeZ)
        {
            // Retrieve the tile type from the 2D array using the given position
            string[] tileType = tileMap[x, z].getPossibilities();
            return tileType;
            //return "";
        }
        else
        {
            // If the given position is out of bounds, return an empty(water) string
            return error;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
