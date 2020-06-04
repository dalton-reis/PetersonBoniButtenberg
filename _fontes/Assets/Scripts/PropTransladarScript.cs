using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PropTransladarScript : PropriedadePecaPadrao
{
    void Update()
    {     
        if (jaClicouEmAlgumObjeto() && Global.gameObjectName.Contains(Consts.Transladar))
        {
            tipoTransformacao = typeTransformacao.Transladar;

            if (!inicializou() || pieceChanged())
                mainMethod();

            if (Input.GetKey(KeyCode.Return))
                updatePosition();

            toggleChanged();            
        }            
    }
}
