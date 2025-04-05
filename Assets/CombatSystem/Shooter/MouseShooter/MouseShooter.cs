using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class MouseShooter : MonoBehaviour
{
    [SerializeField] GameObject _bulletPrefab;

    private Vector2 _start = Vector2.zero;
    private Vector2 _end = Vector2.zero;

    public void StartTracking()
    {
        _start = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void EndTracking()
    {
        _end = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    public void Shoot()
    {
        GameObject bulletObject = Instantiate(_bulletPrefab, transform);
        IBullet bullet = bulletObject.GetComponent<IBullet>();
        bullet.Init(new IBulletParameters(_start, 5f, (_end - _start).normalized, 1f, 10, "shooter"));
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartTracking();
        }

        if (Input.GetMouseButtonUp(0))
        {
            EndTracking();
            Shoot();
        }
    }
}