using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class PropCameraScript : PropCameraPadrao {

    public TMP_InputField mainInputField;
    public Transform cuboAmb;
    public Camera cam;

    public void Start()
    {
        if (mainInputField != null)
            mainInputField.onValueChanged.AddListener(delegate { AdicionaValorPropriedade(); });
        else
            preencheCampos();
    }    

    void Update()
    {
        if (jaClicouEmAlgumObjeto() && Global.propCameraGlobal.ExisteCamera)
        {
            if (!Global.propCameraGlobal.JaIniciouValores)
                preencheCampos();
        }

        if (podeAtualizarCamera)
        {
            AjustaCameraEmX();
            podeAtualizarCamera = false;
        }            
    }

    public void AdicionaValorPropriedade()
    {
        if (gameObject.GetComponent<TMP_InputField>().text != string.Empty)
            updatePosition(cam);
    }
}
