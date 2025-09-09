using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] private int x;
    [SerializeField] private int y;
    [SerializeField] private Color color = Color.cyan;

    public int X => x;
    public int Y => y;

    void Awake()
    {
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = color;
    }

    public void SetGridPosition(int gx, int gy, float cellSize)
    {
        x = gx; y = gy;
        transform.position = new Vector3(gx * cellSize, gy * cellSize, 0f);
    }
}