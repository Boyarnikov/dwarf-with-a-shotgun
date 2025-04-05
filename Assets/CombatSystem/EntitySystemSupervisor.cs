using NUnit.Framework;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEngine;

public class EntitySystemSupervisor : MonoBehaviour
{
    static public EntitySystemSupervisor _instance;

    static public EntitySystemSupervisor GetPathFinderSystemSupervisor()
    {
        return _instance;
    }

    List<IEntity> _entities;
    Queue<IEntity> _entitiesToRemove;

    public void Awake()
    {
        _instance = this;
        _entities = new();
        _entitiesToRemove = new();
    }

    public void RegisterEntity(IEntity entity)
    {
        
    }

    public void UnregisterEntity(IEntity entity)
    {
        
    }

    public void RemoveEntities()
    {
        
    }

    public void Update()
    {
        
    }


}
