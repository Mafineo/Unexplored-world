using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestScript : MonoBehaviour
{
    private bool Sppeeed = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Time.timeScale = 1f;
            GetComponent<Text>().enabled = false;
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 3f;
            GetComponent<Text>().text = "▶x3";
            GetComponent<Text>().enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Time.timeScale = 10f;
            GetComponent<Text>().text = "▶x10";
            GetComponent<Text>().enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Time.timeScale = 20f;
            GetComponent<Text>().text = "▶x20";
            GetComponent<Text>().enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            Time.timeScale = 100f;
            GetComponent<Text>().text = "▶x100";
            GetComponent<Text>().enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            Time.timeScale = 0.2f;
            GetComponent<Text>().text = "▶x0.2";
            GetComponent<Text>().enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            Time.timeScale = 0f;
            GetComponent<Text>().enabled = false;
        }
    }

    public void UseSpeed()
    {
        Sppeeed = !Sppeeed;
        if (Sppeeed) Time.timeScale = 20f;
        else Time.timeScale = 1f;
        GetComponent<Text>().text = (Time.timeScale != 1) ? "▶x" + Time.timeScale : null;
    }
}
