using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class CombatCollisionHandler
{
    static private CombatCollisionHandler _instance;

    static public CombatCollisionHandler GetInstance()
    {
        if (_instance != null)
        {
            return _instance;
        }
        else
        {
            return new CombatCollisionHandler();
        }
    }

    public CombatCollisionHandler()
    {

    }

    public void HandleCollision(Collision2D collision)
    {
        ICombatAgent first = collision.collider.GetComponent<ICombatAgent>();
        ICombatAgent second = collision.otherCollider.GetComponent<ICombatAgent>();

        if (!(first != null && second != null))
        {
            return;
        }

        if (first.GetTeam() == second.GetTeam())
        {
            return;
        }

        second.ConsumeDamage(first.ProduceDamage());

        second.CollideWithEnemy();

        second.CheckHP();
    }
}
