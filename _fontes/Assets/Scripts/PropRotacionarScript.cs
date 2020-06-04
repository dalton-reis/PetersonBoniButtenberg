using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class PropRotacionarScript : PropriedadePecaPadrao {

    void Update()
    {
        if (jaClicouEmAlgumObjeto() && Global.gameObjectName.Contains(Consts.Rotacionar))
        {
            tipoTransformacao = typeTransformacao.Rotacionar;

            if (!inicializou() || pieceChanged())
                mainMethod();

            if (Input.GetKey(KeyCode.Return))
                updatePosition();

            toggleChanged();
        }        
    }   
    
}
