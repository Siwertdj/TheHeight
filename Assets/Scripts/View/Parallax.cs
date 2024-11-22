using UnityEngine;

// Credit to: https://www.youtube.com/watch?v=ZYZfKbLxoHI
public class Parallax : MonoBehaviour
{
    Material mat;
    float distance;

    [Range(0f,5f)]
    public float speed = 0.2f;

    void Start()
    {
        mat = GetComponent<Renderer>().material;
    }

    void Update()
    {
        distance += Time.deltaTime * speed; 
        mat.SetTextureOffset("_MainTex", Vector2.right * distance);
    }
}
