# Video Demonstrations

This folder contains supplementary video demonstrations for the thesis:

**Development of Reinforcement Learning Agent for Custom 2D Action Game**

The videos are included to provide clearer examples of the trained agent's behavior and the game mechanics.

## Important Note

In all gameplay videos, the trained RL agent controls **Player 2**, who is positioned on the **right side** of the screen.

Player 1 is positioned on the **left side**.

## Why These Videos Are Included

The standalone demo allows the user to play directly against the trained agent. However, this can be difficult to evaluate properly without prior knowledge of the game mechanics, controls, characters, and combat system.

In addition, agent-vs-agent matches are not always representative of the intended gameplay experience, since they can sometimes produce unusual or less natural behaviors. For this reason, this folder includes selected videos that provide more controlled and understandable examples of the final result.

These videos are meant to support the demo by showing:

* the basic game mechanics,
* complete human-vs-agent rounds,
* representative behavior of the trained agent,
* and one slow-motion example of a fast, complex decision sequence.

## Files

### `Game_Tutorial.gif`

This file presents the basic gameplay mechanics and controls of the game.

It is included to help the viewer understand the environment before evaluating the trained agent.

### `Human_vs_Agent_Round1.mp4`

This file shows a complete round between a human player and the trained RL agent.

The agent is **Player 2**, on the **right side** of the screen.

### `Human_vs_Agent_Round2.mp4`

This file shows a second complete round between a human player and the trained RL agent.

It provides an additional example of the agent's behavior during normal gameplay conditions.

The agent is **Player 2**, on the **right side** of the screen.

### `Decision_Complexity_Sample.gif`

This file shows a selected gameplay moment in slow motion, with explanation.

It highlights a fast and relatively complex decision sequence performed by the trained agent. The purpose of this example is to make the agent's decision-making easier to observe, since the same event happens very quickly during normal gameplay.

## Recommended Viewing Order

1. `Game_Tutorial.gif`
2. `Human_vs_Agent_Round1.mp4`
3. `Human_vs_Agent_Round2.mp4`
4. `Decision_Complexity_Sample.gif`

## Notes

These videos are not part of the training process itself. They are provided only as supplementary demonstration material for understanding and evaluating the final trained RL agent.

The final trained agent shown in these videos runs in inference mode. No training is performed during the recordings.
