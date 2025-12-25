# Required Scene Changes for Telemetry + Instincts

## Summary

Yes, you need to add/configure a few new components in your Unity scene to support the telemetry system and instinct inheritance:

## Required Additions

### 1. TelemetryManager (NEW)

**Required:** Yes (telemetry won't work without it)

**Setup:**
1. Create Empty GameObject → Name: "TelemetryManager"
2. Add Component: `TelemetryManager`
3. Assign `TelemetryConfig` ScriptableObject to the `config` field
4. TelemetryManager is a singleton - only one per scene

**Location in Scene:** Can be anywhere (it's a singleton)

### 2. TelemetryConfig ScriptableObject (NEW)

**Required:** Yes (for TelemetryManager)

**Creation:**
- Menu: `SyntheticLife` → `Create Default Configs` (creates all configs including TelemetryConfig)
- OR manually: Create → SyntheticLife/Config/TelemetryConfig
- Save as: `Assets/ScriptableObjects/Configs/TelemetryConfig.asset`

**Settings:** Defaults are fine, but you can adjust:
- `enableTelemetry`: Enable/disable logging
- `logEpisodes`, `logEvents`, `logTrajectories`: What to log
- `trajectorySampleInterval`: How often to sample trajectories
- `worldSnapshotInterval`: How often to snapshot world state

### 3. PopulationManager Update (MODIFIED)

**Required:** Yes (genome mutation won't work without it)

**Changes:**
- PopulationManager now requires `CreatureDefaults` ScriptableObject assigned
- **New field:** `creatureDefaults` (must be assigned!)

**Setup:**
1. Select PopulationManager GameObject
2. In Inspector, find `Creature Defaults` field
3. Drag `CreatureDefaults` ScriptableObject from Project

**Why:** Needed for genome mutation (access to GenomeRanges and mutation parameters)

### 4. CreatureDefaults Update (MODIFIED)

**Required:** Yes (for instinct inheritance)

**Changes:**
- Now includes `genomeRanges` field (GenomeRanges struct)
- Added `maturityAgeMin/Max` and `maxAgeMin/Max` fields

**Setup:**
- If using `Create Default Configs`, this is auto-configured
- If manually creating, the default values should work
- GenomeRanges are initialized with defaults in code

### 5. Existing Components (NO CHANGES)

**No changes needed to:**
- CreatureAgent (works with telemetry automatically if TelemetryManager exists)
- GrowthSystem (uses genome automatically)
- PhiField, FoodSpawner, Zones (unchanged)

## Quick Setup (Automated)

**Option 1: Use Scene Setup Helper**
1. Menu: `SyntheticLife` → `Setup Training Scene`
2. This creates everything including TelemetryManager
3. Still need to manually configure Creature prefab

**Option 2: Manual Setup**
1. Menu: `SyntheticLife` → `Create Default Configs`
2. Add TelemetryManager GameObject manually
3. Assign TelemetryConfig
4. Update PopulationManager to assign CreatureDefaults

## Verification Checklist

After setup, verify:

- [ ] TelemetryManager GameObject exists in scene
- [ ] TelemetryManager has TelemetryConfig assigned
- [ ] PopulationManager has CreatureDefaults assigned
- [ ] CreatureDefaults.genomeRanges has valid values (should auto-initialize)
- [ ] No errors in Console about missing references

## Testing

1. Press Play
2. Check Console for: "Run manifest created" or telemetry logs
3. Check `Application.persistentDataPath/Runs/` for new run directory
4. Verify `manifest.json` and `telemetry/` folder are created

## Notes

- **Telemetry is optional**: If TelemetryManager doesn't exist, the system gracefully degrades (no telemetry logging, but game runs fine)
- **Genome mutation requires PopulationManager.creatureDefaults**: If not assigned, mutations will fail (errors in Console)
- **All configs are ScriptableObjects**: Easy to adjust without code changes

## Troubleshooting

**"Genome mutation failed" errors:**
→ PopulationManager.creatureDefaults is not assigned

**No telemetry logs:**
→ TelemetryManager doesn't exist or TelemetryConfig.enableTelemetry is false

**Missing GenomeRanges:**
→ CreatureDefaults should auto-initialize, but if not, check that CreateDefaultConfigs was run

