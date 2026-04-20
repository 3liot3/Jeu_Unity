using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MenuManager : MonoBehaviour
{
    [Header("Panneaux Principaux")]
    public GameObject panelPrincipal;
    public GameObject panelReglages;
    public GameObject panelInfo;

    [Header("Sous-Panneaux Réglages")]
    public GameObject panelVolume;
    public GameObject panelBinds;

    [Header("Audio")]
    public AudioMixer masterMixer;
    public Slider sliderMusique;
    public Slider sliderSFX;

    void Start()
    {
        InitialiserVolume();
        OuvrirMenuPrincipal();
    }

    // --- INITIALISATION AUDIO ---
    private void InitialiserVolume()
    {
        float volMusique = PlayerPrefs.GetFloat("VolMusique", 0.02f);
        float volSFX = PlayerPrefs.GetFloat("VolSFX", 0.02f);

        if (sliderMusique) sliderMusique.value = volMusique;
        if (sliderSFX) sliderSFX.value = volSFX;

        SetVolumeMusique(volMusique);
        SetVolumeSFX(volSFX);
    }

    public void SetVolumeMusique(float value)
    {
        masterMixer.SetFloat("VolumeMusique", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("VolMusique", value);
    }

    public void SetVolumeSFX(float value)
    {
        masterMixer.SetFloat("VolumeSFX", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("VolSFX", value);
    }

    // --- LANCEMENT DU JEU ---
    public void LancerJeuNoob() { LancerJeuAvecDifficulte(0); }
    public void LancerJeuNormal() { LancerJeuAvecDifficulte(1); }
    public void LancerJeuExpert() { LancerJeuAvecDifficulte(2); }

    private void LancerJeuAvecDifficulte(int difficulte)
    {
        PlayerPrefs.SetInt("DifficulteJeu", difficulte);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneJeu");
    }

    // --- NAVIGATION PRINCIPALE ---
    public void OuvrirMenuPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelReglages.SetActive(false);
        panelInfo.SetActive(false);
        panelVolume.SetActive(false); // Sécurité
        panelBinds.SetActive(false);  // Sécurité
    }

    public void OuvrirReglages()
    {
        panelPrincipal.SetActive(false);
        panelReglages.SetActive(true);
        panelInfo.SetActive(false);
    }

    public void OuvrirInfo()
    {
        panelPrincipal.SetActive(false);
        panelReglages.SetActive(false);
        panelInfo.SetActive(true);
    }

    // --- NAVIGATION SOUS-MENUS ---
    public void ClicBoutonVolume()
    {
        panelReglages.SetActive(false);
        panelVolume.SetActive(true);
    }

    public void ClicBoutonBinds()
    {
        panelReglages.SetActive(false);
        panelBinds.SetActive(true);
    }

    public void RetourAuxReglages() // Bouton Retour DEPUIS Volume ou Binds
    {
        panelVolume.SetActive(false);
        panelBinds.SetActive(false);
        panelReglages.SetActive(true); // On réaffiche le menu Réglages !
    }
}