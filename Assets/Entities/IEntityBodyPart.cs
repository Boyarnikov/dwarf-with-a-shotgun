using System;
using UnityEngine;

public struct EntityBodyPartParameters
{
    private IEntity _root;
    private BatBodyPart _parent;
    private int _index;
    private string _team;

    public IEntity Root => _root;
    public BatBodyPart Parent => _parent;
    public int Index => _index;
    public string Team => _team;

    public EntityBodyPartParameters(IEntity root, BatBodyPart parent, int index, string team)
    {
        _root = root;
        _parent = parent;
        _index = index;
        _team = team;
    }
}

public interface IEntityBodyPart
{
    public void Init(EntityBodyPartParameters data);
    public void Destroying();
    public void Destroyed();
    public void Tick();
}
