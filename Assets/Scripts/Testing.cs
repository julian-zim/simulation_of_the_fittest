using UnityEngine;
using UnityEngine.UI;

public class Testing : MonoBehaviour
{   private bool Enabled = false;

    public GameObject window;
    private Texture2D texture;
    
    private void Start()
    {   if (!Enabled) return;
        
        Debug.Log("test");
        window.GetComponent<Image>().material.mainTexture = texture = new Texture2D(350, 220);
        for (int j = 0; j < 220; j++)
            for (int i = 0; i < 350; i++)
                texture.SetPixel(i, j, Color.red);
        texture.Apply();
        
    }
    
}
