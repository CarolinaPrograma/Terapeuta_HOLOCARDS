using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RegisterPatientFirestore : MonoBehaviour
{
    public TMP_InputField nombreInput;
    public TMP_InputField apellidoInput;
    public TMP_InputField apellido2Input;
    public TMP_InputField usernameInput;
    public TMP_InputField edadInput;
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;
    private FirebaseFirestore firestore;
    private string therapistId;

    public GenerarBotonesUsuarios generarBotonesUsuario;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        if (PlayerPrefs.HasKey("therapistId"))
        {
            therapistId = PlayerPrefs.GetString("therapistId");
            Debug.Log("therapistId: " + therapistId);
        }
        else
        {
            Debug.LogError("therapistId no encontrado en PlayerPrefs.");
        }
    }


    public async void RegisterNewPatient()
    {
        List<string> errores = new List<string>();
        string email = emailInput.text;
        string password = passwordInput.text;
        string nombre = nombreInput.text;
        string apellido = apellidoInput.text;
        string apellido2 = apellido2Input.text;
        string username = usernameInput.text;
        string edad_s = edadInput.text;

        Debug.Log("1");

        // Validations
        if (string.IsNullOrEmpty(email) || !System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            errores.Add("Por favor, introduce un correo v·lido.");

        if (string.IsNullOrEmpty(nombre) && !System.Text.RegularExpressions.Regex.IsMatch(nombre, @"^[a-zA-Z¡…Õ”⁄·ÈÌÛ˙Ò—\s]+$"))
            errores.Add("El campo 'nombre' solo debe contener letras y espacios.");

        if (string.IsNullOrEmpty(apellido) && !System.Text.RegularExpressions.Regex.IsMatch(apellido, @"^[a-zA-Z¡…Õ”⁄·ÈÌÛ˙Ò—\s]+$"))
            errores.Add("El campo 'apellido' solo debe contener letras y espacios.");

        if (string.IsNullOrEmpty(apellido2) && !System.Text.RegularExpressions.Regex.IsMatch(apellido2, @"^[a-zA-Z¡…Õ”⁄·ÈÌÛ˙Ò—\s]+$"))
            errores.Add("El campo 'apellido2' solo debe contener letras y espacios.");

        if (!int.TryParse(edad_s, out int edad) || edad <= 0)
            errores.Add("El campo 'edad' debe ser un n˙mero positivo.");

        Debug.Log("2");
        Debug.Log(errores.Count);

        if (errores.Count > 0)
        {
            feedbackText.text = string.Join("\n", errores);
            return;
        }

        try
        {
            var task = auth.CreateUserWithEmailAndPasswordAsync(email, password);
            await task;

            if (task.IsCompletedSuccessfully)
            {
                Firebase.Auth.AuthResult newUser = task.Result;
                string userId = newUser.User.UserId;

                Debug.Log("Voy a llamar y el userId es: " + userId);
                await SavePatientDataAsync(userId, therapistId, nombre, apellido, apellido2, username, edad);
            }
            else
            {
                Debug.LogError("Error creando el usuario: " + task.Exception);
            }
        }
        catch (Exception e)
        {
            if (e is Firebase.FirebaseException fbEx)
            {
                switch (fbEx.ErrorCode)
                {
                    case (int)Firebase.Auth.AuthError.EmailAlreadyInUse:
                        feedbackText.text = "El correo ya est· en uso.";
                        break;
                    case (int)Firebase.Auth.AuthError.InvalidEmail:
                        feedbackText.text = "El correo introducido no es v·lido.";
                        break;
                    // Add more cases if needed
                    default:
                        feedbackText.text = "Error de autenticaciÛn: " + fbEx.Message;
                        break;
                }
            }
            else
            {
                feedbackText.text = "OcurriÛ un error inesperado: " + e.Message;
            }
        }
    }



    private async Task SavePatientDataAsync(string userId, string therapistId, string nombre, string apellido, string apellido2, string username, int edad)
    {

        Dictionary<string, object> patientData = new Dictionary<string, object>
        {
            { "nombre", nombre },
            { "apellido", apellido },
            { "apellido2", apellido2 },
            { "username", username },
            { "edad", edad },
            { "therapistId", therapistId }
        };


        if (string.IsNullOrEmpty(userId) || patientData == null)
        {
            Debug.LogError("El userId o patientData no son v·lidos.");
            return;
        }

        DocumentReference docRef = firestore.Collection("patients").Document(userId);
        await docRef.SetAsync(patientData);
        Debug.Log("Datos del paciente guardados correctamente en Firestore.");

        limpiar_inputs();
        feedbackText.text = "Usuario registrado correctamente";
        generarBotonesUsuario.GetPatientsForTherapist(therapistId);
        Debug.Log("Se ha llamado a generar botones desde Registro");
       
    }

    public void limpiar_inputs()
    {
        nombreInput.text = "";
        apellidoInput.text = "";
        apellido2Input.text = "";
        usernameInput.text = "";
        edadInput.text = "";
        emailInput.text = "";
        passwordInput.text = "";
    }
}


