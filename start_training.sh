#!/bin/bash
# Start ML-Agents training server

cd "$(dirname "$0")"

# Activate virtual environment
source mlagents-env/bin/activate

# Fix protobuf compatibility
export PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python

# Start training
echo "Starting ML-Agents training server..."
echo "Waiting for Unity to connect..."
mlagents-learn Config/ppo_life_v1.yaml --run-id life_v1 --train
