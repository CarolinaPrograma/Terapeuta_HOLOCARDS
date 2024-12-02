using Firebase.Extensions;
using Firebase.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Detalles_Paciente : MonoBehaviour
{
    public GameObject Detalles_Paciente_Vista;
    public GameObject Usuario_Paciente_Vista;

    public TMP_Text nombre_paciente;
    FirebaseFirestore db;

    public int boton_elegido;

    public Button boton_SumarCartas;
    public Button boton_RecordarCartas;
    public Button boton_MemorizarFrases;

    public Button Subir;

    public GameObject inputs_SumarCartas;
    public GameObject inputs_RecordarCartas;
    public GameObject inputs_MemorizarFrases;

    public RecordarCartas recordarCartas;
    public MemorizarFrases memorizarFrases;
    public SumarCartas sumarCartas;

    public Historial historial;

    private string id_patient;

    public async void cargar_detalles(string id)
    {
        await GetPatient(id);
        await historial.Generar_Historial(id);
        Detalles_Paciente_Vista.SetActive(true);
        Usuario_Paciente_Vista.SetActive(false);
    }

    public async Task GetPatient(string id)
    {
        db = FirebaseFirestore.DefaultInstance;
        Debug.Log("Entro a GetPatient");

        try
        {
            DocumentSnapshot document = await db.Collection("patients")
                                                .Document(id)
                                                .GetSnapshotAsync();
            if (document.Exists)
            {
                Dictionary<string, object> patientData = document.ToDictionary();
                string nombre = patientData["nombre"].ToString();
                string apellido = patientData["apellido"].ToString();
                string apellido2 = patientData["apellido2"].ToString();
                nombre_paciente.text = nombre + " " + apellido + " " + apellido2;
                Debug.Log("Paciente encontrado: " + nombre);
                id_patient = id;
            }
            else
            {
                Debug.LogError("No se encontró el paciente con la ID proporcionada");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error al obtener los datos del paciente: " + e.Message);
        }
    }

    public void botones(int boton)
    {
        SetButtonColor(boton_RecordarCartas, Color.gray);
        SetButtonColor(boton_MemorizarFrases, Color.gray);
        SetButtonColor(boton_SumarCartas, Color.gray);

        if (boton == 1)
        {
            boton_elegido = 1;
            inputs_RecordarCartas.SetActive(true);
            inputs_MemorizarFrases.SetActive(false);
            inputs_SumarCartas.SetActive(false);

            SetButtonColor(boton_RecordarCartas, Color.blue);
        }
        else if (boton == 2)
        {
            boton_elegido = 2;
            inputs_RecordarCartas.SetActive(false);
            inputs_MemorizarFrases.SetActive(true);
            inputs_SumarCartas.SetActive(false);

            SetButtonColor(boton_MemorizarFrases, Color.blue);
        }
        else if (boton == 3)
        {
            boton_elegido = 3;
            inputs_RecordarCartas.SetActive(false);
            inputs_MemorizarFrases.SetActive(false);
            inputs_SumarCartas.SetActive(true);

            SetButtonColor(boton_SumarCartas, Color.blue);
        }
    }

    private void SetButtonColor(Button button, Color color)
    {
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        button.colors = colors;
    }

    public async void Subir_click()
    {
        if (boton_elegido == 1)
        {
            recordarCartas.publicar_juego(id_patient);
        }

        else if (boton_elegido == 2)
        {
            memorizarFrases.publicar_juego(id_patient);
        }
        else if (boton_elegido == 3)
        {
            sumarCartas.publicar_juego(id_patient);
        }

        await historial.Generar_Historial(id_patient);
    }

}
