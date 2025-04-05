using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Bat : MonoBehaviour, IEntity
{
    [SerializeField] private string _team;
    [SerializeField] private List<BatBodyPart> _body;

    public void Awake()
    {
        Init();
    }

    public void Init()
    {
        for (int i = 0; i < _body.Count; i++)
        {
            _body[i].Init(new EntityBodyPartParameters(this, null, 0, _team));
        }
    }
    public void Tick()
    {

    }
    public void Destroying()
    {
        for (int i = 0; i < _body.Count; i++)
        {
            _body[i].Destroyed();
        }
    }

    public void PartDestroyed(int number, IEntityBodyPart part)
    {
        Destroying();
    }

}
