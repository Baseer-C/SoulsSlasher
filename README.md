
---

# Souls Slasher: A Technical Deep Dive into Action-RPG Systems

> **Souls Slasher** is a high-fidelity, third-person action RPG focusing on precise combat mechanics, procedural arena generation, and a modular architecture. This project serves as a demonstration of advanced game development patterns, including decoupled input handling, finite state machines (FSM) for AI, and robust event-driven systems.

**Architected by [Baseer Clark**](https://www.linkedin.com/in/baseer-clark1/)

---

## 🎮 Core Features

### ⚔️ Precision Combat System

* **"Perfect Parry" Mechanic:** Strict  timing windows that reward skill with frame-perfect damage immunity (I-Frames).
* **Dynamic Hitboxes:** Differentiation between **Blocking** (stamina reduction) and **Parrying** (zero damage + VFX feedback).
* **Stamina-Based Combos:** A tiered attack system with reset timers and resource management.

### 🧠 Intelligent AI (Finite State Machine)

* **Poise Mechanics:** Bosses feature "super armor" and poise-breaking thresholds.
* **Distance-Based Logic:** Adaptive behavior switching between **Strafing** (mid-range) and **Chasing** (long-range).
* **Contextual UI:** Boss health bars dynamically reveal based on player proximity to enhance cinematic tension.

### 🏗️ Procedural Generation & Systems

* **Noise-Based Arenas:** Custom algorithm for circular arena generation with terrain deformation, ensuring valid **NavMesh** pathfinding.
* **Manager-Centric Design:** Strict adherence to the **Single Responsibility Principle** by decoupling logic into specialized controllers.

---

## 🛠️ Technical Architecture

This project moves away from the "monolithic PlayerController" anti-pattern in favor of a **Modular Component Architecture**.

### 1. Input & State Management

* **`InputHandler`**: A dedicated script that buffers inputs to ensure responsiveness. It is completely decoupled from logic, acting only as a provider of flags (`rb_Input`, `rt_Input`).
* **`PlayerManager`**: The central "Brain" that coordinates between Locomotion, Combat, and Stats. It manages high-level states like `isInteracting` or `isGrounded` to prevent state conflicts (e.g., preventing an attack animation while in a roll state).

### 2. The Combat Loop (Sekiro-Style)

The system is built on a "risk vs. reward" philosophy using an interface-driven approach.

* **`WeaponHitbox.cs`**: A reusable component that uses an `AttemptParry()` interface to check the victim's state before applying damage.
* **Invincibility Logic**: Rather than relying on physics layers which can be inconsistent, the system injects a temporary `isInvincible` flag into the locomotion state upon a successful parry.

```csharp
// Snippet: Parry Logic ensuring 0 damage on perfect timing
public bool AttemptParry(float damage)
{
    if (isParrying)
    {
        // Force Invincibility to ensure NO damage leaks through
        if (playerManager.playerLocomotion != null)
        {
            playerManager.playerLocomotion.isInvincible = true;
            StartCoroutine(ResetInvincibilityAfterParry());
        }
        return true; // Signal to attacker that hit was deflected
    }
    return false; 
}

```

### 3. Camera & Locomotion

* **Pivot-Based Camera:** Uses a hierarchical system (`CameraHolder` -> `CameraPivot` -> `Camera`) for independent rotation and collision detection using `SphereCast`.
* **Dynamic Physics:** Implements a **Lock-On System** that toggles movement physics between "Free Look" (camera-relative) and "Strafing" (target-relative).

---

## 📂 Project Structure

```text
Assets/
├── Game/
│   ├── Scripts/
│   │   ├── Managers/    # Global Systems (GameManager, SoundFX)
│   │   ├── Player/      # Modular Logic (Locomotion, Combat, Input)
│   │   ├── AI/          # Boss FSM and Behavior Trees
│   │   ├── Items/       # ScriptableObjects & Weapon Logic
│   │   └── UI/          # Dynamic HUD & Menu Systems
│   ├── Prefabs/         # Pre-configured GameObjects
│   └── Audio/           # Spatialized SFX and Music

```

---

## 🚀 Future Roadmap

* [ ] **Talent Tree Integration:** Node-based progression system for build diversity.
* [ ] **Persistence:** JSON-based save/load system for character stats and world state.
* [ ] **Visual Overhaul:** Integration of URP Post-Processing (Bloom, Vignette) for a dark fantasy aesthetic.

---
