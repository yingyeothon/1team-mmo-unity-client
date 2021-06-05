using UnityEngine;

public class CardWorld : MonoBehaviour
{
    [SerializeField, Range(1, 8)]
    int worldNumber = 1;

    [SerializeField]
    Camera cam;

    [SerializeField]
    ShipPivot shipPivot;

    public int WorldNumber
    {
        set
        {
            worldNumber = value;
            UpdateWorldLayer();
        }
    }

    public RenderTexture RenderTexture
    {
        set => cam.targetTexture = value;
    }

    public ShipPivot ShipPivot => shipPivot;

    void UpdateWorldLayer()
    {
        worldNumber = Mathf.Clamp(worldNumber, 1, 8);
        var transformList = transform.GetComponentsInChildren<Transform>();
        var layerName = $"World {worldNumber}";
        var layer = LayerMask.NameToLayer(layerName);
        foreach (var t in transformList)
        {
            t.gameObject.layer = layer;
        }

        if (worldNumber > 4)
        {
            shipPivot.transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        cam.cullingMask = LayerMask.GetMask(layerName, "Bullet");
    }

    public bool IsShipDestroyed => ShipPivot.IsShipModelNull;
}