using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour {
    public int Health;
    public float SightRange;
    public int AttackStrength;
    public List<Vector2> GuardingPointsList;
    public int CurrentGuardPointIndex=0;

    public PlayerController Player;
    public Text textbox;

    public Test_HSMController testHSM;
    public void Awake()
    {
        Player = GameObject.Find("Player").GetComponent<PlayerController>();
        textbox = transform.FindChild("Canvas").FindChild("Text").GetComponent<Text>();
        testHSM=new Test_HSMController(this,Player);
    }

    public void Start()
    {

    }

    public void Update()
    {
        testHSM.DoActions();
    }
}
