# Thesis Delivery Package

**Development of Reinforcement Learning Agent for Custom 2D Action Game**
Fotios Chrysomallis
University of Thessaly
Department of Electrical and Computer Engineering

## Overview

This package contains the main deliverable material for the diploma thesis.

The project focuses on the development of a reinforcement learning agent capable of acting as a competitive opponent in a custom 2D action/fighting game. The trained agent was developed using Unity ML-Agents and PPO, and is provided here together with the final report, presentation, selected source code, trained models, and supplementary video material.

## Folder Structure

### `Demo`

Contains the standalone demo build of the project.

The demo allows the user to run the game and interact with the trained RL agent in inference mode. It does not require Unity, Python, or the ML-Agents training tools in order to run.

Please read the README file inside the folder for detailed demo instructions.

### `Report_and_Presentation`

Contains the final thesis report and the presentation slides.

These files describe the motivation, game environment, reinforcement learning setup, training pipeline, evaluation process, results, and conclusions of the thesis.

### `Sample_Video_Material`

Contains supplementary video examples.

These videos are included to make the behavior of the trained agent easier to evaluate, especially for viewers who are not already familiar with the game mechanics. They include tutorial material, full human-vs-agent rounds, and a slow-motion example of a fast decision sequence.

Please read the README file inside the folder for details about each video.

### `Selected_Source_Code`

Contains selected thesis-related source files.

This folder does not necessarily contain the full Unity project. It contains representative source code related to the reinforcement learning integration, agent behavior, input abstraction, training support systems, reward logic, opponent management, and safety/debugging mechanisms.

### `Used Models`

Contains trained ONNX model files produced during the reinforcement learning training process.

The default model used for the official thesis evaluation is `AgentSmith2.2.onnx`. Other models are included for reference or experimental comparison.

Please read the README file inside the folder for details about each model.

## Recommended Starting Point

For a quick evaluation of the project, the recommended order is:

1. Read the thesis report or presentation summary.
2. Open the `Sample_Video_Material` folder to understand the game and observe the trained agent.
3. Run the standalone demo from the `Demo` folder.
4. Inspect the selected source files and trained models if needed.

## Important Notes

* The standalone demo runs the trained agent in inference mode.
* No training is performed inside the demo.
* Unity, Python, and `mlagents-learn` are not required to run the standalone demo.
* The trained RL agent is Player 2 in the provided gameplay videos.
* The selected source code is provided for inspection of the thesis-related implementation, not necessarily as a complete cleaned production repository.

## Contact

For questions regarding the project, source code, or demo execution, please contact the thesis author.
