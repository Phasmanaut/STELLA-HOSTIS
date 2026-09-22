using System;
using System.Collections;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FloatingPoints : MonoBehaviour
{
    public TextMeshPro floatingPointsText;
    public int pointWorth;
    public string message; //if set, shown instead of the points (e.g. "EXTRA LIFE!")
    private float timer;
    private void Start()
    {
        floatingPointsText.text = string.IsNullOrEmpty(message) ? $"+{pointWorth} " : message;
    }


    void Update()
    {
        transform.Translate(0, 1.5f * Time.deltaTime, 0);
        timer += Time.deltaTime;
        if (timer > 1)
        { 
        Destroy(gameObject);
        }
    }







}
