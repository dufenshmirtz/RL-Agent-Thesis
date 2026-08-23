# Trained Models

This folder contains trained ONNX model files produced during the reinforcement learning training process of the thesis:

**Development of Reinforcement Learning Agent for Custom 2D Action Game**

These models represent trained Unity ML-Agents policies and are intended for inference use inside the Unity project.

## Important Note

The default and recommended model is:

`AgentSmith2.2.onnx`

This is the model used for the main thesis evaluation, including the reported statistics, human-player tests, and final performance analysis.

## Model Files

| File                 | Description                                                                                                                                                                                                                                                                                                                                      |
| -------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `AgentSmith2.0.onnx` | Earlier trained model, obtained at approximately the same training scale as the final evaluated model, around 50 million training steps. It is included mainly for reference and comparison.                                                                                                                                                     |
| `AgentSmith2.2.onnx` | Default thesis model. This is the final evaluated model used for the official thesis results, statistics, and human-player evaluation. It was trained for approximately 50–60 million steps and represents the main practical result of the project.                                                                                             |
| `AgentSmith3.2.onnx` | Experimental extended-training model. It was trained for roughly twice as many steps as the final evaluated model. During its training, some game-related conditions or parameters had been changed, so it should not be considered directly equivalent to the main evaluated model. It is included only as supplementary experimental material. |

## Recommended Usage

For normal demo use, evaluation, and comparison with the thesis results, use:

`AgentSmith2.2.onnx`

This model was selected because it achieved strong and competitive gameplay while still remaining suitable as a practical game opponent.

## About the Experimental Model

`AgentSmith3.2.onnx` may exhibit stronger or different behavior compared to the official evaluated model. However, because it was trained under modified game conditions and for a longer training duration, it is not the model on which the thesis evaluation is based.

For this reason, it should be treated as an experimental model rather than the main thesis result.

## Usage in Unity

To use one of these models inside the Unity project, assign the corresponding `.onnx` file to the appropriate `Behavior Parameters` component of the RL agent.

These models are used in inference mode. They do not require the Python trainer or the `mlagents-learn` command in order to run inside a built demo.

## Summary

* Use `AgentSmith2.2.onnx` for the official thesis demo and evaluation.
* `AgentSmith2.0.onnx` is included as an earlier comparable checkpoint.
* `AgentSmith3.2.onnx` is an experimental extended-training model and is not part of the official reported evaluation.
