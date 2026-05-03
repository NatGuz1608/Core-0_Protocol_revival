# 🍭 Robot Candy 4 | Core-0 Protocol Revival

![Unity](https://img.shields.io/badge/Unity-2022.3+-black?logo=unity)
![Mirror](https://img.shields.io/badge/Networking-Mirror-orange)
![License](https://img.shields.io/badge/License-MIT-green)

**Robot Candy 4** es un videojuego multijugador dinámico desarrollado en **Unity**, utilizando el framework **Mirror** para una arquitectura de red autoritativa. Este proyecto representa la evolución del *Protocolo Core-0*, enfocado en la sincronización precisa de físicas, combate y sistemas de salud en tiempo real.

---

## 🚀 Características Principales

### 🌐 Arquitectura de Red (Server-Authoritative)
[cite_start]El juego utiliza un modelo donde el servidor valida todas las acciones críticas para prevenir trampas y asegurar la consistencia[cite: 79, 142].
* [cite_start]**Gestión de Escenas:** Transición fluida entre `MenuScene` (Offline) y `FinalScene` (Online) mediante el `NetworkManager`[cite: 56].
* [cite_start]**Sincronización de Jugadores:** Uso de `NetworkTransform` con autoridad de cliente para un movimiento suave y `NetworkAnimator` para replicar estados de animación (Idle, Speed, Jump, Shoot)[cite: 68, 182].

### ⚔️ Sistema de Combate y Daño
* [cite_start]**Disparo Sincronizado:** Implementación de `[Command]` y `NetworkServer.Spawn` para que los proyectiles sean visibles por todos los clientes simultáneamente [cite: 122-127].
* [cite_start]**Anti-Fuego Amigo:** Lógica de validación mediante `netId` que evita que el jugador se inflija daño a sí mismo[cite: 162].
* [cite_start]**Salud con SyncVars:** La vida del jugador se gestiona mediante `[SyncVar]` con *Hooks*, permitiendo actualizaciones automáticas de la UI en todos los clientes cuando cambia el estado de salud[cite: 143, 146].

### 🛡️ Mecánicas de Juego
* [cite_start]**Health Boosters:** Pickups distribuidos en el mapa que curan al jugador, procesados exclusivamente en el servidor[cite: 83, 169].
* [cite_start]**Sistema Anti-Caída:** Teletransporte seguro para jugadores que caen al vacío (Y < -20), evitando errores de física y asegurando el flujo de juego [cite: 98-114].

---

## 📁 Estructura del Proyecto

[cite_start]Siguiendo las mejores prácticas de organización, el proyecto se divide en módulos claros [cite: 19-38]:

```text
Assets/
 └── Primary/
      ├── Network/      # Scripts de lógica Mirror (Health, Weapon, Controller)
      ├── Scripts/      # Scripts de gameplay base (Bullet, Boosters)
      ├── Prefabs/      # Objetos con NetworkIdentity (Player, Bullet, FX)
      ├── Animations/   # Controladores de animación (Arma.controller)
      └── Scenes/       # Escenas de Menú y Juego Final
