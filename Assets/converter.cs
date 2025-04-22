using UnityEngine;
using System.IO;

public class converter : MonoBehaviour
{
    public string fileName;

    void Start()
    {
        string filePath = Path.Combine(Application.dataPath, fileName);

        // Read the file contents
        string fileContents;
        using (StreamReader reader = new StreamReader(filePath))
        {
            fileContents = reader.ReadToEnd();
        }

        // Replace semicolons with commas in the file contents
        string replacedContents = fileContents.Replace(";", ",");

        // Write the modified contents back to the file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.Write(replacedContents);
        }

        Debug.Log("Semicolons replaced with commas in " + fileName);
    }
}
