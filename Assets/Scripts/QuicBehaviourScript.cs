using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuicBehaviourScript : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(Quiche.GetVersion());
        var quiche = new Quiche();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
