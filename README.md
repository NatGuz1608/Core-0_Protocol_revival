# Re-creating the CLEAN README.md content without citation tags and without MenuScene
clean_readme = """# 🍭 Robot Candy 4 | Core-0 Protocol Revival

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Mirror](https://img.shields.io/badge/Networking-Mirror-orange)
![License](https://img.shields.io/badge/License-MIT-green)

**Robot Candy 4** es un videojuego multijugador dinámico desarrollado en **Unity**, utilizando el framework **Mirror** para una arquitectura de red autoritativa. Este proyecto se enfoca en la sincronización precisa de físicas, combate y sistemas de salud en tiempo real.

---

## 🚀 Características Principales

### 🌐 Arquitectura de Red (Server-Authoritative)
El juego utiliza un modelo donde el servidor valida todas las acciones críticas para asegurar la consistencia y estabilidad de la partida.
* **Gestión de Escenas:** Configuración optimizada para la `FinalScene` (Online) mediante el `NetworkManager`.
* **Sincronización de Jugadores:** Uso de `NetworkTransform` con autoridad de cliente para un movimiento suave y `NetworkAnimator` para replicar estados de animación (Idle, Speed, Jump, Shoot) en todos los clientes.

### ⚔️ Sistema de Combate y Daño
* **Disparo Sincronizado:** Implementación de `[Command]` y `NetworkServer.Spawn` para que los proyectiles sean instanciados por el servidor y replicados en todas las instancias de juego.
* **Anti-Fuego Amigo:** Lógica de validación mediante `netId` que evita que el jugador se inflija daño a sí mismo al disparar.
* **Salud con SyncVars:** La vida del jugador se gestiona mediante `[SyncVar]` con *Hooks*, permitiendo actualizaciones automáticas de la interfaz de usuario (HUD) cuando cambia el estado de salud.

### 🛡️ Mecánicas de Juego
* **Health Boosters:** Pickups distribuidos en el mapa que curan al jugador, con detección de colisión y procesamiento de datos exclusivo en el servidor.
* **Sistema Anti-Caída:** Teletransporte seguro para jugadores que caen fuera de los límites del mapa (Y < -20), garantizando que el personaje regrese a una posición válida sin errores de física.

---

## 📁 Estructura del Proyecto

El proyecto sigue una organización modular para facilitar la escalabilidad y el mantenimiento:

```text
Assets/
 └── Primary/
      ├── Network/      # Lógica Mirror (Health, Weapon, Controller)
      ├── Scripts/      # Gameplay base (Bullet, Boosters)
      ├── Prefabs/      # Objetos de red (Player, Bullet, FX)
      ├── Animations/   # Controladores y clips de animación
      └── Scenes/       # Escena de juego principal
