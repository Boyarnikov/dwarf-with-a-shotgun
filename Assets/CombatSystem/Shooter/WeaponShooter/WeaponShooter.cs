using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponShooter : MonoBehaviour
{
    [SerializeField] GameObject _bulletPrefab;
    [SerializeField] Minecart _cart;

    [Header("Weapon settings")]
    [SerializeField] private float _angleDispersion;
    [SerializeField] private float _lifeTimeDispersion;
    [SerializeField] private float _speedDispersion;
    [SerializeField] private int _bulletCount;
    [SerializeField] private float _coolDown;
    [SerializeField] private float _reloadTime;
    [SerializeField] private int _fullMagazineAmmo;
    [SerializeField] private int _auto;
    [SerializeField] private int _autoCount;
    [SerializeField] private float _kickbackStrength;

    [Header("Ammo settings")]
    [SerializeField] private float _ammoDamage;
    [SerializeField] private float _ammoSpeed;
    [SerializeField] private float _ammoLifeTime;
    [SerializeField] private string _ammoTeam;

    [Header("Status")]
    [SerializeField] private int _autoUsed;
    [SerializeField] private float _lastShotTime;
    [SerializeField] private int _magazineAmmo;
    [SerializeField] private bool _reloading;
    [SerializeField] private float _reloadingTime;
    private Vector2 _end;

    public void Awake()
    {
        _autoUsed = 0;
        _lastShotTime = 0f;
        _magazineAmmo = _fullMagazineAmmo;
        _reloadingTime = 0;
    }

    public void Shoot()
    {
        Vector2 start = transform.position;
        float angle = Mathf.Atan2((_end - start).y, (_end - start).x);
        for (int i = 0; i < _bulletCount; i++)
        {
            float bulletAngle = angle + Random.Range(-_angleDispersion, _angleDispersion);
            float bulletLifeTime = _ammoLifeTime + Random.Range(-_lifeTimeDispersion, _lifeTimeDispersion);
            float bulletSpeed = _ammoSpeed + Random.Range(-_speedDispersion, _speedDispersion);
            GameObject bulletObject = Instantiate(_bulletPrefab);
            IBullet bullet = bulletObject.GetComponent<IBullet>();
            bullet.Init(new IBulletParameters(start, _ammoDamage, new Vector2(Mathf.Cos(bulletAngle), Mathf.Sin(bulletAngle)), bulletSpeed, bulletLifeTime, _ammoTeam));
        }
        _lastShotTime = 0f;
        _magazineAmmo -= 1;

        if (_auto == 1) {
            _autoUsed++;
        }

        if (_cart != null)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;
            Vector2 direction = -(mousePos - transform.position).normalized;
            _cart.ApplyShootingImpulse(direction, _kickbackStrength);
        } 
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            _autoUsed = 0;
        }

        if (_lastShotTime >= _coolDown && _magazineAmmo > 0 && !_reloading)
        {
            if (_auto == 0)
            {
                if (Input.GetMouseButtonUp(0))
                {
                    _end = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Shoot();
                }
            }

            if (_auto == 2 || (_auto == 1 && _autoUsed < _autoCount))
            {
                if (Input.GetMouseButton(0))
                {
                    _end = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Shoot();
                }
            }
        }
        else
        {
            if (_lastShotTime < _coolDown)
            {
                _lastShotTime += Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            _reloading = true;
        }

        if (_reloading)
        {
            _reloadingTime += Time.deltaTime;
            if (_reloadingTime >= _reloadTime)
            {
                _reloadingTime = 0f;
                _reloading = false;
                _magazineAmmo = _fullMagazineAmmo;
            }
        }

    }
}