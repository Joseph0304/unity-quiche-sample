using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuicBehaviourScript : MonoBehaviour
{

    private Quiche quiche;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Quiche.GetVersion());
        Quiche.DebugLog((line, argp) => {
            Debug.Log(line);
        });
        quiche = new Quiche();
        quiche.Connect("https://127.0.0.1:4433/index.html");
    }

    // Update is called once per frame
    void Update()
    {
        quiche.Poll();
    }
}
