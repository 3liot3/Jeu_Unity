using UnityEngine;

/// <summary>
/// Comportement d'un projectile joueur :
/// - se déplace en avant à une vitesse fixe,
/// - s'autodétruit après une durée de vie configurable,
/// - inflige des dégâts aux différents types d'ennemis détectés et gère sa propre destruction visuelle/physique.
/// </summary>
public class Projectile : MonoBehaviour
{
    /// <summary>Vitesse de déplacement du projectile (unités / seconde).</summary>
    public float vitesse = 100f;

    /// <summary>Durée de vie en secondes avant destruction automatique.</summary>
    public float dureeVie = 3f;

    /// <summary>Points de dégâts infligés lors de l'impact.</summary>
    public int pointsDeDegats = 10;

    /// <summary>Programme la destruction automatique après <see cref="dureeVie"/> secondes.</summary>
    void Start()
    {
        Destroy(gameObject, dureeVie);
    }

    /// <summary>Déplace le projectile vers l'avant chaque frame.</summary>
    void Update()
    {
        transform.Translate(Vector3.forward * vitesse * Time.deltaTime);
    }

    /// <summary>
    /// Gestion des collisions en mode trigger :
    /// - ignore les collisions avec le joueur ou d'autres projectiles,
    /// - tente de trouver un composant d'ennemi parent et lui applique des dégâts,
    /// - détruit le projectile ou déclenche son explosion si nécessaire.
    /// </summary>
    /// <param name="other">Collider entrant dans le trigger.</param>
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Projectile")) return;

        Boss1_Tuto b1 = other.GetComponentInParent<Boss1_Tuto>();
        Boss2_Kamikaze b2 = other.GetComponentInParent<Boss2_Kamikaze>();
        Boss3_Sniper b3 = other.GetComponentInParent<Boss3_Sniper>();
        Boss4_Doigt b4 = other.GetComponentInParent<Boss4_Doigt>();
        Boss5_Putrefaction b5 = other.GetComponentInParent<Boss5_Putrefaction>();

        if (b1 != null) { b1.PrendreDegats(pointsDeDegats); DestructionProjectile(); }
        else if (b2 != null) { b2.PrendreDegats(pointsDeDegats); DestructionProjectile(); }
        else if (b3 != null) { b3.PrendreDegats(pointsDeDegats); DestructionProjectile(); }
        else if (b4 != null) { b4.PrendreDegats(pointsDeDegats); DestructionProjectile(); }
        else if (b5 != null) { b5.PrendreDegats(pointsDeDegats); DestructionProjectile(); }
        else
        {
            DestructionProjectile();
        }
    }

    /// <summary>
    /// Gère la destruction du projectile :
    /// - si le projectile possède un composant <see cref="MissileSpirale"/>, appelle son explosion spécifique,
    /// - sinon détruit simplement le GameObject (ex: lasers).
    /// </summary>
    private void DestructionProjectile()
    {
        MissileSpirale missile = GetComponent<MissileSpirale>();
        if (missile != null)
        {
            missile.Exploser();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}