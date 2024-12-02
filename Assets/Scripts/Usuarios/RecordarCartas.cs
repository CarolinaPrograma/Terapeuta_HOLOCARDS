using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RecordarCartas : MonoBehaviour
{
    public TMP_InputField numero_cartas;
    public TMP_InputField tiempo_total;
    public TMP_InputField tiempo_panel;
    public TMP_Dropdown tipo_cartas;
    public TMP_InputField fecha;

    public TMP_Text feedback;

    FirebaseFirestore firestore;
    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public void publicar_juego(string id_paciente)
    {
        Debug.Log("Soy llamado");
        if (ValidateGameSetup() == true) {
            SaveGameToFirestore(id_paciente);
        }
    }
    public bool ValidateGameSetup()
    {
        bool isValid = true;
        string errorMessage = "";

        // Validación para numero_cartas
        if (string.IsNullOrEmpty(numero_cartas.text))
        {
            isValid = false;
            errorMessage += "El campo 'Número de Cartas' no puede estar vacío.\n";
        }
        else
        {
            int numCartas;
            if (int.TryParse(numero_cartas.text, out numCartas))
            {
                if (numCartas < 1 || numCartas > 10)
                {
                    isValid = false;
                    errorMessage += "El 'Número de Cartas' debe estar entre 1 y 10.\n";
                }
            }
            else
            {
                isValid = false;
                errorMessage += "El 'Número de Cartas' debe ser un número.\n";
            }
        }


        if (string.IsNullOrEmpty(fecha.text))
        {
            isValid = false;
            errorMessage += "El campo 'Fecha' no puede estar vacío.\n";
        }

        DateTime inputFecha;
        DateTime.TryParse(fecha.text, out inputFecha);
        if (inputFecha < DateTime.Now.Date)
        {
            isValid = false;
            errorMessage += "La fecha debe ser igual o posterior a hoy.\n";
        }


        if (!isValid)
        {
            Debug.Log("Error en la configuración del juego:\n" + errorMessage);
            feedback.text = errorMessage;
            return false;
        }
        else
        {
            Debug.Log("Configuración del juego válida.");
            return true;
        }
    }

    private void SaveGameToFirestore(string id_paciente)
    {
        int juego = 1;
        int numCartas = int.Parse(numero_cartas.text);
        int totalTiempo = int.Parse(tiempo_total.text);
        int tiempoPanel = int.Parse(tiempo_panel.text);
        string tipoCartaSeleccionada = tipo_cartas.options[tipo_cartas.value].text;
        DateTime fechaJuego = DateTime.Parse(fecha.text);

        Dictionary<string, object> gameData = new Dictionary<string, object>
        {
            { "juego" , juego },
            { "numero_cartas", numCartas },
            { "tiempo_total", totalTiempo },
            { "tiempo_panel", tiempoPanel },
            { "tipo_cartas", tipoCartaSeleccionada },
            { "fecha", fechaJuego.ToString("yyyy-MM-dd") },
            { "status", "pendiente" }
        };

        // Referencia al documento del paciente en Firestore
        DocumentReference pacienteRef = firestore.Collection("patients").Document(id_paciente).Collection("assignedGames").Document();

        pacienteRef.SetAsync(gameData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Juego publicado correctamente en Firestore.");
                feedback.text = "Juego publicado correctamente.";
                limpiar_inputs();
                StartCoroutine(LimpiarFeedback(3));
            }
            else
            {
                Debug.LogError("Error al publicar el juego: " + task.Exception);
                feedback.text = "Error al publicar el juego. Inténtalo de nuevo.";
            }
        });
    }

    private IEnumerator LimpiarFeedback(float delay)
    {
        yield return new WaitForSeconds(delay);
        feedback.text = "";  // Limpia el texto después del retraso
    }

    public void limpiar_inputs()
    {
        numero_cartas.text = "";
        tiempo_total.text = "";
        tiempo_panel.text = "";
        fecha.text = "";
    }
}


