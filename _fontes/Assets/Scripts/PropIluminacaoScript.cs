using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Diagnostics;
using UnityEngine.UI;

public class PropIluminacaoScript : PropIluminacaoPadrao {

    public TMP_InputField mainInputField;
    public Toggle toggleField;

    public void Start()
    {    
        if (mainInputField != null)
            mainInputField.onValueChanged.AddListener(delegate { AdicionaValorPropriedadeInput(); });

        if (toggleField != null)
            toggleField.onValueChanged.AddListener(delegate { AdicionaValorPropriedadeToggle(); });

        if (clicouPeca)
        {
            propIluminacao = GameObject.Find("PropIluminacao");
            preencheCampos();
        }
            
    }

    void LateUpdate()
    {       
        if (jaClicouEmAlgumObjeto() && Global.gameObjectName.Contains(Consts.Iluminacao))
        {
            if (Global.propriedadePecas.ContainsKey(Global.gameObjectName))
            {
                if (!Global.propriedadePecas[Global.gameObjectName].JaIniciouValores)
                {
                    propIluminacao = GameObject.Find("PropIluminacao");
                    preencheCampos();
                }
                    
            }
        }
    }

    public void AdicionaValorPropriedadeInput()
    {
        if (gameObject.GetComponent<TMP_InputField>().text != string.Empty)
            updatePosition();
    }

    public void AdicionaValorPropriedadeToggle()
    {
        updatePosition();
    }

    public void Start(bool clicouPeca)
    {
        this.clicouPeca = clicouPeca;
        Start();
    }
}
