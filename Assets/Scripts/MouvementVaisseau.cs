using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le mouvement du vaisseau : déplacement avant, rotations et système de boost.
/// - Déplacement en avant avec vitesse de croisière et boost.
/// - Contrôle des rotations (pitch, yaw, roll) via touches.
/// - Gestion d'une ressource de boost avec régénération et affichage via Slider.
/// </summary>
public class MouvementVaisseau : MonoBehaviour
{
    [Header("Vitesses")]
    /// <summary>Vitesse de croisière quand il n'y a pas de boost.</summary>
    public float vitesseCroisiere = 20f;

    /// <summary>Multiplicateur appliqué à la vitesse lors du boost.</summary>
    public float multiplicateurBoost = 300f;

    [Header("Vitesses de rotation")]
    /// <summary>Vitesse de rotation pour le pitch (haut/bas).</summary>
    public float vitessePitch = 80f;

    /// <summary>Vitesse de rotation pour le yaw (gauche/droite).</summary>
    public float vitesseYaw = 80f;

    /// <summary>Vitesse de rotation pour le roll (roulis).</summary>
    public float vitesseRoll = 100f;

    [Header("Système de Boost")]
    /// <summary>Quantité maximale de boost disponible (en secondes de boost).</summary>
    public float boostMax = 3f;

    /// <summary>Quantité actuelle de boost restante.</summary>
    public float boostActuel;

    /// <summary>Slider UI optionnel représentant la barre de boost.</summary>
    public Slider barreBoost;

    /// <summary>Indique si le vaisseau est actuellement en boost. Lecture publique, écriture privée.</summary>
    public bool estEnBoost { get; private set; }

    private bool besoinResetBoost = false;

    /// <summary>
    /// Initialisation : configure la quantité de boost initiale et synchronise le slider si présent.
    /// </summary>
    void Start()
    {
        boostActuel = boostMax;
        if (barreBoost != null)
        {
            barreBoost.maxValue = boostMax;
            barreBoost.value = boostActuel;
        }
    }

    /// <summary>
    /// Boucle de mise à jour :
    /// - Gère l'activation/désactivation du boost et sa régénération.
    /// - Met à jour l'UI du boost si fournie.
    /// - Déplace le vaisseau vers l'avant et applique les rotations selon les entrées clavier.
    /// </summary>
    void Update()
    {
        // --- LOGIQUE DU BOOST ---

        // Si on relâche Shift, on autorise à nouveau le boost
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            besoinResetBoost = false;
        }

        // On peut booster si : Shift est pressé ET on a du carburant ET on n'est pas bloqué par la sécurité
        if (Input.GetKey(KeyCode.LeftShift) && boostActuel > 0 && !besoinResetBoost)
        {
            estEnBoost = true;
            boostActuel -= Time.deltaTime;

            // Si on tombe à zéro pile maintenant
            if (boostActuel <= 0)
            {
                boostActuel = 0;
                estEnBoost = false;
                besoinResetBoost = true; // Force le joueur à relâcher la touche
            }
        }
        else
        {
            estEnBoost = false;
            // Régénération (1s de boost toutes les 3s)
            if (boostActuel < boostMax)
            {
                boostActuel += (1f / 3f) * Time.deltaTime;
            }
        }

        if (barreBoost != null) barreBoost.value = boostActuel;

        // --- DÉPLACEMENT ---
        float v = estEnBoost ? (vitesseCroisiere * multiplicateurBoost) : vitesseCroisiere;
        transform.Translate(Vector3.forward * v * Time.deltaTime);

        // --- ROTATIONS (Z/S, Q/D, A/E) ---
        float pitch = 0f; float yaw = 0f; float roll = 0f;
        if (Input.GetKey(KeyCode.W)) pitch = vitessePitch * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) pitch = -vitessePitch * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) yaw = vitesseYaw * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) yaw = -vitesseYaw * Time.deltaTime;
        if (Input.GetKey(KeyCode.Q)) roll = vitesseRoll * Time.deltaTime;
        if (Input.GetKey(KeyCode.E)) roll = -vitesseRoll * Time.deltaTime;

        transform.Rotate(pitch, yaw, roll, Space.Self);
    }
}