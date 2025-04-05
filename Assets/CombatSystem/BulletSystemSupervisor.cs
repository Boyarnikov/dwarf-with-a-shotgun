using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class BulletSystemSupervisor : MonoBehaviour
{
    static public BulletSystemSupervisor _instance;

    static public BulletSystemSupervisor GetBulletSystemSupervisor()
    {
        return _instance;
    }

    List<IBullet> _bullets;
    Queue<IBullet> _bulletsToRemove;

    public void Awake()
    {
        _instance = this;
        _bullets = new();
        _bulletsToRemove = new();
    }

    public void RegisterBullet(IBullet bullet)
    {
        _bullets.Add(bullet);
    }

    public void UnregisterBullet(IBullet bullet)
    {
        _bulletsToRemove.Enqueue(bullet);
    }

    public void RemoveBullets()
    {
        while (_bulletsToRemove.Count > 0)
        {
            IBullet bullet = _bulletsToRemove.Dequeue();
            _bullets.Remove(bullet);
            bullet.Destroyed();
        }
    }

    public void Update()
    {
        for (int i = 0; i < _bullets.Count; i++)
        {
            _bullets[i].Tick();
        }
        RemoveBullets();
    }


}
