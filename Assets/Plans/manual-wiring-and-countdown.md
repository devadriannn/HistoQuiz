# Project Overview
- Game Title: HistoQuiz
- High-Level Concept: An educational history quiz with real-time multiplayer.
- Objective: Wire manual UI designs to the logic and implement a 5-second countdown before the quiz begins for all students.

# Implementation Steps

## 1. Expose Script References (Inspector-Friendly)
- **TeacherDashboardController.cs**:
    - Change private fields (`dashboardPanel`, `navButtons`, `roomStatusText`, `startQuizButton`, etc.) to `[SerializeField] public`.
    - Update `ResolveSceneReferences()` to only search for objects if the serialized field is `null`.
- **TeacherDashboardDataManager.cs**:
    - Ensure metric fields (`studentsValueText`, `quizzesValueText`, etc.) are `[SerializeField]`.
    - Skip automatic "Find" logic in `ResolveUI()` if references are already assigned.

## 2. Implement 5-Second Quiz Countdown
- **MultiplayerQuizManager.cs (Student Side)**:
    - Modify `OnRoomUpdate` to start a `StartCountdownCoroutine` when `quizStarted` becomes true.
    - During the countdown, display a large overlay text (5, 4, 3, 2, 1).
    - Disable option buttons during the countdown.
    - Call `StartQuiz()` only after the countdown finishes.
- **TeacherDashboardController.cs (Teacher Side)**:
    - When `OnStartQuizClicked` is called, update the local UI to show a countdown as well, so the teacher knows when the quiz has actually started for the students.

## 3. User Instructions (How to Wire)
- Provide a guide on:
    - Where to find the `TeacherDashboardManager`.
    - Which UI objects to drag into each slot (e.g., "Total Students Value" text object).
    - How to create the "Countdown Text" object in the design.

# Verification & Testing
- Teacher clicks "Start Quiz".
- Both Teacher and Student screens show a 5-second countdown.
- After 5 seconds, the first question appears and students can answer.
- Verify the design (font, colors, positions) remains unchanged because no code is overriding the RectTransform properties.
