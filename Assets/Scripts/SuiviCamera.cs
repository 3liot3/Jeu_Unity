using UnityEngine;

/// <summary>
/// Suivi de caméra dédié au vaisseau.
/// La caméra suit une cible avec un décalage configurable, adapte la vitesse de suivi
/// en fonction de la distance (utile pour rattraper pendant un boost) et effectue
/// une rotation fluide pour toujours regarder la cible.
/// </summary>
public class SuiviCamera : MonoBehaviour
{
    /// <summary>Transform de la cible à suivre.</summary>
    public Transform cible;

    /// <summary>Décalage local par rapport à la cible (position souhaitée de la caméra).</summary>
    public Vector3 decalage = new Vector3(0f, 2.5f, -7f);

    [Header("Réglages de base")]
    /// <summary>Vitesse de suivi de base (position).</summary>
    public float vitesseSuivi = 10f;

    /// <summary>Vitesse de rotation pour lisser l'orientation de la caméra.</summary>
    public float vitesseRotation = 10f;

    [Header("Gestion du Boost")]
    /// <summary>
    /// Multiplicateur appliqué à la composante dépendante de la distance pour augmenter
    /// la vitesse de rattrapage lorsque la cible s'éloigne (par ex. en boost).
    /// Plus la valeur est élevée, plus la caméra rattrape rapidement.
    /// </summary>
    public float multiplicateurRattrapage = 2f;

    /// <summary>
    /// LateUpdate : met à jour la position et la rotation de la caméra après les déplacements de la cible.
    /// - Calcule la position souhaitée à partir du décalage local.
    /// - Adapte dynamiquement la vitesse de déplacement selon la distance à la position souhaitée.
    /// - Applique une interpolation linéaire pour la position et une interpolation sphérique pour la rotation.
    /// </summary>
    void LateUpdate()
    {
        if (cible == null) return;

        Vector3 positionVoulue = cible.TransformPoint(decalage);
        float distance = Vector3.Distance(transform.position, positionVoulue);
        float vitesseDynamique = vitesseSuivi + (distance * multiplicateurRattrapage);

        transform.position = Vector3.Lerp(transform.position, positionVoulue, vitesseDynamique * Time.deltaTime);

        Quaternion rotationVoulue = Quaternion.LookRotation(cible.position - transform.position, cible.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationVoulue, vitesseRotation * Time.deltaTime);
    }
}