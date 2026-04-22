using System;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class FirebaseManager : MonoBehaviour
{
    private static FirebaseManager instance;

    private readonly List<Action<bool, string>> initializationCallbacks = new List<Action<bool, string>>();
    private bool isInitializing;

    public static FirebaseManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<FirebaseManager>();
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("FirebaseManager");
                    instance = managerObject.AddComponent<FirebaseManager>();
                }
            }

            return instance;
        }
    }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseFirestore Firestore { get; private set; }
    public bool IsReady { get; private set; }
    public string LastError { get; private set; }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
            return;
        }

        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeFirebase(Action<bool, string> onInitialized = null)
    {
        if (IsReady)
        {
            onInitialized?.Invoke(true, null);
            return;
        }

        if (onInitialized != null)
        {
            initializationCallbacks.Add(onInitialized);
        }

        if (isInitializing)
        {
            return;
        }

        isInitializing = true;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            isInitializing = false;

            if (task.IsCanceled || task.IsFaulted)
            {
                LastError = ExtractTaskError(task.Exception, "Firebase initialization failed.");
                FlushInitializationCallbacks(false, LastError);
                return;
            }

            if (task.Result != DependencyStatus.Available)
            {
                LastError = "Firebase dependencies are unavailable: " + task.Result;
                FlushInitializationCallbacks(false, LastError);
                return;
            }

            Auth = FirebaseAuth.DefaultInstance;
            Firestore = FirebaseFirestore.DefaultInstance;
            IsReady = true;
            LastError = null;
            FlushInitializationCallbacks(true, null);
        });
    }

    public void UpdateStudentProfile(string name, string email, string photoUrl, Action<bool, string> onCompleted)
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            onCompleted?.Invoke(false, "User is not logged in.");
            return;
        }

        WithInitialization(onCompleted, () =>
        {
            string userId = Auth.CurrentUser.UserId;
            
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "name", name },
                { "email", email },
                { "photoUrl", photoUrl }
            };

            // Update Firestore Document
            Firestore.Collection("users").Document(userId).SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(firestoreTask =>
            {
                if (firestoreTask.IsCanceled || firestoreTask.IsFaulted)
                {
                    onCompleted?.Invoke(false, ExtractTaskError(firestoreTask.Exception, "Failed to update profile in database."));
                    return;
                }

                onCompleted?.Invoke(true, null);
                });
                });
                }

                public void UpdatePassword(string oldPassword, string newPassword, Action<bool, string> onCompleted)
                {
                if (Auth == null || Auth.CurrentUser == null)
                {
                onCompleted?.Invoke(false, "User is not logged in.");
                return;
                }

                Credential credential = EmailAuthProvider.GetCredential(Auth.CurrentUser.Email, oldPassword);

                Auth.CurrentUser.ReauthenticateAsync(credential).ContinueWithOnMainThread(reauthTask =>
                {
                if (reauthTask.IsCanceled || reauthTask.IsFaulted)
                {
                onCompleted?.Invoke(false, ExtractTaskError(reauthTask.Exception, "Incorrect old password."));
                return;
                }

                Auth.CurrentUser.UpdatePasswordAsync(newPassword).ContinueWithOnMainThread(updateTask =>
                {
                if (updateTask.IsCanceled || updateTask.IsFaulted)
                {
                    onCompleted?.Invoke(false, ExtractTaskError(updateTask.Exception, "Failed to update password."));
                    return;
                }

                onCompleted?.Invoke(true, null);
                });
                });
                }

                public void CheckEmailExists(string email, Action<bool, string> onCompleted)
    {
        WithInitialization(onCompleted, () =>
        {
            string trimmedEmail = (email ?? string.Empty).Trim();
            Firestore.Collection("users")
                .WhereEqualTo("email", trimmedEmail)
                .Limit(1)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
                {
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        onCompleted?.Invoke(false, ExtractTaskError(task.Exception, "Database query failed."));
                        return;
                    }

                    QuerySnapshot snapshot = task.Result;
                    onCompleted?.Invoke(snapshot != null && snapshot.Count > 0, null);
                });
        });
    }

    public void SendPasswordReset(string email, Action<bool, string> onCompleted)
    {
        WithInitialization(onCompleted, () =>
        {
            string trimmedEmail = (email ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedEmail))
            {
                onCompleted?.Invoke(false, "Email is required.");
                return;
            }

            Auth.SendPasswordResetEmailAsync(trimmedEmail).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onCompleted?.Invoke(false, ExtractTaskError(task.Exception, "Failed to send password reset email."));
                    return;
                }

                onCompleted?.Invoke(true, null);
            });
        });
    }

    public void SignInWithIdentifier(string identifier, string password, Action<FirebaseUser, string> onCompleted)
    {
        WithInitialization(onCompleted, () =>
        {
            string trimmedIdentifier = (identifier ?? string.Empty).Trim();
            if (trimmedIdentifier.Contains("@"))
            {
                SignInWithEmail(trimmedIdentifier, password, onCompleted);
                return;
            }

            ResolveUsernameToEmail(trimmedIdentifier, (email, error) =>
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    onCompleted?.Invoke(null, error);
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    onCompleted?.Invoke(null, "Username not found in database.");
                    return;
                }

                SignInWithEmail(email, password, onCompleted);
            });
        });
    }

    public void RegisterStudent(string firstName, string lastName, string role, string username, string email, string password, string photoUrl, Action<FirebaseUser, string> onCompleted)
    {
        WithInitialization(onCompleted, () =>
        {
            string trimmedFirstName = (firstName ?? string.Empty).Trim();
            string trimmedLastName = (lastName ?? string.Empty).Trim();
            string trimmedRole = (role ?? "student").Trim().ToLowerInvariant();
            string trimmedUsername = (username ?? string.Empty).Trim();
            string trimmedEmail = (email ?? string.Empty).Trim();

            Auth.CreateUserWithEmailAndPasswordAsync(trimmedEmail, password).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onCompleted?.Invoke(null, ExtractTaskError(task.Exception, "Registration failed."));
                    return;
                }

                FirebaseUser user = task.Result.User;

                // Send Email Verification
                user.SendEmailVerificationAsync().ContinueWithOnMainThread(verifyTask =>
                {
                    if (verifyTask.IsCanceled || verifyTask.IsFaulted)
                    {
                        Debug.LogWarning("Failed to send verification email.");
                    }
                });

                Dictionary<string, object> userData = new Dictionary<string, object>
                {
                    { "firstName", trimmedFirstName },
                    { "lastName", trimmedLastName },
                    { "name", trimmedFirstName + " " + trimmedLastName },
                    { "username", trimmedUsername },
                    { "usernameLower", trimmedUsername.ToLowerInvariant() },
                    { "email", trimmedEmail },
                    { "role", trimmedRole },
                    { "status", "pending" }, // Start as pending
                    { "points", 0 },
                    { "coins", 0 },
                    { "photoUrl", photoUrl ?? string.Empty }
                };

                Firestore.Collection("users").Document(user.UserId).SetAsync(userData).ContinueWithOnMainThread(saveTask =>
{
                    if (saveTask.IsCanceled || saveTask.IsFaulted)
                    {
                        onCompleted?.Invoke(null, ExtractTaskError(saveTask.Exception, "Failed to save profile."));
                        return;
                    }

                    onCompleted?.Invoke(user, null);
                });
            });
        });
    }

    public void LoadUserDocument(string userId, Action<DocumentSnapshot, string> onCompleted)
    {
        WithInitialization(onCompleted, () =>
        {
            Firestore.Collection("users").Document(userId).GetSnapshotAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onCompleted?.Invoke(null, ExtractTaskError(task.Exception, "Failed to load user data."));
                    return;
                }

                DocumentSnapshot snapshot = task.Result;
                if (snapshot == null || !snapshot.Exists)
                {
                    onCompleted?.Invoke(null, "No Firestore user document found for this account.");
                    return;
                }

                onCompleted?.Invoke(snapshot, null);
            });
        });
    }

    public void UpdateScore(int newScore, Action<bool, string> onCompleted = null)
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            onCompleted?.Invoke(false, "User is not logged in.");
            return;
        }

        WithInitialization(onCompleted, () =>
        {
            string userId = Auth.CurrentUser.UserId;
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "points", newScore },
                { "lastUpdated", FieldValue.ServerTimestamp }
            };

            Firestore.Collection("users").Document(userId).SetAsync(updates, SetOptions.MergeAll).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onCompleted?.Invoke(false, ExtractTaskError(task.Exception, "Failed to update points."));
                    return;
                }

                onCompleted?.Invoke(true, null);
            });
        });
    }

    public void MarkQuestionCompleted(string questionId, Action<bool, string> onCompleted = null)
{
        if (Auth == null || Auth.CurrentUser == null || string.IsNullOrWhiteSpace(questionId))
        {
            onCompleted?.Invoke(false, "Cannot mark question completed.");
            return;
        }

        WithInitialization(onCompleted, () =>
        {
            string userId = Auth.CurrentUser.UserId;
            DocumentReference doc = Firestore.Collection("users").Document(userId);
            
            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "completedQuestions", FieldValue.ArrayUnion(questionId) }
            };

            doc.UpdateAsync(updates).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onCompleted?.Invoke(false, ExtractTaskError(task.Exception, "Failed to update completed questions."));
                    return;
                }

                onCompleted?.Invoke(true, null);
            });
        });
    }

    public void ResolveUsernameToEmail(string username, Action<string, string> onCompleted)
    {
        WithInitialization(onCompleted, () =>
        {
            string trimmedUsername = (username ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(trimmedUsername))
            {
                onCompleted?.Invoke(null, "Username is required.");
                return;
            }

            QueryUsernameField("usernameLower", trimmedUsername.ToLowerInvariant(), (email, error) =>
            {
                if (!string.IsNullOrWhiteSpace(error))
                {
                    onCompleted?.Invoke(null, error);
                    return;
                }

                if (!string.IsNullOrWhiteSpace(email))
                {
                    onCompleted?.Invoke(email, null);
                    return;
                }

                QueryUsernameField("username", trimmedUsername, onCompleted);
            });
        });
    }

    public void SignOut()
    {
        if (Auth != null)
        {
            Auth.SignOut();
        }
    }

    public void GetLeaderboard(int limit, Action<List<Dictionary<string, object>>, string> onCompleted)
    {
        if (Auth == null || Auth.CurrentUser == null)
        {
            Debug.LogWarning("GetLeaderboard: User is not authenticated in Firebase. Attempting initialization check...");
        }

        WithInitialization(onCompleted, () =>
        {
            if (Auth.CurrentUser == null)
            {
                onCompleted?.Invoke(null, "Firebase Auth: No user signed in. Cannot fetch leaderboard with current security rules.");
                return;
            }

            Debug.Log($"Fetching leaderboard for user: {Auth.CurrentUser.UserId}");

            Firestore.Collection("users")
                .OrderByDescending("points")
                .Limit(limit)
                .GetSnapshotAsync()
                .ContinueWithOnMainThread(task =>
{
                    if (task.IsCanceled || task.IsFaulted)
                    {
                        string error = ExtractTaskError(task.Exception, "Failed to load leaderboard.");
                        Debug.LogError($"Leaderboard Query Error: {error}");
                        onCompleted?.Invoke(null, error);
                        return;
                    }

                    QuerySnapshot snapshot = task.Result;
                    List<Dictionary<string, object>> leaderboard = new List<Dictionary<string, object>>();
                    foreach (DocumentSnapshot document in snapshot.Documents)
                    {
                        if (document.Exists)
                        {
                            Dictionary<string, object> data = document.ToDictionary();
                            data["userId"] = document.Id; // Store document ID as userId
                            leaderboard.Add(data);
                        }
                    }
onCompleted?.Invoke(leaderboard, null);
                });
        });
    }

    private void SignInWithEmail(string email, string password, Action<FirebaseUser, string> onCompleted)
    {
        Auth.SignInWithEmailAndPasswordAsync((email ?? string.Empty).Trim(), password ?? string.Empty).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                string error = ExtractTaskError(task.Exception, "Email or password is incorrect.");
                onCompleted?.Invoke(null, error);
                return;
            }

            onCompleted?.Invoke(task.Result.User, null);
        });
    }

    private void QueryUsernameField(string fieldName, string value, Action<string, string> onCompleted)
    {
        Firestore.Collection("users")
            .WhereEqualTo(fieldName, value)
            .Limit(1)
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled || task.IsFaulted)
                {
                    onCompleted?.Invoke(null, "Username login requires Firestore read access.");
                    return;
                }

                QuerySnapshot snapshot = task.Result;
                if (snapshot == null || snapshot.Count == 0)
                {
                    onCompleted?.Invoke(null, null);
                    return;
                }

                foreach (DocumentSnapshot document in snapshot.Documents)
                {
                    if (document == null || !document.Exists)
                    {
                        continue;
                    }

                    string email = document.ContainsField("email") ? document.GetValue<string>("email") : string.Empty;
                    onCompleted?.Invoke(string.IsNullOrWhiteSpace(email) ? null : email.Trim(), null);
                    return;
                }

                onCompleted?.Invoke(null, null);
            });
    }

    private void WithInitialization<T>(Action<T, string> onCompleted, Action onReady)
    {
        InitializeFirebase((ready, error) =>
        {
            if (!ready)
            {
                onCompleted?.Invoke(default(T), string.IsNullOrWhiteSpace(error) ? "Firebase is not ready." : error);
                return;
            }

            onReady?.Invoke();
        });
    }

    private void FlushInitializationCallbacks(bool success, string error)
    {
        for (int i = 0; i < initializationCallbacks.Count; i++)
        {
            initializationCallbacks[i]?.Invoke(success, error);
        }

        initializationCallbacks.Clear();
    }

    private static string ExtractTaskError(AggregateException exception, string fallback)
    {
        if (exception == null)
        {
            return fallback;
        }

        // Flatten and search through all inner exceptions for a FirebaseException
        var flattened = exception.Flatten();
        foreach (var inner in flattened.InnerExceptions)
        {
            if (inner is FirebaseException firebaseEx)
            {
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                Debug.LogWarning($"Firebase Error: {errorCode} (Code: {firebaseEx.ErrorCode}) - {firebaseEx.Message}");

                switch (errorCode)
                {
                    case AuthError.UserNotFound:
                        return "Email is not registered yet.";
                    case AuthError.EmailAlreadyInUse:
                        return "This email is already registered.";
                    case AuthError.WrongPassword:
                        return "Email and password doesn't match.";
                    case AuthError.InvalidEmail:
                        return "The email address is badly formatted.";
                    case AuthError.UserDisabled:
                        return "This account has been disabled.";
                    case AuthError.TooManyRequests:
                        return "Too many attempts. Try again later.";
                    case AuthError.Failure:
                        // 'Failure' is often the generic 'Internal Error' in the Editor
                        return "Email and password doesn't match.";
                    default:
                        // If we have a specific message from Firebase that isn't 'Internal Error', use it
                        if (!string.IsNullOrWhiteSpace(firebaseEx.Message) && !firebaseEx.Message.ToLower().Contains("internal error"))
                            return firebaseEx.Message;
                        break;
                    }
                    }
                    }

                    // Fallback to the first inner exception message if found
                    Exception firstInner = flattened.InnerException;
                    if (firstInner != null)
                    {
                    string msg = firstInner.Message;
                    if (msg.ToLower().Contains("internal error"))
                    return "Email and password doesn't match.";
                    return msg;
                    }

        return fallback;
    }
}
