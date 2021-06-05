using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    float speed = 10.0f;

    [SerializeField]
    GameObject owner;

    [SerializeField]
    GameObject explosionPrefab;

    bool triggerEntered;

    public GameObject Owner
    {
        get => owner;
        set => owner = value;
    }

    void Awake()
    {
        Destroy(gameObject, 10);
    }

    void Update()
    {
        transform.Translate(speed * Time.deltaTime * transform.forward, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerEntered) return;

        if (other.GetComponentInParent<ShipPivot>().gameObject != owner)
        {
            triggerEntered = true;
            
            Destroy(gameObject);
            
            var explosion = Instantiate(explosionPrefab);
            explosion.transform.position = other.transform.position;
            explosion.transform.localScale = Vector3.one * 10;
            Destroy(explosion, 3);
        }
    }
}
