# RL-Agent Thesis

This repository contains curated portfolio material for the thesis **Development of Reinforcement Learning Agent for Custom 2D Action Game**.

The thesis work studies a reinforcement-learning opponent for the custom Unity arena fighter **D.I.E.N.A.M.O.** The agent was trained with Unity ML-Agents using PPO and runs in inference mode inside the Unity game environment.

## Contents

- `docs/Thesis_Report.pdf` - final thesis report.
- `docs/RL_Agent_Thesis_Presentation_English.pptx` - thesis presentation slides.
- `src/selected/` - selected source files related to the RL agent, training curriculum, environment integration, and demo inference.
- `models/` - selected ONNX models used for thesis evaluation and comparison.
- `media/` - short GIF demonstrations without audio.
- `demo/README_DEMO.txt` - instructions for the playable Windows demo build.
- `demo/DIENAMO-RL-Agent-Demo-Windows.zip` - playable Windows demo build, tracked with Git LFS.

The playable demo build is large, so it is tracked with Git LFS rather than normal Git storage.

## Recommended Reading Order

1. Read `docs/Thesis_Report.pdf` or skim the presentation.
2. Watch the GIFs in `media/` to understand the environment and agent behavior.
3. Review the selected source code in `src/selected/`.
4. Inspect the trained models in `models/`.
5. Download and extract `demo/DIENAMO-RL-Agent-Demo-Windows.zip` to run the playable Windows demo.

## Technical Summary

- Engine: Unity `2022.3.24f1`
- ML framework: Unity ML-Agents
- Trainer: PPO
- Behavior name: `Fighter`
- Observation vector: 67 values
- Action space: 9 discrete action branches
- Primary evaluated model: `AgentSmith2.2.onnx`
- Runtime mode: inference only

## Relationship To The Game Repository

This is a focused thesis repository. The full game project is maintained separately:

https://github.com/dufenshmirtz/Head-of-Hell

The game repository contains the Unity project and broader D.I.E.N.A.M.O. development history. This thesis repository keeps the academic/agent material easier to review.

## Demo Distribution Note

The demo build is useful for private review and employer evaluation, but it should be distributed thoughtfully because it may contain development-stage game assets and placeholder media. If the project is made fully public, review media and asset licenses first.

## License

No open-source license is granted for this repository. The material is provided for academic, portfolio, and review purposes only. Do not reuse code, models, media, or game assets without permission from the author/project team.

