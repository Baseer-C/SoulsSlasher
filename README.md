Souls Slasher: A Technical Deep Dive into Action-RPG Systems

Souls Slasher is a high-fidelity, third-person action RPG developed in Unity, focusing on precise combat mechanics ("Sekiro-style" parrying), procedural arena generation, and a modular architecture designed for scalability. This project demonstrates advanced game development patterns including decoupled input handling, finite state machines for AI, and a robust event-driven architecture.

🎮 Core Features

Precision Combat System: * Implemented a "Perfect Parry" mechanic with strict timing windows (0.5s) that rewards skill with damage immunity (Invincibility Frames).

Dynamic hitbox system that differentiates between Blocking (stamina damage) and Parrying (zero damage + VFX).

Combo system with reset timers and stamina management.

Intelligent AI (Finite State Machine):

Boss AI features "poise" mechanics, tactical retreat logic, and distance-based behavior switching (Strafe vs. Chase).

Dynamic UI that reveals boss health bars only upon proximity, enhancing cinematic tension.

Procedural Arena Generation:

Custom algorithm to generate a circular arena with noise-based terrain deformation, ensuring valid NavMesh pathfinding for AI.

Modular "Manager" Architecture:

Decoupled logic into PlayerManager, InputHandler, PlayerLocomotion, and PlayerCombat to adhere to the Single Responsibility Principle.

🛠️ Technical Architecture

This project moves away from the monolithic PlayerController anti-pattern in favor of a Modular Component Architecture.

1. Input & State Management

InputHandler: A dedicated script that buffers inputs (for responsive combat) and sets flags (rb_Input, rt_Input). It is completely decoupled from the logic that uses those inputs.

PlayerManager: Acts as the "Brain," coordinating between Locomotion, Combat, and Stats. It manages high-level states like isInteracting or isGrounded to prevent state conflicts (e.g., attacking while rolling).

2. The Combat Loop (Sekiro-Style)

The combat system is built on a "risk vs. reward" philosophy.

WeaponHitbox.cs: A reusable component attached to any weapon (Player or Enemy). It uses an AttemptParry() interface to check the victim's state before applying damage.

Invincibility Logic: Instead of relying on physics layers, the PlayerCombat script injects a temporary isInvincible flag into the locomotion state upon a successful parry, guaranteeing frame-perfect safety.

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


3. Camera & Locomotion

Pivot-Based Camera: Implements a standard 3rd-person pivot system (CameraHolder -> CameraPivot -> Camera) to allow independent vertical/horizontal rotation and collision detection (SphereCast against walls).

Lock-On System: A target-locking mechanism that alters movement physics from "Free Look" (moving relative to camera) to "Strafing" (moving relative to target) dynamically.

📂 Project Structure

Assets/
├── Game/
│   ├── Scripts/
│   │   ├── Managers/       # Global Systems (GameManager, WorldSoundFX)
│   │   ├── Player/         # Modular Player Logic (Locomotion, Combat, Input)
│   │   ├── AI/             # Boss and Minion Behaviors
│   │   ├── Items/          # ScriptableObjects and Weapon Logic
│   │   └── UI/             # Health Bars, Menus, Interaction
│   ├── Prefabs/            # Pre-configured game objects
│   └── Audio/              # SFX and Music


🚀 Future Roadmap

Talent Tree Integration: Implementing a node-based progression system (currently in backend) to allow build diversity.

Save/Load System: JSON-based persistence for character stats and world state.

Post-Processing Polish: Integration of Bloom, Vignette, and Color Grading to enhance the dark fantasy atmosphere.

👨‍💻 About the Developer

I am a Backend Engineer (AWS EMR Team) with a Computer Science degree, applying rigorous software engineering principles to game development. This project serves as a sandbox for implementing complex systems like distributed event handling and state management in a real-time environment.
