using UnityEngine;

/// <summary>
/// Abstract base class for tutorial objectives.
/// </summary>
public abstract class TutorialAttribute : ScriptableObject
{
    [SerializeField] protected int requiredProgress = 3;

    /// <summary>
    /// Tracks whether the tutorial objective is completed.
    /// </summary>
    protected bool _isObjectiveComplete = false;

    /// <summary>
    /// Indicates if the objective is completed.
    /// </summary>
    public bool IsComplete => _isObjectiveComplete;

    /// <summary>
    /// Checks if the objective conditions have been met. 
    /// Must be implemented by subclasses.
    /// </summary>
    public abstract void CheckingObjective();

    /// <summary>
    /// Resets the objective state to its initial state.
    /// </summary>
    public virtual void SetBegin()
    {
        _isObjectiveComplete = false;

        if (SSSAdvancedTutorialManager.Instance != null)
        {
            SSSAdvancedTutorialManager.Instance.ResetProgress(SSSAdvancedTutorialManager.Instance.CurrentTutorial.Type);
        }
    }

    protected void CompleteWhenProgressReaches(TutorialType tutorialType)
    {
        int targetProgress = Mathf.Max(1, requiredProgress);
        _isObjectiveComplete = SSSAdvancedTutorialManager.Instance != null
            && SSSAdvancedTutorialManager.Instance.GetProgress(tutorialType) >= targetProgress;
    }
}
