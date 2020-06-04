using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;
using UnityEngine.UI;

enum Transforms {PosX, PosY, PosZ};

public class PropCuboScript : PropCuboPadrao {

    public Toggle toggleField;

    public void Start()
    {
        if (toggleField != null)
            toggleField.onValueChanged.AddListener(delegate { AdicionaValorPropriedadeToggle(); });
    }        

    void Update()
    {
        if (jaClicouEmAlgumObjeto() && Global.gameObjectName.Contains(Consts.Cubo))
        {
            if (!inicializou() || pieceChanged())
                mainMethod();

            if (Input.GetKey(KeyCode.Return))
                updatePosition();

            if (gameObject.name == "PropCubo")
                updatePosition();
        }
    }

    private void AdicionaValorPropriedadeToggle()
    {
        toggleChanged();
    }


}
