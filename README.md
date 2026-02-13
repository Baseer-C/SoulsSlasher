# Souls Slasher: Modular Event-Driven Architecture in C#

> A high-fidelity real-time simulation leveraging **Hierarchical Finite State Machines (HFSM)**, **Observer Patterns**, and **Decoupled Input Systems**. This project demonstrates advanced Object-Oriented programming patterns to manage complex state transitions, procedural generation, and component-based logic.

**Architected by [Baseer Clark**](https://www.linkedin.com/in/baseer-clark1/)

---

## 🛠️ Technical Architecture

This project moves away from the "monolithic PlayerController" anti-pattern in favor of a **Modular Component Architecture** adhering to the **Single Responsibility Principle (SRP)**.

### 1. Decoupled Input & Command Pattern

Moving away from direct polling in logic classes, this project utilizes a buffer-based approach to input handling.

* **Input Buffering:** Implemented a dedicated `InputHandler` that buffers inputs into a queue. This decouples raw user input from execution logic, ensuring thread-safe responsiveness and preventing "dropped" inputs during frame spikes.
* **State Isolation:** The `PlayerManager` acts as a central orchestrator, utilizing boolean flags to prevent race conditions between Locomotion, Combat, and Interaction states (e.g., locking input during specific animation frames).

### 2. Hierarchical Finite State Machine (AI)

Boss behaviors and enemy logic are governed by a robust HFSM, allowing for complex decision trees without "spaghetti code."

* **Polymorphic Design:** Utilizes interfaces (`IState`, `IEnemyAction`) to allow for modular expansion of enemy types without modifying the core behavior loop.
* **Distance-Based Heuristics:** Adaptive behavior switching involves calculating vector distances to the player context, triggering transitions between states like `Strafing` (mid-range) and `Chasing` (long-range) based on NavMesh proximity.

### 3. Interface-Based Interaction (The "Parry" Contract)

To solve the "Risk vs. Reward" timing window, the system uses an interface-driven contract rather than physics-based collisions alone.

* **Deterministic Logic:** `WeaponHitbox` components use an `AttemptParry()` interface to check the victim's state *before* applying damage.
* **State Injection:** Upon a successful parry, the system injects a temporary `isInvincible` flag into the locomotion state, decoupling the visual effect from the logic to ensure data integrity.

```csharp
// Example: Interface implementation for deterministic state handling
public bool AttemptParry(float damage)
{
    // Check internal state flag before calculating logic
    if (isParrying)
    {
        // Inject State: Grant temporary immunity (I-Frame logic)
        if (playerManager.playerLocomotion != null)
        {
            playerManager.playerLocomotion.isInvincible = true;
            // Coroutine manages the lifecycle of the invincibility state
            StartCoroutine(ResetInvincibilityAfterParry());
        }
        return true; // Return successful interception to the caller
    }
    return false;
}

```

---

## 🎮 Core Features & Systems

### ⚔️ Deterministic Combat System

* **"Perfect Parry" Mechanic:** Strict 0.5s timing windows that reward skill with frame-perfect damage immunity.
* **Dynamic Hitboxes:** Differentiation between **Blocking** (stamina reduction) and **Parrying** (zero damage + VFX feedback).
* **Resource Management:** A tiered attack system with reset timers and stamina consumption logic.

### 🏗️ Algorithmic Generation

* **Noise-Based Arenas:** Custom algorithm for circular arena generation with terrain deformation.
* **NavMesh Validation:** The generation step includes a validation pass to ensure the resulting terrain supports valid AI pathfinding.

### 📷 Camera & Physics

* **Hierarchical Pivot System:** Uses a parent-child hierarchy (`CameraHolder` -> `CameraPivot` -> `Camera`) for independent rotation control.
* **SphereCast Collision:** Implements dynamic collision detection to prevent the camera from clipping through geometry while maintaining smooth interpolation.

---

## 📂 Project Structure

The codebase is structured to enforce modularity, separating global managers from entity-specific logic.

```text
Assets/Game/
├── Scripts/
│   ├── Managers/       # Global State (GameManager, SoundManager)
│   ├── Player/         # Modular Logic (Locomotion, Combat, Input)
│   ├── Controllers/    # Input & Camera Logic (Decoupled from logic)
│   ├── AI/             # Boss FSM (Abstract State Classes & Concrete Implementations)
│   ├── Components/     # Reusable Logic (Health, Stamina, Hitbox)
│   └── UI/             # Dynamic HUD & Event-Driven Menu Systems
└── Prefabs/            # Pre-configured GameObjects

```

---

## 🚀 Future Roadmap

* [ ] **Persistence Layer:** Implementation of a JSON-based save/load system for serializing character stats and world state.
* [ ] **Talent Tree Integration:** Node-based progression system utilizing graph data structures.
* [ ] **Visual Overhaul:** Integration of URP Post-Processing (Bloom, Vignette) for a dark fantasy aesthetic.
