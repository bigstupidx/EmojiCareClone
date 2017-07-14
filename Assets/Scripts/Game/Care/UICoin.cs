﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICoin : MonoBehaviour {
	private static UICoin instance = null;
	public static UICoin Instance{
		get{return instance;}
	}

	public MainHUDController hudController;
	public GameObject coinObject;
	List<GameObject> coinInstances;

	void Awake()
	{
		if (instance != null && instance != this) {
			Destroy(this.gameObject);  // destroy any other singleton object of this class
			return;
		} else instance = this;
	}

	public void GenerateCoinObject()
	{
		GameObject coinInstance = Instantiate(coinObject);
		RectTransform tempTransform = coinInstance.GetComponent<RectTransform>();
		tempTransform.SetParent(GetComponent<RectTransform>());
		tempTransform.anchoredPosition = Vector2.zero;
		tempTransform.localScale = Vector2.one;
		tempTransform.localRotation = Quaternion.identity;
		coinInstance.GetComponent<CoinObj>().OnCoinDestroyed += OnCoinDestroyed;
	}

	void OnCoinDestroyed(GameObject obj)
	{
		obj.GetComponent<CoinObj>().OnCoinDestroyed -= OnCoinDestroyed;
		PlayerData.Instance.playerCoin += 10;
		hudController.UpdatePlayerCoin();
	}
}