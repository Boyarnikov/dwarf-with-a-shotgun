using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatAgent
{
    public string GetTeam();
    public float ProduceDamage();
    public void ConsumeDamage(float damage);
    public void CheckHP();

    public void CollideWithEnemy();
}
