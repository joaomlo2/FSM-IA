using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Xml.Serialization;
using JetBrains.Annotations;

//Componentes Principais das Máquina de Estados
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
    public SM parentMachine;
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

public class State
{
    public string Name;
    public SM parentMachine;
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

public class HSMTransition : ITransition
{
    public int TransitionLevel;
    public HSMState Parent;
    public List<IAction> Actions;
    public List<ICondition> ConditionsList;
    public HSMState targetState;

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
    public HSMState currentState;//Para se saber onde se está neste momento

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

public class HSMState
{
    public string Name;
    public int StateLevel;
    public HSMState ParentState;
    public List<IAction> Actions;
    public IAction EntryAction;
    public IAction ExitAction;
    public List<HSMTransition> transitions;

    public HSMTransition CheckForTriggeredTransition()
    {
        foreach (HSMTransition t in transitions)
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

public class SM
{
    public string Name;
    public List<State> States;
    public State CurrentState;
    public State InitialState;

    public List<IAction> GetActions()
    {
        List<IAction> actions = new List<IAction>();
        foreach (Transition t in CurrentState.transitions)
        {
            if (t.CanPerformTransition())
            {
                //if(CurrentState.EntryAction!=null)
                //    actions.Add(CurrentState.ExitAction);
                CurrentState = t.targetState;
                //if (CurrentState.ExitAction != null)
                //    actions.Add(CurrentState.ExitAction);
                return actions;
            }
        }
        actions.AddRange(CurrentState.Actions);
        return actions;
    }

    public void update()
    {
        if (CurrentState == null)
        {
            CurrentState = InitialState;
        }
        List<IAction> actionsToRun = GetActions();
        foreach (IAction action in actionsToRun)
        {
            if(action!=null)
                action.DoAction();
        }
    }
}

public class HSM : HSMState
{
    public int HSMLevel;//(Será mesmo necessário?) 0 significa o nível principal da HSM
    public List<HSMState> states;//Estados ou outras sub-HSMs
    public HSMState initialState;//Para saber o estado inicial da máquina
    public HSMState currentState;//Para saber em que estado é que esta máquina se encontra

    public List<IAction> update()
    {
        if (currentState == null)
        {
            currentState = initialState;
        }
        foreach (HSMTransition transition in transitions)
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

    public HSMState GetStateForTransition(HSMState stateToLookFor,HSMTransition transition)
    {
        foreach (HSMState s in states)
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
        transitions = new List<HSMTransition>();//Não haverão transições a este nível
        states = new List<HSMState>();//Povoar esta lista com a máquina "EnemyAlive" e com o estado de morte do personagem
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
        states = new List<HSMState>();//Adicionar a HSM de Patrulha e os estados de Perseguição e Ataque
        states.Add(new HSM_Patrol(this));
        states.Add(new State_Pursuit(this));
        states.Add(new State_Attack(this));
        initialState = states[0];
        transitions = new List<HSMTransition>();//Só haverá uma transição que irá para o State_Death
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
        states = new List<HSMState>();
        //states.Add(State_Guard);
        //states.Add(State_Relocate);
        //initialState=states[0];
        transitions = new List<HSMTransition>();
        //transitions.Add(new Transition_EnemyDetectsPlayer())
    }
}
//States
public class State_Death : HSMState
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
        transitions = new List<HSMTransition>();//Idem aspas
    }
}
public class State_Pursuit : HSMState
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

        transitions = new List<HSMTransition>();
        //transitions.Add(new Transition_EnemyGetsInRangeOfPlayer);
        //transitions.Add(new Transition_EnemyLoosesSightOfPlayer);
    }
}
public class State_Attack : HSMState
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
        transitions = new List<HSMTransition>();
        //transitions.Add(Transition_PlayerExitsAttackRange);
        //transitions.Add(Transition_EnemyKillsPlayer)
    }
}
//Transitions
public class Transition_EnemyDies : HSMTransition
{
    public EnemyController enemy;

    public Transition_EnemyDies()
    {
        TransitionLevel = 1;
        Actions = new List<IAction>();//Deixa-se a acção para a de entrada no estado de morte
        ConditionsList = new List<ICondition>();//Aqui adiciona-se a condição da causa da morte do personagem (a vida do personagem fica a 0)
        ConditionsList.Add(new Condition_IsEnemyHealthNull(enemy));
        targetState = new State_Death();
    }
}
//Conditions
public class Condition_IsEnemyHealthNull : ICondition
{
    public EnemyController enemy;

    public Condition_IsEnemyHealthNull(EnemyController Enemy)
    {
        enemy = Enemy;
    }

    public bool test()
    {
        return enemy.Health <= 0;
    }
}
public class Condition_IsPlayerDetectedByEnemy : ICondition
{
    public EnemyController enemy;
    public PlayerController player;

    public Condition_IsPlayerDetectedByEnemy(EnemyController Enemy,PlayerController Player)
    {
        enemy = Enemy;
        player = Player;
    }

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

    public Condition_IsPlayerNotDetectedByEnemy(EnemyController Enemy, PlayerController Player)
    {
        enemy = Enemy;
        player = Player;
    }

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

    public Action_AttackPlayer(EnemyController Enemy, PlayerController Player)
    {
        enemy = Enemy;
        player = Player;
    }

    public void DoAction()
    {
        player.Health -= enemy.AttackStrength;
    }
}
public class Action_MoveToPoint : IAction
{
    public EnemyController enemy;
    public Vector2 destination;

    public Action_MoveToPoint(EnemyController Enemy)
    {
        enemy = Enemy;
    }

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
//Componentes Específicos da Máquina de estados hierárquica de teste
//Controlador
public class Test_HSMController : HSMController
{
    public Test_HSMController(EnemyController EnemyScript,PlayerController PlayerScript)
    {
        MainHSM=new Test_HSM_Main();
        MainHSM.states[0].transitions.Add(new Test_HSM_Transition_PlayerGotInAttackRange
        {
            enemy = EnemyScript,
            Player = PlayerScript,
            targetState = MainHSM.states[1]
        });
        MainHSM.states[1].transitions.Add(new Test_HSM_Transition_PlayerGotOutOfAttackRange
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
        transitions = new List<HSMTransition>();
        states = new List<HSMState>();
        states.Add(new Test_HSM_State_Guard(this));
        states.Add(new Test_HSM_State_Attack(this));
        initialState = states[0];
    }
}
//States
public class Test_HSM_State_Guard:HSMState
{
    public EnemyController enemy;
    public PlayerController player;

    public Test_HSM_State_Guard(Test_HSM_Main parent)
    {
        Name = "Test_Guard";
        StateLevel = 1;
        ParentState = parent;
        Actions = new List<IAction>();
        EntryAction = null;
        ExitAction = null;
        transitions = new List<HSMTransition>();
    }
}
public class Test_HSM_State_Attack:HSMState
{
    public EnemyController enemy;
    public PlayerController player;

    public Test_HSM_State_Attack(Test_HSM_Main parent)
    {
        enemy = parent.enemy;
        player = parent.player;
        Name = "Test_Attack";
        StateLevel = 1;
        ParentState = parent;
        Actions = new List<IAction>();
        Actions.Add(new Action_AttackPlayer(enemy,player));
        EntryAction = null;
        ExitAction = null;
        transitions = new List<HSMTransition>();
    }
}
//Transitions
public class Test_HSM_Transition_PlayerGotInAttackRange : HSMTransition
{
    public EnemyController enemy;
    public PlayerController Player;

    public Test_HSM_Transition_PlayerGotInAttackRange()
    {
        TransitionLevel = 1;
        Actions = new List<IAction>();
        ConditionsList = new List<ICondition>();
        ConditionsList.Add(new Condition_IsPlayerDetectedByEnemy(enemy,Player));
        targetState = null;
    }
}
public class Test_HSM_Transition_PlayerGotOutOfAttackRange : HSMTransition
{
    public EnemyController enemy;
    public PlayerController Player;

    public Test_HSM_Transition_PlayerGotOutOfAttackRange()
    {
        TransitionLevel = 1;
        Actions = new List<IAction>();
        ConditionsList = new List<ICondition>();
        ConditionsList.Add(new Condition_IsPlayerNotDetectedByEnemy(enemy,Player));
        targetState = null;
    }
}
//Condições e acções são aproveitadas da máquina principal

//Componentes Específicos da Máquina de Estados Simples de Teste
//SM
public class Test_SM : SM
{
    public EnemyController Enemy;
    public PlayerController Player;
    public Test_SM(EnemyController thisGuy,PlayerController thePlayer)
    {
        Enemy = thisGuy;
        Player = thePlayer;
        this.Name = "Máquina de Estados de Teste";
        this.States=new List<State>();
        States.Add(new Test_SM_State_Guard(Enemy,Player,this));
        States.Add(new Test_SM_State_Attack(Enemy,Player,this));
        States[0].transitions[0].targetState = States[1];
        States[1].transitions[0].targetState = States[0];
        this.InitialState = States[0];
        this.CurrentState = null;
    }
}
//States
public class Test_SM_State_Guard : State
{
    public EnemyController enemy;
    public PlayerController player;
    public Test_SM_State_Guard(EnemyController theEnemy,PlayerController thePlayer,SM parent)
    {
        parentMachine = parent;
        enemy = theEnemy;
        player = thePlayer;
        Name = "Test_Guard";
        Actions = new List<IAction>();
        EntryAction = null;
        ExitAction = null;
        transitions = new List<Transition>();
        transitions.Add(new Test_SM_Transition_PlayerGotInAttackRange(enemy,player,parent));
    }
}
public class Test_SM_State_Attack : State
{
    public SM parentMachine;
    public EnemyController enemy;
    public PlayerController player;

    public Test_SM_State_Attack(EnemyController thisEnemy,PlayerController thisPlayer,SM parent)
    {
        parentMachine = parent;
        enemy = thisEnemy;
        player = thisPlayer;
        Name = "Test_Attack";
        Actions = new List<IAction>();
        Actions.Add(new Action_AttackPlayer(enemy,player));
        EntryAction = null;
        ExitAction = null;
        transitions = new List<Transition>();
        transitions.Add(new Test_SM_Transition_PlayerGotOutOfAttackRange(enemy,player,parent));
    }
}
//Transições
public class Test_SM_Transition_PlayerGotInAttackRange: Transition
{
    public SM parentMachine;
    public EnemyController enemy;
    public PlayerController Player;

    public Test_SM_Transition_PlayerGotInAttackRange(EnemyController theEnemy,PlayerController thePlayer,SM parent)
    {
        parentMachine = parent;
        enemy = theEnemy;
        Player = thePlayer;
        Actions = new List<IAction>();
        ConditionsList = new List<ICondition>();
        ConditionsList.Add(new Condition_IsPlayerDetectedByEnemy(enemy,Player));
    }
}
public class Test_SM_Transition_PlayerGotOutOfAttackRange : Transition
{
    public SM parentMachine;
    public EnemyController enemy;
    public PlayerController Player;

    public Test_SM_Transition_PlayerGotOutOfAttackRange(EnemyController theEnemy,PlayerController thePlayer,SM parent)
    {
        parentMachine = parent;
        enemy = theEnemy;
        Player = thePlayer;
        Actions = new List<IAction>();
        ConditionsList = new List<ICondition>();
        ConditionsList.Add(new Condition_IsPlayerNotDetectedByEnemy(enemy,Player));
    }
}
//Classe Principal deste script (NÃO APAGAR)
public class EnemyHFSM : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
