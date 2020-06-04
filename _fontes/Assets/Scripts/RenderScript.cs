using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderScript : MonoBehaviour {
    
    void OnMouseOver()
    {        
        Vector3 pos = gameObject.transform.position;
        pos.y -= Input.mouseScrollDelta.y * 0.5f;

        if (pos.y > 619.8)
        {
            gameObject.transform.position = pos;
            moveObjetos();
        }

    }

    private void moveObjetos()
    {
        if (Global.listaObjetos != null)
        {
            foreach (GameObject go in Global.listaObjetos)
            {
                Vector3 pos = go.transform.position;
                pos.y -= Input.mouseScrollDelta.y * 0.5f;
                go.transform.position = pos;
            }
        }        
    }

    
    

    

}
