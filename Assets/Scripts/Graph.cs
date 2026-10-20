using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class Graph
{
    
    #region Variables
    
    private struct Entry
    {
        public readonly int Time;
        public readonly int Value;
        
        public Entry(int time, int value)
        {
            Time = time;
            Value = value;
        }
    }

    private readonly Texture2D texture;
    private readonly int maxX;
    private readonly int maxY;
    private int biggestX;
    private int biggestY;
    private Entry[] data;

    #endregion

    #region Initialization
    
    public Graph(GameObject window, string captionY)
    {
        maxX = (int) Mathf.Round(window.GetComponent<RectTransform>().sizeDelta.x);
        maxY = (int) Mathf.Round(window.GetComponent<RectTransform>().sizeDelta.y);
        texture = new Texture2D(maxX, maxY){filterMode = FilterMode.Point};
        window.GetComponent<Image>().material.mainTexture = texture;
        window.GetComponentInChildren<Text>().text = captionY;
        biggestX = maxX - 1;
        biggestY = maxY - 1;
        data = new Entry[0];
    }
    
    #endregion

    #region Functions
    
    public void AddEntry(int time, int value)
    {
        
        // TODO: calculate how many data is in one pixel and scale accordingly
        
        if (time > biggestX)
            biggestX = time;
        if (value > biggestY)
            biggestY = value;

        Entry[] newData = new Entry[data.Length + 1];
        data.CopyTo(newData, 0);
        newData[newData.Length - 1] = new Entry(time, value);
        data = newData;
        
    }

    public void Plot()
    {
        Clear();
        
        for (int i = 0; i < data.Length; i++)
            texture.SetPixel(ConvertX(data[i].Time), ConvertY(data[i].Value), Color.red);

        texture.Apply();
    }

    public void Save(String name)
    {
        String fullDirectory = Application.dataPath + "/Graphs/";
        byte[] bytes = texture.EncodeToPNG();
        if (!Directory.Exists(fullDirectory))
            Directory.CreateDirectory(fullDirectory);
        File.WriteAllBytes(fullDirectory + name + ".png", bytes);
    }

    private void Clear()
    {
        for (int j = 0; j < maxY+1; j++)
        {
            for (int i = 0; i < maxX+1; i++)
            {
                texture.SetPixel(i, j, new Color(0.7f, 0.7f, 0.7f));
            }
        }
    }
    
    private int ConvertX(int originalX)
    {
        if (biggestX == maxX)
            return originalX;
        return (int) Mathf.Round(originalX * (maxX / (float) biggestX));
    }

    private int ConvertY(int originalY)
    {
        if (biggestY == maxY)
            return originalY;
        return (int) Mathf.Round(originalY * (maxY / (float) biggestY));
    }

    #endregion
 
}
