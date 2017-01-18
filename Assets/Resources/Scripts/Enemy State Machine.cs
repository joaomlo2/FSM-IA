using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
//Interfaces
public interface ITransition
{
    bool isTriggered();
    IState getTargetState();
    IAction GetAction();
}

public interface IState
{
    IAction getAction();
    IAction getEntryAction();
    IAction getExitAction();
    List<ITransition> getTransitions();
}

public interface IAction
{
    void DoAction();
}

public interface ICondition
{
    bool test();
}


//Conditions
public class PointProximityCondition : ICondition
{
    public Vector2 PointA;
    public Vector2 PointB;
    public float minRadius;

    public bool test()
    {
        return Vector2.Distance(PointA, PointB) >= minRadius;
    }
}

public class IsPlayerDead : ICondition
{
    public PlayerController player { get; set; }

    public bool test()
    {
        return player.Health >= 0;
    }
}
//Actions
public class IdleAction : IAction
{
    public void DoAction()
    { }
}

public class GoToPosition : MonoBehaviour,IAction
{
    public EnemyStateMachine thisCharacter { get; set; }
    public Vector2 Destination { get; set; }

    public void DoAction()
    {
        StartCoroutine(Smooth.SmoothTranslationCoroutine(thisCharacter.transform, Destination, 3));
    }
}

public class Attack : IAction
{
    public PlayerController player;
    public void DoAction()
    {
        player.Health -= 10;
    }
}

//Transitions
public class SeesPlayer : ITransition
{
    public EnemyStateMachine thisCharacter { get; set; }
    public PlayerController Player { get; set; }

    public IAction GetAction()
    {
        //Criar acção de perseguição
        GoToPosition action = new GoToPosition { thisCharacter = thisCharacter, Destination = Player.transform.position };
        return action;
    }

    public IState getTargetState()
    {
        PursuitState state = new PursuitState
        {
            player = Player,
            thisCharachter = thisCharacter
        };
        return state;
    }

    public bool isTriggered()
    {
        PointProximityCondition condition = new PointProximityCondition { minRadius = thisCharacter.Sight, PointA = thisCharacter.transform.position, PointB = Player.transform.position };
        return condition.test();
    }
}

public class ApproachedPlayer : ITransition
{
    public EnemyStateMachine thisCharacter { get; set; }
    public PlayerController player { get; set; }

    public IAction GetAction()
    {
        GoToPosition action = new GoToPosition
        {
            Destination = player.transform.position,
            thisCharacter = thisCharacter
        };
        return action;
    }

    public IState getTargetState()
    {
        AttackState state = new AttackState {player = player, thisCharachter = thisCharacter};
        return state;
    }

    public bool isTriggered()
    {
        PointProximityCondition condition = new PointProximityCondition { minRadius = 0.1f, PointA = thisCharacter.transform.position, PointB = player.transform.position };
        return condition.test();
    }
}

public class PlayerEscapes : ITransition
{
    public EnemyStateMachine thisCharachter { get; set; }
    public PlayerController Player { get; set; }

    public bool isTriggered()
    {
        PointProximityCondition condition = new PointProximityCondition
        {
            minRadius = thisCharachter.Sight,
            PointA = thisCharachter.transform.position,
            PointB = Player.transform.position
        };
        return !condition.test();
    }

    public IState getTargetState()
    {
        GuardingState state = new GuardingState
        {
            player = Player,
            PointToGuard = thisCharachter.NearestGuardingPoint,
            thisCharacter = thisCharachter
        };
        return state;
    }

    public IAction GetAction()
    {
        return new IdleAction();
    }
}

public class PlayerDied : ITransition
{
    public EnemyStateMachine thisCharacter { get; set; }
    public PlayerController player { get; set; }
    public IAction GetAction()
    {
        GoToPosition action = new GoToPosition
        {
            Destination = thisCharacter.NearestGuardingPoint,
            thisCharacter = thisCharacter
        };
        return action;
    }

    public IState getTargetState()
    {
        GuardingState state = new GuardingState
        {
            player = player,
            PointToGuard = thisCharacter.NearestGuardingPoint,
            thisCharacter = thisCharacter
        };
        return state;
    }

    public bool isTriggered()
    {
        IsPlayerDead condition = new IsPlayerDead {player = player};
        return condition.test();
    }
}

//States
public class GuardingState : IState
{
    public Vector2 PointToGuard { get; set; }
    public EnemyStateMachine thisCharacter { get; set; }
    public PlayerController player { get; set; }

    public IAction getAction()
    {
        return new IdleAction();
    }

    public IAction getEntryAction()
    {
        GoToPosition action = new GoToPosition { thisCharacter = thisCharacter, Destination = PointToGuard };
        return action;
    }

    public IAction getExitAction()
    {
        //Substituir pelo disparo no futuro?
        GoToPosition action = new GoToPosition { thisCharacter = thisCharacter, Destination = player.transform.position };
        return action;
    }

    List<ITransition> IState.getTransitions()
    {
        List<ITransition> transitionsList=new List<ITransition>();
        transitionsList.Add(new SeesPlayer { Player = player, thisCharacter = thisCharacter });
        return transitionsList;
    }
}

public class PursuitState : IState
{
    public EnemyStateMachine thisCharachter { get; set; }
    public PlayerController player { get; set; }

    public IAction getAction()
    {
        GoToPosition action = new GoToPosition
        {
            Destination = player.transform.position,
            thisCharacter = thisCharachter
        };
        return action;
    }

    public IAction getEntryAction()
    {
        GoToPosition action = new GoToPosition
        {
            thisCharacter = thisCharachter,
            Destination = player.transform.position
        };
        return action;
    }

    public IAction getExitAction()
    {
        return new IdleAction();
    }

    List<ITransition> IState.getTransitions()
    {
        List<ITransition> transitionsList=new List<ITransition>();
        transitionsList.Add(new PlayerEscapes { Player = player, thisCharachter = thisCharachter });
        transitionsList.Add(new ApproachedPlayer {player = this.player,thisCharacter = this.thisCharachter});
        return transitionsList;
    }
}

public class AttackState : IState
{
    public EnemyStateMachine thisCharachter { get; set; }
    public PlayerController player { get; set; }


    public IAction getAction()
    {
        Attack action=new Attack {player = player};
        return action;
    }

    public IAction getEntryAction()
    {
        Attack action = new Attack { player = player };
        return action;
    }

    public IAction getExitAction()
    {
        Attack action = new Attack { player = player };
        return action;
    }

    List<ITransition> IState.getTransitions()
    {
        List<ITransition> list=new List<ITransition>();
        list.Add(new PlayerEscapes {Player = player,thisCharachter = thisCharachter});
        list.Add(new PlayerDied {player = player,thisCharacter = thisCharachter});
        return list;
    }
}

public class EnemyStateMachine : MonoBehaviour
{
    private List<IState> states;
    private IState initialState;
    private IState currentState;

    public int Health = 100;
    public float Sight = 3;
    public Vector2 NearestGuardingPoint;

    public void Start()
    {
        NearestGuardingPoint = GlobalVariables.singleton.GuardPoints[0];
    }

    public void Update()
    {
        if (transform.position.x!=NearestGuardingPoint.x || transform.position.y != NearestGuardingPoint.y)
        {
            SmoothPosition op=new SmoothPosition();
            op.
        }
    }

    private void FSM_Update()
    {

    }
}
