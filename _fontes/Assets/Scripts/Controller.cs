﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Controller : MonoBehaviour {

    enum Slots { ObjGrafSlot, FormasSlot, TransformacoesSlot, IluminacaoSlot };

    private const string AMB = "Amb";
    private const string VIS = "Vis";

    public Camera cam;
    public GameObject posicaoColliderDestino;
    public GameObject abrePropriedade;
    public GameObject[] objetoModificado;

    public static Collider colliderPecas;

    private DropPeca dropPeca;
    private Vector3 screenPoint, offset, scanPos, startPos;
    private bool pecaJaEncaixada;
    private bool pecaVisible;
    private bool existeForma;
    private bool geraCopia;
    private bool podeGerarCopia;
    private GameObject o;
    private bool isFabricaPecas;
    private List<GameObject> listaObjZero;
    private List<string> listaProp;
    private Collider collPecas;
    private string concatNumber = "";
    private GameObject cloneFab;
    private bool podeDestruirObjeto;
    private float posColliderDestinoX, posColliderDestinoY, posColliderDestinoZ;
    private string objName;
    private string numObjetoGrafico;
    private Util_VisEdu criaFormas;

    private List<GameObject> listaAuxRender = new List<GameObject>();

    private RenderController renderController;

    private void Start()
    {
        scanPos = startPos = gameObject.transform.position;
        pecaJaEncaixada = false;
        pecaVisible = false;
        geraCopia = true;
        listaObjZero = new List<GameObject>();
        listaProp = new List<string>();
        renderController = new RenderController();
        criaFormas = new Util_VisEdu();

        //if (Global.podeInciarLista)
        //{
        //    Global.iniciaListaSequenciaSlots(0);
        //    Global.podeInciarLista = false;
        //}

    }

    private void Update()
    {
        //Global.atualizaListaSlot();

        scanPos = gameObject.transform.position;

        if (posicaoColliderDestino != null)
            dropPeca = posicaoColliderDestino.GetComponent<DropPeca>();

        //if (pecaVisible)
        //    demonstraObjeto.SetActive(true);        
    }

    void OnMouseDown()
    {
        Global.atualizaListaSlot();
        Global.slotName = "";

        screenPoint = cam.WorldToScreenPoint(scanPos);

        offset = scanPos - cam.ScreenToWorldPoint(
            new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z));

        podeGerarCopia = screenPoint.y > cam.pixelRect.height / 2;    

        if (podeGerarCopia)
        {
            GeraCopiaPeca();
        }
        else
        {
            if (Global.listaObjetos != null && Global.listaObjetos.Contains(gameObject))
            {
                if (Global.lastPressedButton != null)
                    Global.lastPressedButton.SetActive(false);

                if (!Global.propriedadePecas.ContainsKey(gameObject.name))
                {
                    PropriedadePeca prPeca = new PropriedadePeca();
                    prPeca.Nome = gameObject.name;
                    prPeca.PodeAtualizar = true;
                    prPeca.NomeCuboAmbiente = "CuboAmbiente" + getNumObjeto(Global.listaEncaixes[gameObject.name]);
                    prPeca.NomeCuboVis = "CuboVis" + getNumObjeto(Global.listaEncaixes[gameObject.name]);
                    
                    Global.propriedadePecas.Add(gameObject.name, prPeca); 
                    
                    Global.lastPressedButton = abrePropriedade.gameObject;
                    Global.gameObjectName = gameObject.name;

                    abrePropriedade.SetActive(true);                    

                    BotaoPropPadrao btn = new BotaoPropPadrao();
                    btn.setButton(GameObject.Find("BtnPropPecas"), GameObject.Find(getSohLetras(gameObject.name)));

                    if(gameObject.name.Contains(Consts.ObjetoGrafico))
                        new PropObjetoGraficoScript().Start();
                }
                else
                {
                    Global.gameObjectName = gameObject.name;
                    abrePropriedade.SetActive(true);
                    Global.lastPressedButton = abrePropriedade.gameObject;                    

                    BotaoPropPadrao btn = new BotaoPropPadrao();
                    btn.setButton(GameObject.Find("BtnPropPecas"), GameObject.Find(getSohLetras(gameObject.name)));

                    //Chama o script de iluminação
                    if (gameObject.name.Contains(Consts.Iluminacao))
                        new PropIluminacaoScript().Start(true);

                    if (gameObject.name.Contains(Consts.ObjetoGrafico))
                        new PropObjetoGraficoScript().Start();
                }                
            }        

        }
        // O atributo "posicaoColliderDestino" está null porque o GameObject Iluminacao foi destruído.
        // Então deve ser atribuído novamente o GameObject "IluminacaoSlot" ao atributo "posicaoColliderDestino".   
        if (posicaoColliderDestino == null && gameObject.name == Consts.Iluminacao)
        {
            GameObject IluminacaoSlot = GameObject.Find("IluminacaoSlot");
            posicaoColliderDestino = IluminacaoSlot;
        }

    }

    void OnMouseDrag()
    {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);
        Vector3 curPosition = cam.ScreenToWorldPoint(curScreenPoint) + offset;
        transform.position = curPosition;
    }

    private void OnMouseUp()
    {    
        //if (Global.podeExcruiObjeto)
        if (podeExcluirObjeto())
        {
            Global.podeExcruiObjeto = false;
            processaExclusao();            
        }            
        else if (podeEncaixar())
        {  
            float incX = 0;
            float incY = 0;            

            Global.addObject(gameObject);

            ajustaPecaAoSlot(ref incX, ref incY);

            if (podeGerarCopia)
            {
                if (gameObject.name.Contains("Transladar") || gameObject.name.Contains("Rotacionar") || gameObject.name.Contains("Escalar"))
                {    
                    string numObjetoGrafico = getNumeroSlotObjetoGrafico();
                    GameObject ObjGrafSlot = GameObject.Find("ObjGraficoSlot" + numObjetoGrafico);

                    //Retorna  de TransformacoesSlot
                    string slot = "";

                    for (int i = 0; i < ObjGrafSlot.transform.childCount; i++)
                    {
                        if (ObjGrafSlot.transform.GetChild(i).name.Contains("TransformacoesSlot"))
                        {
                            if (Global.listaEncaixes.ContainsValue(ObjGrafSlot.transform.GetChild(i).name))
                            {
                                slot = ObjGrafSlot.transform.GetChild(i).name;                                
                            }                                
                        }
                    }

                    int val = 0;
                    string countTransformacoes = "";
                    Int32.TryParse(slot.Substring(slot.IndexOf("_") + 1), out val);

                    if (val > 0)
                        countTransformacoes = Convert.ToString(val + 1);
                    else                    
                        countTransformacoes = "1";                     
                    //-------------

                    GameObject t = GameObject.Find(slot);  
                    GameObject cloneTrans = Instantiate(t, t.transform.position, t.transform.rotation, t.transform.parent);
                    cloneTrans.name = "TransformacoesSlot" + numObjetoGrafico + "_" + countTransformacoes;
                    cloneTrans.transform.position = new Vector3(t.transform.position.x, t.transform.position.y - 3f, t.transform.position.z); 

                    addTransformacoeSequenciaSlots(cloneTrans.name);

                    posicaoColliderDestino = t;

                    if (renderController == null)
                        renderController = new RenderController();

                    renderController.ResizeBases(t, Consts.Transladar, true); // o Segundo parâmetro pode ser qualquer tranformação 
                   
                    concatNumber = numObjetoGrafico;

                    addGameObjectTree("GameObjectAmb" + getNumeroSlotObjetoGrafico(), AMB, "CuboAmbiente" + getNumeroSlotObjetoGrafico());
                    addGameObjectTree("CuboVisObject" + getNumeroSlotObjetoGrafico(), VIS, "CuboVis" + getNumeroSlotObjetoGrafico());

                    configuraIluminacao("-");

                    reorganizaObjetos(numObjetoGrafico);

                }
                else if (gameObject.name.Contains("ObjetoGrafico"))
                {
                    if (DropPeca.countObjetosGraficos == 0)
                    {
                        if (criaFormas == null)
                            criaFormas = new Util_VisEdu();

                        criaFormas.criaFormasVazias();
                    }                        

                    string countObjGrafico = "";
                    if (DropPeca.countObjetosGraficos > 0)
                        countObjGrafico = Convert.ToString(DropPeca.countObjetosGraficos);

                    if (!Equals(countObjGrafico, string.Empty))
                        createGameObjectTree(DropPeca.countObjetosGraficos);

                    Global.iniciaListaSequenciaSlots(DropPeca.countObjetosGraficos);

                    GameObject t = GameObject.Find("ObjGraficoSlot" + countObjGrafico);

                    GameObject cloneObjGrafico = Instantiate(t, t.transform.position, t.transform.rotation, t.transform.parent);
                    cloneObjGrafico.name = "ObjGraficoSlot" + Convert.ToString(++DropPeca.countObjetosGraficos);
                    cloneObjGrafico.transform.position = new Vector3(t.transform.position.x, t.transform.position.y - 11f, t.transform.position.z);

                    posicaoColliderDestino = t;

                    setActiveAndRenameGameObject(t, cloneObjGrafico);

                    if (renderController == null)
                        renderController = new RenderController();

                    renderController.ResizeBases(t, Consts.ObjetoGrafico, true);
                    adicionaObjetoRender();                    
                }
                else if (gameObject.name.Contains("Cubo"))
                {        
                    string numFormas = "";
                    GameObject t;

                    numFormas = getNumeroSlotObjetoGrafico();                    

                    t = GameObject.Find("FormasSlot" + numFormas);

                    posicaoColliderDestino = t;                     
                    adicionaObjetoRender();

                    if (Global.cameraAtiva && new PropIluminacaoPadrao().existeIluminacao())
                        GameObject.Find("CameraVisInferior").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("Formas");

                    // Verificar se o Objeto Gráfico pai está ativo para demonstrar o cubo.
                    string goObjGraficoSlot = GameObject.Find(Global.listaEncaixes[gameObject.name]).transform.parent.name;

                    string peca = string.Empty;

                    // Verifica peça conectada ao slot
                    foreach (KeyValuePair<string, string> pecas in Global.listaEncaixes)
                    {
                        if (Equals(pecas.Value, goObjGraficoSlot))
                        {
                            peca = pecas.Key;
                            break;
                        }
                    }
                    
                    MeshRenderer mr = GameObject.Find("CuboAmbiente" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();

                    bool statusCubo = false;

                    // Verifica se o Objeto Gráfico ja foi clicado.
                    foreach (KeyValuePair<string, PropriedadePeca> pec in Global.propriedadePecas)
                    {                            
                        if (Equals(pec.Key, peca))
                        {
                            if (Global.propriedadePecas[peca].Ativo)
                                statusCubo = true;                            
                        }
                        else
                            statusCubo = true;                      
                    }            
                    
                    if (statusCubo || Global.propriedadePecas.Count == 0)
                        mr.enabled = true;

                    #region Código antigo             

                    foreach (KeyValuePair<string, string> slot in Global.listaEncaixes)
                    {
                        if (Equals(slot.Value, "IluminacaoSlot" + getNumeroSlotObjetoGrafico()))
                        {
                            mr = GameObject.Find("CuboVis" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();

                            if (Global.cameraAtiva)
                                mr.enabled = true;

                            Global.cuboVisComIluminacao.Add(mr.name);
                            break;
                        }
                    }
                    #endregion
                }

                else if (gameObject.name.Contains("Iluminacao"))
                {
                    string numIlum = "";
                    GameObject t;

                    numIlum = getNumeroSlotObjetoGrafico();

                    t = GameObject.Find("IluminacaoSlot" + numIlum);

                    posicaoColliderDestino = t;
                    adicionaObjetoRender();

                    //Verifica se há câmera e aplica luz aos objetos com Layer "Formas"
                    if (Global.cameraAtiva)
                        GameObject.Find("CameraVisInferior").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("Formas");

                    PropriedadePeca prPeca = new PropriedadePeca();
                    prPeca.Nome = gameObject.name;
                    prPeca.PodeAtualizar = true;
                    prPeca.NomeCuboAmbiente = "CuboAmbiente" + getNumObjeto(Global.listaEncaixes[gameObject.name]);
                    prPeca.NomeCuboVis = "CuboVis" + getNumObjeto(Global.listaEncaixes[gameObject.name]);
                    prPeca.TipoLuz = 0;
                    Global.propriedadePecas.Add(gameObject.name, prPeca);

                    if (gameObject.name.Length > "Iluminacao".Length)
                        CriaLightObject(gameObject.name.Substring("Iluminacao".Length, 1));

                    #region Código antigo
                    MeshRenderer mr = GameObject.Find("CuboVis" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();
                    mr.enabled = true;

                    foreach (KeyValuePair<string, string> slot in Global.listaEncaixes)
                    {
                        if (Equals(slot.Value, "FormasSlot" + getNumeroSlotObjetoGrafico()))
                        {
                            mr = GameObject.Find("CuboVis" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();

                            if (Global.cameraAtiva)
                                mr.enabled = true;

                            Global.cuboVisComIluminacao.Add(mr.name);
                            break;
                        }
                    }
                    #endregion

                }
                else if (gameObject.name.Contains("Camera"))
                {
                    PropIluminacaoPadrao lightProperty = new PropIluminacaoPadrao();

                    new PropCameraPadrao().demosntraCamera(true);                    
                    
                    if (lightProperty.existeIluminacao())
                        GameObject.Find("CameraVisInferior").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("Formas");
                    
                    //lightProperty.AtualizaCamera();

                    // Verifica se existem cubos e iluminações mas a câmera ainda não foi colocada.
                    bool exitemFormas = false;

                    foreach (string nomeCuboVis in Global.cuboVisComIluminacao)
                    {
                        //GameObject.Find(nomeCuboVis).GetComponent<MeshRenderer>().enabled = true;
                        exitemFormas = true;
                    }

                    //if (exitemFormas)
                        Global.propCameraGlobal.ExisteCamera = true;
                }

            }
            // A posição x é incrementada para que a peça fique no local correto.

            posColliderDestinoX = posicaoColliderDestino.transform.position.x + incX;
            posColliderDestinoY = posicaoColliderDestino.transform.position.y - incY;
            posColliderDestinoZ = posicaoColliderDestino.transform.position.z;

            transform.position = new Vector3(posColliderDestinoX, posColliderDestinoY, posColliderDestinoZ);
            pecaJaEncaixada = true;
        }
        else
        {
            bool podeDestruir = screenPoint.y > cam.pixelRect.height / 2;

            if (podeDestruir && !Global.listaEncaixes.ContainsKey(gameObject.name))
            {
                transform.position = startPos;
                Destroy(cloneFab);
                screenPoint = cam.WorldToScreenPoint(scanPos);

                int ObjectValue = --Global.listObjectCount[objName];
                Global.listObjectCount.Remove(objName);
                Global.listObjectCount.Add(objName, ObjectValue);
            }
            else
            {
                if (!Global.listaEncaixes.ContainsKey(gameObject.name))
                    transform.position = new Vector3(posColliderDestinoX, posColliderDestinoY, posColliderDestinoZ);

                float incX = 0;
                float incY = 0;

                GameObject newPosition = GameObject.Find(Global.listaEncaixes[gameObject.name]);

                ajustaPecaAoSlot(ref incX, ref incY);
                transform.position = new Vector3(newPosition.transform.position.x + incX,
                                                 newPosition.transform.position.y - incY,
                                                 newPosition.transform.position.z);
            }

        }
            
    }

    private void ajustaPecaAoSlot(ref float incX, ref float incY)
    {
        switch (getNomeObjeto(gameObject.name))
        {
            case Consts.Iluminacao:
                setIluminacao(ref incX, ref incY);
                break;
            case Consts.ObjetoGrafico:
                setObjetoGrafico(ref incX, ref incY);
                break;
            case Consts.Cubo:
                setCubo(ref incX, ref incY);
                break;
            case Consts.Poligono:
                setPoligono(ref incX, ref incY);
                break;
            case Consts.Spline:
                setSpline(ref incX, ref incY);
                break;
            case Consts.Camera:
                setCamera(ref incX, ref incY);
                break;
            case Consts.Rotacionar:
                setRotacionar(ref incX, ref incY);
                break;
            case Consts.Transladar:
                setTransladar(ref incX, ref incY);
                break;
            default:
                incX = 2.95f;
                incY = 0;
                break;
        }
    }

    private void SetActiveObjects()
    {
        for (int i = 0; i < objetoModificado.Length; i++)
        {
            objetoModificado[i].SetActive(true);
        }
    }

    private void mostrarPeca()
    {
        if (existeForma)
        {
           // demonstraObjeto.SetActive(true);
        }
    }

    private void setIluminacao(ref float x, ref float y)
    {
        x = 3.8f;
        existeForma = true;
        //mostrarPeca();
    }

    private void setObjetoGrafico(ref float x, ref float y)
    {
        x = 3.9f;
        //SetActiveObjects();
    }

    private void setCubo(ref float x, ref float y)
    {
        x = 2.2f;
        y = 0.15f;
        pecaVisible = true;
        existeForma = true;
    }

    private void setPoligono(ref float x, ref float y)
    {
        x = 2.9f;
        y = -0.08f;
        pecaVisible = true;
        existeForma = true;
    }

    private void setSpline(ref float x, ref float y)
    {
        x = 2.33f;
        pecaVisible = true;
        existeForma = true;
    }

    private void setCamera(ref float x, ref float y)
    {
        x = 2.95f;
        y = 0;
        pecaVisible = true;
    }

    private void setRotacionar(ref float x, ref float y)
    {
        x = 3.8f;
        y = 0;
    }

    private void setTransladar(ref float x, ref float y)
    {
        x = 3.8f;
        y = 0;
    }

    private string getNomeObjeto(string nomeObj)
    {
        if (nomeObj.Contains(Consts.Rotacionar))
            return Consts.Rotacionar;
        else if (nomeObj.Contains(Consts.Transladar))
            return Consts.Transladar;
        else if (nomeObj.Contains(Consts.Escalar))
            return Consts.Escalar;
        else if (nomeObj.Contains(Consts.Iluminacao))
            return Consts.Iluminacao;
        else if (nomeObj.Contains(Consts.ObjetoGrafico))
            return Consts.ObjetoGrafico;
        else if (nomeObj.Contains(Consts.Cubo))
            return Consts.Cubo;
        else if (nomeObj.Contains(Consts.Poligono))
            return Consts.Poligono;
        else if (nomeObj.Contains(Consts.Spline))
            return Consts.Spline;
        else if (nomeObj.Contains(Consts.Camera))
            return Consts.Camera;
        else
            return "";
    }

    private void configuraIluminacao(string sinal)
    {
        float valorInc = 0;

        if (Equals(sinal, "+"))
            valorInc = 3f;
        else
            valorInc = -3f;

        GameObject ilumunacao = GameObject.Find("IluminacaoSlot" + concatNumber);
        Vector3 pos = ilumunacao.transform.position;
        pos.y = pos.y + valorInc;
        ilumunacao.transform.position = pos;

        // Se a peça "Iluminação já foi selecionada, será devidamente reposicionada"        
        GameObject IlumPeca = GameObject.Find("Iluminacao" + concatNumber);

        if (Global.listaObjetos.Contains(IlumPeca))
            IlumPeca.transform.position = new Vector3(IlumPeca.transform.position.x, pos.y, IlumPeca.transform.position.z);  
    }


    private void zerarTransform(GameObject go)
    {
        if (go != null)
        {
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            if (go.name != "GameObjectCubo")
                go.transform.localScale = new Vector3(1, 1, 1);
            else
                go.transform.localScale = new Vector3(1000, 1000, 1000);
        }
    }    
    
    // firstNameObject = primeiro GameObject pai
    // extensionName   = concatena com o nome do GameObject
    // mainGameObject  = nome do GameObject princial
    private void addGameObjectTree(string firstNameObject, string extensionName, string mainGameObject)
    {     
        GameObject goUltimo = GameObject.Find(firstNameObject);

        try
        {
            while (goUltimo.transform.GetChild(0).name != string.Empty)
            {
                if (Equals(goUltimo.transform.GetChild(0).name, mainGameObject))
                    goUltimo = GameObject.Find(goUltimo.transform.GetChild(1).name);
                else if (goUltimo.transform.GetChild(0).name.Contains("CuboAmbiente") || goUltimo.transform.GetChild(0).name.Contains("CuboVis"))
                    break;
                else
                    goUltimo = GameObject.Find(goUltimo.transform.GetChild(0).name);
            }
        }
        catch (Exception) { }

        GameObject objPai = GameObject.Find(goUltimo.name);
        GameObject go = Instantiate(new GameObject(), Vector3.zero, Quaternion.identity);

        go.name = gameObject.name + extensionName;
        go.transform.parent = objPai.transform;

        GameObject mainGO = GameObject.Find(mainGameObject);

        if (mainGO != null)
            mainGO.transform.parent = go.transform;

        zerarTransform(go);
        zerarTransform(mainGO);
    }

    private void createGameObjectTree(int numObj)
    {
        // Cria novo 'CuboVisObjectMain'
        string numObjGraf = Convert.ToString(numObj);
        GameObject goCubo = Global.cuboVis; //GameObject.Find("CuboVisObjectMain");

        GameObject newGo = Instantiate(goCubo, goCubo.transform.position, goCubo.transform.rotation, goCubo.transform.parent);
        newGo.name = "CuboVisObjectMain" + numObjGraf;
        
        newGo.transform.GetChild(0).name += numObjGraf; // CuboVisObject
        newGo.transform.GetChild(0).GetChild(0).name += numObjGraf; // CuboVis

        // Cria novo 'PosicaoAmb'
        GameObject goPosAmb = Global.posAmb;//GameObject.Find("PosicaoAmb");

        GameObject newGoPos = Instantiate(goPosAmb, goPosAmb.transform.position, goPosAmb.transform.rotation, goPosAmb.transform.parent);
        newGoPos.name = "PosicaoAmb" + numObjGraf;

        newGoPos.transform.GetChild(0).name += numObjGraf; // GameObjectAmb
        newGoPos.transform.GetChild(0).GetChild(0).name += numObjGraf; // CuboAmbiente
    }

    private void setActiveAndRenameGameObject(GameObject goActive, GameObject goRename)
    {     
        for (int i = 0; i < goActive.transform.childCount; i++)
        {
            if (goActive.transform.GetChild(i).name.Contains("Slot") || goActive.transform.GetChild(i).name.Contains("Base"))
            {
                goActive.transform.GetChild(i).gameObject.SetActive(true);

                if (goActive.transform.GetChild(i).name.Contains("Slot"))
                    goRename.transform.GetChild(i).name =
                        goRename.transform.GetChild(i).name.Substring(0, goRename.transform.GetChild(i).name.IndexOf("Slot") + 4) + Convert.ToString(DropPeca.countObjetosGraficos);                
                else
                {                    
                    goRename.transform.GetChild(i).name = goRename.transform.GetChild(i).name.Substring(0, goRename.transform.GetChild(i).name.IndexOf("GO") + 2) + Convert.ToString(DropPeca.countObjetosGraficos);

                    for(int j = 0; j < goRename.transform.GetChild(i).childCount; j++)
                    {
                        int value = 0;
                        Int32.TryParse(goRename.transform.GetChild(i).GetChild(j).name.Substring(goRename.transform.GetChild(i).GetChild(j).name.Length -1, 1), out value);

                        if (value > 0)
                            goRename.transform.GetChild(i).GetChild(j).name = goRename.transform.GetChild(i).GetChild(j).name.Substring(0, goRename.transform.GetChild(i).GetChild(j).name.Length - 1) + Convert.ToString(DropPeca.countObjetosGraficos);
                        else
                            goRename.transform.GetChild(i).GetChild(j).name += Convert.ToString(DropPeca.countObjetosGraficos);
                    }                        
                }                    
            }
            else
                continue;
        }    
    }     
    
    private GameObject GetTransformacoesSlotVazio(GameObject parent)
    {        
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).name.Contains("TransformacoesSlot_")){
                return parent.transform.GetChild(i).gameObject;
            }
        }

        return null;
    }

    private void adicionaObjetoRender()
    {
        if (DropPeca.parentName != "Render")
        {
            if (!Global.listaPosicaoObjetosRender.ContainsKey(DropPeca.parentName))
            {
                List<GameObject> arrayObj = new List<GameObject>();
                arrayObj.Add(gameObject);
                Global.listaPosicaoObjetosRender.Add(DropPeca.parentName, arrayObj);
            }
            else
            {
                List<GameObject> arrayObj = Global.listaPosicaoObjetosRender[DropPeca.parentName];
                arrayObj.Add(gameObject);
                Global.listaPosicaoObjetosRender.Remove(DropPeca.parentName);
                Global.listaPosicaoObjetosRender.Add(DropPeca.parentName, arrayObj);
            }
        }
    }

    private bool podeEncaixar()
    {
        const float VALOR_APROXIMADO = 2;

        float pecaY = transform.position.y;        

        foreach (KeyValuePair<string, float> slot in Global.listaPosicaoSlot)  // Slot / posição no eixo y
        {
            //Verifica se o encaixe existe na lista 
            if (slot.Key.Contains(Global.GetSlot(gameObject.name)))
            {
                //Verifica se a peça está próxima do encaixe e se o Slot ainda não está na lista de encaixes.
                if(slot.Value + VALOR_APROXIMADO > pecaY && slot.Value - VALOR_APROXIMADO < pecaY 
                    && !Global.listaEncaixes.ContainsValue(slot.Key))
                {
                    if (!Global.listaEncaixes.ContainsKey(gameObject.transform.name))
                        Global.listaEncaixes.Add(gameObject.transform.name, slot.Key);
                    else if (Global.listaEncaixes.ContainsKey(gameObject.transform.name) 
                        && Global.listaEncaixes[gameObject.name] != slot.Key)
                    {
                        reposicionaSlots(Global.listaEncaixes[gameObject.name], slot.Key);  
                        return false;
                    }
                    else
                        return false;

                    return true;
                }
            }
        }

        return false;
    }

    private void reposicionaSlots(string slotOrigem, string slotDestino)
    {
        //Permite reposicionar o slot somente se for um 'TransformacoesSlot' e os slots estiverem dentro do mesmo Objeto Gráfico
        if (slotOrigem.Contains("TransformacoesSlot") && getNumObjeto(slotOrigem) == getNumObjeto(slotDestino)) 
        {
            float incX = 0;
            float incY = 0;

            ajustaPecaAoSlot(ref incX, ref incY);

            GameObject slot = GameObject.Find(slotDestino);

            transform.position = new Vector3(slot.transform.position.x + incX,
                                             slot.transform.position.y - incY,
                                             slot.transform.position.z);

            Destroy(GameObject.Find(slotOrigem));

            GameObject t = GameObject.Find(slotDestino);

            int val = 0;
            string countTransformacoes = "";
            Int32.TryParse(slotDestino.Substring(slotDestino.IndexOf("_") + 1), out val);

            if (val > 0)
                countTransformacoes = Convert.ToString(val + 1);
            else
                countTransformacoes = "1";

            GameObject cloneTrans = Instantiate(t, t.transform.position, t.transform.rotation, t.transform.parent);
            cloneTrans.name = "TransformacoesSlot" + getNumeroSlotObjetoGrafico() + "_" + countTransformacoes;
            cloneTrans.transform.position = new Vector3(t.transform.position.x, t.transform.position.y - 3f, t.transform.position.z);

            addTransformacoeSequenciaSlots(cloneTrans.name);

            posicaoColliderDestino = t;

            bool podeReposicionar = false;

            for (int i = 0; i < Global.listaSequenciaSlots.Count; i++)
            {
                if (i > 2)
                {
                    if (Global.listaSequenciaSlots[i].Contains("TransformacoesSlot" + getNumeroSlotObjetoGrafico()) && getNumObjeto(Global.listaSequenciaSlots[i]) == getNumeroSlotObjetoGrafico())
                    {
                        if (Global.listaSequenciaSlots[i] == slotOrigem)
                        {
                            podeReposicionar = true;
                            continue;
                        }

                        if (podeReposicionar)
                        {
                            GameObject GO_Slot = GameObject.Find(Global.listaSequenciaSlots[i]);
                            GameObject GO_Peca;

                            foreach (KeyValuePair<string, string> pair in Global.listaEncaixes)
                            {
                                if (pair.Value == Global.listaSequenciaSlots[i])
                                {
                                    GO_Peca = GameObject.Find(pair.Key);
                                    GO_Peca.transform.position = new Vector3(GO_Peca.transform.position.x, GO_Peca.transform.position.y + 3f, GO_Peca.transform.position.z);
                                    break;
                                }
                            }

                            GO_Slot.transform.position = new Vector3(GO_Slot.transform.position.x, GO_Slot.transform.position.y + 3f, GO_Slot.transform.position.z);
                        }
                    }
                }                
            }

            Global.listaSequenciaSlots.Remove(slotOrigem);
            Global.listaEncaixes.Remove(gameObject.name);
            Global.listaEncaixes.Add(gameObject.name, slotDestino);

            reposicionaPosicaoAmbCuboVis();

            AtualizaTrasformGameObjectAmb();
            AtualizaTrasformGameObjectAmb();
        }

    }

    private void addTransformacoeSequenciaSlots(string slot)
    {
        string numObj = getNumeroSlotObjetoGrafico();
        bool encontrouTransf = false;
        bool isTransf = false;

        for (int i = 0; i < Global.listaSequenciaSlots.Count; i++)
        {
            isTransf = Global.listaSequenciaSlots[i].Contains("TransformacoesSlot" + numObj);

            if (Global.listaSequenciaSlots[i].Contains("TransformacoesSlot" + numObj))
            {
                encontrouTransf = true;
                continue;
            }                    
            else if (encontrouTransf && !isTransf)
            {
                Global.listaSequenciaSlots.Insert(i, slot);
                break;
            }
        }
    }

    private void GeraCopiaPeca()
    {
        objName = getNomeObjeto(gameObject.name);
        int ObjectValue = 1;

        if (Global.listObjectCount.ContainsKey(objName))
        {
            ObjectValue = ++Global.listObjectCount[objName];
            Global.listObjectCount.Remove(objName);
            Global.listObjectCount.Add(objName, ObjectValue);
        }
        else
            Global.listObjectCount.Add(objName, ObjectValue);

        string concatNum = "1";

        concatNum = ObjectValue == 1 ? concatNum : Convert.ToString(ObjectValue);

        cloneFab = Instantiate(gameObject, gameObject.transform.position, gameObject.transform.rotation, gameObject.transform.parent);
        cloneFab.name = objName + concatNum;
        cloneFab.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, gameObject.transform.position.z);        
    }  
    
    private void processaExclusao()
    {     
        string slotOrigem = Global.listaEncaixes[gameObject.name];        

        bool podeReposicionar = false;

        if (Global.GetSlot(gameObject.name).Contains("TransformacoesSlot"))
        {
            GameObject render = GameObject.Find("Render");
            GameObject objGrafico = GameObject.Find(Global.listaEncaixes[gameObject.name]).transform.parent.gameObject;            

            for (int i = 0; i < render.transform.childCount; i++)
            {
                if (podeReposicionar && render.transform.GetChild(i).name.Contains("ObjGraficoSlot"))
                {
                    render.transform.GetChild(i).position = new Vector3(render.transform.GetChild(i).position.x, render.transform.GetChild(i).position.y + 3f, render.transform.GetChild(i).position.z);

                    foreach (KeyValuePair<string, string> pair in Global.listaEncaixes)
                    {
                        if (pair.Value == render.transform.GetChild(i).name)
                        {
                            GameObject GO_Peca = GameObject.Find(pair.Key);
                            GO_Peca.transform.position = new Vector3(GO_Peca.transform.position.x, GO_Peca.transform.position.y + 3f, GO_Peca.transform.position.z);

                            for (int j = 0; j < render.transform.GetChild(i).transform.childCount; j++)
                            {
                                GameObject GO_PecaAux;
                                foreach (KeyValuePair<string, string> peca in Global.listaEncaixes)
                                {
                                    if (peca.Value == render.transform.GetChild(i).transform.GetChild(j).name)
                                    {
                                        GO_PecaAux = GameObject.Find(peca.Key);
                                        GO_PecaAux.transform.position = new Vector3(GO_PecaAux.transform.position.x, GO_PecaAux.transform.position.y + 3f, GO_PecaAux.transform.position.z);
                                        break;
                                    }
                                }
                            }

                            break;
                        }
                    }                    
                }                    

                if (!Equals(render.transform.GetChild(i).name, objGrafico.transform.name))
                    continue;

                for (int j = 0; j < objGrafico.transform.childCount; j++)
                {
                    if (Equals(objGrafico.transform.GetChild(j).name, slotOrigem))
                    {
                        podeReposicionar = true;
                        continue;
                    }

                    if (podeReposicionar)
                    {
                        GameObject GO_Slot = GameObject.Find(objGrafico.transform.GetChild(j).name);
                        GameObject GO_Peca;

                        foreach (KeyValuePair<string, string> pair in Global.listaEncaixes)
                        {
                            if (pair.Value == objGrafico.transform.GetChild(j).name)
                            {
                                GO_Peca = GameObject.Find(pair.Key);
                                GO_Peca.transform.position = new Vector3(GO_Peca.transform.position.x, GO_Peca.transform.position.y + 3f, GO_Peca.transform.position.z);
                                break;
                            }
                        }

                        if (!GO_Slot.name.Contains("Base") && !GO_Slot.name.Contains("IluminacaoSlot"))
                            GO_Slot.transform.position = new Vector3(GO_Slot.transform.position.x, GO_Slot.transform.position.y + 3f, GO_Slot.transform.position.z);  
                    }
                }                 
                
            }

            renderController.ResizeBases(GameObject.Find(slotOrigem), Consts.Transladar, false);

            excluiHierarquiaOuAlteraPropriedade(Slots.TransformacoesSlot);

            Destroy(GameObject.Find(Global.listaEncaixes[gameObject.name]));
            Destroy(GameObject.Find(gameObject.name));
            Global.listaSequenciaSlots.Remove(Global.listaEncaixes[gameObject.name]);
            Global.removeObject(gameObject);
            AtualizaTrasformGameObjectAmb();
            Global.listaEncaixes.Remove(gameObject.name);
            Global.propriedadePecas.Remove(gameObject.name);

            configuraIluminacao("+");            
        }
        else if (Global.GetSlot(gameObject.name).Contains("ObjGraficoSlot"))
        {
            excluiHierarquiaOuAlteraPropriedade(Slots.ObjGrafSlot);
            exclusaoCompletaObjetos(Slots.ObjGrafSlot);

            if (!new PropIluminacaoPadrao().existeIluminacao())
                GameObject.Find("CameraVisInferior").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("Nothing");
        }
        else
        {
            Destroy(gameObject);
            Global.listaSequenciaSlots.Remove(Global.listaEncaixes[gameObject.name]);
            Global.removeObject(gameObject);            

            if (Global.GetSlot(gameObject.name).Contains("FormasSlot"))
                excluiHierarquiaOuAlteraPropriedade(Slots.FormasSlot);
            else if (Global.GetSlot(gameObject.name).Contains("IluminacaoSlot"))
            {
                excluiHierarquiaOuAlteraPropriedade(Slots.IluminacaoSlot);

                if (!new PropIluminacaoPadrao().existeIluminacao())
                    GameObject.Find("CameraVisInferior").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("Nothing");

                Global.propriedadeIluminacao.Remove(gameObject.name);

                if (gameObject.name.Length > "Iluminacao".Length)
                    Destroy(GameObject.Find("LightObjects" + gameObject.name));
                else
                {
                    if (Global.propriedadePecas.ContainsKey(gameObject.name))
                    {
                        PropIluminacaoPadrao luz = new PropIluminacaoPadrao();
                        luz.AtivaIluminacao(luz.GetTipoLuzPorExtenso(Global.propriedadePecas[gameObject.name].TipoLuz) + gameObject.name, false);
                        Global.propriedadePecas[gameObject.name].JaInstanciou = false;
                    }                        
                }                
            }                
            else if (Global.GetSlot(gameObject.name).Contains("CameraSlot"))
            {
                //Global.propCameraGlobal.JaIniciouValores = false; // seta 'false' para que quando incluir outra câmera preencha os campos de propriedade com valores padrões.
                Global.propCameraGlobal.ExisteCamera = false; // seta 'false' para que não atualize as propriedades.

                new PropCameraPadrao().demosntraCamera(false);

                GameObject.Find("CameraVisInferior").GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer("Nothing");

                // Desabilita todos CuboVis.
                foreach (string nomeCubiVis in Global.cuboVisComIluminacao)
                {
                    //GameObject.Find(nomeCubiVis).GetComponent<MeshRenderer>().enabled = false;
                }
            }

            Global.listaEncaixes.Remove(gameObject.name);
            Global.propriedadePecas.Remove(gameObject.name);
        }            
    }

    private string getNumeroSlotObjetoGrafico()
    {        
        GameObject render = GameObject.Find("Render");
        float value = 0f;
        string numSlotObjGrafico = "";

        for (int i = 0; i < render.transform.childCount; i++)
        {
            if (render.transform.GetChild(i).name.Contains("ObjGraficoSlot"))
            {
                if (Global.listaPosicaoSlot.ContainsKey(render.transform.GetChild(i).name))
                {
                    if (Global.listaPosicaoSlot.TryGetValue(render.transform.GetChild(i).name, out value))
                    {
                        if (transform.position.y < value)
                        {
                            if (render.transform.GetChild(i).name.Length > "ObjGraficoSlot".Length)
                                numSlotObjGrafico = render.transform.GetChild(i).name.Substring(render.transform.GetChild(i).name.IndexOf("Slot") + 4, 1);
                        }
                    }
                }
            }
        }        

        return numSlotObjGrafico;
    }

    private void reorganizaObjetos(string numObjeto)
    {
        const string ObjGrafico = "ObjGraficoSlot";
        bool podeOrganizarProximoObjeto = false;
        GameObject goRender = GameObject.Find("Render");

        for(int i = 0; i < goRender.transform.childCount; i++)
        {
            if(goRender.transform.GetChild(i).name.Contains(ObjGrafico))
            {
                if (podeOrganizarProximoObjeto)
                {
                    Vector3 pos = goRender.transform.GetChild(i).position;
                    pos.y -= 3;
                    goRender.transform.GetChild(i).position = pos;

                    foreach (KeyValuePair<string, string> encaixe in Global.listaEncaixes)
                    {
                        if (Equals(getNumObjeto(encaixe.Value), getNumObjeto(goRender.transform.GetChild(i).name)))
                        {
                            GameObject goPeca = GameObject.Find(encaixe.Key);
                            pos = goPeca.transform.position;
                            pos.y -= 3;
                            goPeca.transform.position = pos;
                        }
                    }
                }

                if (goRender.transform.GetChild(i).name == ObjGrafico + numObjeto)
                {
                    podeOrganizarProximoObjeto = true;                    
                }         
            }
        }
    }

    public string getNumObjeto(string objeto)
    {
        int val = 0;
        string numObj = "";

        if (objeto.Length > objeto.Substring(0, objeto.IndexOf("Slot") + 4).Length)   
            Int32.TryParse(objeto.Substring(objeto.IndexOf("Slot") + 4, 1), out val);

        if (val > 0)
            numObj = Convert.ToString(val);

        return numObj;
    }

    private bool podeExcluirObjeto()
    {     
        // Verifica se objeto esta em cima da lixeira.
        return (transform.position.x >= 715 && transform.position.x <= 722 &&
                transform.position.y >= 610 && transform.position.y <= 616 &&
                Global.listaEncaixes.ContainsKey(gameObject.name));
    }

    private void exclusaoCompletaObjetos(Slots slot)
    {
        switch (slot)
        {
            case Slots.ObjGrafSlot :
                ArrayList listaNomeObjGrafico = new ArrayList();
                ArrayList listaPecas = new ArrayList();
                GameObject render = GameObject.Find("Render");
                GameObject fabricaPecas = GameObject.Find("FabricaPecas");

                // Adiciona todos ObjGraficoSlots em uma lista 
                for (int i = 0; i < render.transform.childCount; i++)
                {
                    if (render.transform.GetChild(i).name.Contains("ObjGraficoSlot"))
                        listaNomeObjGrafico.Add(render.transform.GetChild(i).name);
                }

                Vector3 posGO = Vector3.zero;
                Vector3 posPecas = Vector3.zero;
                GameObject go = null;
                GameObject goPecas = null;

                for (int i = 0; i < listaNomeObjGrafico.Count; i++)
                {
                    // Verifica os objetos graficos da lista até chegar o objeto selecionado
                    if (!Equals((string)listaNomeObjGrafico[i], Global.listaEncaixes[gameObject.name]))
                        continue;
                    
                    // Pega a posição do objeto após o objeto a ser excluído
                    posGO = GameObject.Find((string)listaNomeObjGrafico[i+1]).transform.position;                   

                    // Instancia um GameObject vazio tendo Render como pai
                    go = Instantiate(new GameObject(), render.transform);
                    go.name = "GO_ObjetosGraficos";

                    goPecas = Instantiate(new GameObject(), fabricaPecas.transform);
                    goPecas.name = "GO_Pecas";

                    // Joga a posição do objeto a ser excluído para o GameObject vazio
                    go.transform.position = posGO;

                    goPecas.transform.position = posGO;

                    // Pega a posição do objeto a ser excluído
                    posGO = GameObject.Find((string)listaNomeObjGrafico[i]).transform.position;

                    // Remove peças dos slos
                    string value = "";
                    for (int j = 0; j < GameObject.Find((string)listaNomeObjGrafico[i]).transform.childCount; j++)
                    {
                        foreach (KeyValuePair<string, string> pair in Global.listaEncaixes)
                        {
                            if (Equals(GameObject.Find((string)listaNomeObjGrafico[i]).transform.GetChild(j).name, pair.Value) &&
                               !GameObject.Find((string)listaNomeObjGrafico[i]).transform.GetChild(j).name.Contains("ObjGraficoSlot"))
                            {
                                Destroy(GameObject.Find(Global.listaEncaixes[pair.Key]));
                                Destroy(GameObject.Find(pair.Key));                             
                                Global.listaSequenciaSlots.Remove(Global.listaEncaixes[pair.Key]);
                                Global.removeObject(GameObject.Find(pair.Key));
                                Global.listaEncaixes.Remove(pair.Key);

                                DestroyIluminacao(pair.Key);
                                break;
                            }                                
                        }
                    }

                    // Incrementa 'i' para pegar a partir do próximo objeto da lista
                    i++;

                    for (int j = i; j < listaNomeObjGrafico.Count; j++)
                    {
                        // Instancia os próximos objetos da lista e coloca o GameObjet vazio como pai deles
                        GameObject goEmpty = GameObject.Find((string)listaNomeObjGrafico[j]);
                        goEmpty.transform.parent = go.transform;

                        foreach (KeyValuePair<string, string> pair in Global.listaEncaixes)
                        {
                            if (Equals(getNumObjeto(pair.Value), getNumObjeto((string)listaNomeObjGrafico[j])))
                                listaPecas.Add(pair.Key);
                        }
                    }
                    break;
                }
                // Atualiza a posição do GameObject com todos objetos para a posição do objeto excluído.
                go.transform.position = posGO;
                
                foreach (string peca in listaPecas) 
                {
                    GameObject goEmpty = GameObject.Find(peca);
                    goEmpty.transform.parent = goPecas.transform;
                }

                // Atualiza a posição do GameObject com todos objetos para a posição do objeto excluído.
                goPecas.transform.position = posGO;

                //retira objetos dos GameObjects vazios
                GameObject GO_ObjGraf = GameObject.Find("GO_ObjetosGraficos");
                GameObject GO_Pecas = GameObject.Find("GO_Pecas");

                List<GameObject> listaGO_ObjGraf = new List<GameObject>();
                List<GameObject> listaGO_Pecas = new List<GameObject>();
                
                // Criada lista de objetos para troca de pais
                for (int i = 0; i < GO_ObjGraf.transform.childCount; i++)
                    listaGO_ObjGraf.Add(GO_ObjGraf.transform.GetChild(i).gameObject);

                // Criada lista de objetos para troca de pais
                for (int i = 0; i < GO_Pecas.transform.childCount; i++)
                    listaGO_Pecas.Add(GO_Pecas.transform.GetChild(i).gameObject);

                // Faz a troca dos pais
                for (int i = 0; i < listaGO_ObjGraf.Count; i++)
                    listaGO_ObjGraf[i].transform.parent = render.transform;
                Destroy(GO_ObjGraf);                

                for (int i = 0; i < listaGO_Pecas.Count; i++)
                   listaGO_Pecas[i].transform.parent = fabricaPecas.transform;
                Destroy(GO_Pecas);
                
                Destroy(GameObject.Find(Global.listaEncaixes[gameObject.name]));
                Destroy(GameObject.Find(gameObject.name));
                Global.listaSequenciaSlots.Remove(Global.listaEncaixes[gameObject.name]);
                Global.removeObject(gameObject);
                Global.listaEncaixes.Remove(gameObject.name);
                Global.propriedadePecas.Remove(gameObject.name);
                break;

            case Slots.TransformacoesSlot:
                Destroy(GameObject.Find(Global.listaEncaixes[gameObject.name]));
                Destroy(GameObject.Find(gameObject.name));
                Global.listaSequenciaSlots.Remove(Global.listaEncaixes[gameObject.name]);
                Global.removeObject(gameObject);
                Global.listaEncaixes.Remove(gameObject.name);
                Global.propriedadePecas.Remove(gameObject.name);
                break;
        }        
    }

    private void DestroyIluminacao(string key)
    {
        PropIluminacaoPadrao luz = new PropIluminacaoPadrao();

        if (key.Contains(Consts.Iluminacao))
        {
            Global.propriedadeIluminacao.Remove(key);      

            if (key.Length > "Iluminacao".Length)
                Destroy(GameObject.Find("LightObjects" + key));
            else
            {
                if (Global.propriedadePecas.ContainsKey(key))
                    luz.AtivaIluminacao(luz.GetTipoLuzPorExtenso(Global.propriedadePecas[key].TipoLuz) + key, false);

                Global.propriedadePecas[Global.gameObjectName].JaInstanciou = false;
            }
                
        }        
    }

    private void reposicionaPosicaoAmbCuboVis()
    {
        GameObject goAmb = GameObject.Find("GameObjectAmb" + getNumeroSlotObjetoGrafico());

        GameObject goUltimo = GameObject.Find(goAmb.name);

        // Retorna ultimo Objeto de GameObjectAmb
        try
        {
            while (goUltimo.transform.GetChild(0).name != string.Empty)
            {     
                if (Equals(goUltimo.transform.GetChild(0).name, "CuboAmbiente" + getNumeroSlotObjetoGrafico()))
                    goUltimo = GameObject.Find(goUltimo.transform.GetChild(1).name);
                else
                    goUltimo = GameObject.Find(goUltimo.transform.GetChild(0).name);
            }
        }
        catch (Exception e) { }  
        
        GameObject goTroca = GameObject.Find(gameObject.name + AMB);

        // Altera o parent do filho da peça selecionada para o pai da peça selecionada
        goTroca.transform.GetChild(0).parent = goTroca.transform.parent;

        // Altera o parent da peça selecionada para o ultimo objeto
        goTroca.transform.parent = goUltimo.transform;

        // Altera o parent do CuboAmbiente para a peça atual
        GameObject goCuboAmb = GameObject.Find("CuboAmbiente" + getNumeroSlotObjetoGrafico());

        if (goCuboAmb != null)
            goCuboAmb.transform.parent = goTroca.transform;

        // Faz o mesmo procedimento com CuboVisObject
        GameObject goVis = GameObject.Find("CuboVisObject" + getNumeroSlotObjetoGrafico());

        goUltimo = GameObject.Find(goVis.name);

        // Retorna ultimo Objeto de CuboVisObject
        try
        {
            while (goUltimo.transform.GetChild(0).name != string.Empty)
            {
                if (Equals(goUltimo.transform.GetChild(0).name, "CuboVis" + getNumeroSlotObjetoGrafico()))
                    goUltimo = GameObject.Find(goUltimo.transform.GetChild(1).name);
                else
                    goUltimo = GameObject.Find(goUltimo.transform.GetChild(0).name);
            }
        }
        catch (Exception) { }

        GameObject goTrocaVis = GameObject.Find(gameObject.name + VIS);

        goTrocaVis.transform.GetChild(0).parent = goTrocaVis.transform.parent;
        goTrocaVis.transform.parent = goUltimo.transform;

        GameObject goCuboVis = GameObject.Find("CuboVis" + getNumeroSlotObjetoGrafico());

        if (goCuboVis != null)
            goCuboVis.transform.parent = goTrocaVis.transform;

    }

    private void excluiHierarquiaOuAlteraPropriedade(Slots tipoSlot)
    {
        int tipoSlotInt = (int) tipoSlot;
        MeshRenderer mr;
        string numObj = string.Empty;
        string valueOut = string.Empty;
        int val;

        switch (tipoSlotInt)
        {          
            case 0: //ObjGrafSlot
                if (Global.listaEncaixes.TryGetValue(gameObject.name, out valueOut))
                {
                    if (valueOut.Length > "ObjGraficoSlot".Length)
                    {
                        Int32.TryParse(valueOut.Substring(valueOut.IndexOf("Slot") + 4, 1), out val);

                        if (val > 0)
                            numObj = Convert.ToString(val);
                    }                    
                }                

                Destroy(GameObject.Find("PosicaoAmb" + numObj));
                Destroy(GameObject.Find("CuboVisObjectMain" + numObj));                
                break;
            case 1: //FormasSlot
                //mr = GameObject.Find("CuboAmbiente" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();
                mr = GameObject.Find("CuboAmbiente" + getNumObjeto(GameObject.Find(Global.listaEncaixes[gameObject.name]).transform.parent.name)).GetComponent<MeshRenderer>();
                mr.enabled = false;

                //mr = GameObject.Find("CuboVis" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();
                //mr.enabled = false;

                //if (Global.cuboVisComIluminacao.Contains("CuboVis" + getNumeroSlotObjetoGrafico()))
                //    Global.cuboVisComIluminacao.Remove("CuboVis" + getNumeroSlotObjetoGrafico());
                break;
            case 2: //TransformacoesSlot
                GameObject goExcluido = GameObject.Find(gameObject.name + AMB);
                goExcluido.transform.GetChild(0).parent = goExcluido.transform.parent;
                Destroy(goExcluido);

                goExcluido = GameObject.Find(gameObject.name + VIS);
                goExcluido.transform.GetChild(0).parent = goExcluido.transform.parent;
                Destroy(goExcluido);
                break;
            case 3: //IluminacaoSlot
                //mr = GameObject.Find("CuboVis" + getNumeroSlotObjetoGrafico()).GetComponent<MeshRenderer>();
                //mr.enabled = false;

                //if (Global.cuboVisComIluminacao.Contains("CuboVis" + getNumeroSlotObjetoGrafico()))
                //    Global.cuboVisComIluminacao.Remove("CuboVis" + getNumeroSlotObjetoGrafico());
                break;
        }

    }

    private string getSohLetras(string texto)
    {
        string textReturn = String.Empty;

        if (texto.Contains(Consts.Transladar))
            textReturn = "PropTransladar";
        else if (texto.Contains(Consts.Rotacionar))
            textReturn = "PropRotacionar";
        else if (texto.Contains(Consts.Escalar))
            textReturn = "PropEscalar";
        else if (texto.Contains(Consts.ObjetoGrafico))
            textReturn = "PropObjGrafico";
        else if (texto.Contains(Consts.Cubo))
            textReturn = "PropCubo";
        else if (texto.Contains(Consts.Camera))
            textReturn = "PropCamera";
        else if (texto.Contains(Consts.Iluminacao))
            textReturn = "PropIluminacao";

        return textReturn;
    }

    private void CriaLightObject(string numLight)
    {
        const string LightObjectsIluminacao = "LightObjectsIluminacao";

        if (!Global.lightObjectList.Contains(LightObjectsIluminacao + numLight))
        {           
            GameObject cloneGO;
            GameObject GOCloneLightObjects = GameObject.Find(LightObjectsIluminacao);

            Global.lightObjectList.Add(LightObjectsIluminacao + numLight);

            if (LightObjectsIluminacao + numLight != LightObjectsIluminacao)
            {
                cloneGO = Instantiate(GOCloneLightObjects, GOCloneLightObjects.transform.position, GOCloneLightObjects.transform.rotation, GOCloneLightObjects.transform.parent);
                cloneGO.name = LightObjectsIluminacao + numLight;
                cloneGO.transform.position = new Vector3(GOCloneLightObjects.transform.position.x, GOCloneLightObjects.transform.position.y, GOCloneLightObjects.transform.position.z);

                RenomeiaLightObject(cloneGO, numLight);
            }
        }        
    }

    private void RenomeiaLightObject(GameObject go, string numLight)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            switch (i)
            {
                case 0:
                    go.transform.GetChild(i).name += numLight;
                    break;
                case 1:
                    go.transform.GetChild(i).name += numLight;
                    go.transform.GetChild(i).GetChild(0).name += numLight;
                    break;
                case 2:
                    go.transform.GetChild(i).name += numLight;
                    go.transform.GetChild(i).GetChild(0).name += numLight;
                    break;
                case 3:
                    go.transform.GetChild(i).name += numLight;
                    go.transform.GetChild(i).GetChild(0).name += numLight;
                    go.transform.GetChild(i).GetChild(0).GetChild(0).name += numLight;
                    break;
            }
        }
    }  
    
    private void AtualizaTrasformGameObjectAmb()
    {     
        string nomeAmb = "GameObjectAmb" + getNumObjeto(GameObject.Find(Global.listaEncaixes[gameObject.name]).transform.parent.name);
        int cont = 0;
        int breakLoop = 50;

        Transform GoAmb = GameObject.Find(nomeAmb).transform;

        while (cont < breakLoop)
        {
            if (GoAmb.childCount > 0) 
                GoAmb = GoAmb.GetChild(0);

            if (GoAmb.name.Contains("CuboAmbiente"))
            {
                Vector3 pos = Vector3.zero;
                Vector3 tam = new Vector3(1, 1, 1);
                Tamanho tamanho;
                Posicao posicao;

                if (Global.propriedadePecas.Count > 0)
                {
                    // Verifica posição e tamanho do cubo.
                    foreach (KeyValuePair<string, string> enc in Global.listaEncaixes)
                    {
                        if (Equals(enc.Value, "FormasSlot" + getNumeroSlotObjetoGrafico()))
                        {
                            if (Global.propriedadePecas.ContainsKey(enc.Key))
                            {
                                tamanho = Global.propriedadePecas[enc.Key].Tam;
                                posicao = Global.propriedadePecas[enc.Key].Pos;
                                tam = new Vector3(tamanho.X, tamanho.Y, tamanho.Z);
                                pos = new Vector3(posicao.X *-1, posicao.Y, posicao.Z);
                                break;
                            }                            
                        }
                    }
                }                

                GoAmb.localPosition = pos;
                GoAmb.localRotation = Quaternion.Euler(0,0,0);
                GoAmb.localScale = tam;
                break;
            }

            // Verifica se o GO foi excluído então pega o próximo.
            if (GoAmb.childCount == 0 && !GoAmb.name.Contains("CuboAmbiente"))
            {
                if (GoAmb.parent.childCount > 1)
                    GoAmb = GoAmb.parent.GetChild(1);
            }

            if (GoAmb.name.Contains(Consts.Transladar))
            {
                GoAmb.localRotation = Quaternion.Euler(0, 0, 0);
                GoAmb.localScale = new Vector3(1, 1, 1);
            }
            else if (GoAmb.name.Contains(Consts.Rotacionar))
            {
                GoAmb.localPosition = Vector3.zero;
                GoAmb.localScale = new Vector3(1, 1, 1);
            }
            else if (GoAmb.name.Contains(Consts.Escalar))
            {
                GoAmb.localPosition = Vector3.zero;
                GoAmb.localRotation = Quaternion.Euler(0, 0, 0);                
            }  

            cont++;
        }
    }
}
