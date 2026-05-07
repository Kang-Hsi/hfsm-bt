<!-- UNITY CODE ASSIST INSTRUCTIONS START -->
- Project name: IAV26-G07-P3
- Unity version: Unity 6000.3.13f1
- Active scene:
  - Name: PrisonScene
  - Tags:
    - Untagged, Respawn, Finish, EditorOnly, MainCamera, Player, GameController, RespawnPoint, ArmaVoto, PatrolPoint, Enemy
  - Layers:
    - Default, TransparentFX, Ignore Raycast, Water, UI, PostProcessing, FirstPersonWeapon, PhysicalAmmo
- Active game object:
  - Name: GameObject
  - Tag: Untagged
  - Layer: Default
<!-- UNITY CODE ASSIST INSTRUCTIONS END -->

## Netcode — registro de prefabs (NGO)

El `NetworkManager` combina **varias** listas (`NetworkPrefabsLists` en la escena, p. ej. `DefaultNetworkPrefabs.asset` **y** `UniversidadesPrefabsList.asset`). Cada `NetworkObject` prefab debe aparecer **como mucho una vez** en el conjunto fusionado; si el mismo prefab está en dos listas, Unity muestra *duplicate GlobalObjectIdHash*.

Los prefabs de jugador seleccionables (`Human_Prefab`, `UPV_Bot`, `UCM_Bot`, etc.) deben registrarse **solo** en `Assets/UniversidadesPrefabsList.asset`. **No** los añadas a `Assets/DefaultNetworkPrefabs.asset` (ni duplicados en otra lista del mismo `NetworkManager`).

Si el error *duplicate GlobalObjectIdHash* vuelve tras tocar el proyecto en Unity: abre `DefaultNetworkPrefabs.asset` y elimina cualquier entrada cuyo prefab sea uno de esos tres; el inspector de Netcode a veces vuelve a registrar prefabs globales automáticamente.

### NetworkConfig mismatch (cliente vs host)

NGO exige que **cliente y servidor** compartan el mismo hash de `NetworkConfig` (listas de prefabs, `ConnectionApproval`, transporte serializado, etc.). Hay **varias escenas** con un `NetworkManager` (`IntroMenu`, `PrisonScene`, `MainScene`…): deben usar las **mismas** `NetworkPrefabsLists` (mismo orden y mismos assets), el mismo `ConnectionApproval` (este proyecto usa **1** con `ServerConnectionHandler`), y conviene alinear **puerto UTP por defecto** y campos del `UnityTransport` (p. ej. `WebSocketPath`) para no divergir entre escenas.

Con **`ForceSamePrefabs` = true**, el hash incluye **todos** los `GlobalObjectIdHash` registrados; si dos máquinas difieren aunque sea en un prefab o en el orden de inicialización, el cliente es rechazado. En este proyecto las escenas de menú/juego llevan **`ForceSamePrefabs` desactivado** para que el handshake no falle entre editores/builds del mismo repo; si necesitas la comprobación estricta en producción, reactívalo y asegúrate de builds idénticos y mismas listas de red.