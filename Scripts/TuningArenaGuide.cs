using UnityEngine;

public class TuningArenaGuide
{
    /***
     * Here is a breakdown of every parameter in your `ArenaGenerator` script. I’ve organized this exactly how it appears in your Unity Inspector so you can read along while you tweak values.

### **1. Map Settings**

These control the physical "mesh" (the ground geometry).

* **Width (220):**
* *What it does:* The size of the map from Left to Right (X-axis).
* *How to tune:* Increase this if you want a wider forest to explore on the sides. Decrease it for a narrow "corridor" feel.


* **Depth (220):**
* *What it does:* The *theoretical* length of the map from Start to Finish (Z-axis).
* *How to tune:* This sets the scale for the noise calculation. Even though we cut the map off at the boss now, this value still determines how "stretched" the terrain noise looks.


* **Height (5):**
* *What it does:* The maximum vertical height of the bumps/hills.
* *How to tune:* You just set this to 5, which is subtle.
* **Higher (10-20):** Dramatic, steep hills that might block movement.
* **Lower (1-3):** Almost flat ground with tiny bumps.




* **Scale (20):**
* *What it does:* Controls the "roughness" or frequency of the hills.
* *How to tune:*
* **Higher Number (e.g., 50):** The terrain will look "noisier" with many small, jagged bumps.
* **Lower Number (e.g., 5):** The terrain will have large, rolling, smooth hills.





---

### **2. Path Settings**

These control the clear path leading to the boss.

* **Path Width (15):**
* *What it does:* How wide the flat area is in the center.
* *How to tune:* Make this wider if the player feels cramped or gets stuck on trees while running.


* **Path Wander (20):**
* *What it does:* How far left and right the path curves away from the exact center.
* *How to tune:*
* **0:** A perfectly straight line.
* **High (e.g., 50):** A very winding, "S" shaped river-like path.




* **Path Frequency (0.02):**
* *What it does:* How *often* the path curves left and right.
* *How to tune:*
* **Low (0.01):** Long, lazy curves.
* **High (0.1):** Tight, rapid zig-zags (like a snake).





---

### **3. Boss Arena Settings**

These control where the boss lives and the shape of his fighting pit.

* **Arena Z Position (0.85):**
* *What it does:* A percentage (0 to 1) of how far back in the `Depth` the boss spawns.
* *How to tune:*
* **1.0:** Boss is at the very end of the generated noise map.
* **0.5:** Boss is in the middle.
* *Note:* Since we are now "cutting" the map after the boss, this effectively just decides how long your level is.




* **Arena Radius (30):**
* *What it does:* The size of the flat circle for the boss fight.
* *How to tune:* Increase this if you want a massive arena for kiting the boss. Decrease it for a claustrophobic cage match.


* **Arena Wall Steepness (20):**
* *What it does:* How sharply the ground rises up at the edge of the arena circle.
* *How to tune:* This creates the "bowl" effect so the player can't easily run out of the arena.



---

### **4. Enemy Spawner**

* **Boss Prefab:** The actual GameObject to spawn as the boss.
* **Minion Prefabs:** A list of enemies (skeletons, etc.) that spawn *inside* the boss arena with him.
* **Minion Count (25):** How many minions spawn in the arena.
* **Minion Scatter (10):** (Currently unused in the new logic, as we use `Random.insideUnitCircle`, but originally meant for spread).
* **Spawn Height Offset (1.5):** How high off the ground units spawn. Increase this if enemies are spawning stuck inside the floor.

---

### **5. Environment Settings**

* **Tree/Rock Prefabs:** The 3D models you want to scatter.
* **Tree Count (3000) / Rock Count (1400):**
* *What it does:* The number of attempts the script makes to place an object.
* *How to tune:*
* **Increase:** Dense forest. Good for blocking vision/movement.
* **Decrease:** Sparse plains. Good for performance and ease of movement.


* *Note:* The script is smart enough *not* to place these on the path or in the arena, so the actual number spawned will be lower than this number.



### **Next Step for You**

Would you like me to create a "Minion Spawner" setting for the **forest path** as well? Right now, minions only spawn in the boss arena, but we could add a setting to scatter some enemies along the path leading up to the boss.
    */
    
}
