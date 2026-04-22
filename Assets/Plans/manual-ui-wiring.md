# Project Overview
- Game Title: HistoQuiz
- High-Level Concept: An educational quiz game about Philippine history with real-time multiplayer features.
- Goal: Enable the user to "Wire" their manual UI design to the functional scripts without the scripts overriding the layout.

# Implementation Steps

## 1. Modify TeacherDashboardController.cs
- **Objective**: Expose UI references to the Inspector so the user can drag-and-drop their custom-designed objects.
- **Actions**:
    - Change private fields for Panels, Buttons, and Texts to `[SerializeField]`.
    - Update `ResolveSceneReferences()` to respect existing references. It will only search for objects if the field is empty.
    - Ensure `Awake` and `Start` logic does not force-change RectTransform values (Anchors, Offsets) if they are already set.

## 2. Modify TeacherDashboardDataManager.cs
- **Objective**: Allow manual wiring of metric displays (e.g., Total Students, Quizzes Created).
- **Actions**:
    - Ensure all metric `TMP_Text` fields are `[SerializeField]`.
    - Update `ResolveUI()` to skip searching if a reference is already provided in the Inspector.

## 3. Instruction Guide for Wiring
- **Objective**: Teach the user how to connect their design to the logic.
- **Details**:
    - Identify the `TeacherDashboardManager` GameObject.
    - Drag the manual `HomeBtn`, `QuizzesBtn`, etc. into the respective slots.
    - Drag the `RoomCodeDisplay` (Text) and `StartQuizBtn` into the Multiplayer slots.
    - Explain that the script will now use these objects to update text from Firestore/Firebase.

# Verification & Testing
- The user will drag their objects into the script components.
- In Play Mode, the script should update the text of the *dragged* objects instead of searching or creating new ones.
- The design layout (size, position, color) should remain exactly as the user set it in the Editor.
