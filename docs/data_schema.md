# Telemetry Data Schema

This document describes the data pipeline and file formats for SYNTHETIC-LIFE-PHI telemetry.

## Run Structure

All telemetry data is organized under `Runs/<runId>/` where `runId` is `run_YYYYMMDD_HHMMSS`.

```
Runs/
  <runId>/
    manifest.json
    report.md
    telemetry/
      episodes.csv
      events.jsonl
      trajectories/
        agent_<id>.csv
    world/
      phi_<timestamp>.csv
      resources_<timestamp>.json
    replay/
      ep<k>_seed.json
      ep<k>_actions.jsonl
```

## Files

### manifest.json

Created once per run on start. Contains:

```json
{
  "timestamp": "2024-01-01 12:00:00",
  "runId": "run_20240101_120000",
  "sceneName": "TrainingArena",
  "unityVersion": "2022.3.22f1",
  "mlAgentsPackageVersion": "2.0.1",
  "randomSeed": 12345,
  "configs": {
    "creatureDefaults": {},
    "phiFieldConfig": {},
    "rewardConfig": {},
    "spawnerConfig": {},
    "telemetryConfig": {}
  },
  "yamlConfig": "ppo_life_v1.yaml"
}
```

### telemetry/episodes.csv

One row per episode. Columns:

- `runId`: Run identifier
- `episodeIndex`: Episode number (0-indexed)
- `agentId`: Unity instance ID of agent
- `episodeStartTime`: Simulation time at start
- `episodeEndTime`: Simulation time at end
- `durationSeconds`: Episode duration
- `survivalSteps`: Number of steps survived
- `foodEatenCount`: Number of food items consumed
- `totalNutrientsGained`: Total nutrient value consumed
- `scanCount`: Number of scan actions performed
- `reproduceAttempts`: Number of reproduction attempts
- `reproduceSuccessCount`: Number of successful reproductions
- `damageEvents`: Number of damage events
- `totalDamage`: Total integrity damage taken
- `stageMaxReached`: Highest life stage reached (Juvenile/Adult/Elder)
- `finalStage`: Life stage at death/end
- `meanEnergy`: Average energy during episode
- `meanTemp`: Average temperature during episode
- `meanIntegrity`: Average integrity during episode
- `causeOfDeath`: Death cause (starvation/integrity/temperature/unknown)
- `genomeSignature`: Hash of genome values
- `hungerUrgencyBias`: Instinct parameter
- `dangerAversionBias`: Instinct parameter
- `scanCuriosityBias`: Instinct parameter
- `explorationNoiseBias`: Instinct parameter
- `reproductionConservatism`: Instinct parameter

### telemetry/events.jsonl

Newline-delimited JSON, one event per line. Each event has:

**Common fields:**
- `eventType`: Event type string
- `runId`: Run identifier
- `episodeIndex`: Episode number
- `simTime`: Simulation time
- `stepIndex`: Step number
- `agentId`: Agent ID
- `parentId`: Parent agent ID (if applicable)
- `generation`: Generation number (if applicable)

**Event-specific fields:**

**BIRTH:**
- `genome`: Full genome object
- `x`, `y`, `z`: Spawn position

**STAGE_CHANGE:**
- `fromStage`: Previous stage
- `toStage`: New stage
- `age`: Age at transition

**EAT:**
- `nutrients`: Nutrient value gained
- `x`, `y`, `z`: Position

**SCAN:**
- `phiSense`: Phi value sensed
- `x`, `y`, `z`: Position

**REPRODUCE_ATTEMPT:**
- `success`: Boolean

**REPRODUCE_SUCCESS:**
- `offspringId`: New agent ID
- `childGenome`: Offspring genome
- `x`, `y`, `z`: Spawn position

**DAMAGE:**
- `damage`: Damage amount
- `source`: Damage source type

**HEAL:**
- `healAmount`: Amount healed

**DEATH:**
- `cause`: Death cause
- `age`: Age at death
- `stage`: Stage at death
- `genome`: Final genome
- `x`, `y`, `z`: Death position

### telemetry/trajectories/agent_<id>.csv

Per-agent trajectory traces, downsampled. Columns:

- `time`: Simulation time
- `step`: Step index
- `agentId`: Agent ID
- `x`, `z`: Position (2D, y=0)
- `vx`, `vz`: Velocity components
- `energy`: Current energy
- `temp`: Current temperature
- `integrity`: Current integrity
- `age`: Current age
- `size`: Current size
- `stage`: Current life stage
- `phiSense`: Phi value sensed
- `scanFlag`: Whether scan was used this step
- `reproduceFlag`: Whether reproduce was attempted

Sampled every N steps (configurable via TelemetryConfig.trajectorySampleInterval).

### world/phi_<timestamp>.csv

Phi field snapshot. Format: grid of phi values.

### world/resources_<timestamp>.json

Resource state snapshot. Contains:
- Food positions and counts
- Hazard positions
- Shelter positions

### replay/ep<k>_seed.json

Episode seed for deterministic replay.

### replay/ep<k>_actions.jsonl

Action log for replay (future feature).

## Genome Schema

Genome is a struct containing:

**Instinct Biases:**
- `hungerUrgencyBias`: float (0.5-2.0)
- `dangerAversionBias`: float (0.5-2.0)
- `scanCuriosityBias`: float (0.0-1.0)
- `explorationNoiseBias`: float (0.0-0.2)
- `reproductionConservatism`: float (0.0-1.0)

**Life History:**
- `growthRate`: float (0.5-1.5)
- `maturityAgeNorm`: float (0.3-0.7)
- `maxAgeNorm`: float (0.5-1.0)

**Physiology:**
- `baseEmax`: float (50-200)
- `basalMetabolism`: float (0.1-5.0)
- `moveEfficiency`: float (0.5-2.0)
- `tempTolerance`: float (0.1-0.9)
- `antennaGain`: float (0.1-3.0)
- `scanCost`: float (0.5-10.0)
- `scanRadius`: float (2.0-15.0)
- `phiAffinity`: float (-1.0-1.0)

## Notes

- All times are in seconds (Unity Time.time)
- Positions are in Unity world space (meters)
- Energy/temperature/integrity are normalized or absolute depending on context
- Genome hash is computed via `GetHash()` method

