# Project Overview
- Game Title: HistoQuiz
- Goal: Strip the Teacher Dashboard scripts of all automatic UI generation code and transition to a fully manual, Inspector-driven wiring system. Implement a 5-second quiz countdown.

# Implementation Steps

## 1. Purge Automatic Design from TeacherDashboardController.cs
- **Actions**:
    - Delete `BuildSections()`, `ConfigurePanelFrame()`, `ConfigureBottomNav()`, and all helper methods (`CreateCard`, `CreateText`, `CreatePageContent`, etc.).
    - Change private UI fields to `[SerializeField]` (HomeBtn, QuizzesBtn, HomeSection, MultiplayerSection, RoomCodeText, etc.).
    - Simplify `Awake()` to only call `ResolveSceneReferences()` and `WireBottomNavButtons()`.
    - Update `ShowSection()` to simply toggle the serialized section GameObjects.

## 2. Purge Automatic Design from TeacherDashboardDataManager.cs
- **Actions**:
    - Remove `PopulateSimpleList()`, `CreateRow()`, and `CreateText()` methods.
    - Ensure all metric displays are `[SerializeField]`.
    - Update the main data loop to only update text values of existing, manually placed UI elements.

## 3. Implement 5-Second Countdown (MultiplayerQuizManager.cs)
- **Actions**:
    - Create a `StartCountdownCoroutine` that triggers when the teacher starts the quiz.
    - Display a countdown overlay (5, 4, 3, 2, 1) on both Teacher and Student screens.
    - Delay the actual question display until the timer hits zero.

## 4. Final Wiring Instructions
- Provide the user with a list of components to drag into the Inspector slots.
- Explain that the design is now 100% controlled in the Editor, and the scripts only handle the data and functionality.

# Verification & Testing
- The Inspector for `TeacherDashboardManager` should now show several empty slots for Buttons, Sections, and Text.
- Dragging manual UI objects into these slots and entering Play Mode should result in a working dashboard with the user's custom design.
- Starting a multiplayer session should trigger a 5-second countdown before the first question.
