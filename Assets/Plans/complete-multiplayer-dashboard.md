# Project Overview
- Game Title: HistoQuiz
- High-Level Concept: An educational quiz game about Philippine history with real-time multiplayer classroom features and AI question generation.
- Players: Single player (Students) / Teacher-led multiplayer.
- Target Platform: Android (Mobile).
- Render Pipeline: URP.

# Game Mechanics
## Core Gameplay Loop
- Teacher generates room code.
- Students join via code.
- Teacher starts quiz.
- Live leaderboard updates as students answer.

# UI
## Multiplayer Section Content
- **Room Status Card**: Shows the active room code and status. Includes a button to generate the code.
- **Session Metrics**: Quick view of how many students are connected and how many questions are ready.
- **Live Student List**: A scrollable list showing the names of students who have joined the room.
- **Start Button**: Large button to begin the quiz session.

# Key Asset & Context
- `MultiplayerSection`: The parent container for the multiplayer UI.
- `TeacherDashboardController.cs`: The script that manages the logic for this section.
- `RealtimeLeaderboard`: Component used to update the student list.

# Implementation Steps
1. **Build Multiplayer UI Hierarchy**: Create the cards, text displays, buttons, and scroll view inside `MultiplayerSection` using a `CommandScript`.
2. **Update TeacherDashboardController**:
    - Update `ResolveSceneReferences` to link to the new manual UI objects.
    - Implement a `WireMultiplayerButtons` method to connect the buttons to the logic.
3. **Ensure Compatibility**: Follow the existing project aesthetic (Cream/Brown palette).

# Verification & Testing
- Enter Play Mode.
- Navigate to Multiplayer tab.
- Click "Generate Room Code" and verify `RoomCodeDisplay` updates (requires Firebase connectivity).
- Verify student list placeholder appears.
