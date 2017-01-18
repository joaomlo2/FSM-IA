using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public int Health=100;

	void Update ()
	{
	    var x = Input.GetAxis("Horizontal");
        var y = Input.GetAxis("Vertical");

	    float speed = 0.2f;

        transform.position=new Vector2(transform.position.x+x*speed,transform.position.y+y*speed);
	    transform.FindChild("Canvas").FindChild("Text").GetComponent<Text>().text = Health.ToString();
	}
}
