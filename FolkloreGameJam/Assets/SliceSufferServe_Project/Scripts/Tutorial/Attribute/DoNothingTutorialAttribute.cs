using UnityEngine;

[CreateAssetMenu(menuName = "TutorialAttribute/DoNothing")]
public class DoNothingTutorialAttribute : TutorialAttribute
{
    public override void CheckingObjective()
    {
        _isObjectiveComplete = true;
    }
}
