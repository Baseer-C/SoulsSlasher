# Souls Slasher: Modular Event-Driven Architecture in C#

> A high-fidelity real-time simulation leveraging **Finite State Machines (FSM)**, **Observer Patterns**, and **Decoupled Input Systems**. This project demonstrates advanced Object-Oriented programming patterns to manage complex state transitions and component-based logic.

**Architected by [Baseer Clark**](https://www.linkedin.com/in/baseer-clark1/)

---

## 🛠️ System Architecture

### 1. Decoupled Input & Event Bus

Moving away from monolithic controllers, this project utilizes a **Command Pattern** approach to input handling.

* **Input Buffering:** Implemented a queue-based buffer system (`InputHandler`) to decouple raw user input from execution logic, ensuring thread-safe responsiveness.
* **State Isolation:** The `PlayerManager` acts as a central orchestrator, utilizing boolean flags to prevent race conditions between Locomotion, Combat, and Interaction states (e.g., locking input during specific animation frames).

### 2. Finite State Machine (AI)

Boss behaviors are governed by a **Hierarchical Finite State Machine**.

* **State Logic:** Transitions are driven by distance heuristics and player context (e.g., switching from `ChasingState` to `StrafingState` based on NavMesh proximity).
* **Polymorphic Design:** Utilizing interfaces (`IState`, `IEnemyAction`) to allow for modular expansion of enemy types without modifying the core behavior loop.

### 3. Interface-Based Interaction (The "Parry" Logic)

To solve the "Risk vs. Reward" timing window, the system uses an interface-driven contract rather than physics-based collisions alone.

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

## 📂 Project Structure (Modular Monolith)

The codebase adheres to the **Single Responsibility Principle (SRP)**, separating concerns into distinct managers:

```text
Assets/Scripts/
├── Managers/       # Global State (GameManager, SoundManager)
├── Controllers/    # Input & Camera Logic (Decoupled from logic)
├── FSM/            # Abstract State Classes & Concrete Implementations
└── Components/     # Reusable Logic (Health, Stamina, Hitbox)

```

---

**Final Advice:** Put the **Mini-Spark** project at the top of your resume/portfolio. Put this one second. It's the perfect "I'm also a well-rounded engineer" closer.
