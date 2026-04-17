using UnityEngine;
using System.Collections.Generic;
using System;

[CreateAssetMenu(fileName = "AchievementConfigList", menuName = "Achievements/Achievement List")]
public class AchievementConfigList : ScriptableObject
{
    public List<AchievementConfig> Achievements;
}


[Serializable]
public class AchievementConfig : ScriptableObject
{
    public string Id;
    public string Title;
    [TextArea]
    public string Description;
    public Sprite Icon;
}
