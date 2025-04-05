using NUnit.Framework;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SinusoidBullet : MonoBehaviour, IBullet, ICombatAgent
{
    [SerializeField] private float _lifeTime;
    private IBulletParameters _initData;

    private float k;

    private float _cos;
    private float _sin;

    // IBullet
    public void Init(IBulletParameters data) {
        k = 5;
        _lifeTime = 0;
        _initData = data;
        float angle = Mathf.Atan2(_initData.Direction.y, _initData.Direction.x);
        _cos = Mathf.Cos(angle);
        _sin = Mathf.Sin(angle);
        transform.position = _initData.Start;
        BulletSystemSupervisor.GetBulletSystemSupervisor().RegisterBullet(this);
    }
    public void Tick()
    {
        _lifeTime += Time.deltaTime;
        float x = _lifeTime * _initData.Speed;
        float y = Mathf.Sin(k * x);
        Vector2 pos = new Vector2(x * _cos - y * _sin, x * _sin + y * _cos) + _initData.Start;
        Debug.Log(pos);
        transform.position = pos;

        if (_lifeTime > _initData.LifeTime) {
            Destroying();
        }
    }
    public void Destroyed()
    {
        Destroy(gameObject);
    }
    public void Destroying()
    {
        BulletSystemSupervisor.GetBulletSystemSupervisor().UnregisterBullet(this);
    }



    // ICombatAgent
    public string GetTeam()
    {
        return _initData.Team;
    }
    public float ProduceDamage()
    {
        return _initData.Damage;
    }
    public void ConsumeDamage(float damage)
    {
        return;
    }
    public void CheckHP()
    {
        return;
    }
    public void CollideWithEnemy()
    {
        Destroying();
    }




    public void OnCollisionEnter2D(Collision2D collision)
    {
        CombatCollisionHandler.GetInstance().HandleCollision(collision);
    }
}
