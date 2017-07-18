﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinObj : MonoBehaviour {
	public Vector2 coinDestination = new Vector2(-313f,406f);
	public delegate void CoinDestroyed(GameObject obj);
	public event CoinDestroyed OnCoinDestroyed;

	Rigidbody2D rigidBody;
	RectTransform coinTransform;

	bool isClicked = false;

	void Awake()
	{
		rigidBody = GetComponent<Rigidbody2D>();
		coinTransform = GetComponent<RectTransform>();
	}

	void Start ()
	{
		rigidBody.AddForce(new Vector2(Random.Range(-2500f,2500f),50000f));
		StartCoroutine(CoinAutoCollect());
	}

	public void OnPointerClick()
	{
		if(isClicked == true) return;

		isClicked = true;
		StopCoroutine(CoinAutoCollect());
		StartCoroutine(CoinAbsorb());
	}

	IEnumerator CoinAutoCollect()
	{
		yield return new WaitForSeconds(10f);
		isClicked = true;
		StartCoroutine(CoinAbsorb());
	}

	IEnumerator CoinAbsorb()
	{
		rigidBody.gravityScale = 0f;
		GetComponent<CircleCollider2D>().enabled = false;

		float t = 0f;
		float x,y;
		while (t <= 1f){
			t += Time.deltaTime;
			x = Mathf.Lerp(coinTransform.anchoredPosition.x,coinDestination.x,t);
			y = Mathf.Lerp(coinTransform.anchoredPosition.y,coinDestination.y,t);
			GetComponent<RectTransform>().anchoredPosition = new Vector2(x,y);
			if(Mathf.Abs(y-coinDestination.y) < 1f || Mathf.Abs(x-coinDestination.x) < 1f) break;
			yield return new WaitForSeconds(Time.deltaTime);
		}

		if(OnCoinDestroyed != null) OnCoinDestroyed(gameObject);
		SoundManager.Instance.PlaySFX(eSFX.COIN);
		Destroy(gameObject);
	}
}