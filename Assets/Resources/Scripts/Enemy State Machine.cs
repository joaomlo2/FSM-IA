using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Runtime.CompilerServices;
using System.Security;
using JetBrains.Annotations;
using UnityEngine.UI;


//public interface ICondition
//{
//    bool test();
//}

//public interface ITransition
//{
//    int getLevel();
//    bool isTriggered();
//    State getTargetState();
//    List<IAction> GetAction();
//}

//public interface IAction
//{
//    void DoAction();
//}

//public class Transition : ITransition
//{
//    public List<IAction> actions;
//    public int level;
//    public State targetState;
//    public ICondition condition;


//    public List<IAction> GetAction()
//    {
//        return actions;
//    }

//    public bool isTriggered()
//    {
//        return condition.test();
//    }

//    public int getLevel()
//    {
//        return level;
//    }

//    State ITransition.getTargetState()
//    {
//        return targetState;
//    }
//}

//public abstract class Action
//{
//    public abstract void DoAction();
//}

//public struct UpdateResult
//{
//    public List<IAction> actions;
//    public ITransition transition;
//    public int level;
//}

//public interface IHSMBase
//{
//    UpdateResult update();
//}

//public abstract class HSMBase : IHSMBase
//{
//    public List<IAction> actions { get; set; }

//    public UpdateResult result;

//    public UpdateResult update()
//    {
//        result = new UpdateResult();
//        result.actions = actions;
//        result.transition = null;
//        result.level = 0;
//        return result;
//    }


//    public abstract List<HSMBase> getStates();
//}

//public class State : HSMBase
//{
//    public State parent;
//    public List<HSMBase> states;
//    public IAction action;
//    public IAction entryAction;
//    public IAction exitAction;
//    public List<Transition> transitions;
//    public State initialState;
//    public State currentState;

//    public override List<HSMBase> getStates()
//    {
//        return states;
//    }

//    public IAction getAction()
//    {
//        return action;
//    }

//    public IAction getEntryAction()
//    {
//        return entryAction;
//    }

//    public IAction getExitAction()
//    {
//        return exitAction;
//    }

//    public List<Transition> GetTransitions()
//    {
//        return transitions;
//    }

//    public List<IAction> updateDown(State state, int level)
//    {
//        if (level > 0)
//        {
//            actions = parent.updateDown(state, level - 1);
//        }
//        else
//        {
//            actions=new List<IAction>();
//        }
//        if (currentState != null)
//        {
//            actions.Add(currentState.exitAction);
//        }
//        currentState = state;
//        actions.Add(state.getEntryAction());
//        return actions;
//    }
//}

//public class HierarchicalStateMachine : State
//{
//    public List<HSMBase> states;

//    public override List<HSMBase> getStates()
//    {
//        if (currentState != null)
//        {
//            return currentState.getStates();
//        }
//        else
//        {
//            return states;
//        }
//    }

//    public UpdateResult update()
//    {
//        UpdateResult result = new UpdateResult();
//        if (currentState != null)
//        {
//            currentState = initialState;
//            result = new UpdateResult();
//            result.actions.Add(currentState.entryAction);
//            result.level = 0;//?
//            result.transition = null;
//        }
//        Transition triggeredTransition = null;
//        foreach (Transition t in currentState.transitions)
//        {
//            if (t.isTriggered())
//            {
//                triggeredTransition = t;
//                break;
//            }
//        }
//        if (triggeredTransition != null)
//        {
//            result = new UpdateResult();
//            result.actions = new List<IAction>();
//            result.transition = triggeredTransition;
//            result.level = triggeredTransition.getLevel();
//        }
//        else
//        {
//            result = currentState.update();
//        }
//        if (result.transition != null)
//        {
//            State targetState = null;
//            if (result.level == 0)
//            {
//                targetState = result.transition.getTargetState();
//                result.actions.Add(currentState.entryAction);
//                result.actions.AddRange(result.transition.GetAction());
//                result.actions.Add(targetState.entryAction);
//                currentState = targetState;

//                result.actions.Add(getAction());
//                result.transition = null;
//            }
//            else
//            {
//                if (result.level > 0)
//                {
//                    result.actions.Add(currentState.exitAction);
//                    currentState = null;
//                    result.level--;
//                }
//                else
//                {
//                    targetState = result.transition.getTargetState();
//                    State targetMachine = targetState.parent;
//                    result.actions.AddRange(result.transition.GetAction());
//                    result.actions.AddRange(targetMachine.updateDown(targetState, -result.level));
//                    result.transition = null;
//                }
//            }
//        }
//        else
//        {
//            result.actions.Add(getAction());
//        }
//        return result;
//    }
//}

//public class SubMachineState : HierarchicalStateMachine
//{
//    public IAction getAction()
//    {
//        return action;
//    }

//    public UpdateResult update()
//    {
//        return update();
//    }

//    public List<HSMBase> getStates()
//    {
//        List<HSMBase> statesList = new List<HSMBase>();
//        if (currentState == null)
//        {
//            statesList.Add(this);
//            statesList.AddRange(currentState.getStates());
//        }
//        else
//        {
//            statesList.Add(this);
//        }
//        return statesList;
//    }
//}

////Condições
//public class PointProximityCondition : ICondition
//{
//    public Vector2 PointA;
//    public Vector2 PointB;
//    public float minRadius;

//    public bool test()
//    {
//        return Vector2.Distance(PointA, PointB) >= minRadius;
//    }
//}

//public class IsPlayerDead : ICondition
//{
//    public PlayerController player { get; set; }

//    public bool test()
//    {
//        return player.Health >= 0;
//    }
//}

public class EnemyStateMachine : MonoBehaviour
{
    public int Health = 100;
    public float Sight = 3;
    public Vector2 NearestGuardingPoint;

    public void Start()
    {
        NearestGuardingPoint = GlobalVariables.singleton.GuardPoints[0];
    }

    public void Update()
    {
        if (transform.position.x != NearestGuardingPoint.x || transform.position.y != NearestGuardingPoint.y)
        {
            SmoothPosition op = new SmoothPosition();
        }
    }

    private void HFSM_Update()
    {
    }
}