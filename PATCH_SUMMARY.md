# Patch Summary: Telemetry + Darwinian Instinct Inheritance

## Overview

This patch implements:
1. Complete self-documenting telemetry data pipeline
2. Darwinian instinct inheritance system through Genome

## Files Modified

### Core Scripts

**Assets/Scripts/Core/Genome.cs**
- Added instinct bias parameters: hungerUrgencyBias, dangerAversionBias, scanCuriosityBias, explorationNoiseBias, reproductionConservatism
- Added life history traits: growthRate, maturityAgeNorm, maxAgeNorm
- Created GenomeRanges struct for configurable min/max bounds
- Added CreateDefault() static method
- Updated Mutate() to use System.Random for deterministic mutation
- Added Clamp() method
- Added GetHash() for genome signature

**Assets/Scripts/Core/GrowthSystem.cs**
- Updated to use instinct parameters: growthRate, maturityAgeNorm, maxAgeNorm
- Age progression now uses growthRate
- Stage transitions use maturityAgeNorm (mapped to real age range)
- Max age uses maxAgeNorm
- Added GetMaxAge() and GetMaturityAge() helper methods
- Added GetGenome() accessor

**Assets/Scripts/Core/CreatureAgent.cs**
- Added telemetry logging integration throughout
- Instinct-based reward shaping:
  - Hunger urgency scales starvation penalties
  - Danger aversion scales damage penalties
  - Scan curiosity provides intrinsic reward for information gain
  - Exploration noise added to continuous actions
  - Reproduction conservatism raises reproduction thresholds
- Added stage change event logging
- Added death cause tracking (starvation/integrity/temperature)
- Added lineage tracking (parentId, generation)
- Added step-by-step telemetry updates
- Updated Reproduce() to use PopulationManager's deterministic mutation

**Assets/Scripts/Core/PopulationManager.cs**
- Added deterministic genome mutation via MutateGenome()
- Seed calculation: runSeed XOR parentIdHash XOR birthIndex
- Added CreatureDefaults reference for genome ranges
- Tracks birth counter for deterministic seeding

### Configuration

**Assets/Scripts/Config/CreatureDefaults.cs**
- Added GenomeRanges genomeRanges field
- Added maturityAgeMin/Max and maxAgeMin/Max for mapping normalized values

**Assets/Scripts/Config/TelemetryConfig.cs** (NEW)
- Configurable telemetry settings
- Episode logging, event logging, trajectory snapshots
- World snapshot intervals
- Replay data logging

### Utilities

**Assets/Scripts/Utils/TelemetryManager.cs** (NEW)
- Singleton manager for telemetry pipeline
- Run manifest generation
- Episode CSV logging
- Events JSONL logging (buffered)
- Trajectory snapshot logging
- World snapshot generation (stub)
- Auto-report generation
- Handles lineage tracking

### Editor

**Assets/Scripts/Editor/CreateDefaultConfigs.cs**
- Added TelemetryConfig creation
- Updated to use SyntheticLife.Phi.Utils namespace

### Documentation

**docs/data_schema.md** (NEW)
- Complete schema documentation
- File formats and column descriptions
- Event JSON structure
- Genome schema

**docs/instincts.md** (NEW)
- Darwinian inheritance explanation
- How instincts affect behavior
- Design philosophy
- Future extensions

## Key Features

### Telemetry Pipeline

1. **Run Manifest**: Auto-generated on start with run metadata
2. **Episodes CSV**: Per-episode statistics with genome data
3. **Events JSONL**: Stream of all agent events (BIRTH, DEATH, EAT, SCAN, etc.)
4. **Trajectory Snapshots**: Downsampled per-agent traces
5. **World Snapshots**: Periodic Phi field and resource state (stub)
6. **Auto Report**: Summary report generation

### Instinct Inheritance

1. **Genome Extension**: 5 instinct biases + life history + physiology traits
2. **Reward Shaping**: Instincts modulate reward signals (not hard-coded behavior)
3. **Deterministic Mutation**: Reproducible evolution via seeded RNG
4. **Lineage Tracking**: Parent-child relationships and generation numbers
5. **Telemetry Integration**: Genome logged at birth/death for analysis

## Behavioral Changes

### Before
- Fixed reward penalties
- No instinct variation
- Simple genome (physiology only)

### After
- Instinct-modulated rewards (varies by individual)
- Exploration noise (bounded, seeded)
- Scan curiosity rewards (information gain)
- Conservative reproduction (trait-dependent thresholds)
- Growth/life history variation (maturity timing, max age)

## Configuration

All parameters configurable via ScriptableObjects:
- TelemetryConfig: Sampling rates, what to log
- CreatureDefaults.genomeRanges: Instinct min/max bounds
- Mutation rate/strength in CreatureDefaults

## Breaking Changes

- Genome struct extended (existing saves may need migration)
- CreatureAgent now requires TelemetryManager.Instance (optional, graceful if missing)
- PopulationManager requires CreatureDefaults reference for mutation

## Testing Checklist

- [ ] Telemetry creates run directory on start
- [ ] Manifest.json generated correctly
- [ ] Episodes.csv logs all episodes
- [ ] Events.jsonl logs all event types
- [ ] Trajectory files created per agent
- [ ] Genome mutation is deterministic (same seed = same mutations)
- [ ] Instinct parameters affect rewards correctly
- [ ] Lineage tracking works (parentId, generation)
- [ ] Stage changes logged
- [ ] Death causes logged correctly

## Usage

1. Assign TelemetryConfig to TelemetryManager in scene
2. Ensure CreatureDefaults has genomeRanges set
3. Assign CreatureDefaults to PopulationManager
4. Run training - telemetry auto-creates in `Application.persistentDataPath/Runs/`

## Data Analysis

- Episodes.csv: Aggregate statistics, survival analysis
- Events.jsonl: Timeline analysis, event correlation
- Trajectories: Movement patterns, state evolution
- Genome signatures: Evolution tracking, selection analysis

