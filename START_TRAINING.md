# Quick Start: Training

## Step-by-Step Training Startup

### 1. Open Terminal in Project Root

```bash
cd /path/to/synthetic
```

### 2. Activate Python Environment

```bash
source mlagents-env/bin/activate   # macOS / Linux
# or
venv\Scripts\activate              # Windows
```

### 3. Start Training Server

```bash
mlagents-learn Config/ppo_life_v1.yaml --run-id life_v1 --train
```

**Leave this terminal running.** You should see:
```
INFO:mlagents.trainers:Started training
INFO:mlagents.trainers:Listening on port 5004. Start training by pressing the Play button in the Unity Editor.
```

### 4. Press Play in Unity

- Open Unity Editor
- Open the `TrainingArena` scene
- Press **▶️ Play**

### 5. Verify Connection

**In Unity Console**, you should see:
```
Connected to trainer on port 5004 using API version 1.5.0
```

**In Terminal**, you should see:
```
INFO:mlagents.trainers:New training session started
INFO:mlagents.trainers:Episode 1 started
```

## What Happens Next

- Unity sends observations to Python
- Python computes actions using PPO policy
- Unity applies actions and computes rewards
- Policy weights update after each batch
- Training data logs to `results/life_v1/`

## Stopping Training

1. Press **Play** again in Unity (stops the scene)
2. Press **Ctrl+C** in terminal (stops training server)

## Viewing Results

### TensorBoard

```bash
# In a new terminal
source mlagents-env/bin/activate
tensorboard --logdir results
```

Open http://localhost:6006 in your browser.

### Telemetry Data

Telemetry logs are written to:
```
<Unity Persistent Data Path>/Runs/run_YYYYMMDD_HHMMSS/
├── manifest.json
├── telemetry/
│   ├── episodes.csv
│   ├── events.jsonl
│   └── trajectories/
└── world/
```

See `docs/data_schema.md` for details.

## Troubleshooting

### "Couldn't connect to trainer on port 5004"

- **Check**: Is the Python training server running?
- **Fix**: Start the server first (step 3), then press Play

### "Will perform inference instead"

- This is normal if the server isn't running
- Start the server, then press Play again

### Port 5004 already in use

```bash
# Find and kill the process
lsof -ti:5004 | xargs kill
```

### Unity crashes or freezes

- Check Unity Console for errors
- Verify all ScriptableObjects are assigned
- Ensure Physics module is enabled (Window → Package Manager → Built-in → Physics)

