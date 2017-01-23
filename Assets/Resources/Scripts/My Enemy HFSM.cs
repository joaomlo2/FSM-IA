using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Serialization;

public interface IAction
{
    void DoAction();
}

public interface ITransition
{
    bool CanPerformTransition();
}

public interface ICondition
{
    bool test();
}

public class Transition : ITransition
{
    public int TransitionLevel;
    public List<ICondition> ConditionsList;
    public State targetState;

    public bool CanPerformTransition()
    {
        foreach (ICondition condition in ConditionsList)
        {
            if (!condition.test())
                return false;
        }
        return true;
    }
}

public class State
{
    protected string StateName;
    public int StateLevel;
    public List<IAction> Actions;
    public IAction EntryAction;
    public IAction ExitAction;
    public List<Transition> transitions;

    public Transition CheckForTriggeredTransition()
    {
        foreach (Transition t in transitions)
        {
            bool AllConditionsAreMet=true;
            foreach (ICondition condition in t.ConditionsList)
            {
                if (!condition.test())
                    AllConditionsAreMet = false;
            }
            if (AllConditionsAreMet)
            {
                return t;
            }
        }
        return null;
    }
}

public class HSMController
{
    public State initialState;//May be important to keep in mind which is the main HSM
    public State currentState;//You have to know where you left off

    public List<IAction> update()
    {

    }
}

public class HSM:State
{
    public string HSMName;
    public HSM parent;
    public int HSMLevel; //0 means main HSM Level
    public List<State> states; //States or other sub-HSM's
    public State initialState;
    public State currentState;//You have to know where you left off

    public List<IAction> update()
    {
        if (currentState == null)
        {
            currentState = initialState;
            if (currentState is HSM)
            {
                HSM returner = currentState as HSM;
                List<IAction> a;
                return returner.update();
            }
        }
    }
}

public class MyEnemyHFSM : MonoBehaviour
{
}