using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WireCompletion : MonoBehaviour
{

    public Text WiresText;
    public int wires = 0;
    public int maxWires;


    // Start is called before the first frame update
    void Start()
    {
        wires = 0; 
    }

    public void AddWires(int newWires)
    {
        wires += newWires;
    }

    public void UpdateWires()
    {
        WiresText.text = "0" + wires;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateWires();
    }
}
