using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tileBlock
{
    public string[] tileTypes;
    public int x;
    public int y;
    int[] changeCoordinates = new int[2];
    public bool hasChanged = false;
    public bool collapsed = false;
    public tileBlock(string[] tileStrings, int xcoor, int ycoor)
    {
        x = xcoor;
        y = ycoor;
        tileTypes = tileStrings;
        changeCoordinates[0] = xcoor;
        changeCoordinates[1] = ycoor;
        //Debug.Log(x + " " + y + " Generated");
    }
    public void updateRules()
    {
        hasChanged = false;
    }

    public int[] ChangeRules(string[] tileStrings)
    {

        tileTypes = tileStrings;
        hasChanged = true;
        
        if (tileTypes.Length < 2)
        {
            collapse();
        }

        return changeCoordinates;
    }

    public void collapse()
    {
        collapsed = true;
        //Instantioi objekti.
        Debug.Log(x + "-" + y + " Collapsed as " + tileTypes[0]);
    }

    public string[] getPossibilities()
    {
        return tileTypes;
    }
    public bool ArraysAreEqual(string[] arr1, string[] arr2)
    {
        if (arr1.Length != arr2.Length)
        {
            return false;
        }

        for (int i = 0; i < arr1.Length; i++)
        {
            if (arr1[i] != arr2[i])
            {
                return false;
            }
        }

        return true;
    }
}
