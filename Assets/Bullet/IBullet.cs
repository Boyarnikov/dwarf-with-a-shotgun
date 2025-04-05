using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public struct IBulletParameters
{
    private Vector2 _start;
    private float _damage;
    private Vector2 _direction;
    private float _speed;
    private float _lifeTime;
    private string _team;

    public Vector2 Start => _start;
    public float Damage => _damage;
    public Vector2 Direction => _direction;
    public float Speed => _speed;
    public float LifeTime => _lifeTime;
    public string Team => _team;

    public IBulletParameters(Vector2 start, float damage, Vector2 direction, float speed, float lifeTime, string team)
    {
        _start = start;
        _damage = damage;
        _direction = direction;
        _speed = speed;
        _lifeTime = lifeTime;
        _team = team;
    }
}

public interface IBullet
{
    public void Init(IBulletParameters data);
    public void Tick();
    public void Destroyed();
    public void Destroying();
}
