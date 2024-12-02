using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Registro_UI : MonoBehaviour
{
    public GameObject Usuarios;
    public GameObject PaginaAuth;

    public GenerarBotonesUsuarios generarBotonesUsuario;

    public TMP_InputField emailInputField;
    public TMP_InputField passwordInputField;
    public TMP_Text feedbackText;

    private FirebaseAuth auth;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        passwordInputField.contentType = TMP_InputField.ContentType.Password;
    }

    public void RegisterNewUser()
    {
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Por favor, introduce un correo y una contraseña válidos.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                feedbackText.text = "Error al registrar";
            }
            else
            {

                Firebase.Auth.AuthResult authResult = task.Result;
                FirebaseUser newUser = authResult.User;
                string therapistId = authResult.User.UserId;

                PlayerPrefs.SetString("therapistId", therapistId);
                PlayerPrefs.Save(); 

                feedbackText.text = "Registro exitoso. Usuario: " + newUser.Email;
                Debug.Log("Usuario registrado con éxito: " + newUser.Email);

                SaveUserToDatabase(newUser);

                Usuarios.SetActive(true);
                PaginaAuth.SetActive(false);
            }
        });
    }


    private void SaveUserToDatabase(FirebaseUser user)
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;

        Dictionary<string, object> userInfo = new Dictionary<string, object>
    {
        { "email", user.Email },
        { "uid", user.UserId },
        { "role", "terapeuta" },
        { "registrationDate", FieldValue.ServerTimestamp }
    };

        db.Collection("users").Document(user.UserId).SetAsync(userInfo).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Usuario guardado en la base de datos Firestore.");
            }
            else
            {
                Debug.LogError("Error guardando en Firestore: " + task.Exception);
            }
        });
    }

    public async void LoginUser()
    {
        Debug.Log("Entro a loginuser");
        string email = emailInputField.text;
        string password = passwordInputField.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            Debug.Log("Por favor, introduce un correo y una contraseña válidos.");
            feedbackText.text = "Por favor, introduce un correo y una contraseña válidos.";
            return;
        }

        await auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(async task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                Debug.Log("Por favor, introduce un correo y una contraseña válidos.");
                feedbackText.text = "Error al iniciar sesión";
                return;
            }
            Debug.Log("Inicio de sesión exitoso");

            FirebaseUser user = task.Result.User;
            Debug.Log("USER: " + user.UserId);
            PlayerPrefs.SetString("therapistId", user.UserId);
            PlayerPrefs.Save();

            feedbackText.text = "Inicio de sesión exitoso. Bienvenido: " + user.Email;

            // Esperar a que se complete la carga de los pacientes
            generarBotonesUsuario.GetPatientsForTherapist(user.UserId);

            Debug.Log("Holaa he hecho login");
            Usuarios.SetActive(true);
            PaginaAuth.SetActive(false);
        });
    }
}
