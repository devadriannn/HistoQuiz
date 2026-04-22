# Create Account UI - Unity Setup Guide

This guide walks you through setting up the Complete Create Account UI system with the `CreateAccountManager` script.

---

## Overview

The Create Account system consists of:
- **One Main Script**: `CreateAccountManager.cs` - Handles all validation, formatting, and UI logic
- **No Dynamic Prefabs**: All errors display in fixed TMP_Text fields assigned via Inspector
- **Real-time Validation**: Validates as user types
- **Profile Image Support**: Optional image with automatic default fallback
- **Email Duplication Check**: Prevents duplicate account registration

---

## Canvas Hierarchy Structure

Create this exact hierarchy in your Canvas:

```
Canvas (Create Account)
├── Panel (Background)
│   ├── TitleText (TMP_Text)
│   └── ScrollView (for the form)
│       └── Viewport
│           └── Content
│               ├── ProfileImageSection
│               │   ├── ProfileImagePreview (Image)
│               │   └── ChangeImageButton (Button) [Optional]
│               │       └── Text (TMP_Text)
│               │
│               ├── FullNameSection
│               │   ├── Label (TMP_Text) - "Full Name"
│               │   ├── FullNameInput (TMP_InputField)
│               │   └── NameErrorText (TMP_Text)
│               │
│               ├── EmailSection
│               │   ├── Label (TMP_Text) - "Email"
│               │   ├── EmailInput (TMP_InputField)
│               │   └── EmailErrorText (TMP_Text)
│               │
│               ├── PasswordSection
│               │   ├── Label (TMP_Text) - "Password"
│               │   ├── PasswordInput (TMP_InputField)
│               │   └── PasswordErrorText (TMP_Text)
│               │
│               ├── ConfirmPasswordSection
│               │   ├── Label (TMP_Text) - "Confirm Password"
│               │   ├── ConfirmPasswordInput (TMP_InputField)
│               │   └── ConfirmPasswordErrorText (TMP_Text)
│               │
│               ├── GeneralStatusText (TMP_Text)
│               │
│               └── CreateAccountButton (Button)
│                   └── Text (TMP_Text)
│
└── CreateAccountManager (GameObject) [Where you attach the script]
```

---

## Step-by-Step Setup

### Step 1: Create the Main Manager GameObject

1. In your Canvas, create a new empty GameObject named **CreateAccountManager**
2. Attach the **CreateAccountManager.cs** script to this GameObject
3. Select this GameObject to see its Inspector

### Step 2: Create UI Elements

#### 2.1 Create Profile Image Section

1. In Content, create a new Image element named **ProfileImagePreview**
   - Set it as a child of a panel or layout if needed
   - Set size: 150x150 (or your preferred size)
   - Component: **Image** (do NOT check "Raycast Target" if just for display)
   - Set a temporary placeholder image as the source image

2. In Content, create a Button named **ChangeImageButton** (optional)
   - Add a child Text (TMP_Text) with label "Change Picture"
   - This button can be wired later to open image picker

#### 2.2 Create Full Name Input Section

1. Create a new GameObject named **FullNameSection** as a child of Content
2. Add child elements:
   - **Label** (TMP_Text): Text = "Full Name"
   - **FullNameInput** (TMP_InputField):
     - Component Type: **TMP_InputField**
     - Input Type: Standard
     - Content Type: Standard
     - Placeholder: "Enter your full name"
   - **NameErrorText** (TMP_Text):
     - Color: Red (255, 0, 0)
     - Font Size: 20-22
     - Initial Text: "" (empty)

#### 2.3 Create Email Input Section

1. Create a new GameObject named **EmailSection** as a child of Content
2. Add child elements:
   - **Label** (TMP_Text): Text = "Email"
   - **EmailInput** (TMP_InputField):
     - Component Type: **TMP_InputField**
     - Input Type: Standard
     - Content Type: Email
     - Placeholder: "Enter your email"
   - **EmailErrorText** (TMP_Text):
     - Color: Red
     - Font Size: 20-22
     - Initial Text: "" (empty)

#### 2.4 Create Password Input Section

1. Create a new GameObject named **PasswordSection** as a child of Content
2. Add child elements:
   - **Label** (TMP_Text): Text = "Password"
   - **PasswordInput** (TMP_InputField):
     - Component Type: **TMP_InputField**
     - Input Type: Standard
     - Content Type: Password
     - Placeholder: "Enter password (8+ characters)"
   - **PasswordErrorText** (TMP_Text):
     - Color: Red
     - Font Size: 20-22
     - Initial Text: "" (empty)

#### 2.5 Create Confirm Password Input Section

1. Create a new GameObject named **ConfirmPasswordSection** as a child of Content
2. Add child elements:
   - **Label** (TMP_Text): Text = "Confirm Password"
   - **ConfirmPasswordInput** (TMP_InputField):
     - Component Type: **TMP_InputField**
     - Input Type: Standard
     - Content Type: Password
     - Placeholder: "Confirm password"
   - **ConfirmPasswordErrorText** (TMP_Text):
     - Color: Red
     - Font Size: 20-22
     - Initial Text: "" (empty)

#### 2.6 Create Status Text

1. Create a new TMP_Text element as a child of Content named **GeneralStatusText**
   - Color: Green (0, 200, 0) or Yellow (255, 200, 0)
   - Font Size: 24
   - Initial Text: "" (empty)
   - This displays success/general messages

#### 2.7 Create Create Account Button

1. Create a Button element named **CreateAccountButton** as a child of Content
   - Add a child Text (TMP_Text) with label "Create Account"
   - Set the button's interactable state to **false** (it will be enabled when all fields are valid)
   - Button colors: Normal (gray), Pressed (dark gray), Disabled (very dark gray)

### Step 3: Assign Components in Inspector

Select the **CreateAccountManager** GameObject and assign all references:

#### Input Fields Section
- **Full Name Input**: Drag the FullNameInput (TMP_InputField) here
- **Email Input**: Drag the EmailInput (TMP_InputField) here
- **Password Input**: Drag the PasswordInput (TMP_InputField) here
- **Confirm Password Input**: Drag the ConfirmPasswordInput (TMP_InputField) here

#### Profile Image Section
- **Profile Image Preview**: Drag the ProfileImagePreview (Image) here
- **Default Profile Sprite**: Drag a default avatar/profile image sprite here (or you can use a placeholder from Unity's built-in resources)

#### Error/Status Text Fields (CRITICAL - DO NOT LEAVE EMPTY)
- **Name Error Text**: Drag the NameErrorText (TMP_Text) here
- **Email Error Text**: Drag the EmailErrorText (TMP_Text) here
- **Password Error Text**: Drag the PasswordErrorText (TMP_Text) here
- **Confirm Password Error Text**: Drag the ConfirmPasswordErrorText (TMP_Text) here
- **General Status Text**: Drag the GeneralStatusText (TMP_Text) here

#### Button
- **Create Account Button**: Drag the CreateAccountButton (Button) here

#### Validation Settings (Optional - Pre-configured)
- **Min Name Length**: 2
- **Min Password Length**: 8
- **Existing Emails**: Pre-populated with sample emails ("test@email.com", "user@example.com", "admin@domain.com")

### Step 4: Wire Button Click

1. Select the **CreateAccountButton** (Button)
2. In Inspector, find the **Button** component
3. Under "On Click ()", click the "+" button
4. Drag the **CreateAccountManager** GameObject to the object field
5. In the function dropdown, select **CreateAccountManager > OnClickCreateAccount()**

### Step 5: (Optional) Set Default Profile Image

1. Find or import a default profile/avatar image
2. In your project's Assets folder, make sure it's imported as a **Sprite**
3. In the CreateAccountManager Inspector, drag this sprite to the **Default Profile Sprite** field

---

## Component Breakdown

### CreateAccountManager Script

**Serialized Fields (Inspector Assignments):**

| Field | Type | Purpose |
|-------|------|---------|
| fullNameInput | TMP_InputField | User's full name input |
| emailInput | TMP_InputField | User's email input |
| passwordInput | TMP_InputField | User's password input |
| confirmPasswordInput | TMP_InputField | Confirm password input |
| profileImagePreview | Image | Profile image display |
| defaultProfileSprite | Sprite | Default avatar when no image selected |
| nameErrorText | TMP_Text | Displays name validation errors |
| emailErrorText | TMP_Text | Displays email validation errors |
| passwordErrorText | TMP_Text | Displays password validation errors |
| confirmPasswordErrorText | TMP_Text | Displays confirm password errors |
| generalStatusText | TMP_Text | Displays success/general messages |
| createAccountButton | Button | Account creation button |
| minNameLength | int | Minimum name length (default: 2) |
| minPasswordLength | int | Minimum password length (default: 8) |
| existingEmails | List<string> | Sample existing emails for duplicate check |

---

## Validation Rules Summary

### Full Name
✓ Required  
✓ 2+ characters  
✓ Must not be only numbers  
✓ Allows: letters, spaces, apostrophe ('), hyphen (-)  
✓ Auto-capitalizes each word  
✗ No special characters  

**Example**: "juan dela cruz" → "Juan Dela Cruz"

### Email
✓ Required  
✓ Valid email format (something@domain.com)  
✓ No spaces  
✓ Must not already exist in system  
✗ Shows "This email already exists." if duplicate  

### Password
✓ Required  
✓ 8+ characters  
✓ Must contain uppercase letter  
✓ Must contain lowercase letter  
✓ Must contain number  
✓ Must contain special character (!@#$%^&* etc)  
✗ No spaces  

**Example Valid**: "MyPassword123!"  
**Example Invalid**: "mypassword" (no uppercase, number, special char)

### Confirm Password
✓ Required  
✓ Must exactly match Password field  

---

## Real-Time Validation Behavior

### How It Works

1. **As user types**, each input field's validation runs automatically
2. **Individual error messages** appear only in the corresponding error text field
3. **Create Account button**:
   - **Disabled** until ALL fields are valid
   - **Enabled** when all fields pass validation
4. **Name formatting** automatically triggers when user finishes editing (on End Edit)

### Validation Flow

```
User types in FullNameInput
    ↓
ValidateName() runs
    ↓
Check: Empty? Length? Only numbers? Invalid chars?
    ↓
If invalid → nameErrorText shows error message
If valid → nameErrorText shows "" (empty)
    ↓
isNameValid = true/false
    ↓
UpdateCreateButtonState() checks all fields
    ↓
Button enabled only if ALL fields valid
```

---

## Testing the System

### Test Case 1: Empty Fields
1. Don't enter anything
2. Result: Button should be DISABLED
3. All error texts should show "...is required."

### Test Case 2: Valid Input
1. Full Name: "John Doe"
2. Email: "john@example.com"
3. Password: "SecurePass123!"
4. Confirm Password: "SecurePass123!"
5. Result: Button should be ENABLED

### Test Case 3: Invalid Email (Duplicate)
1. Email: "test@email.com"
2. Result: emailErrorText shows "This email already exists."
3. Button should remain DISABLED

### Test Case 4: Weak Password
1. Password: "weak"
2. Result: passwordErrorText shows "Password must be at least 8 characters."

### Test Case 5: Name Auto-Capitalization
1. Enter: "juan dela cruz"
2. Press Tab/click away
3. Result: Name changes to "Juan Dela Cruz" automatically

---

## Backend Integration (Firebase/Custom Backend)

### Current Implementation
- Script uses a sample list of emails for testing
- OnClickCreateAccount() logs data to console

### To Connect to Firebase Authentication

In `OnClickCreateAccount()`, replace the TODO with:

```csharp
// Firebase Authentication Example
FirebaseAuth auth = FirebaseAuth.DefaultInstance;
email = emailInput.text.Trim().ToLower();
password = passwordInput.text;

auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task =>
{
    if (task.IsCanceled)
    {
        Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
        return;
    }
    if (task.IsFaulted)
    {
        Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
        generalStatusText.text = "Account creation failed. Please try again.";
        return;
    }

    // Account created successfully
    FirebaseUser newUser = task.Result;
    Debug.LogFormat("User created successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);
    generalStatusText.text = "Account created successfully!";
});
```

### To Add Email Duplication Check via Backend

Replace CheckIfEmailExists() with a backend query:

```csharp
private bool CheckIfEmailExists(string email)
{
    // Example: Query Firebase Realtime Database
    // return FirebaseDatabase.DefaultInstance.Reference
    //     .Child("users")
    //     .Child(email.Replace(".", ","))
    //     .GetValueAsync()
    //     .Result.Exists;

    // For now, checks local list
    return existingEmails.Contains(email.ToLower());
}
```

---

## Troubleshooting

### Problem: Button doesn't enable even when fields are filled
**Solution**: Make sure all error text fields are assigned in Inspector. Missing references prevent button state updates.

### Problem: Errors appear in wrong text fields
**Solution**: Double-check that each input's corresponding error text is assigned correctly in Inspector.

### Problem: Name doesn't auto-capitalize
**Solution**: Make sure fullNameInput and nameErrorText are assigned. Check that OnNameEndEdit is registered in OnEnable().

### Problem: Create Account button click does nothing
**Solution**: 
1. Check that createAccountButton is assigned
2. Make sure OnClickCreateAccount() is wired to the button in Inspector
3. Verify all validation fields are assigned

### Problem: Profile image doesn't show default
**Solution**: 
1. Make sure defaultProfileSprite is assigned in Inspector
2. Check that profileImagePreview Image component exists
3. Verify Start() runs (check console for errors)

---

## Performance Notes

- ✓ Validation is extremely fast (regex comparisons)
- ✓ No instantiation of prefabs
- ✓ No UI redraws unless text changes
- ✓ Scalable to hundreds of concurrent users

---

## Security Considerations

### Client-Side Validation
- ✓ Provides immediate feedback to user
- ✗ NOT sufficient for actual security
- ✗ Always validate on backend as well

### Backend Validation Checklist
- [ ] Validate all inputs server-side
- [ ] Use HTTPS for all communications
- [ ] Never transmit passwords in plain text
- [ ] Hash passwords using bcrypt or similar
- [ ] Implement rate limiting on account creation
- [ ] Verify email ownership before activation
- [ ] Use prepared statements to prevent SQL injection

---

## Customization Options

### Change Validation Rules

In Inspector or in code:

```csharp
[SerializeField] private int minNameLength = 3; // Changed from 2
[SerializeField] private int minPasswordLength = 10; // Changed from 8
```

### Add More Validation Requirements

For example, to require a phone number:

1. Add `TMP_InputField phoneNumberInput;` to script
2. Add `TMP_Text phoneErrorText;` to script
3. Create `ValidatePhoneNumber()` method
4. Register listener: `phoneNumberInput.onValueChanged.AddListener(OnPhoneValueChanged);`
5. Update `ValidateAllFields()` to include phone

### Change Error Message Colors

In Inspector:
1. Select each error text element
2. In TMP_Text component, change Color to red, orange, etc.

---

## Summary

Your Create Account UI is now complete with:
- ✓ Fixed error text fields (no dynamic prefabs)
- ✓ Real-time validation
- ✓ Auto-capitalize name
- ✓ Profile image with default fallback
- ✓ Email duplication check
- ✓ Strong password requirements
- ✓ Button state management
- ✓ Clean, beginner-friendly code
- ✓ Ready for backend integration

Happy coding!
