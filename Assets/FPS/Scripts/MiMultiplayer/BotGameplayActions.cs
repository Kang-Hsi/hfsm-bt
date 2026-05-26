using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Gestor de acciones para el bot jugador (<c>UCM_Bot</c>).
/// <para>
/// Propósito: Ejemplo de clase que centraliza en un solo sitio llamadas con nombre claro ("ir aquí", "disparar",
/// "cambiar arma", etc.) para que la <see cref="FSM"/> (u otra IA) no tenga que conocer todos los
/// detalles de <see cref="PlayerCharacterController"/>, <see cref="PlayerWeaponsManager"/>, etc.
/// </para>
/// <para>
/// <b>Importante (diseño actual del proyecto):</b> el prefab del bot suele desactivar
/// <see cref="PlayerInputHandler"/> y <see cref="PlayerWeaponsManager"/> en <c>Awake</c> para que no
/// compitan con el teclado/ratón. Eso retrasa la inicialización de armas hasta que alguien vuelva a
/// habilitar <see cref="PlayerWeaponsManager"/> (por ejemplo desde
/// <see cref="InitializeWeaponSystemsIfNeeded"/>). ¡Ojo, sin armas inicializadas, los métodos de combate
/// no tendrán efecto!
/// </para>
/// <para>
/// <b>Movimiento:</b> hoy el bot se desplaza en servidor con <see cref="NavMeshAgent"/> (autoridad de
/// transform en red para bots). Los métodos de navegación de esta clase encapsulan ese camino.
/// Si en el futuro queréis controlar al personaje igual que un humano (<see cref="CharacterController"/>),
/// tendréis que inyectar "input sintético" (ampliando <see cref="PlayerInputHandler"/>) o duplicar
/// parte de la física 
/// </para>
/// </summary>
[DisallowMultipleComponent]
public class BotGameplayActions : MonoBehaviour
{
    [Header("Navegación (NavMeshAgent)")]
    [Tooltip("Si no hay agente en el prefab, se crea uno en tiempo de ejecución al inicializar.")]
    [SerializeField] bool m_AutoCreateNavMeshAgent = true;

    [SerializeField] float m_DefaultStoppingDistance = 1.5f;

    [Header("Combate (opcional)")]
    [Tooltip("Si es true, en InitializeWeaponSystemsIfNeeded se habilita PlayerWeaponsManager para que ejecute Start y cree las armas iniciales.")]
    [SerializeField] bool m_EnableWeaponManagerForBot = false;

    NavMeshAgent m_NavMeshAgent;
    PlayerWeaponsManager m_Weapons;
    Health m_Health;
    PlayerCharacterController m_PlayerCc;
    Transform m_target;

    Vector3 m_LastWorldPosForAnim;
    bool m_HasLastWorldPosForAnim;

    /// <summary>Referencia al agente de navegación del bot (puede ser null antes de inicializar).</summary>
    public NavMeshAgent NavMeshAgent => m_NavMeshAgent;

    /// <summary>Vida del personaje; útil para transiciones.</summary>
    public Health Health => m_Health;

    public Transform Target => m_target;

    void Awake()
    {
        m_Health = GetComponent<Health>();
        m_PlayerCc = GetComponent<PlayerCharacterController>();
        m_Weapons = GetComponent<PlayerWeaponsManager>();
        m_NavMeshAgent = GetComponent<NavMeshAgent>();

        InitializeWeaponSystemsIfNeeded();
        GetComponent<CharacterController>().enabled = true;
    }
    void OnEnable()
    {
        m_HasLastWorldPosForAnim = false;
    }

    /// <summary>
    /// Llamar desde la IA en el servidor cuando queráis asegurar que el stack de armas está listo
    /// para usar <see cref="SwitchToWeaponSlot"/> / <see cref="TryFireCurrentWeaponPrimary"/>.
    /// </summary>
    public void InitializeWeaponSystemsIfNeeded()
    {
        if (!m_EnableWeaponManagerForBot || m_Weapons == null)
            return;

        if (!m_Weapons.enabled)
            m_Weapons.enabled = true;
    }

    /// <summary>
    /// Comprueba si la escena activa tiene datos de NavMesh bakeados (p. ej. ya cargó el mapa de juego).
    /// Útil para no llamar a <see cref="NavMeshAgent"/> mientras sigue activa la escena de menú.
    /// </summary>
    public static bool SceneHasNavMeshData()
    {
        var tri = NavMesh.CalculateTriangulation();
        return tri.indices != null && tri.indices.Length >= 3;
    }

    /// <summary>
    /// Prepara o configura el <see cref="NavMeshAgent"/> para el modo bot (servidor).
    /// No crea el componente hasta que exista NavMesh en escena; si el transform aún no está sobre la malla,
    /// intenta un <see cref="NavMeshAgent.Warp"/> al punto más cercano.
    /// </summary>
    public void EnsureNavMeshAgentReady()
    {
        if (m_NavMeshAgent == null && m_AutoCreateNavMeshAgent)
        {
            if (!SceneHasNavMeshData())
                return;

            m_NavMeshAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        if (m_NavMeshAgent == null)
            return;

        m_NavMeshAgent.enabled = true;
        m_NavMeshAgent.stoppingDistance = Mathf.Max(0.25f, m_DefaultStoppingDistance);
        m_NavMeshAgent.autoBraking = true;
        m_NavMeshAgent.updatePosition = true;
        m_NavMeshAgent.updateRotation = true;

        // Radio pequeño: si el jugador ya está bien posicionado en un sótano, un radio grande podía
        // proyectar a otra capa de NavMesh más alta (otra planta) y provocar desplazamientos raros.
        if (!m_NavMeshAgent.isOnNavMesh &&
            NavMesh.SamplePosition(transform.position, out var hit, 2.5f, NavMesh.AllAreas))
        {
            m_NavMeshAgent.Warp(hit.position);
        }
    }

    void LateUpdate()
    {
        // UCM_Bot desactiva PlayerCharacterController; el Animator no recibe Forward/Strafe. Replicamos
        // la idea del PCC usando la velocidad real del transform (válida en servidor y clientes vía red).
        if (GetComponent<FSM>() == null)
            return;

        DriveThirdPersonLocomotionAnimator();
    }

    void DriveThirdPersonLocomotionAnimator()
    {
        if (m_PlayerCc == null)
            return;

        var anim = m_PlayerCc.CharacterAnimator;
        if (anim == null)
            return;

        if (m_Health != null && m_Health.CurrentHealth <= 0f)
            return;

        if (anim.GetBool("IsDead"))
            return;

        float dt = Time.deltaTime;
        if (dt < 1e-5f)
            return;

        if (!m_HasLastWorldPosForAnim)
        {
            m_LastWorldPosForAnim = transform.position;
            m_HasLastWorldPosForAnim = true;
            return;
        }

        Vector3 worldVel = (transform.position - m_LastWorldPosForAnim) / dt;
        m_LastWorldPosForAnim = transform.position;

        Vector3 localVel = transform.InverseTransformDirection(worldVel);
        float maxSpd = Mathf.Max(0.01f, m_PlayerCc.MaxSpeedOnGround);
        float forward = Mathf.Clamp(localVel.z / maxSpd, -1f, 1f);
        float strafe = Mathf.Clamp(localVel.x / maxSpd, -1f, 1f);

        anim.SetFloat("Forward", forward, 0.12f, dt);
        anim.SetFloat("Strafe", strafe, 0.12f, dt);
        anim.SetBool("IsGrounded", true);
        anim.SetBool("IsAiming", m_Weapons != null && m_Weapons.IsAiming);
    }

    /// <summary>Ordena moverse hacia un punto del mundo (debe ser alcanzable por NavMesh).</summary>
    /// <returns><c>true</c> si se pudo fijar un destino válido.</returns>
    public bool TryMoveToWorldPosition(Vector3 worldPosition)
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.isActiveAndEnabled)
        {
            return false;
        }
        if (!m_NavMeshAgent.isOnNavMesh)
        {
            return false;
        }
        m_NavMeshAgent.isStopped = false;
        return m_NavMeshAgent.SetDestination(worldPosition);
    }

    /// <summary>Detiene la navegación y cancela el path actual.</summary>
    public void StopNavigation()
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled)
            return;

        m_NavMeshAgent.isStopped = true;
        m_NavMeshAgent.ResetPath();
    }

    /// <summary>Desactiva por completo el agente (p. ej. al morir).</summary>
    public void DisableNavMeshAgent()
    {
        if (m_NavMeshAgent == null)
            return;

        StopNavigation();
        m_NavMeshAgent.enabled = false;
    }

    /// <summary>Vuelve a habilitar el agente tras un respawn.</summary>
    public void EnableNavMeshAgent()
    {
        EnsureNavMeshAgentReady();
        if (m_NavMeshAgent != null && m_NavMeshAgent.enabled)
            m_NavMeshAgent.isStopped = false;
    }

    /// <summary>Distancia restante aproximada en el path actual (o infinito si no hay path).</summary>
    public float GetPathRemainingDistance()
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled || !m_NavMeshAgent.hasPath)
            return float.PositiveInfinity;
        return m_NavMeshAgent.remainingDistance;
    }

    /// <summary>¿Ha llegado (aprox.) al destino con el umbral del agente?</summary>
    public bool HasReachedCurrentDestination()
    {
        if (m_NavMeshAgent == null || !m_NavMeshAgent.enabled)
            return true;
        if (m_NavMeshAgent.pathPending)
            return false;
        return !m_NavMeshAgent.hasPath || m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance + 0.35f;
    }

    /// <summary>
    /// Gira el cuerpo del bot (eje Y) para mirar hacia un punto del suelo.
    /// Útil antes de disparar. No modifica la inclinación vertical de la cámara del prefab humano
    /// (eso sigue ligado al stack de FPS); solo alinea el forward horizontal.
    /// </summary>
    public void FaceTowardsWorldPoint(Vector3 worldPoint)
    {
        Vector3 flat = worldPoint - transform.position;
        flat.y = 0f;
        if (flat.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);
    }

    public void FaceTowardsWorldPointSmooth(Vector3 worldPoint)
    {
        Vector3 flat = worldPoint - transform.position;
        flat.y = 0f;

        if (flat.sqrMagnitude < 0.0001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(flat.normalized, Vector3.up);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * 12f
        );
    }

    /// <summary>
    /// Gira el cuerpo hacia una dirección horizontal (XZ).
    /// </summary>
    public void SetFacingDirection(Vector3 horizontalDirection)
    {
        horizontalDirection.y = 0f;
        if (horizontalDirection.sqrMagnitude < 0.0001f)
            return;
        transform.rotation = Quaternion.LookRotation(horizontalDirection.normalized, Vector3.up);
    }

    // --- Armas (vía APIs públicas del proyecto) -----------------------------------------------

    /// <summary>Índice de arma activa, o -1 si ninguna.</summary>
    public int GetActiveWeaponSlotIndex()
    {
        return m_Weapons != null ? m_Weapons.ActiveWeaponIndex : -1;
    }

    /// <summary>Referencia al arma activa (puede ser null).</summary>
    public WeaponController GetActiveWeaponOrNull()
    {
        return m_Weapons != null ? m_Weapons.GetActiveWeapon() : null;
    }

    /// <summary>Cambia al arma del slot (0..8 según el array interno del manager).</summary>
    public void SwitchToWeaponSlot(int slotIndex, bool force = false)
    {
        if (m_Weapons == null)
            return;
        m_Weapons.SwitchToWeaponIndex(slotIndex, force);
    }

    /// <summary>Pasa al siguiente/previo arma equipada (orden circular del manager).</summary>
    public void SwitchToNextWeaponInInventory()
    {
        if (m_Weapons == null)
            return;
        m_Weapons.SwitchWeapon(ascendingOrder: true);
    }

    public void SwitchToPreviousWeaponInInventory()
    {
        if (m_Weapons == null)
            return;
        m_Weapons.SwitchWeapon(ascendingOrder: false);
    }

    /// <summary>
    /// Disparo primario del arma activa usando la misma API que el input humano acaba llamando.
    /// Pasad los tres flags como en un botón: pulsación, mantener, soltar.
    /// </summary>
    public bool TryFireCurrentWeaponPrimary(bool pressedDown, bool held, bool released)
    {
        var w = GetActiveWeaponOrNull();
        if (w == null)
            return false;
        return w.HandleShootInputs(pressedDown, held, released);
    }

    /// <summary>Inicia la recarga del arma activa (animación + estado interno del arma).</summary>
    public void TryReloadActiveWeapon()
    {
        var w = GetActiveWeaponOrNull();
        if (w == null || w.IsReloading)
            return;
        if (!w.AutomaticReload && w.CurrentAmmoRatio < 1f)
            w.StartReloadAnimation();
    }

    // --- "Intención" de movimiento estilo FPS (útil para conectar la IA aquí) ---------

    /// <summary>
    /// Valores que un humano produce con WASD + sprint + agacharse. El proyecto <i>no</i> los lee
    /// todavía desde aquí: están expuestos para que podáis redirigirlos a
    /// <see cref="PlayerInputHandler"/> en un futuro, cuando queráis hilar muy fino para IMITAR la forma de controlar al personaje de un humano.
    /// </summary>
    public struct LocomotionIntent
    {
        /// <summary>En espacio local del jugador: X strafe, Z adelante/atrás (como GetMoveInput).</summary>
        public Vector3 Move;

        public bool Sprint;
        public bool Crouch;
        public bool JumpPressed;
        public bool AimHeld;
    }

    /// <summary>Buffer de intención que la FSM puede rellenar; integración con PCC pendiente.</summary>
    public LocomotionIntent BufferedLocomotion;

    /// <summary>Fija la intención de movimiento para un posible puente futuro con <see cref="PlayerInputHandler"/>.</summary>
    public void SetLocomotionIntent(in LocomotionIntent intent)
    {
        BufferedLocomotion = intent;
    }

    /// <summary>Ejemplo de uso: moverse "como teclas" hacia delante/derecha en espacio local.</summary>
    public void SetLocomotionIntentSimple(Vector2 xz, bool sprint, bool crouch, bool jump, bool aim)
    {
        BufferedLocomotion = new LocomotionIntent
        {
            Move = new Vector3(xz.x, 0f, xz.y),
            Sprint = sprint,
            Crouch = crouch,
            JumpPressed = jump,
            AimHeld = aim
        };
    }

    // --- Consultas rápidas (podéis añadir más si lo consideráis necesario -------------------------------------------------------------------

    /// <summary>¿Sigue vivo el bot?</summary>
    public bool IsAlive()
    {
        return m_Health == null || m_Health.CurrentHealth > 0f;
    }

    public bool EnemyNear(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        foreach (var col in hits)
        {
            if (col.transform.root == transform)
                continue;

            if (!col.transform.root.CompareTag("Enemy"))
                continue;

            Transform enemy = col.transform.root;

            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 target = enemy.position + Vector3.up * 1.5f;

            Vector3 dir = (target - origin);
            float dist = dir.magnitude;
            dir.Normalize();

            // probar si hay muro entre medio
            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                if (hit.transform.root != enemy)
                    continue;
            }
            m_target = enemy;
            return true;
        }
        m_target = null;
        return false;
    }

    public void ResetAI()
    {
        StopNavigation();
        m_target = null;
        TryFireCurrentWeaponPrimary(false, false, true);
    }

    public bool TryPickAWeapon(Vector3 origin, out Vector3 result)
    {
        var points = WaypointsManager.Instance.weaponWaypoints;

        Transform nearest = null;
        float minDist = float.MaxValue;

        foreach (var t in points)
        {
            if (t == null || !t.gameObject.activeInHierarchy) continue;
            var pickup = t.GetComponent<WeaponPickup>();
            if (pickup == null) continue;
            if (m_Weapons != null && pickup.WeaponPrefab != null && m_Weapons.HasWeapon(pickup.WeaponPrefab) != null) continue;

            float d = (t.position - origin).sqrMagnitude;
            if (d < minDist)
            {
                minDist = d;
                nearest = t;
            }
        }

        if (nearest == null)
        {
            result = origin;
            return false;
        }
        result = nearest.position;
        return true;
    }

    //find medkit
    public bool TryPickHealth(Vector3 origin, out Vector3 result)
    {
        var pickups = Object.FindObjectsByType<HealthPickup>(FindObjectsSortMode.None);
        return TryNearestActive(origin, pickups, out result);
    }

    static bool TryNearestActive<T>(Vector3 origin, T[] components, out Vector3 result) where T : Component
    {
        Vector3 best = Vector3.zero;
        float bestDist = float.MaxValue;
        bool found = false;

        foreach (var c in components)
        {
            if (c == null || !c.gameObject.activeInHierarchy) continue;
            Vector3 pos = c.transform.position;
            float d = (pos - origin).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = pos;
                found = true;
            }
        }

        result = found ? best : origin;
        return found;
    }

    public bool TryPickRandomNavMeshPoint(Vector3 origin, float radius, out Vector3 result)
    {
        for (int i = 0; i < 20; i++)
        {
            var rnd = Random.insideUnitSphere * radius;
            var candidate = origin + rnd;
            if (NavMesh.SamplePosition(candidate, out var hit, 2.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }

        result = origin;
        return false;
    }


    //buscar arma para cambiar
    public int FindValidWeapon()
    {
        if (m_Weapons == null)
            return -1;

        // Keep the current weapon if it is still valid.
        int currentSlot = GetActiveWeaponSlotIndex();
        if (currentSlot >= 0)
        {
            WeaponController current = m_Weapons.GetWeaponAtSlotIndex(currentSlot);
            if (current != null && !current.IsReloading && current.CurrentAmmoRatio > 0.5f)
                return currentSlot;
        }

        List<int> validWeapons = new List<int>();

        for (int i = 0; i < m_Weapons.WeaponSlotNumber; i++)
        {
            if (i == currentSlot) continue;
            WeaponController w = m_Weapons.GetWeaponAtSlotIndex(i);
            if (w == null || w.IsReloading || w.CurrentAmmoRatio <= 0.5f) continue;
            else validWeapons.Add(i);
        }

        if (validWeapons.Count == 0) return -1;

        int randomIndex = Random.Range(0, validWeapons.Count);
        return validWeapons[randomIndex];
    }

    /// <summary>
    /// Busca el enemigo con menos vida dentro del radio (con línea de visión) y lo
    /// guarda como Target. Devuelve true si encontró alguno.
    /// </summary>
    public bool SelectLowestHealthEnemy(float radius)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        Transform best = null;
        float bestHealth = float.MaxValue;

        foreach (var col in hits)
        {
            if (col.transform.root == transform) continue;
            if (!col.transform.root.CompareTag("Enemy")) continue;

            Transform enemy = col.transform.root;
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth == null) continue;
            if (enemyHealth.CurrentHealth <= 0f) continue;

            Vector3 origin = transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = enemy.position + Vector3.up * 1.5f;
            Vector3 dir = targetPos - origin;
            float dist = dir.magnitude;
            dir.Normalize();

            if (Physics.Raycast(origin, dir, out RaycastHit hit, dist))
            {
                if (hit.transform.root != enemy) continue;
            }

            if (enemyHealth.CurrentHealth < bestHealth)
            {
                bestHealth = enemyHealth.CurrentHealth;
                best = enemy;
            }
        }

        if (best != null)
        {
            m_target = best;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Mueve al bot en dirección opuesta al Target actual a la velocidad dada.
    /// Devuelve true si pudo fijar un destino válido en NavMesh.
    /// </summary>
    public bool TryFleeFromTarget(float speed, float distance)
    {
        if (m_target == null) return false;
        if (m_NavMeshAgent == null || !m_NavMeshAgent.isOnNavMesh) return false;

        Vector3 awayDir = transform.position - m_target.position;
        if (awayDir.sqrMagnitude < 0.0001f) return false;
        awayDir.Normalize();

        Vector3 fleeTarget = transform.position + awayDir * distance;

        if (NavMesh.SamplePosition(fleeTarget, out var hit, 5f, NavMesh.AllAreas))
        {
            m_NavMeshAgent.speed = speed;
            m_NavMeshAgent.isStopped = false;
            return m_NavMeshAgent.SetDestination(hit.position);
        }
        return false;
    }
}
