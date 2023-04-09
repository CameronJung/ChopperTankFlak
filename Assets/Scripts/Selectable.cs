using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Selectable objects are those that can have data displayed in the recon panel
public interface ISelectable
{
    public abstract string GetDescription();

    public abstract string GetTitle();
}
