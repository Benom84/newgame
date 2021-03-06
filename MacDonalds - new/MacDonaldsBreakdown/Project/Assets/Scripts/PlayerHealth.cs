using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{	
	public float health = 100f;					// The player's health.
	public float repeatDamagePeriod = 2f;		// How frequently the player can be damaged.
	public float hurtForce = 10f;				// The force with which the player is pushed when hurt.
	public float damageAmount = 10f;			// The amount of damage to take when enemies touch the player
	public GameObject playerSpirit;				// The Object of the spirit of the player for the death
	[HideInInspector]
	public bool dead = false;	
	public AudioClip ouchClip;				// Array of clips to play when the player is damaged.
	public AudioClip[] deathClips;

	
	private SpriteRenderer healthBar;			// Reference to the sprite renderer of the health bar.
	private float lastHitTime;					// The time at which the player was last hit.
	private Vector3 healthScale;				// The local scale of the health bar initially (with full health).
	private PlayerControl playerControl;		// Reference to the PlayerControl script.
	private bool createdSpirit = false;

//	private Animator anim;						// Reference to the Animator on the player

	
	
	void Awake ()
	{
		// Setting up references.
		playerControl = GetComponent<PlayerControl>();
		healthBar = GameObject.Find("HealthBar").GetComponent<SpriteRenderer>();
//		anim = GetComponent<Animator>();
		
		// Getting the intial scale of the healthbar (whilst the player has full health).
		healthScale = healthBar.transform.localScale;
	}
	
	
	void OnCollisionEnter2D (Collision2D col)
	{
		// If the colliding gameobject is an Enemy...
		if(col.gameObject.tag == "Enemy")
		{
			// ... and if the time exceeds the time of the last hit plus the time between hits...
			if (Time.time > lastHitTime + repeatDamagePeriod) 
			{
				// ... and if the player still has health...
				if(health > 0f)
				{
					// ... take damage and reset the lastHitTime.
					TakeDamage(col.transform); 
					lastHitTime = Time.time; 
				}
				// If the player doesn't have health, do some stuff, let him fall into the river to reload the level.
				else
				{
					dead = true;

					// Find all of the colliders on the gameobject and set them all to be triggers.
					Collider2D[] cols = GetComponents<Collider2D>();
					foreach(Collider2D c in cols)
					{
						c.isTrigger = true;
					}
					
					// Move all sprite parts of the player to the front
					SpriteRenderer[] spr = GetComponentsInChildren<SpriteRenderer>();
					foreach(SpriteRenderer s in spr)
					{
						s.sortingLayerName = "UI";
					}
					
					// ... disable user Player Control script
					GetComponent<PlayerControl>().enabled = false;
					
					// ... disable the Gun script to stop a dead guy shooting a nonexistant bazooka
					GetComponentInChildren<Gun>().enabled = false;
					rigidbody2D.gravityScale = 0;
					foreach(SpriteRenderer s in spr)
					{
						s.enabled = false;
					}

					int i = Random.Range(0, deathClips.Length);
					AudioSource.PlayClipAtPoint(deathClips[i], transform.position);


					if (!createdSpirit) {
						createdSpirit = true;
						GameObject.Instantiate(playerSpirit,transform.position, transform.rotation);
					}
					// ... Trigger the 'Die' animation state
//					anim.SetTrigger("Die");
				}
			}
		}
	}

	void OnCollisionStay2D(Collision2D col)
	{
		// If the colliding gameobject is an Enemy...
		if (col.gameObject.tag == "Enemy")
			// ... and if the time exceeds the time of the last hit plus the time between hits...
			if (Time.time > lastHitTime + repeatDamagePeriod)
				OnCollisionEnter2D (col);

	}
	
	void TakeDamage (Transform enemy)
	{
		// Make sure the player can't jump.
		playerControl.jump = false;
		
		// Create a vector that's from the enemy to the player with an upwards boost.
		Vector3 hurtVector = transform.position - enemy.position + Vector3.up * 5f;

		// If the player is to the right of the enemy
		if (transform.position.x > enemy.position.x)
			hurtVector.x = 10f;
		else
			hurtVector.x = -10f;
		
		// Add a force to the player in the direction of the vector and multiply by the hurtForce.
		rigidbody2D.AddForce(hurtVector * hurtForce);
		
		// Reduce the player's health by 10.
		health -= damageAmount;
		
		// Update what the health bar looks like.
		UpdateHealthBar();
		
		// Play a random clip of the player getting hurt.
		AudioSource.PlayClipAtPoint(ouchClip, transform.position);
	}
	
	
	public void UpdateHealthBar ()
	{
		// Set the health bar's colour to proportion of the way between green and red based on the player's health.
		healthBar.material.color = Color.Lerp(Color.green, Color.red, 1 - health * 0.01f);
		
		// Set the scale of the health bar to be proportional to the player's health.
		healthBar.transform.localScale = new Vector3(healthScale.x * health * 0.01f, healthScale.y, 1);
	}

	//setting delay to 2 seconds
	IEnumerator wait(float seconds) {
		yield return new WaitForSeconds(seconds);
		Debug.Log ("Waited for: " + seconds);
	}
}
