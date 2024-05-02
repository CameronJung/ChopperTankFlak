using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class Level : ScriptableObject, ISelectable
{


    [SerializeField] private string LevelName;
    [SerializeField] [TextArea] private string Description;

    [SerializeField] private string SceneID;
    
    public virtual string GetTitle()
    {
        return this.LevelName;
    }


    public virtual string GetDescription()
    {
        return this.Description;
    }

    public string GetLevelID()
    {
        return this.SceneID;
    }


}
