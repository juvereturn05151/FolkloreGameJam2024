using UnityEngine;

/// <summary>
/// Abstract base class for tutorial objectives.
/// </summary>
public abstract class TutorialAttribute : ScriptableObject
{
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
    }
}
