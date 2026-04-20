using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Indispensable pour les Sliders
using UnityEngine.Audio; // Indispensable pour le Mixer

public class MenuManager : MonoBehaviour
{
    [Header("Panneaux")]
    public GameObject panelPrincipal;
    public GameObject panelReglages;
    public GameObject panelInfo;

    [Header("Audio")]
    public AudioMixer masterMixer;
    public Slider sliderMusique;
    public Slider sliderSFX;

    void Start()
    {
        // On charge les volumes sauvegardés au démarrage
        InitialiserVolume();
        OuvrirMenuPrincipal();
    }

    private void InitialiserVolume()
    {
        // On récupère les valeurs sauvegardées (0.75 par défaut)
        float volMusique = PlayerPrefs.GetFloat("VolMusique", 0.75f);
        float volSFX = PlayerPrefs.GetFloat("VolSFX", 0.75f);

        // On applique aux sliders
        if (sliderMusique) sliderMusique.value = volMusique;
        if (sliderSFX) sliderSFX.value = volSFX;

        // On applique au mixer
        SetVolumeMusique(volMusique);
        SetVolumeSFX(volSFX);
    }

    // Fonctions appelées par les Sliders (On Value Changed)
    public void SetVolumeMusique(float value)
    {
        // Le Mixer utilise des Décibels (-80 à 20). Le slider va de 0 à 1.
        // On utilise un calcul logarithmique pour que ce soit naturel à l'oreille.
        masterMixer.SetFloat("VolumeMusique", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("VolMusique", value);
    }

    public void SetVolumeSFX(float value)
    {
        masterMixer.SetFloat("VolumeSFX", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("VolSFX", value);
    }

    // --- NAVIGATION ET JEU (Garde tes fonctions précédentes) ---
    public void LancerJeuNoob() { LancerJeuAvecDifficulte(0); }
    public void LancerJeuNormal() { LancerJeuAvecDifficulte(1); }
    public void LancerJeuExpert() { LancerJeuAvecDifficulte(2); }

    private void LancerJeuAvecDifficulte(int difficulte)
    {
        PlayerPrefs.SetInt("DifficulteJeu", difficulte);
        PlayerPrefs.Save();
        SceneManager.LoadScene("SceneJeu");
    }

    public void OuvrirMenuPrincipal() { panelPrincipal.SetActive(true); panelReglages.SetActive(false); panelInfo.SetActive(false); }
    public void OuvrirReglages() { panelPrincipal.SetActive(false); panelReglages.SetActive(true); panelInfo.SetActive(false); }
    public void OuvrirInfo() { panelPrincipal.SetActive(false); panelReglages.SetActive(false); panelInfo.SetActive(true); }
}