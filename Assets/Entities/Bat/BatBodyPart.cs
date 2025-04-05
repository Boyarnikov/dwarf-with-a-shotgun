using System;
using UnityEngine;

public class BatBodyPart : MonoBehaviour, IEntityBodyPart, ICombatAgent
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _collider;

    EntityBodyPartParameters _initData;

    [Header("Monster settings")]
    private float _maxHp = 10;
    private float _hp = 10;
    private float _damage = 5;

    // IEntityBodyPart
    public void Init(EntityBodyPartParameters data)
    {
        _initData = data;
        _hp = _maxHp;
    }
    public void Destroying()
    {
        _initData.Root.PartDestroyed(_initData.Index, this);
    }
    public void Destroyed()
    {
        Destroy(this.gameObject);
    }

    public void Tick()
    {

    }




    // ICombatAgent
    public string GetTeam()
    {
        return _initData.Team;
    }
    public float ProduceDamage()
    {
        return 0;
    }
    public void ConsumeDamage(float damage)
    {
        _hp -= damage;
        return;
    }
    public void CheckHP()
    {
        if (_hp <= 0)
        {
            Destroying();
        }
    }
    
    public void CollideWithEnemy()
    {

    }




    public void OnCollisionEnter2D(Collision2D collision)
    {
        CombatCollisionHandler.GetInstance().HandleCollision(collision);
    }
}
