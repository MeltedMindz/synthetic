# Quick Setup Guide

## Step-by-Step Scene Creation

### 1. Base Scene Setup

1. Open Unity 2022.3 LTS
2. Create new scene: `Assets/Scenes/TrainingArena.unity`
3. Create ground plane: GameObject → 3D Object → Plane
   - Scale: (5, 1, 5)
   - Position: (0, 0, 0)

### 2. Create ScriptableObject Assets

**CreatureDefaults:**
- Right-click in Project: Create → SyntheticLife/Config/CreatureDefaults
- Name: "CreatureDefaults"
- Set default values:
  - initialEnergy: 100
  - juvenileAge: 10
  - adultAge: 30
  - elderAge: 60
  - defaultGenome: Set reasonable values

**PhiFieldConfig:**
- Create → SyntheticLife/Config/PhiFieldConfig
- Name: "PhiFieldConfig"
- scale: 0.1
- speedX: 0.01
- speedY: 0.01
- enableBackAction: false (can toggle)

**SpawnerConfig:**
- Create → SyntheticLife/Config/SpawnerConfig
- Name: "SpawnerConfig"
- maxFoodItems: 50
- spawnInterval: 2
- arenaSize: 50

**RewardConfig:**
- Create → SyntheticLife/Config/RewardConfig
- Name: "RewardConfig"
- Use default values (already tuned)

### 3. Create Food Prefab

1. Create → 3D Object → Cube
2. Name: "Food"
3. Scale: (0.5, 0.5, 0.5)
4. Add Component: `FoodItem`
5. Tag: "Food"
6. Layer: "Food"
7. Drag to Project to make prefab
8. Delete from scene

### 4. Setup PhiField

1. Create Empty: "PhiField"
2. Add Component: `PhiField`
3. Assign PhiFieldConfig ScriptableObject

### 5. Setup FoodSpawner

1. Create Empty: "FoodSpawner"
2. Add Component: `FoodSpawner`
3. Assign SpawnerConfig ScriptableObject
4. Assign Food prefab
5. Assign PhiField reference (drag from scene)

### 6. Create Temperature Zones

**Hot Zone:**
- Create → 3D Object → Cube → "HotZone"
- Position: (15, 0.5, 0)
- Scale: (10, 1, 10)
- Add Component: `TemperatureZone`
- temperatureValue: 0.8
- Tag: "TemperatureZone"

**Cold Zone:**
- Duplicate HotZone → "ColdZone"
- Position: (-15, 0.5, 0)
- temperatureValue: 0.2

### 7. Create Hazards

- Create → 3D Object → Cube → "Hazard1"
- Position: (10, 0.5, 10)
- Scale: (5, 1, 5)
- Add Component: `HazardZone`
- damagePerSecond: 0.1
- Tag: "Hazard"
- Color: Red (material)

### 8. Create Shelter

- Create → 3D Object → Cube → "Shelter"
- Position: (0, 1, 0)
- Scale: (8, 2, 8)
- Add Component: `ShelterZone`
- Tag: "Shelter"
- Color: Green (material)

### 9. Create Creature Prefab

1. Create → 3D Object → Capsule → "Creature"
2. Position: (0, 1, 0)

**Components to Add:**

- **Rigidbody**:
  - Freeze Rotation Y: checked
  - Drag: 5

- **Behavior Parameters**:
  - Behavior Name: "Creature"
  - Vector Observation:
    - Space Size: 17 (ray perception adds observations automatically)
    - Stacked Vectors: 1
  - Actions:
    - Continuous Actions: 2
    - Discrete Branch Size: [2, 2]

- **Ray Perception Sensor 3D**:
  - Rays Per Direction: 12
  - Max Ray Degrees: 90
  - Ray Length: 10
  - Detectable Tags: Food, Hazard, Shelter, Creature, TemperatureZone

- **CreatureAgent**:
  - Assign all config ScriptableObjects
  - Assign PhiField, FoodSpawner, PopulationManager references
  - Set offspringPrefab (self-reference after making prefab)

- **GrowthSystem**:
  - (No inspector fields, uses config)

3. Make prefab: Drag to Project
4. Assign prefab to itself in CreatureAgent.offspringPrefab

### 10. Setup Population Manager

1. Create Empty: "PopulationManager"
2. Add Component: `PopulationManager`
3. maxCreatures: 20
4. Assign Creature prefab

### 11. Add Debug UI (Optional)

1. Right-click Hierarchy → UI → Canvas
2. Right-click Canvas → UI → Text
3. Name: "StatusText"
4. Add Component: `DebugUI`
5. Assign StatusText to statusText field
6. Assign a Creature from scene to trackedCreature

### 12. Initial Creature in Scene

- Drag Creature prefab into scene
- Position: (0, 1, 0)

### 13. Final Checks

- [ ] All ScriptableObjects created and assigned
- [ ] Tags set correctly (Food, Hazard, Shelter, Creature, TemperatureZone)
- [ ] Creature prefab has all components
- [ ] PopulationManager has prefab assigned
- [ ] No missing references in Console

### 14. Test Run

1. Press Play
2. Creature should spawn and move
3. Food should spawn
4. Check Console for errors

### 15. Start Training

```bash
mlagents-learn Config/ppo_life_v1.yaml --run-id life_v1 --train
```

Then press Play in Unity.

## Inspector Wiring Reference

### CreatureAgent Component

**Configs:**
- Creature Config: CreatureDefaults ScriptableObject
- Reward Config: RewardConfig ScriptableObject
- Phi Config: PhiFieldConfig ScriptableObject

**References:**
- Phi Field: Drag PhiField GameObject
- Food Spawner: Drag FoodSpawner GameObject
- Population Manager: Drag PopulationManager GameObject
- Offspring Prefab: Drag Creature prefab

### PhiField Component

- Config: PhiFieldConfig ScriptableObject

### FoodSpawner Component

- Config: SpawnerConfig ScriptableObject
- Phi Field: Drag PhiField GameObject
- Food Prefab: Drag Food prefab

### PopulationManager Component

- Creature Prefab: Drag Creature prefab

## Common Issues

**"NullReferenceException" on CreatureAgent:**
- Check all ScriptableObjects are assigned
- Check all GameObject references are assigned

**"Tag not found":**
- Edit → Project Settings → Tags and Layers
- Add missing tags

**"Behavior name mismatch":**
- Behavior Parameters → Behavior Name must match YAML file behavior name
- Default: "Creature"

**Food not spawning:**
- Check FoodSpawner has config and prefab assigned
- Check arenaSize is large enough

**Agent not moving:**
- Check Rigidbody is present
- Check Behavior Parameters has correct action spaces
- Check training is running (or model loaded for inference)

