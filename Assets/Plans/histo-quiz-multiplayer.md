# Project Overview
- **Game Title:** HistoQuiz
- **High-Level Concept:** A real-time multiplayer historical quiz tool for classrooms. Teachers generate quizzes from PDF uploads using AI (OpenAI), and students join rooms via code to compete in live sessions with a real-time leaderboard.
- **Players:** Single-player Teacher (Dashboard/Host), 30+ Students (Multiplayer Clients).
- **Inspiration / Reference Games:** Kahoot!, Quizizz.
- **Tone / Art Direction:** Classic / Educational (Clean, professional, high readability).
- **Target Platform:** Android (Mobile-first).
- **Screen Orientation:** Auto Rotation (Optimized for Landscape on tablets/desktops for teachers, Portrait for students).
- **Render Pipeline:** Universal Render Pipeline (URP).

# Game Mechanics
## Core Gameplay Loop
1. **Teacher:** Uploads a historical text/PDF.
2. **AI Integration:** OpenAI API processes the text into Multiple Choice Questions (MCQs).
3. **Room Creation:** Teacher generates a unique 6-digit room code.
4. **Students:** Join the room using the code and "Ready Up".
5. **Live Quiz:** Teacher starts the session; questions are synced across all devices.
6. **Live Leaderboard:** Real-time ranking updates as students answer, showing scores and time taken.

## Controls and Input Methods
- **Touch UI:** All interactions are via buttons and input fields (uGUI).
- **Network Sync:** Real-time data synchronization via Firebase Realtime Database.

# UI
- **Teacher Dashboard:** Updated to include "Create Room" and "Multiplayer Monitor".
- **Student Lobby:** Input field for Room Code and "Ready" button.
- **Quiz Interface:** Question text at the top, 4 large choice buttons below, and a progress bar.
- **Leaderboard Overlay:** Live ranking list showing Name, Score, and Speed.

# Key Asset & Context
- **Scripts:**
    - `FirebaseRoomManager.cs`: Handles room creation, joining, and state (Waiting/Playing).
    - `OpenAIQuizService.cs`: Interfaces with OpenAI API to generate JSON quiz data from PDF text.
    - `MultiplayerQuizController.cs`: Manages the quiz flow (Question 1 -> Question 2) and syncing.
    - `RealtimeLeaderboard.cs`: Listens to Firebase updates for student scores.
- **Prefabs:**
    - `RoomCodePanel`: UI for entering/displaying room codes.
    - `LeaderboardRow`: Template for displaying a student's rank.

# Implementation Steps
## Phase 1: Multiplayer Foundation (Day 1)
1. **Firebase Realtime Database Setup:**
    - Integrate `Firebase.Database` SDK (if missing).
    - Configure security rules for room-based access.
2. **Room System Implementation in `FirebaseRoomManager`:**
    - Create Room: Generate code, set status to `waiting`.
    - Join Room: Add student data to `rooms/{code}/students`.
    - Ready Check: Track `ready` status for each student.

## Phase 2: AI Quiz Generation (Day 2)
1. **PDF Text Extraction:**
    - Implement simple text extraction or placeholder for PDF data.
2. **OpenAI API Integration:**
    - Implement `OpenAIQuizService` to send text and receive structured JSON quizzes.
    - Store generated quizzes in Firestore/Realtime DB linked to the Room ID.
    - *Dependency: FirebaseRoomManager (Phase 1)*

## Phase 3: Real-time Quiz Flow (Day 3)
1. **Syncing the Start:**
    - Teacher triggers `quizStarted = true` in Firebase.
    - Students' `MultiplayerQuizController` detects the change and loads the first question.
2. **Scoring & Leaderboard:**
    - Students write `score` and `timestamp` to their student node on each answer.
    - `RealtimeLeaderboard` on the Teacher dashboard listens to the `students` node for live updates.
    - *Dependency: Phase 1 & 2*

## Phase 4: UI Integration & Polishing (Day 4)
1. **UI Wiring:**
    - Connect `TeacherDashboard` buttons to the Room System.
    - Create the `StudentRoomJoin` scene/UI.
2. **Testing:**
    - Simulate 30+ connections using multiple editor instances or devices.
    - Verify AI quiz accuracy and synchronization latency.

# Verification & Testing
- **Room Join Test:** Verify that entering a code correctly adds the student to the correct Firebase node.
- **AI Payload Test:** Log the JSON response from OpenAI to ensure it matches the `Question` class structure.
- **Sync Test:** Start the quiz on one device and ensure all other connected devices move to the first question within <500ms.
- **Stress Test:** Verify the leaderboard doesn't flicker or crash with 30 concurrent updates.
