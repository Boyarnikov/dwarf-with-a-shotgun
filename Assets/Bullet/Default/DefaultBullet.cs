using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class DefaultBullet : MonoBehaviour, IBullet, ICombatAgent
{
    [SerializeField] private float _lifeTime;
    private IBulletParameters _initData;

    private Vector2 _velocity;

    // IBullet
    public void Init(IBulletParameters data) {
        _lifeTime = 0;
        _initData = data;
        _velocity = _initData.Direction * _initData.Speed;
        transform.position = _initData.Start;
        BulletSystemSupervisor.GetBulletSystemSupervisor().RegisterBullet(this);
    }
    public void Tick()
    {
        _lifeTime += Time.deltaTime;
        Vector2 move = Time.deltaTime * _velocity;
        transform.position = new Vector2(transform.position.x + move.x, transform.position.y + move.y);

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
