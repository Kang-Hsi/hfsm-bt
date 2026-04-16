# Inteligencia Artificial para Videojuegos Práctica 3: Disturbios Orbitales

***Autores***
- Haoshuang Hou ([HaoshuangHou](https://github.com/HaoshuangHou))
- Bingcheng Wang ([bingoo30](https://github.com/bingoo30))
- Leopold Popper ([Kang-Hsi](https://github.com/Kang-Hsi))

# Resumen

Un prototipo básico de un un entorno virtual que nos sirve para poner en práctica la **toma de decisiones**. Este es el [enunciado](https://narratech.com/es/inteligencia-artificial-para-videojuegos/decision/disturbios-orbitales/) en la web de Narratech.

El objetivo es utilizar máquina de estados jeárquica, técnica frecuentemente usada en la industria, combinado con búsquedas de caminos mediante mallas de navegación y comportamientos de dirección.

# Instalación y uso

Todo el contenido del proyecto está disponible en este repositorio, utilizamos Unity 6 **(6000.3.12f1).**

En el repositorio se localiza el ejecutable de dicha práctica para poder experimentar los comportamientos.
ENLACE

# Introducción

Este proyecto es una práctica de la asignatura de Inteligencia Artificial para Videojuegos del Grado en Desarrollo de Videojuegos de la UCM. Esta práctica consiste en plantear un escenario virtual 3D que representa una  prisión espacial con las siguientes instancias: 
- **Prisioneros**: son enemigos entre sí, cada uno de ello puede ser controlado por un jugador o una IA. Aparecen de alguno de los puntos de regeneración. Pueden moverse, disparar, apuntar, cambiar de arma, saltar, agacharse, correr y volar.
- **Armas**: recolectables sólo para prisioneros. Existen distintos tipos:
    1. *Pistola (gun)*, el arma por defecto.
    2. *Lanzamisiles (launcher)*, lanza unos discos que explotan.
    3. *Fusil de francotirador (sniper)*, arma de largo alcance.
    4. *Lanzallamas (flamethower)*, interesante aunque no haya mecánicas de transmisión de fuego.
    5. *Rifle (rifle)*, arma más pesada pero más estable.
    6. *Dos pistola (guns)*, a falta de una.
    7. *Escopeta (shot gun)*, causa daño en un radio más ancho.
- **Utensilios**: botequines para recuperar salud o el propulsor para poder «volar» con él.
- **Vigilantes robóticos**: Disparan a los prisioneros y pueden matarlos. 
Distinguimos dos tipos:
    1. Torretas: poderosas pero inmóviles.
    2. Robots flotantes: más déviles pero móviles. 

# Punto de partida

Se parte de un proyecto base proporcionado por el profesor y disponible en el siguiente repositorio: [fps-online](https://github.com/narratech/fps-online)

A continuación, se enumera una serie de informaciones básicas del repositorio original:
- *Human_Prefab* representa al jugador humano y *UCM_Bot* es la IA que hay que programar si se quiere tener un bot contra el que enfrentarse.

### Human_Prefab
- Lo más relevante que puede encontrarse en la raíz del prefab:
* NetworkObject: identidad de red del jugador.
* PlayerInput (Input System): componente de Unity que gestiona dispositivos/mapas de entrada.
* NewMonoBehaviourScript (tu “ClientPlayerMove” real, el hombre es que no está bien puesto): habilita cámara/controles sólo para el propietario, crea el HUD del marcador, etc.
* PlayerRespawner: maneja muerte/respawn en red (RPC al server y respawn al cliente).
* ClientNetworkTransform: sincroniza transform (owner authority en tu setup).
* PlayerHealthSync: sincroniza vida/estado.
* PlayerVotingSync (solo en Human): sistema de votación/acciones especiales.
* PlayerNameTag: nombre/kills/deaths en red.
* ClientNetworkAnimator: Script para hacer animación sincronizada.
* Rigging / IK / Weapon sync: WeaponIKSync, ThirdPersonWeaponSync, RigBuilder, constraints, etc. Son scripts de sincronización (por ejemplo PlayerHealthSync, ThirdPersonWeaponSync, LocalVisibility...).
* UI (CanvasScaler, GraphicRaycaster, TMP): el canvas world-space del nametag y elementos.
* CharacterController: componente nativo de Unity para mover un “personaje tipo cápsula” en el mundo sin usar un Rigidbody. Gestiones colisiones, deslizamiento, movimiento 'cinemático', grounding básico... pero no hace nada más.
* PlayerCharacterController: Script de este proyecto que hace las veces de MENTE del CharacterController, lee la entrada con PlayerInputHandler, y lo convierte en movimiento, rotación, coordina la cámara, la animación, está pendiente de la salud, muerte, apuntado, etc. 

### UCM_Bot
En UCM_Bot encontramos componentes muy parecidos, aunque se ha añadido FSM como ejemplo de dónde podría ir una máquina de estados que tome las decisiones de ese bot, y BotGameplayActions para hacer las veces de gestor de acciones.



## Estructura del proyecto

El repositorio nos proporciona el proyecto organizado de esta forma:

Nos proporciona ciertos componentes importantes para arrancar:

COMPLETAR
| Script | Tipo | Definición |
|--------|------|------------|
| `GameManager.cs`| *Manager* | Singleton. Maneja cambios de escena y parametrización de la creación del laberinto. Se encarga de actualizar la interfaz de usuario y comprobar si el jugador ha llegado a la salida|

# Planteamiento del problema
Tenemos un escenario de Unity 3D que está compuesto por varias escenas: menú inicial, victoria, derrota, escena principal, escena secundaria y escena de prisión.

El flujo del juego se ilustra con el siguiente diagrama:
<diagrama>

Desarrollamos un prototipo que nos permitirá:

- **A**. Hay un mundo virtual dividido por la malla de navegación de Unity, con las estancias y todos los elementos descritos anteriormente, distribuidos según una serie de puntos representativos del escenario (waypoints). Hay una cámara principal preparada para seguir al protagonista en primera persona desde el comienzo y otra cámara secundaria para tener una vista general de la escena (mini-mapa).

- **B**. El agente que controlamos aparece en uno de los puntos predefinidos del escenario y puede moverse por el escenario con todas las acciones de movimiento descritas anteriormente.

- **C**. La navegación del agente utiliza la malla de navegación de Unity, siendo su comportamiento por defecto intentar recorrer cuidadosamente toda la prisión buscando armas y atacar a otros prisioneros.

- **D**. Para la decisión del agente se usa el código de una máquina de estados finita jerárquica cargada desde un fichero .cs.

- **E**. En resumen, el agente trata de maximizar la métrica principal del juego: número de enemigos que he eliminado – número de veces que he sido eliminado, aunque también se mostrarán por pantalla otras métricas interesantes como número de armas conseguidas, número de utensilios conseguidos, número de vigilantes robóticos eliminados… todo ello con el contexto de la duración de la partida en segundos y el ratio de fotogramas por segundo.

# Diseño de la solución

## Menú
![Interfaz](DocImages/menu.png)
| Elemento | Tipo | Funcionalidad |
|----------|------|--------------|
|Tamaño|Selector|Seleccionar el tamaño del laberinto|
|Minotauros|Selector|Seleccionar el número de minotauros que se van a colocar en la escena|
|Comenzar|Botón|Crear el laberinto y comenzar la simulación|

## HUD
![Interfaz](DocImages/juego.png)

## Controles
|Input|Función|
|--|--|
|Ratón|Mover Teseo hacia el cursor|
|Clic izquierdo ratón|Correr con mayor velocidad (1.5x)|
|Rueda ratón|Zoom|
|F|Cambiar el FPS de la simulación|
|R|Reiniciar simulación con los presets de este laberinto|
|C|Cambiar de heurística|

# Implementación
Los siguientes scripts han sido desarrollados por el equipo durante la práctica:

|Script|Tipo|Definición|
|--|--|--|
|Vigia|*Comportamiento*|Rotar aleatoriamente de forma periódica|
|Patrullero|*Comportamiento*|Caminar hacia delante y cambiar la dirección aleatoriamente en un cruce|
|VisionMinotauro|*Componente auxiliar*|Cono de visión de cada enemigo, sirve para comprobar si Teseo es visto|
|IAMinotauro| *Controlador IA* | Controlador de la IA del Minotauro. Gestiona el cambio entre el estado habitual y perseguir a Teseo. |

## **Comportamientos implementados**
**VisionMinotauro**
- Representa el cono de visión de un enemigo.
- Características:
    1. El enemigo solo puede detectar al jugador dentro de un rango de visión máximo.
    2. La detección está limitada por un ángulo de visión, formando un cono frente al enemigo.
    3. El jugador debe estar dentro tanto del rango como del ángulo para ser visible.
    4. Se utiliza un raycast para comprobar si existen obstáculos (como paredes) que bloqueen la visión.
    5. Si hay un obstáculo entre el enemigo y el jugador, este no será detectado.
- Pseudocódigo:
    >```c
    >class MinotaurVision:
    >
    ># references
    >character: Kinematic
    >target: Kinematic
    >
    ># variables
    >visionRange: float
    >visionAngle: float
    >obstacles: LayerMask 
    >
    ># functions
    >function CanSeePlayer() -> bool:
    >    if target == null:
    >	return false
    >
    >    directionToTarget= target.position - character.position
    >    distance = magnitude(directionToTarget)
    >	
    >    # 1. check distance
    >    if distance > visionRange:
    >	return false
    >
    >    # 2. check angle
    >    angle = angleBetween(character.forward, directionToPlayer)
    >    if angle > visionAngle / 2:
    >	return false
    >
    >    # 3. raycast to check obstacles
    >    if raycast(character.position, normalize(directionToPlayer), distance, obstacles):
    >	return false
    >
    >    return true
    >``` 

## Implementación de cada apartado

**Apartado A**


**Apartado B**

**Apartado C**

**Apartado D**

**Apartado E** 

## Evolución
|**Estado**|**Tarea**|**Fecha**|
|--|--|--|
|✔|Preparación: Primer contacto y creación del repositorio|9-4-2026|
## Pruebas y métricas

A continuación se detalla un plan de pruebas para verificar que se cumplen todos los requisitos del enunciado. Se organiza por apartados (A–E) según las características.

- **Apartado A**

    ---
- **Apartado B**

    ---
- **Apartado C**
    
    ---
- **Apartado D**
    
    ---
- **Apartado E**

        
## Conclusiones

### Vídeo explicativo
[Vídeo de demostración en YouTube](https://youtu.be/n7ElxGUhQ7Y)
    
## Licencia
- Uso educativo y de investigación - UCM
- Copyright (c) 2026 Haoshuang Hou, Bingcheng Wang, Leopold Popper
- Se concede permiso permanente a los profesores de la Facultad de Informática de la Universidad Complutense de Madrid para utilizar, reproducir, modificar o distribuir total o parcialmente este material (documentación, código y recursos), con fines educativos o de investigación. Se permite obtener datos agregados de forma anónima o utilizar el material con reconocimiento expreso de los autores.
- Esta licencia no otorga derechos de uso comercial fuera del ámbito educativo o de investigación, salvo autorización expresa de los autores.


## Referencias

### Bibliografía
* Millington, I. (2019). *AI for Games* (3.ª ed.). CRC Press. — Referencia principal para el pseudocódigo de A*, suavizado de caminos y steering behaviors.

### Recursos y assets

### Videojuegos referentes

