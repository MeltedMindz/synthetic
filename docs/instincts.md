# Darwinian Instinct Inheritance

## Overview

SYNTHETIC-LIFE-PHI implements **Darwinian inheritance** of behavioral priors (instincts) through the Genome system. This is distinct from Lamarckian inheritance (which would pass learned weights).

## What is Inherited

### Genome-Encoded Instincts

The Genome struct contains heritable traits that shape behavior without hard-coding actions:

1. **Hunger Urgency Bias**: Scales penalties for low energy states
2. **Danger Aversion Bias**: Scales penalties for integrity loss and hazards
3. **Scan Curiosity Bias**: Scales intrinsic rewards for information gain from scanning
4. **Exploration Noise Bias**: Adds bounded exploration noise to movement actions
5. **Reproduction Conservatism**: Raises reproduction energy thresholds

### Life History Traits

- **Growth Rate**: Speed of age progression and size growth
- **Maturity Age Norm**: Normalized value mapping to real maturity age
- **Max Age Norm**: Normalized value mapping to maximum lifespan

### Physiological Traits

- Energy capacity, metabolism, movement efficiency
- Temperature tolerance
- Sensing capabilities (antenna gain, scan cost/radius)
- Phi field coupling affinity

## What is NOT Inherited

- **PPO/LSTM network weights**: These are learned per individual during training
- **Episodic memories**: No episodic memory system exists
- **Behavioral policies**: Agents learn policies through RL, not through inheritance

## How Inheritance Works

### Mutation

When an agent reproduces:

1. Parent genome is copied
2. Deterministic mutation seed is computed: `runSeed XOR parentIdHash XOR birthIndex`
3. Each trait mutates with configurable rate and strength
4. Mutated values are clamped to valid ranges
5. Child receives mutated genome

### Determinism

The mutation process is deterministic given:
- Run seed (stored in manifest.json)
- Parent agent ID
- Birth index (sequential)

This allows exact reproduction of evolutionary trajectories for analysis.

## How Instincts Affect Behavior

### Reward Shaping

Instincts **modulate reward signals**, not policy logic:

- **Hunger Urgency**: `starvation_penalty *= hungerUrgencyBias`
- **Danger Aversion**: `damage_penalty *= dangerAversionBias`
- **Scan Curiosity**: Small intrinsic reward when scan reduces uncertainty: `reward = scanCuriosityBias * uncertainty_reduction * 0.01`

### Action Costs

- **Scan Cost**: From genome.scanCost (scaled by size)
- **Movement Cost**: Influenced by genome.moveEfficiency

### Thresholds

- **Reproduction**: Base threshold + (reproductionConservatism * 0.2)
  - Conservative agents require higher energy to reproduce

### Exploration

- **Exploration Noise**: Bounded noise added to continuous actions
  - `action += noise * explorationNoiseBias` (clamped to [-1,1])
  - Seeded per episode for reproducibility

### Development

- **Growth Rate**: Age progression speed
- **Maturity Timing**: From maturityAgeNorm (mapped to real age range)
- **Max Age**: From maxAgeNorm (determines senescence)

## Design Philosophy

### No Hard-Coded Behaviors

Instincts do NOT implement logic like:
- ❌ "If energy < X, then go to food"
- ❌ "If integrity < Y, then avoid hazards"

Instead, they:
- ✅ Modulate how much the agent cares about energy/integrity
- ✅ Influence action costs and thresholds
- ✅ Shape reward signals that guide learning

### Darwinian vs Lamarckian

This is **Darwinian** inheritance:
- Only genetic material (Genome) is passed down
- Learning happens per individual during lifetime
- Evolution acts on instincts that shape learning

**Lamarckian** would be:
- Inheriting learned policy weights
- Epigenetic effects (parent experiences directly modify offspring genome)
- Not implemented in v1

## Telemetry Integration

Genome data is logged at key events:

- **BIRTH**: Full genome logged
- **DEATH**: Final genome logged (for selection analysis)
- **episodes.csv**: Genome signature hash + key instinct parameters

This enables:
- Lineage tracking (parentId, generation)
- Selection analysis (which instincts correlate with survival)
- Evolution visualization (trait changes over generations)

## Future Extensions

### Optional Epigenetic Effects (Still Darwinian)

Future versions could add:
- Parent stress modifies offspring instinct ranges (not direct values)
- Parent nutrition history slightly shifts offspring growth parameters
- Still deterministic and reproducible

### Policy Weight Inheritance (Lamarckian Hybrid)

If exploring Lamarckian inheritance:
- Mark clearly as experimental
- Inherit policy weights with mutation
- Compare Darwinian vs Lamarckian performance

## Configuration

Instinct ranges are configured via `CreatureDefaults.genomeRanges`:

- Each instinct has min/max bounds
- Default ranges balance exploration vs exploitation
- Can be tuned to encourage specific evolutionary pressures

Mutation parameters:
- `mutationRate`: Probability each trait mutates (0-1)
- `mutationStrength`: Maximum mutation magnitude

See `GenomeRanges.Default()` for default values.

