# How to Start Training (Step-by-Step)

## The Problem

Unity says "Couldn't connect to trainer on port 5004" because the Python training server needs to be running **before** you press Play in Unity.

## Solution: Two-Terminal Workflow

### Terminal 1: Start Training Server

Open a new terminal window/tab and run:

```bash
cd /Users/melted/Documents/GitHub/synthetic
source mlagents-env/bin/activate
export PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python
mlagents-learn Config/ppo_life_v1.yaml --run-id life_v1 --train
```

**What you should see:**
```
INFO:mlagents.trainers:Started training
INFO:mlagents.trainers:Listening on port 5004. Start training by pressing the Play button in the Unity Editor.
```

**Leave this terminal running!** Don't close it or press Ctrl+C yet.

### Unity Editor: Press Play

1. Make sure Unity Editor is open
2. Make sure the `TrainingArena` scene is open
3. Press **▶️ Play**

**What you should see in Unity Console:**
- ✅ `Connected to trainer on port 5004 using API version 1.5.0`
- ❌ NOT `Couldn't connect to trainer... Will perform inference instead`

**What you should see in Terminal 1:**
```
INFO:mlagents.trainers:New training session started
INFO:mlagents.trainers:Episode 1 started
```

### Verify It's Working

- **Terminal 1**: Should show episode numbers increasing
- **Unity Console**: Should show training messages, not inference messages
- **Terminal 1**: Should show reward statistics after episodes complete

## Alternative: Use the Helper Script

You can also use the helper script:

```bash
cd /Users/melted/Documents/GitHub/synthetic
./start_training.sh
```

This does the same thing but in one command.

## Common Issues

### Issue: "Couldn't connect to trainer"

**Cause**: Training server isn't running, or Unity pressed Play before server was ready.

**Fix**:
1. Make sure Terminal 1 is running and shows "Listening on port 5004"
2. Wait 2-3 seconds after starting the server
3. Then press Play in Unity

### Issue: Port 5004 already in use

**Fix**: Kill the old process:
```bash
lsof -ti:5004 | xargs kill
```

Then restart the server.

### Issue: Server starts but Unity still says "inference"

**Possible causes**:
1. Wrong Behavior Name: Check that your Creature's `BehaviorParameters` component has `Behavior Name = "Creature"` (must match the YAML config)
2. Server crashed: Check Terminal 1 for error messages
3. Version mismatch: Check Unity Console for API version warnings

## Stopping Training

1. **In Unity**: Press Play again (stops the scene)
2. **In Terminal 1**: Press Ctrl+C (stops the training server)

## Next Steps

Once connected:
- Training data saves to `results/life_v1/`
- Telemetry saves to `<Unity Persistent Data>/Runs/run_YYYYMMDD_HHMMSS/`
- View progress with TensorBoard: `tensorboard --logdir results`

