# Create Account Manager - Quick Inspector Reference

## TL;DR - What Goes Where

### In the CreateAccountManager Script Inspector

```
INPUT FIELDS SECTION
├─ Full Name Input          → FullNameInput (TMP_InputField)
├─ Email Input              → EmailInput (TMP_InputField)
├─ Password Input           → PasswordInput (TMP_InputField)
└─ Confirm Password Input   → ConfirmPasswordInput (TMP_InputField)

PROFILE IMAGE SECTION
├─ Profile Image Preview    → ProfileImagePreview (Image)
└─ Default Profile Sprite   → [Your Avatar Image Sprite]

ERROR/STATUS TEXT FIELDS (DO NOT LEAVE EMPTY!)
├─ Name Error Text              → NameErrorText (TMP_Text) [RED]
├─ Email Error Text             → EmailErrorText (TMP_Text) [RED]
├─ Password Error Text          → PasswordErrorText (TMP_Text) [RED]
├─ Confirm Password Error Text  → ConfirmPasswordErrorText (TMP_Text) [RED]
└─ General Status Text          → GeneralStatusText (TMP_Text) [GREEN]

BUTTON
└─ Create Account Button    → CreateAccountButton (Button)
```

---

## Inspector Setup Checklist

- [ ] **Full Name Input** field assigned
- [ ] **Email Input** field assigned
- [ ] **Password Input** field assigned (Content Type: Password)
- [ ] **Confirm Password Input** field assigned (Content Type: Password)
- [ ] **Profile Image Preview** assigned (Image component)
- [ ] **Default Profile Sprite** assigned (your avatar thumbnail)
- [ ] **Name Error Text** assigned (TMP_Text, RED color)
- [ ] **Email Error Text** assigned (TMP_Text, RED color)
- [ ] **Password Error Text** assigned (TMP_Text, RED color)
- [ ] **Confirm Password Error Text** assigned (TMP_Text, RED color)
- [ ] **General Status Text** assigned (TMP_Text, GREEN color)
- [ ] **Create Account Button** assigned (Button component)
- [ ] Button Click wired to CreateAccountManager.OnClickCreateAccount()

---

## How to Drag References

1. **Open the Scene** with your CreateAccountManager GameObject
2. **Select CreateAccountManager** in the Hierarchy
3. **Open Inspector** (right side of screen)
4. **For each field:**
   - Look at the field name in the script (e.g., "Full Name Input")
   - Find the corresponding UI element in the Hierarchy (e.g., "FullNameInput")
   - **Drag and drop** the Hierarchy element into the Inspector field

### Example: Assigning Full Name Input

```
BEFORE:
┌─ CreateAccountManager Inspector ─┐
│ Full Name Input:     [    None    ] ◄─ Empty
└───────────────────────────────────┘

Hierarchy:
├─ Canvas
│  ├─ FullNameSection
│  │  └─ FullNameInput ◄─ Grab this

AFTER:
┌─ CreateAccountManager Inspector ──────────┐
│ Full Name Input:  [✓ FullNameInput]  ◄─ Assigned
└────────────────────────────────────────────┘
```

---

## Validation Rules at a Glance

### Full Name ✓
- Required
- 2+ characters
- Letters, spaces, apostrophes, hyphens only
- Auto-capitalizes: "juan" → "Juan"
- Error shown in: **nameErrorText**

### Email ✓
- Required
- Valid format: something@domain.com
- No spaces
- Not already registered
- Error shown in: **emailErrorText**

### Password ✓
- Required
- 8+ characters
- Uppercase: A-Z
- Lowercase: a-z
- Number: 0-9
- Special char: !@#$%^&*() etc
- No spaces
- Error shown in: **passwordErrorText**

### Confirm Password ✓
- Required
- Matches password exactly
- Error shown in: **confirmPasswordErrorText**

---

## Real-Time Behavior

| Event | Action |
|-------|--------|
| User types in input | Validation runs immediately |
| Validation fails | Error appears in corresponding error text |
| Validation passes | Error text clears |
| ALL fields valid | Create Account button becomes clickable |
| ANY field invalid | Create Account button stays disabled |

---

## Button State Examples

| State | Button | Appearance |
|-------|--------|-----------|
| Fields empty | Disabled | Grayed out, not clickable |
| Fields partially valid | Disabled | Grayed out, not clickable |
| All fields valid | Enabled | Full color, clickable |
| After clicking (success) | Disabled | Temporarily disabled |

---

## Testing Colors

Set these in the TMP_Text Inspector for testing:

- **Error Messages** (nameErrorText, emailErrorText, etc.):
  - Color: Red `RGB(255, 0, 0)` or `#FF0000`
  - Font Size: 20-22
  
- **Success Message** (generalStatusText):
  - Color: Green `RGB(0, 200, 0)` or `#00C800`
  - Font Size: 24

---

## Public Methods (for external use)

### Call These from Other Scripts

```csharp
// Reset all fields to empty
createAccountManager.ResetAllFields();

// Update profile image when user selects one
createAccountManager.OnSelectProfileImage(selectedSprite);

// Reset profile to default
createAccountManager.ResetProfileImageToDefault();

// Add newly created email to prevent re-registration
createAccountManager.AddEmailToExistingList("newemail@example.com");
```

---

## Common Mistakes to Avoid

❌ **Mistake**: Leaving error text fields as "None" in Inspector
✓ **Fix**: Assign all 5 error/status text fields

❌ **Mistake**: Using "standard" content type for password fields
✓ **Fix**: Set password inputs to Content Type: **Password**

❌ **Mistake**: Forgetting to wire the button click
✓ **Fix**: In Button component, add OnClick listener → CreateAccountManager.OnClickCreateAccount()

❌ **Mistake**: Making default profile sprite optional
✓ **Fix**: Always assign a default avatar sprite

❌ **Mistake**: Not using TextMeshPro components
✓ **Fix**: Use TMP_InputField and TMP_Text, not regular Text

---

## Quick Test Script (Optional)

If you want to test from Play Mode:

```csharp
// In a test script, you can manually call validations:
CreateAccountManager manager = GetComponent<CreateAccountManager>();

// All validations happen automatically when user types
// No need to call them manually unless you're debugging
```

---

## Integration with Other Features

### Add Profile Image Picker Button

```csharp
public void OnChangeProfileImageClicked()
{
    // Open image picker (use NativeGallery or FileBrowser plugin)
    // Then call:
    createAccountManager.OnSelectProfileImage(selectedImageSprite);
}
```

### Connect to Firebase

See the full Setup Guide for Firebase authentication code examples.

### Add Phone Number Validation

1. Add to script: `TMP_InputField phoneInput;` and `TMP_Text phoneErrorText;`
2. Create method: `ValidatePhoneNumber()`
3. Register listener and add to ValidateAllFields()

---

## Support Commands

**If something isn't working:**

1. Check console for errors: `Ctrl+Shift+C` (in Play Mode)
2. Verify all references in Inspector: Should show no "None" values
3. Check that input fields have correct Content Type
4. Verify error text fields have visible colors and font sizes
5. Make sure button is wired to OnClickCreateAccount()

---

Done! Your Create Account system is ready to use. 🎉
