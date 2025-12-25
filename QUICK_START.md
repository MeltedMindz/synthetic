# Quick Start Guide

## Automated Scene Setup

After opening the project in Unity:

1. **Create Configs** (one-time):
   - Menu: `SyntheticLife` → `Create Default Configs`
   - This creates all ScriptableObject configs

2. **Setup Scene** (one-time):
   - Menu: `SyntheticLife` → `Setup Training Scene`
   - This automatically creates:
     - Training arena scene
     - All environment objects (zones, spawners, etc.)
     - Creature prefab with components
     - All manager objects

3. **Configure Creature Prefab** (required):
   - The prefab is created but Behavior Parameters need manual configuration
   - Open `Assets/Prefabs/Creature.prefab`
   - Select the Creature prefab
   - In Inspector, find **Behavior Parameters** component
   - Configure:
     - Behavior Name: `Creature` (already set)
     - Vector Observation:
       - Space Size: `17`
       - Stacked Vectors: `1`
     - Actions:
       - Continuous Actions: `2`
       - Discrete Branch Size: `[2, 2]`

4. **Verify Setup**:
   - Check that PopulationManager has Creature prefab assigned
   - Check that TelemetryManager has TelemetryConfig assigned
   - Check that all CreatureAgent config references are set

5. **Start Training**:
   ```bash
   mlagents-learn Config/ppo_life_v1.yaml --run-id life_v1 --train
   ```
   Then press Play in Unity.

## Manual Configuration (If Needed)

If the automated setup doesn't work completely, see `SETUP_GUIDE.md` for detailed manual steps.

