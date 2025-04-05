using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public interface IEntity
{
    public void Init();
    public void Tick();
    public void Destroying();

    public void PartDestroyed(int number, IEntityBodyPart part);
}
