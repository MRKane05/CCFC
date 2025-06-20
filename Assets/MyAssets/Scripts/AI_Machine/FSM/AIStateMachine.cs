using UnityEngine;
using System.Collections.Generic;

// Base interface for all AI states
public interface IAIState
{
    void Enter(AI_Fighter fighter);
    void Update(AI_Fighter fighter);
    void Exit(AI_Fighter fighter);
    string GetStateName();
}

// FSM Manager - handles state transitions
public class AIStateMachine
{
    private Dictionary<string, IAIState> states = new Dictionary<string, IAIState>();
    private IAIState currentState;
    private AI_Fighter fighter;

    public AIStateMachine(AI_Fighter fighter)
    {
        this.fighter = fighter;
        InitializeStates();
    }

    private void InitializeStates()
    {
        // Register all available states
        states.Add("ATTACK", new AttackState());
        states.Add("EVADE", new EvadeState());
        states.Add("DOEVASIVE", new EvasiveState());
        states.Add("PATROL", new PatrolState());
        states.Add("FOLLOW", new FollowState());
        states.Add("PULLUP", new PullupState());
    }

    public void ChangeState(string stateName)
    {
        if (!states.ContainsKey(stateName))
        {
            Debug.LogWarning($"State {stateName} not found!");
            return;
        }

        // Exit current state
        currentState?.Exit(fighter);

        // Enter new state
        currentState = states[stateName];
        currentState.Enter(fighter);

        fighter.pattern = stateName; // Keep the original pattern variable updated
    }

    public void Update()
    {
        currentState?.Update(fighter);
    }

    public string GetCurrentStateName()
    {
        return currentState?.GetStateName() ?? "NONE";
    }
}