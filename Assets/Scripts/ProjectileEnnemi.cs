using UnityEngine;

/// <summary>
/// Comportement d'un projectile ennemi :
/// - avance à une vitesse fixe,
/// - s'autodétruit après <see cref="dureeVie"/> secondes,
/// - inflige une perte de vie au joueur lors du contact (détecte le joueur via <see cref="MouvementVaisseau"/>).
/// </summary>
public class ProjectileEnnemi : MonoBehaviour
{
    /// <summary>Vitesse de déplacement du projectile (unités/s).</summary>
    public float vitesse = 40f;

    /// <summary>Durée de vie en secondes avant destruction automatique.</summary>
    public float dureeVie = 5f;

    /// <summary>Initialisation : programme la destruction automatique du projectile.</summary>
    void Start()
    {
        Destroy(gameObject, dureeVie);
    }

    /// <summary>Déplacement frontal du projectile chaque frame.</summary>
    void Update()
    {
        transform.Translate(Vector3.forward * vitesse * Time.deltaTime);
    }

    /// <summary>
    /// Détecte les collisions en mode trigger.
    /// Si le collider appartient au joueur (présence du script <see cref="MouvementVaisseau"/>),
    /// demande au <see cref="GameManager"/> de retirer une vie puis détruit le projectile.
    /// </summary>
    /// <param name="other">Collider entrant dans le trigger.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MouvementVaisseau>() != null)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.PerdreUneVie("Touché par un tir ennemi !");
            }

            Destroy(gameObject);
        }
    }
}