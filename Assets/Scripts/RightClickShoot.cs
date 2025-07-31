using UnityEngine;

public class RightClickShoot : MonoBehaviour, IRightClickAction
{
    public GameObject bulletPrefab;
    public Transform muzzlePoint;

    public void OnRightClick()
    {
        Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);
    }
}
