using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GenerarBotonesUsuarios : MonoBehaviour
{
    public Transform buttonContainer; 
    public Button patientButtonPrefab;
    public Detalles_Paciente detalles;

    private string therapistId;
    FirebaseFirestore db;

    //public void Start()
    //{
    //    Debug.Log("Entro a GenerarBotonesUsuarios");
    //    db = FirebaseFirestore.DefaultInstance;
    //    string therapistId = PlayerPrefs.GetString("therapistId");
    //    GetPatientsForTherapist(therapistId);
    //}

    public void GetPatientsForTherapist(string therapistId)
    {

        db = FirebaseFirestore.DefaultInstance;
        //therapistId = PlayerPrefs.GetString("therapistId");


        Debug.Log("Entro a GetPatientsForTherapist");
        ClearPatientButtons();

        db.Collection("patients")
          .WhereEqualTo("therapistId", therapistId)
          .GetSnapshotAsync()
          .ContinueWithOnMainThread(task =>
          {
              if (task.IsCompleted)
              {
                  Debug.Log("Proceso terminado correctamente");
                  QuerySnapshot snapshot = task.Result;
                  Debug.Log("Número de documentos encontrados: " + snapshot.Count);
                  foreach (DocumentSnapshot document in snapshot.Documents)
                  {
                      Dictionary<string, object> patientData = document.ToDictionary();
                      string username = patientData["username"].ToString();
                      string documentId = document.Id; 
                      Debug.Log("Username: " + username + ", Document ID: " + documentId);
                      CreatePatientButton(username, documentId); 
                  }
                  Debug.Log("Se ha creado todo correctamente");
              }
              else
              {
                  Debug.Log("error al obtener los pacientes");
              }
          });
    }

    void CreatePatientButton(string username, string id)
    {
        Debug.Log("Entro a CreatePatientButton");
        Button newButton = Instantiate(patientButtonPrefab, buttonContainer);
        newButton.GetComponentInChildren<TMP_Text>().text = username;
        newButton.onClick.AddListener(() => OnPatientButtonClicked(username, id));
    }

    void OnPatientButtonClicked(string username, string id)
    {
        Debug.Log("Se ha seleccionado el paciente: " + username + " " + id);
        detalles.cargar_detalles(id);

    }

    void ClearPatientButtons()
    {
        foreach (Transform button in buttonContainer) // buttonContainer es un Transform
        {
            Destroy(button.gameObject); // Accede a button.gameObject para destruir el objeto del botón
        }
    }
}
