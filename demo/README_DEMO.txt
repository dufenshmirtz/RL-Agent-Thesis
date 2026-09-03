RL Agent Thesis Demo
====================

What this demo shows
--------------------
This demo lets a human Player 1 play a normal one-round match against Player 2 controlled by the trained reinforcement learning agent. There is also an option that lets the user watch agent vs agent matches. The model runs with Unity ML-Agents inference only.

The demo menu is in:
Assets/Scenes/RLAgentDemo.unity

The RLAgentDemoMenu component exposes AgentSmith2.0, AgentSmith2.2, AgentSmith3.2.

How to run the build
--------------------
Click on the RL-Agent_Demo shortcut inside this folder (if it doesn't work use the .exe file inside the RL_Demo_Files folder).

How to play against the agent
-----------------------------
1. Launch the build.
2. Pick Agent 1 Level and Agent 2 Level with their on-screen arrows.
3. Pick the Player 1 character and the Agent character with the on-screen arrows.
4. Select "Play vs Final RL Agent" for human vs agent, or "Watch Agent vs Agent" to let both players run inference.
5. In human vs agent, Player 1 is human-controlled and Player 2 uses Agent 2 Level.
6. In agent vs agent, Player 1 uses Agent 1 Level and Player 2 uses Agent 2 Level.

Controls
--------
Move: A / D
Jump: W
Drop: S
Quick Attack: U
Heavy Attack: I
Block: O
Special: P
Charge: J
Pause: Esc
Quick restart during a match: Enter
Return to demo menu during a demo match: Backspace

Gameplay rules
--------------
The demo applies the default gameplay rules: 100 HP, one round, normal arena, normal controls, no training mode, and no external trainer.

Technical note
--------------
This demo allows the user to play against 3 final reinforcement learning agents developed for the thesis. Later models generally represent later training runs, but the best model depends on the game version and evaluation context. The model was trained with PPO using Unity ML-Agents. The demo selects PvE ML Agent mode, fixes the bot to Player 2, and uses BehaviorParameters in InferenceOnly mode.
Agent-vs-agent mode can use different selected model slots for Player 1 and Player 2 and does not run training.

The model used for the human evaluation and most of the report statistics is AgentSmith2.2.

Disclaimers
--------------
- The game is still in development, so some animations are missing or incomplete.
- Agent-vs-agent behavior can appear chaotic because both agents may produce similar responses and actions.
- AgentSmith3.2 is a later model trained after changes in the game environment, so it is not guaranteed to perform better than AgentSmith2.2 in every comparison.
- The demo may include temporary placeholder music used during development. That music is not owned by the author/project team and is not licensed for reuse.
- The RL_Demo_Files folder has some filenames derived from the game that formed the basis of the project.
