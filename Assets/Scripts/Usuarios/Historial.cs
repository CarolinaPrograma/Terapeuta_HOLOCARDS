using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class Historial : MonoBehaviour
{
    FirebaseFirestore db;

    public GameObject Boton_prefab;
    public GameObject Scrollview_realizados;
    public GameObject Scrollview_pendientes;

    Color verde = new Color(0.83f, 0.99f, 0.18f);
    Color amarillo = new Color(0.96f, 0.93f, 0.14f);
    Color rojo = new Color(0.75f, 0.18f, 0.22f);

    public async Task Generar_Historial(string idpatient)
    {
        db = FirebaseFirestore.DefaultInstance;
        CollectionReference assignedGamesRef = db.Collection("patients").Document(idpatient).Collection("assignedGames");

        Query query = assignedGamesRef.OrderByDescending("fecha");

        LimpiarScrollView(Scrollview_pendientes);
        LimpiarScrollView(Scrollview_realizados);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();

        Debug.Log("Proceso terminado correctamente");
        Debug.Log("Número de documentos encontrados: " + snapshot.Count);

        foreach (DocumentSnapshot document in snapshot.Documents)
        {
            Dictionary<string, object> gameData = document.ToDictionary();
            string documentId = document.Id;

            bool estadoActualizado = await CambiarEstado(idpatient, documentId, gameData);

            if (estadoActualizado)
            {
                gameData["status"] = "no realizado";
            }

            GenerarBotontesHistorial(documentId, gameData);
        }

        Debug.Log("Se ha creado todo correctamente");
    }


    private void GenerarBotontesHistorial(string documentId, Dictionary<string, object> gameData)
    {
        string titulo = Nombre_Juego(gameData["juego"].ToString());
        string status = gameData["status"].ToString();

        GameObject nuevoBoton = Instantiate(Boton_prefab);

        TMP_Text tipoJuegoText = nuevoBoton.transform.Find("TipoJuego").GetComponent<TMP_Text>();
        tipoJuegoText.text = titulo;

        TMP_Text resumenText = nuevoBoton.transform.Find("Resumen").GetComponent<TMP_Text>();
        if (status == "pendiente")
        {
            resumenText.text = "Fecha: " + gameData["fecha"].ToString();
        }
        else if (status == "no realizado")
        {
            resumenText.text = "Éxito: No  |  " + gameData["fecha"].ToString();
        }
        else if (status == "completado" && gameData.ContainsKey("resultados"))
        {
            Dictionary<string, object> resultados = (Dictionary<string, object>)gameData["resultados"];
            string aciertos = resultados["aciertos"].ToString();
            string fallos = resultados["fallos"].ToString();
            resumenText.text = $"Aciertos: {aciertos}, Fallos: {fallos}";
        }

        nuevoBoton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() =>
        {
            Debug.Log("Botón de " + titulo + " presionado");
            AbrirAnalisisJuego(gameData);
        });

        UnityEngine.UI.Button boton = nuevoBoton.GetComponent<UnityEngine.UI.Button>();
        if (boton != null)
        {
            ColorBlock buttonColors = boton.colors;

            if (status == "pendiente")
            {
                buttonColors.normalColor = amarillo;
            }
            else if (status == "completado")
            {
                buttonColors.normalColor = verde;
            }
            else if (status == "no realizado")
            {
                buttonColors.normalColor = rojo;
            }

            boton.colors = buttonColors;
        }
        else
        {
            Debug.LogWarning("No se encontró el componente Button en el botón.");
        }

        Transform scrollContent;
        if (status == "pendiente")
        {
            scrollContent = Scrollview_pendientes.transform.Find("Viewport/Content");
        }
        else
        {
            scrollContent = Scrollview_realizados.transform.Find("Viewport/Content");
        }

        if (scrollContent != null)
        {
            nuevoBoton.transform.SetParent(scrollContent, false);
        }
        else
        {
            Debug.LogError("No se encontró el Content en el ScrollView.");
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(nuevoBoton.transform as RectTransform);
    }

    private string Nombre_Juego(string numero)
    {
        if (numero == "1") return "Recordar cartas";
        else if (numero == "2") return "Memorizar palabras";
        else return "Sumar cartas";
    }

    private async Task<bool> CambiarEstado(string idpatient, string id, Dictionary<string, object> gameData)
    {
        if (gameData.ContainsKey("fecha"))
        {
            DateTime dueDate = DateTime.Parse(gameData["fecha"].ToString());
            DateTime tomorrow = DateTime.Now.AddDays(1).Date;

            if (tomorrow > dueDate.Date && gameData["status"].ToString() == "pendiente")
            {
                DocumentReference docRef = FirebaseFirestore.DefaultInstance
                                               .Collection("patients")
                                               .Document(idpatient)
                                               .Collection("assignedGames").Document(id);

                await docRef.UpdateAsync(new Dictionary<string, object>
            {
                { "status", "no realizado" }
            });

                Debug.Log("Status actualizado a 'no realizado' para el juego: " + id);
                return true;
            }
        }
        return false;
    }

    private void LimpiarScrollView(GameObject scrollView)
    {
        Transform content = scrollView.transform.Find("Viewport/Content");

        if (content != null)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }
        else
        {
            Debug.LogError("No se encontró el Content en el ScrollView.");
        }
    }

    public GameObject modal_boton_juego;
    public TMP_Text Titulo;
    public TMP_Text Fecha;
    public TMP_Text Parametros;
    public TMP_Text Exito;
    public TMP_Text Aciertos;
    public TMP_Text Tiempo;
    public TMP_Text Pistas;
    public TMP_Text Intentos;
    public TMP_Text Fallos;
    public TMP_Text Tiempo_individual;
    public TMP_Text Status;


    private void AbrirAnalisisJuego(Dictionary<string, object> gameData)
    {
        modal_boton_juego.SetActive(true);
        Titulo.text = Nombre_Juego(gameData["juego"].ToString());
        Fecha.text = gameData["fecha"].ToString();
        string parametrosTexto = "Número de cartas: " + gameData["numero_cartas"];
        if (gameData["tiempo_total"] != null)
        {
            parametrosTexto += "  |  Tiempo total: " + gameData["tiempo_total"];
        }
        if (gameData.ContainsKey("tiempo_panel"))
        {
            parametrosTexto += "  |  Tiempo de panel: " + gameData["tiempo_panel"];
        }
        if (gameData["tipo_cartas"] != null)
        {
            parametrosTexto += "  |  Modalidad: " + gameData["tipo_cartas"];
        }
        Parametros.text = parametrosTexto;

        if (gameData["status"].ToString() == "pendiente"){
            Status.text = "Pendiente";
        }
        else if (gameData["status"].ToString() == "no realizado")
        {
            Status.text = "Juego no realizado";
        }
        else
        {

            Dictionary<string, object> resultados = (Dictionary<string, object>)gameData["resultados"];

            if (resultados.ContainsKey("aciertos") && resultados["aciertos"] != null)
            {
                Aciertos.text = "Aciertos: " + resultados["aciertos"].ToString();
            }
            if (resultados.ContainsKey("exito") && resultados["exito"] != null)
            {
                Exito.text = "Éxito: " + resultados["exito"].ToString();
            }
            if (resultados.ContainsKey("fallos") && resultados["fallos"] != null)
            {
                Fallos.text = "Fallos: " + resultados["fallos"].ToString();
            }
            if (resultados.ContainsKey("numero_pistas") && resultados["numero_pistas"] != null)
            {
                Pistas.text = "Número de pistas: " + resultados["numero_pistas"].ToString();
            }
            if (resultados.ContainsKey("tiempo_tardado") && resultados["tiempo_tardado"] != null)
            {
                Tiempo.text = "Tiempo tardado: " + resultados["tiempo_tardado"].ToString() +"s";
            }
            if (resultados.ContainsKey("tiempo_suma") && resultados["tiempo_suma"] != null)
            {
                var tiempoSumaList = resultados["tiempo_suma"] as List<object>; 
                if (tiempoSumaList != null && tiempoSumaList.Count > 0)
                {
                    List<string> tiemposEnSegundos = new List<string>();

                    foreach (var item in tiempoSumaList)
                    {
                        if (item is IConvertible)
                        {
                            double tiempoEnMilisegundos = Convert.ToDouble(item);
                            double tiempoEnSegundos = tiempoEnMilisegundos / 1000; 
                            tiemposEnSegundos.Add(tiempoEnSegundos.ToString("F2"));
                        }
                    }
                    Tiempo_individual.text = "Tiempo total suma: " + string.Join("s, ", tiemposEnSegundos);
                }
            }

            if (resultados.ContainsKey("tiempo_carta") && resultados["tiempo_carta"] != null)
            {
                var tiempoCartaList = resultados["tiempo_carta"] as List<object>; 
                if (tiempoCartaList != null && tiempoCartaList.Count > 0)
                {
                    List<string> tiemposEnSegundos = new List<string>();

                    foreach (var item in tiempoCartaList)
                    {
                        if (item is IConvertible) 
                        {
                            double tiempoEnMilisegundos = Convert.ToDouble(item);
                            double tiempoEnSegundos = tiempoEnMilisegundos / 1000; 
                            tiemposEnSegundos.Add(tiempoEnSegundos.ToString("F2")); 
                        }
                    }
                    Tiempo_individual.text = "Tiempo por carta: " + string.Join("s, ", tiemposEnSegundos);
                }
            }
        }
    }


    public void CerrarUIAnalisis()
    {
        Parametros.text = "";
        Tiempo_individual.text = "";
        Exito.text = "";
        Aciertos.text = "";
        Tiempo.text = "";
        Pistas.text = "";
        Intentos.text = "";
        Fallos.text = "";
        Status.text = "";
        modal_boton_juego.SetActive(false);
    }
}
