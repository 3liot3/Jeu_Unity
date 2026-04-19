using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestion du menu principal : navigation entre panneaux et lancement de la scène de jeu.
/// Sauvegarde la difficulté sélectionnée via <see cref="PlayerPrefs"/> afin d'être lue par la scène de jeu.
/// </summary>
public class MenuManager : MonoBehaviour
{
    /// <summary>Panneau principal du menu.</summary>
    [Header("Panneaux")]
    public GameObject panelPrincipal;

    /// <summary>Panneau des réglages.</summary>
    public GameObject panelReglages;

    /// <summary>Panneau d'information / crédits.</summary>
    public GameObject panelInfo;

    /// <summary>
    /// Initialise l'interface en ouvrant le panneau principal.
    /// </summary>
    void Start()
    {
        OuvrirMenuPrincipal();
    }

    /// <summary>Lance la partie en mode "Noob".</summary>
    public void LancerJeuNoob() { LancerJeuAvecDifficulte(0); }

    /// <summary>Lance la partie en mode "Normal".</summary>
    public void LancerJeuNormal() { LancerJeuAvecDifficulte(1); }

    /// <summary>Lance la partie en mode "Expert".</summary>
    public void LancerJeuExpert() { LancerJeuAvecDifficulte(2); }

    /// <summary>
    /// Sauvegarde la difficulté choisie et charge la scène de jeu.
    /// Le nom de la scène doit correspondre à celui enregistré dans le projet.
    /// </summary>
    /// <param name="difficulte">Indice de difficulté (0 = Noob, 1 = Normal, 2 = Expert).</param>
    private void LancerJeuAvecDifficulte(int difficulte)
    {
        PlayerPrefs.SetInt("DifficulteJeu", difficulte);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneJeu");
    }

    /// <summary>Affiche le panneau principal et masque les autres panneaux.</summary>
    public void OuvrirMenuPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelReglages.SetActive(false);
        panelInfo.SetActive(false);
    }

    /// <summary>Affiche le panneau des réglages et masque les autres panneaux.</summary>
    public void OuvrirReglages()
    {
        panelPrincipal.SetActive(false);
        panelReglages.SetActive(true);
        panelInfo.SetActive(false);
    }

    /// <summary>Affiche le panneau d'information et masque les autres panneaux.</summary>
    public void OuvrirInfo()
    {
        panelPrincipal.SetActive(false);
        panelReglages.SetActive(false);
        panelInfo.SetActive(true);
    }

    /// <summary>
    /// Point d'entrée pour l'ouverture de la configuration des touches.
    /// Implémentation à fournir selon les besoins du projet.
    /// </summary>
    public void ClicBoutonBinds() { }

    /// <summary>
    /// Point d'entrée pour l'ouverture des options de volume.
    /// Implémentation à fournir selon les besoins du projet.
    /// </summary>
    public void ClicBoutonVolume() { }
}