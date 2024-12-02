using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VistasController : MonoBehaviour
{
    public GameObject Usuarios;
    public GameObject RegistrarNuevoPaciente;
    public GameObject DetallesPaciente;

    public void RegistrarNuevoPaciente_Activar()
    {
        Usuarios.SetActive(false);
        RegistrarNuevoPaciente.SetActive(true);
    }

    public void Usuarios_Activar()
    {
        Usuarios.SetActive(true);
        RegistrarNuevoPaciente.SetActive(false);
        DetallesPaciente.SetActive(false);
    }

}
