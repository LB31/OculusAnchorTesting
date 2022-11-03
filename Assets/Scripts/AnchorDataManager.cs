using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnchorDataManager : Singleton<AnchorDataManager>
{
    public AnchorData AnchorData;
    public string anchorFile;


    protected override void Awake()
    {
        base.Awake();

        anchorFile = Application.persistentDataPath + "/anchorData.json";

        WriteFile();
        ReadFile();
    }

    public void ReadFile()
    {
        // Does the file exist?
        if (File.Exists(anchorFile))
        {
            // Read the entire file and save its contents
            string fileContents = File.ReadAllText(anchorFile);

            // Work with JSON
            AnchorData = JsonUtility.FromJson<AnchorData>(fileContents);

            foreach (var item in AnchorData.SpaceUuids)
            {
                Debug.Log(item);
            }
        }
    }

    public void WriteFile()
    {
        // Serialize the object into JSON and save string
        string jsonString = JsonUtility.ToJson(AnchorData);

        // Write JSON to file
        File.WriteAllText(anchorFile, jsonString);
    }
}
