#!/bin/bash
cd "$(dirname "$0")"
source mlagents-env/bin/activate
export PROTOCOL_BUFFERS_PYTHON_IMPLEMENTATION=python
mlagents-learn Config/ppo_life_v1.yaml --run-id life_v1 --train

