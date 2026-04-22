# Project Overview
- Game Title: HistoQuiz
- Objective: Convert Teacher Dashboard scripts to be purely functional (manual UI) and implement a 5-second quiz start countdown.

# Implementation Steps

## 1. Purge UI Generation from TeacherDashboardController.cs
- **Actions**:
    - Convert all private UI fields (Buttons, TMP_Texts, RectTransforms) to `[SerializeField]`.
    - Delete `BuildSections()`, `ConfigurePanelFrame()`, `ConfigureBottomNav()`, and all helper methods that create UI elements.
    - Update `ResolveSceneReferences()` to only search for objects if the serialized field is `null`.
    - Update `ShowSection()` to use a `List<GameObject>` or a simple toggle of serialized fields.

## 2. Purge UI Generation from TeacherDashboardDataManager.cs
- **Actions**:
    - Remove `PopulateSimpleList()`, `CreateRow()`, and `CreateText()` methods.
    - Update `UpdateDashboardUI()` to only refresh the text values of existing UI elements linked in the Inspector.

## 3. Implement 5-Second Countdown in MultiplayerQuizManager.cs
- **Actions**:
    - Add a `[SerializeField] GameObject countdownOverlay`.
    - Add a `[SerializeField] TMP_Text countdownText`.
    - Modify `OnRoomUpdate` to trigger a `StartCountdownCoroutine` when `quizStarted` is detected.
    - The coroutine will count down from 5 to 1, then hide the overlay and call `StartQuiz()`.

## 4. Final Wiring Instructions for the User
- Provide a summary of which objects to drag into the Inspector slots for `TeacherDashboardManager`.

# Verification & Testing
- Teacher clicks "Start Quiz".
- Countdown 5...4...3...2...1 appears.
- Quiz begins only after the countdown.
- Design remains 100% manual as per the user's Editor layout.
