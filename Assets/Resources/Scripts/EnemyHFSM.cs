using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

//Componentes Principais da Máquina de Estados
//Interfaces
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

//Classes
public class Transition : ITransition
{
    public int TransitionLevel;
    public State Parent;
    public List<IAction> Actions;
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

public class HSMController
{
    public EnemyController enemy;
    public PlayerController player;

    public HSM MainHSM;//Guarda-se a Máquina de Estados Mãe
    public State currentState;//Para se saber onde se está neste momento

    public void DoActions()
    {
        //Vão-se buscar as acções a ser executadas a partir da máquina inicial/principal
        List<IAction> actionsToExecute = MainHSM.update();
        foreach (IAction action in actionsToExecute)
        {
            if (action != null)
            {
                action.DoAction();
            }
        }
    }
}

public class State
{
    public string Name;
    public int StateLevel;
    public State ParentState;
    public List<IAction> Actions;
    public IAction EntryAction;
    public IAction ExitAction;
    public List<Transition> transitions;

    public Transition CheckForTriggeredTransition()
    {
        foreach (Transition t in transitions)
        {
            bool AllConditionsAreMet = true;
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

public class HSM : State
{
    public int HSMLevel;//(Será mesmo necessário?) 0 significa o nível principal da HSM
    public List<State> states;//Estados ou outras sub-HSMs
    public State initialState;//Para saber o estado inicial da máquina
    public State currentState;//Para saber em que estado é que esta máquina se encontra

    public List<IAction> update()
    {
        if (currentState == null)
        {
            currentState = initialState;
        }
        foreach (Transition transition in transitions)
        {
            //Se todas as condições de transição se verificarem
            if (transition.CanPerformTransition())
            {
                List<IAction> actionsToReturn = new List<IAction>();
                //Serão adicionadas à lista de retorno as acções de saída do estado actual, as acções da transição e as acções de entrada do estado alvo da transição
                actionsToReturn.Add(currentState.ExitAction);
                actionsToReturn.AddRange(transition.Actions);
                actionsToReturn.Add(transition.targetState.EntryAction);
                //O estado actual passa a ser o que foi alvo da transição
                currentState = transition.targetState;
                //Retorna-se a lista de acções
                return actionsToReturn;
            }
        }
        if (currentState is HSM)
        {
            HSM returner = currentState as HSM;
            //Cria-se a lista que irá ser retornada por este método
            List<IAction> actionsToReturn = new List<IAction>();
            //Adicionam-se as próprias acções
            actionsToReturn.AddRange(Actions);
            //Sendo que isto não á apenas um estado, mas uma HSM que contem outros estados, adicionam-se também as acções dos estados actuais dentro dela
            actionsToReturn.AddRange(returner.update());
            //Retorna-se a lista com todas as acções
            return actionsToReturn;
        }
        else
        {
            //Caso seja apenas um estado simples (e tendo em conta que não foram accionadas transições), apenas se irão retornar as acções respectivas
            return currentState.Actions;
        }
    }

    public State GetStateForTransition(State stateToLookFor,Transition transition)
    {
        foreach (State s in states)
        {
            if (s.GetType() == states.GetType())
            {
                return s;
            }
        }
        return null;
    }
}
//Componentes Específicos desta Máquina de Estados
//HSM's
public class HSM_Main : HSM
{
    public EnemyController enemy;
    public PlayerController player;

    public HSM_Main()
    {
        Name = "Main Enemy Machine";
        StateLevel = 0;
        ParentState = null;
        Actions = new List<IAction>();//Não haverão acções a este nível
        EntryAction = null;
        ExitAction = null;
        transitions = new List<Transition>();//Não haverão transições a este nível
        states = new List<State>();//Povoar esta lista com a máquina "EnemyAlive" e com o estado de morte do personagem
        states.Add(new HSM_EnemyAlive(this));
        states.Add(new State_Death { enemy = this.enemy, ParentState = this });
        initialState = states[0];
    }
}
public class HSM_EnemyAlive : HSM
{
    public EnemyController enemy;
    public PlayerController player;

    public HSM_EnemyAlive(HSM_Main parent)
    {
        //Aqui será importante receber a informação do inimigo que está a usar esta máquina e do jogador
        this.enemy = parent.enemy;
        this.player = parent.player;
        //O resto não difere da aplicação habitual
        Name = "Enemy is Alive";
        StateLevel = 1;
        ParentState = parent;
        Actions = new List<IAction>();//Acções do inimigo vivo? Nada foi idealizado para já.
        EntryAction = null;
        ExitAction = null;//Quando o inimigo deixa de estar vivo, inicia as acções de entrada no estado de morte
        states = new List<State>();//Adicionar a HSM de Patrulha e os estados de Perseguição e Ataque
        states.Add(new HSM_Patrol(this));
        states.Add(new State_Pursuit(this));
        states.Add(new State_Attack(this));
        initialState = states[0];
        transitions = new List<Transition>();//Só haverá uma transição que irá para o State_Death
        transitions.Add(new Transition_EnemyDies { enemy = this.enemy });
    }
}
public class HSM_Patrol : HSM
{
    public EnemyController enemy;
    public PlayerController player;

    public HSM_Patrol(HSM_EnemyAlive parent)
    {
        this.enemy = parent.enemy;
        this.player = parent.player;

        Name = "Patrol";
        StateLevel = 2;
        ParentState = parent;
        Actions = new List<IAction>();
        EntryAction = null;
        ExitAction = null;
        states = new List<State>();
        //states.Add(State_Guard);
        //states.Add(State_Relocate);
        //initialState=states[0];
        transitions = new List<Transition>();
        //transitions.Add(new Transition_EnemyDetectsPlayer())
    }
}
//States
public class State_Death : State
{
    public EnemyController enemy;

    public State_Death()
    {
        Name = "Enemy Death State";
        StateLevel = 1;
        ParentState = null;//Como está no "topo" da máquina, não tem pai
        Actions = new List<IAction>();//Ele não irá fazer nada enquanto está morto
        //EntryAction=new Action_Death();//(É PRECISO IMPLEMENTAR)Ao entrar neste estado, ele irá desencadear a acção de morte
        ExitAction = null;//Neste jogo os inimigos não regressam da morte!
        transitions = new List<Transition>();//Idem aspas
    }
}
public class State_Pursuit : State
{
    public EnemyController enemy;
    public PlayerController player;

    public State_Pursuit(HSM_EnemyAlive parent)
    {
        this.enemy = parent.enemy;
        this.player = parent.player;

        Name = "Pursuit";
        StateLevel = 2;
        ParentState = parent;
        Actions = new List<IAction>();
        //EntryAction=
        //ExitAction=

        transitions = new List<Transition>();
        //transitions.Add(new Transition_EnemyGetsInRangeOfPlayer);
        //transitions.Add(new Transition_EnemyLoosesSightOfPlayer);
    }
}
public class State_Attack : State
{
    public EnemyController enemy;
    public PlayerController player;

    public State_Attack(HSM_EnemyAlive parent)
    {
        this.enemy = parent.enemy;
        this.player = parent.player;

        Name = "Attack";
        StateLevel = 2;
        ParentState = parent;
        Actions = new List<IAction>();
        //Actions.Add(Action_Attack)
        //EntryAction=
        //ExitAction=
        transitions = new List<Transition>();
        //transitions.Add(Transition_PlayerExitsAttackRange);
        //transitions.Add(Transition_EnemyKillsPlayer)
    }
}
//Transitions
public class Transition_EnemyDies : Transition
{
    public EnemyController enemy;

    public Transition_EnemyDies()
    {
        TransitionLevel = 1;
        Actions = new List<IAction>();//Deixa-se a acção para a de entrada no estado de morte
        ConditionsList = new List<ICondition>();//Aqui adiciona-se a condição da causa da morte do personagem (a vida do personagem fica a 0)
        ConditionsList.Add(new Condition_IsEnemyHealthNull { enemy = this.enemy });
        targetState = new State_Death();
    }
}
//Conditions
public class Condition_IsEnemyHealthNull : ICondition
{
    public EnemyController enemy;
    public bool test()
    {
        return enemy.Health <= 0;
    }
}
public class Condition_IsPlayerDetectedByEnemy : ICondition
{
    public EnemyController enemy;
    public PlayerController player;

    public bool test()
    {
        if (Vector2.Distance(player.transform.position, enemy.transform.position) <= enemy.SightRange)
            return true;
        else
        {
            return false;
        }
    }
}
public class Condition_IsPlayerNotDetectedByEnemy : ICondition
{
    public EnemyController enemy;
    public PlayerController player;

    public bool test()
    {
        if (Vector2.Distance(player.transform.position, enemy.transform.position) <= enemy.SightRange)
            return false;
        else
        {
            return true;
        }
    }
}
//Actions
public class Action_AttackPlayer : IAction
{
    public EnemyController enemy;
    public PlayerController player;

    public void DoAction()
    {
        player.Health -= enemy.AttackStrength;
    }
}
public class Action_MoveToPoint : IAction
{
    public EnemyController enemy;
    public Vector2 destination;
    public void DoAction()
    {
        enemy.transform.position = destination;
    }
}
public class Action_Wait : MonoBehaviour, IAction
{
    //Por alguma razão não entra na co-rotina. Resolver isso quando for possível
    public int SecondsToWait;
    public float ElapsedTime = 0;

    public void DoAction()
    {
        StartCoroutine(Wait(this, 5));
    }

    public IEnumerator Wait(Action_Wait invoking_class, int seconds)
    {
        Debug.Log("Entered Coroutine");
        invoking_class.ElapsedTime = 0;
        while (invoking_class.ElapsedTime < seconds)
        {
            invoking_class.ElapsedTime += Time.deltaTime;
        }
        yield return null;
    }
}
//Main Class
//Componentes Específicos da Máquina de estados de teste
//Controlador
public class Test_HSMController : HSMController
{
    public Test_HSMController(EnemyController EnemyScript,PlayerController PlayerScript)
    {
        MainHSM=new Test_HSM_Main();
        MainHSM.states[0].transitions.Add(new Test_Transition_PlayerGotInAttackRange
        {
            enemy = EnemyScript,
            Player = PlayerScript,
            targetState = MainHSM.states[1]
        });
        MainHSM.states[1].transitions.Add(new Test_Transition_PlayerGotOutOfAttackRange
        {
            enemy = EnemyScript,
            Player = PlayerScript,
            targetState = MainHSM.states[0]
        });
        Debug.Log(MainHSM.states[1].Actions[0].GetType());
    }
}
//HSM
public class Test_HSM_Main : HSM
{
    public EnemyController enemy;
    public PlayerController player;

    public Test_HSM_Main()
    {
        Name = "Main Enemy Machine";
        StateLevel = 0;
        ParentState = null;
        Actions = new List<IAction>();
        EntryAction = null;
        ExitAction = null;
        transitions = new List<Transition>();
        states = new List<State>();
        states.Add(new Test_State_Guard(this));
        states.Add(new Test_State_Attack(this));
        initialState = states[0];
    }
}
//States
public class Test_State_Guard:State
{
    public EnemyController enemy;
    public PlayerController player;

    public Test_State_Guard(Test_HSM_Main parent)
    {
        Name = "Test_Guard";
        StateLevel = 1;
        ParentState = parent;
        Actions = new List<IAction>();
        EntryAction = null;
        ExitAction = null;
        transitions = new List<Transition>();
    }
}
public class Test_State_Attack:State
{
    public EnemyController enemy;
    public PlayerController player;

    public Test_State_Attack(Test_HSM_Main parent)
    {
        enemy = parent.enemy;
        player = parent.player;
        Name = "Test_Attack";
        StateLevel = 1;
        ParentState = parent;
        Actions = new List<IAction>();
        Actions.Add(new Action_AttackPlayer {enemy = this.enemy,player = this.player});
        EntryAction = null;
        ExitAction = null;
        transitions = new List<Transition>();
    }
}
//Transitions
public class Test_Transition_PlayerGotInAttackRange : Transition
{
    public EnemyController enemy;
    public PlayerController Player;

    public Test_Transition_PlayerGotInAttackRange()
    {
        TransitionLevel = 1;
        Actions = new List<IAction>();
        ConditionsList = new List<ICondition>();
        ConditionsList.Add(new Condition_IsPlayerDetectedByEnemy() { enemy = this.enemy,player = this.Player});
        targetState = null;
    }
}
public class Test_Transition_PlayerGotOutOfAttackRange : Transition
{
    public EnemyController enemy;
    public PlayerController Player;

    public Test_Transition_PlayerGotOutOfAttackRange()
    {
        TransitionLevel = 1;
        Actions = new List<IAction>();
        ConditionsList = new List<ICondition>();
        ConditionsList.Add(new Condition_IsPlayerNotDetectedByEnemy() { enemy = this.enemy, player = this.Player });
        targetState = null;
    }
}
//Condições e acções são aproveitadas da máquina principal
//Classe Principal deste script (NÃO APAGAR)
public class EnemyHFSM : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
