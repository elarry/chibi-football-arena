# Chibi Football Arena

A Unity-based environment for training AI football players and simulating matches between models. Find out how good your AI football coaching abilities are. 

![Chibi Football Arena](docs/chibi-gameplay-dynamic-camera.gif)


See also companion projects:
- [Chibi Football Lab](https://github.com/elarry/chibi-football-lab): Train and simulate football players using this project's Unity environment.
- [Chibi Football Player](https://github.com/elarry/chibi-football-player): Player model and animation using Blender.


## What Is This?

A Unity environment in which to use the power of Reinforcement Learning to implant little AI brains into tiny Chibi characters so that they can play football. You train neural network agents using [Unity ML-Agents](https://github.com/Unity-Technologies/ml-agents). Once you have a stable of trained models, you can pit them against each other in head-to-head matches to find out whose AI dominates all others. For more on how to use this environment to train players and simulate matches, see [Chibi Football Lab](https://github.com/elarry/chibi-football-lab)

Based on the ML-Agents example environment [Soccer](https://github.com/Unity-Technologies/ml-agents/tree/develop/Project/Assets/ML-Agents/Examples/Soccer), and inspired by the [Deep Reinforcement Learning Course from Hugging Face](https://huggingface.co/learn/deep-rl-course/en/unit7/hands-on).

## Features

- **Training Environment**: Run self-play training sessions where agents learn football through trial, error, and a lot of own goals
- **Model-vs-Model Matches**: Load any two trained `.onnx` models and watch them compete; great for settling arguments about whose training config was superior
- **Animated Chibi Players**: Replaces the default blocks with animated football players
- **Cinemachine-driven Camera**: Dynamic match camera with ball tracking that can be toggled with the space bar
- **Scoreboard**: On-screen blue and purple score display for match viewing and evaluation


## Project Structure

```
Chibi-Football/Assets/
├── Editor/
│   └── BuildAutomation.cs     # Builds the training, evaluation, and inference executables
├── Scripts/
│   ├── AgentSoccer.cs          # The brain (such as it is) of each player
│   ├── SoccerEnvController.cs  # Referee, scorekeeper, chaos coordinator
│   ├── SoccerBallController.cs # The ball, which agents will largely ignore early in training
│   ├── GameConfig.cs           # Compile-time build behavior switches
│   ├── ScoreManager.cs         # Keeps score so you don't have to
│   └── ModelOverrider.cs       # Swap in different .onnx models at runtime
├── ML-Agents/
│   └── Examples/Soccer/TFModels/  # Pre-trained baseline example models
└── Scenes/
    └── Soccer-Chibi.unity      # The main event
```

## Relationship to SoccerTwos

This project starts from the default [Unity ML-Agents SoccerTwos]((https://github.com/Unity-Technologies/ml-agents/tree/develop/Project/Assets/ML-Agents/Examples/Soccer) environment. The core training setup is still recognizable: two teams, two agents per side, discrete movement actions, ball and goal rewards, and behavior names compatible with ML-Agents self-play workflows.

Chibi Football Arena adds the presentation, evaluation, and packaging layer needed to make trained policies easier to watch and compare:

- Animated football player models replace the default simple agent bodies.
- A Cinemachine match camera can track the ball and is controlled by the space bar.
- A scoreboard displays the blue and purple team scores during matches.
- `ModelOverrider.cs` can load model files at executable startup, including two different models so evaluation and inference builds can pit policies against each other.
- The Unity build menu produces separate training, evaluation, and inference executables instead of a single generic player.
- The player rigidbody damping was changed from the default SoccerTwos value of `3` to `7` in the scene so the animated running looks more natural. This makes random exploratory movement less twitchy, but it can also affect training efficiency, especially early in training when agents rely heavily on random exploration.

## Getting Started

### Prerequisites

- Unity 6.5
- [ML-Agents Python package](https://github.com/Unity-Technologies/ml-agents) (`pip install mlagents`)


### Training and Simulation 

See companion repo [Chibi Football Lab](https://github.com/elarry/chibi-football-lab) for detailed instructions on how to train and simulate matches.

Within the Unity editor, you can also simulate matches by drag-and-dropping pre-trained `.onnx` models directly onto the players' `Behavior Parameters > Model` field. Then just press play!  


## Build Variants

The build automation creates three executable types for both macOS and Linux:

- `football-training`: Development build with `TRAINING_BUILD` defined. `GameConfig.cs` keeps training-oriented timing fast and uses the `SoccerTwos` self-play behavior name. Build accepts only a single `.onnx` model intended to be used for self-training with `ml-agents`.
- `football-evaluation`: Development build with `EVALUATION_BUILD` defined. It keeps fast evaluation timing, but disables the self-play behavior name so that you can supply two different models for direct comparison.
- `football-inference`: Release-style build with no extra scripting define. It uses viewer-friendly timing and delayed exit behavior for watching model-vs-model matches.

All three build types use the same scene, `Assets/Scenes/Soccer-Chibi.unity`. The differences come from `Assets/Editor/BuildAutomation.cs`, which sets build options and scripting defines, and `Assets/Scripts/GameConfig.cs`, which reads those definitions to choose runtime behavior.


## Build Automation

Builds are driven from the Unity editor menu items in `Assets/Editor/BuildAutomation.cs`:

- `Build/Build All` builds all macOS and Linux variants.
- `Build/MacOS/...` builds only macOS variants.
- `Build/Linux/...` builds only Linux variants.

Outputs are written under `Builds/MacOS` and `Builds/Linux` with names matching the build variant. Before each build, the script temporarily changes objects tagged `TrainingOnly`. For training and evaluation builds, those objects are retagged to `Untagged` so they are included. For inference builds, they are retagged to `EditorOnly`, which causes Unity to exclude them from the player. The script saves the scene for the build pipeline, then restores the original tags afterward.

This tag workflow is how the project keeps extra training fields available for high-throughput training while producing a smaller, cleaner inference build. If you want to manually exclude an object or field from a build, set its tag to `EditorOnly` before building.


## Runtime Model Loading

`Assets/Scripts/ModelOverrider.cs` extends the ML-Agents example model override logic so executable builds can load models from disk. The executable reads command-line arguments such as:

```bash
--mlagents-override-model-directory /path/to/models
--mlagents-override-model-extension onnx
--mlagents-quit-after-episodes 9
--mlagents-quit-after-seconds 60
```

The override directory is expected to contain model files named after the agents' behavior names. This lets evaluation and inference runs pass different trained models into the executable and have the blue and purple teams use those models directly, without reopening the Unity editor.


## Why ML-Agents Release 20

This project intentionally keeps the older ML-Agents Release 20 Unity package, `com.unity.ml-agents` C# `2.3.0-exp.4`, through the local packages listed in `Chibi-Football/Packages/manifest.json`. That release still uses Barracuda and allows `.onnx` files to be loaded directly by the executable.

Newer ML-Agents releases, including Release 23, moved inference toward Sentis and deprecated Barracuda. With that path, executable-side model loading expects Sentis models rather than raw ONNX files. At the moment, ONNX-to-Sentis conversion is still awkward enough that the manual conversion step would make the model-vs-model executable workflow much more cumbersome.

Another option is to interact with the executable only through `ml-agents`, letting the Python package handle communication and policy loading. That works, but it means inference runs depend on the ML-Agents Python stack, not just training runs. This project keeps direct ONNX loading so built executables remain easier to run for standalone evaluation and match viewing. [Chibi Football Lab](https://github.com/elarry/chibi-football-lab) includes examples for running both types of simulations--using executables directly and via `ml-agents`.

## License

See [LICENSE](LICENSE).
