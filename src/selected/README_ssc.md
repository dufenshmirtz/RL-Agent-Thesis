# Selected Thesis Source Code

**Thesis:** Development of Reinforcement Learning Agent for Custom 2D Action Game  
**Author:** Fotios Chrysomallis  
**University:** University of Thessaly  
**Supervisor:** Georgios Thanos

## Purpose

This folder contains the source files most directly connected to the reinforcement-learning work described in the thesis presentation. The custom 2D action game is the experimental environment and remains a separate personal project. This selection focuses on the RL agent, training pipeline, environment adaptations, inference demo, telemetry, and evaluation code.

The files preserve their project source as implemented. They are organized for academic inspection and are not intended to compile as an independent Unity project because the private host game contains additional gameplay types, prefabs, scenes, and assets.

## Thesis-to-Code Map

### 01 Core RL Agent

- `FighterAgent.cs`: PPO agent integration, 67 observations, nine discrete action branches, action masking, heuristic control, reward calculation, and episode handling.
- `AIInputProvider.cs`: translates agent decisions into the same input abstraction used by game characters.
- `FighterObservationEncoder.cs`: explicit reusable representation of the 67-value observation vector.
- `CharacterMLProfile.cs` and `CharacterMLProfileDatabase.cs`: character-aware reach semantics used by the shared policy.

### 02 Training and Curriculum

- `TrainingOpponentDirector.cs` and `OpponentMode.cs`: progressive mixture of scripted opponents, previous inference models, and mirror self-play.
- `TrainingSafety.cs`: detects invalid states, stalls, out-of-bounds conditions, excessive holds, and hard timeouts.
- `FighterAgentRewardDebugger.cs`: per-component reward diagnostics used while iterating on reward shaping.
- `SimpleBotController.cs` and `BotInputProvider.cs`: scripted curriculum opponent and its input adapter.
- `TrainingBoost.cs`, `EpisodeTimeout.cs`, and `ArenaSpawner.cs`: supporting training utilities.
- `TrainingCharacterWinrateTracker.cs`: character-level evaluation for shared-policy generalization.
- `ppo_self_play.yaml`: PPO and self-play training configuration.

### 03 Environment Integration

- `IInputProvider.cs` and `KeyboardInputProvider.cs`: common input abstraction for humans, scripted bots, and the RL agent.
- `Character.cs`, `CharacterManager.cs`, and `CharacterSetup.cs`: game-side adaptations for agent control, dynamic character binding, randomized characters, and episode resets.
- `GameManager.cs`: training-mode separation, soft round resets, terminal rewards, opponent rebinding, and telemetry lifecycle.

### 04 Demo and Inference

- `RLAgentDemoMenu.cs`: thesis demo interface, model selection, character selection, and demo-state initialization.
- `PvEMLBotRuntimeSetup.cs`: loads trained Barracuda/ONNX models for inference-only play.
- `PvESelection.cs`: runtime state shared by demo and gameplay setup.


## Presentation Topics Covered

- Simplified training environment and input-system refactoring
- Soft resets and training/runtime separation
- 67-dimensional observation space
- Nine discrete action branches and action masking
- PPO with self-play and dynamic opponent curriculum
- Iterative reward shaping and exploit prevention
- Runtime training safety and debugging
- Telemetry, ELO progression, and behavior analysis
- Shared-policy character generalization
- Demo-time ONNX inference

## Deliberate Exclusions

- Music, sound effects, sprites, animations, visual effects, and other game media
- Unity scenes, prefabs, complete character implementations, and proprietary game content
- Raw player profiles, raw telemetry, generated reports, and personal data
- Executable builds and trained model binaries
- Unrelated menus, customization systems, replay tools, and general game features
- Behavior-cloning experiments that are not part of the PPO thesis narrative

## Environment Reference

- Unity `2022.3.24f1`
- Unity ML-Agents package `2.0.2`
- Python `mlagents` / `mlagents-envs` `0.28.0` recorded in training logs
- Behavior name: `Fighter`
- Trainer: PPO
- Observation size: 67
- Discrete action branches: 9
- Desicion Period: 3

