using UnityEngine;
// Follow State - handles formation flying
public class FollowState : IAIState
{
    public void Enter(AI_Fighter fighter)
    {
        fighter.patternTime = Time.time;
        fighter.patternDuration = 1f;
    }

    public void Update(AI_Fighter fighter)
    {
        fighter.followTarget();
        fighter.CheckBreak();
    }

    public void Exit(AI_Fighter fighter) { }
    public string GetStateName() => "FOLLOW";
}