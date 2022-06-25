using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu_two : MonoBehaviour
{
    public void New()
    {
        SceneManager.LoadScene(1);
    }
    public void Give_up()
    {
        int give_up = 2;
        GameObject.Find("Main Camera").GetComponent<Chess>().Judge(give_up);    
    }

    public void Regret()
    {
        GameObject.Find("Main Camera").GetComponent<Chess>().Regret_1();
    }
    public void Backhand()
    {
        int GetKey = -1;
        GameObject.Find("Main Camera").GetComponent<Chess>().Choice_first(GetKey);
        GameObject.Find("select").GetComponent<Cloaking>().Cloaking_1();
    }

    public void Forehand()
    {
        int GetKey = 1;
        GameObject.Find("Main Camera").GetComponent<Chess>().Choice_first(GetKey);
        GameObject.Find("select").GetComponent<Cloaking>().Cloaking_1();
    }
}
