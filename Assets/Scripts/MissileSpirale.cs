using UnityEngine;

/// <summary>
/// Comportement d'un missile en spirale:
/// - avance en avant tout en décrivant une spirale croissante,
/// - explose soit à la fin de sa durée de vie, soit lors d'une collision avec une cible ennemie,
/// - inflige des dégâts aux différents types de boss détectés.
/// </summary>
public class MissileSpirale : MonoBehaviour
{
    [Header("Réglages Spirale")]
    /// <summary>Vitesse d'avance du missile (unités/s).</summary>
    public float vitesseAvant = 40f;

    /// <summary>Vitesse angulaire de la spirale.</summary>
    public float vitesseRotation = 15f;

    /// <summary>Largeur maximale de la spirale.</summary>
    public float tailleSpiraleMax = 10f;

    /// <summary>Durée de vie totale du missile avant explosion (secondes).</summary>
    public float dureeVie = 4f;

    [Header("Dégâts")]
    /// <summary>Points de dégâts infligés lors de l'impact/explosion.</summary>
    public int pointsDeDegats = 50;

    [Header("Effets Visuels")]
    /// <summary>Prefab de l'effet d'explosion instancié à l'explosion du missile.</summary>
    public GameObject effetExplosion;

    /// <summary>Chronomètre interne (secondes écoulées depuis la création).</summary>
    private float chronometre = 0f;

    /// <summary>
    /// Initialisation. La destruction différée est gérée explicitement via <see cref="Exploser"/>.
    /// </summary>
    void Start()
    {
    }

    /// <summary>
    /// Mise à jour par frame : calcule et applique le mouvement en spirale, et déclenche l'explosion si la durée est atteinte.
    /// </summary>
    void Update()
    {
        chronometre += Time.deltaTime;

        float percent = chronometre / dureeVie;
        float tailleSpiraleActuelle = percent * tailleSpiraleMax;

        transform.Translate(Vector3.forward * vitesseAvant * Time.deltaTime);

        float deplacementX = Mathf.Cos(chronometre * vitesseRotation) * tailleSpiraleActuelle * Time.deltaTime;
        float deplacementY = Mathf.Sin(chronometre * vitesseRotation) * tailleSpiraleActuelle * Time.deltaTime;

        transform.Translate(new Vector3(deplacementX, deplacementY, 0f), Space.Self);

        if (chronometre >= dureeVie)
        {
            Exploser();
        }
    }

    /// <summary>
    /// Explose le missile : instancie l'effet d'explosion (si présent) puis détruit le GameObject.
    /// </summary>
    public void Exploser()
    {
        if (effetExplosion != null)
        {
            Instantiate(effetExplosion, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }

    /// <summary>
    /// Gestion des collisions en mode trigger :
    /// - ignore le joueur et d'autres projectiles,
    /// - détecte les différents types de boss et leur applique des dégâts,
    /// - gère la destruction visuelle/physique du missile.
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
    /// Gère la destruction visuelle du projectile : si ce composant est un <see cref="MissileSpirale"/>,
    /// appelle <see cref="Exploser"/> pour utiliser l'effet d'explosion ; sinon détruit simplement le GameObject.
    /// </summary>
    void DestructionProjectile()
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