using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class TDScene : MonoBehaviour
{
    public bool tdEnabled = true;
    public Collider PlayerChar;
    public Material ZTriggerMat;
    public Material ActiveMat;
    public Material CeilingShadow;
    [HideInInspector]
    public bool isInTDZone;
    [HideInInspector]
    public TDZone ActiveZone;
    [HideInInspector]
    public float ActiveFloor;
    public bool VisibleTriggers = false;
    public bool OptimizeDeco = true;
    [Header("Scene Control")]
    public KeyCode ActiveKey = KeyCode.E;
    private float previousFloor;

    // Start is called before the first frame update
    void Start()
    {
        if ((PlayerChar) == null)
        {
            Debug.Log(PlayerChar);
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj)
            {
                PlayerChar = playerObj.GetComponent<Collider>();
            }
        }
        previousFloor = ActiveFloor;
    }

    public void SetPlayerChar(GameObject player)
    {
        Debug.Log(player);
        if (player)
        {
            PlayerChar = player.GetComponent<Collider>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Tell if character has changed zone floor
        if (isInTDZone)
        {
            if (ActiveFloor != previousFloor)
            {
                ActiveZone.updated = true;
                previousFloor = ActiveFloor;
            }
        }

    }
}
