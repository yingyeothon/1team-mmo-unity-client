using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class ShipPivot : MonoBehaviour
{
    [SerializeField]
    GameObject laserPrefab;

    [SerializeField]
    Transform[] cannonList;

    [SerializeField]
    CardWorldGroup cardWorldGroup;

    [SerializeField]
    GameObject shipModel;

    [SerializeField]
    bool automaticFire;
    
    [SerializeField]
    GameObject explosionPrefab;

    [SerializeField]
    GameObject healPrefab;

    public bool IsShipModelNull => shipModel == null;

    IEnumerator StartFiringCoro()
    {
        if (!automaticFire) yield break;
        
        while (LookAtRandomShip())
        {
            yield return FireMultipleCoro();

            yield return new WaitForSeconds(Random.Range(0.3f, 0.6f));
        }
    }

    public IEnumerator FireMultipleCoro()
    {
        for (var i = 0; i < Random.Range(1, 5); i++)
        {
            Fire();
            yield return new WaitForSeconds(0.1f);
        }
    }

    public Laser Fire()
    {
        var laser = Instantiate(laserPrefab, cannonList[Random.Range(0, cannonList.Length)]).GetComponent<Laser>();
        var laserTransform = laser.transform;
        laserTransform.localPosition = Vector3.zero;
        laserTransform.localRotation = Quaternion.identity;
        laserTransform.parent = null;
        
        laser.Owner = gameObject;
        return laser;
    }

    public void LookAt(ShipPivot shipPivot)
    {
        if (shipPivot == null) return;
        
        transform.LookAt(shipPivot.transform);
    }

    bool LookAtRandomShip()
    {
        var targetShip = cardWorldGroup.GetRandomShip(this);
        if (targetShip == null) return false;
        LookAt(targetShip);
        return true;
    }

    public void ReloadScene()
    {
        SceneManager.LoadScene("Battle");
    }

    public void DestroyWithExplosion()
    {
        var explosion = Instantiate(explosionPrefab);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * 10;
        Destroy(explosion, 3);
        
        Destroy(gameObject);
    }

    public void InstantiateHealEffect()
    {
        var explosion = Instantiate(healPrefab);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * 10;
        Destroy(explosion, 3);
    }
}
