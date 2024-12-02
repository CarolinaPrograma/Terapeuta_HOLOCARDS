using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SumarCartas : MonoBehaviour
{
    public TMP_InputField numero_parejas;
    public TMP_InputField tiempo_total;
    public TMP_Dropdown opciones;
    public TMP_InputField fecha;

    public TMP_Text feedback;

    FirebaseFirestore firestore;
    void Start()
    {
        firestore = FirebaseFirestore.DefaultInstance;
    }

    public void publicar_juego(string id_paciente)
    {
        if (ValidateGameSetup() == true)
        {
            SaveGameToFirestore(id_paciente);
        }
    }

    public bool ValidateGameSetup()
    {
        bool isValid = true;
        string errorMessage = "";

        // Validación para numero_parejas
        if (string.IsNullOrEmpty(numero_parejas.text))
        {
            isValid = false;
            errorMessage += "El campo 'Número de parejas' no puede estar vacío.\n";
        }
        else
        {
            int numCartas;
            if (int.TryParse(numero_parejas.text, out numCartas))
            {
                if (numCartas < 1 || numCartas > 20)
                {
                    isValid = false;
                    errorMessage += "El 'Número de parejas' debe estar entre 1 y 10.\n";
                }
            }
            else
            {
                isValid = false;
                errorMessage += "El 'Número de parejas' debe ser un número.\n";
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
        int juego = 3;
        int numParejas = int.Parse(numero_parejas.text);
        int totalTiempo = int.Parse(tiempo_total.text);
        string tipoCartaSeleccionada = opciones.options[opciones.value].text;
        DateTime fechaJuego = DateTime.Parse(fecha.text);

        Dictionary<string, object> gameData = new Dictionary<string, object>
        {
            { "juego" , juego },
            { "numero_cartas", numParejas },
            { "tiempo_total", totalTiempo },
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
        numero_parejas.text = "";
        tiempo_total.text = "";
        fecha.text = "";
    }
}
