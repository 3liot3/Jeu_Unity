using UnityEngine;

/// <summary>
/// Représente un gros rayon laser physique :
/// - Détruit automatiquement l'objet après <see cref="dureeVie"/> secondes.
/// - Si le laser entre en collision avec le joueur, demande au <see cref="GameManager"/> de retirer une vie.
/// </summary>
public class GrosLaserPhysique : MonoBehaviour
{
    /// <summary>Durée de vie du projectile en secondes avant destruction.</summary>
    public float dureeVie = 0.5f;

    /// <summary>
    /// Initialisation : programme la destruction automatique de l'objet après <see cref="dureeVie"/>.
    /// </summary>
    void Start()
    {
        Destroy(gameObject, dureeVie);
    }

    /// <summary>
    /// Déclenché lorsqu'un collider entre dans le trigger.
    /// Si le collider appartient au joueur (tag "Player" ou contient un composant <see cref="MouvementVaisseau"/>),
    /// invoque la méthode de perte de vie du <see cref="GameManager"/>.
    /// </summary>
    /// <param name="other">Collider reçu par l'événement.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<MouvementVaisseau>() != null)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.PerdreUneVie("Désintégré par le rayon !");
            }
        }
    }
}