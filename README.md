# TextBuddy SDK for Unity

The TextBuddy SDK allows Unity developers to integrate SMS-based user subscription and deep link-based verification flows into their games and applications. It provides a clean API to initiate user signup, handle responses from deep links, and manage subscription status persistently across sessions.

---

## Installation

### Option: Install via Unity Package Manager (Git URL)

1. Open your Unity project.
2. Go to **Window > Package Manager**.
3. Click the **+** button (top-left) and choose **"Add package from Git URL..."**
4. Paste the following Git URL:

```
https://github.com/umair6/TextBuddy-SDK-Umair.git
```

5. Click **Add**.


**Note:** This SDK uses TextMeshPro.  
If it's not already enabled in your project, Unity will prompt you to import it.  
You can also manually install it by going to **Window > Package Manager**, searching for **TextMeshPro**, and clicking **Install**.


After installation:
- A `TextBuddyConfig` asset will be created automatically at:  
  `Assets/TextBuddy/Configs/TextBuddyConfig.asset`

To configure your SDK, go to the **Unity Editor top bar** and select:  
**TextBuddy > Configure**

This will open the config asset where you can enter:
- `TextBuddy Game ID`
- `TextBuddy API Key`
- Enable or disable logs

The GameObject with the `TextBuddySDK` script should be present in the scene, or created at runtime via a prefab. It will persist across scenes automatically.

---

## Usage Guide

### Initialization

Before using the SDK, initialize it:

```csharp
TextBuddySDK.Instance.InitialiseTextBuddy();
```

This will:
- Load the configuration
- Initialize internal states
- Start listening to deep links

You should call this in your main menu or entry point scene.

---

### Subscribe User

To initiate the SMS-based signup flow:

```csharp
TextBuddySDK.Instance.Subscribe();
```

This will:
- Send an SMS to the predefined number (configured via `textBuddyDemoPhoneNumber`)
- Set SDK state to `Pending`
- Wait for user to return via deep link (which will confirm subscription)

---

### Unsubscribe User

To unsubscribe a user:

```csharp
TextBuddySDK.Instance.UnSubscribe();
```

This will:
- Send an unsubscribe SMS
- Trigger unsubscribe event

---

## SDK Properties

These public getters allow you to check SDK and user status:

| Property                         | Description                                                                 |
|----------------------------------|-----------------------------------------------------------------------------|
| `IsInitialized()`               | Returns `true` if SDK has been initialized successfully                     |
| `IsUserSubscribed()`            | Returns `true` if user is currently subscribed                              |
| `IsUserSignupInProgress()`      | Returns `true` if user signup is currently pending                          |
| `TextBuddyUserID()`             | Returns the unique ID of the subscribed user (empty if unsubscribed)        |

Example:

```csharp
if (TextBuddySDK.Instance.IsUserSubscribed())
{
    Debug.Log("User is already subscribed with ID: " + TextBuddySDK.Instance.TextBuddyUserID());
}
```

---

## Events

The SDK uses Unity-style events so you can hook into key subscription lifecycle points:

```csharp
TextBuddySDK.OnSDKInitialized += YourInitHandler;
TextBuddySDK.OnUserSubscribed += YourSubscribeSuccessHandler;
TextBuddySDK.OnUserUnSubscribed += YourUnsubscribeHandler;
TextBuddySDK.OnUserSubscribeFail += YourSubscribeFailureHandler;
```

| Event Name                   | Description                                                          |
|-----------------------------|----------------------------------------------------------------------|
| `OnSDKInitialized`          | Fired when the SDK finishes initializing                             |
| `OnUserSubscribed`          | Fired when the user has successfully subscribed via deep link        |
| `OnUserUnSubscribed`        | Fired when the user has been unsubscribed                            |
| `OnUserSubscribeFail`       | Fired if subscription fails, passes an error message (`string`)      |

---

## Deep Link Handling

To ensure subscription confirmation works:
- The app must be launched via the deep link sent to the user in SMS.
- SDK will listen to `Application.deepLinkActivated`.

---

## Example Flow

```csharp
void Start()
{
    TextBuddySDK.OnSDKInitialized += OnTextBuddyInitialized;
    TextBuddySDK.Instance.InitialiseTextBuddy();
}

void OnTextBuddyInitialized()
{
    if (!TextBuddySDK.Instance.IsUserSubscribed())
        TextBuddySDK.Instance.Subscribe();
}
```

---

## File Structure

```
Assets/
└── TextBuddy/
    ├── Core/
    │   └── TextBuddySDK.cs
    ├── Configs/
    │   └── TextBuddyConfig.asset
    └── Editor/
```

---

## License

This SDK is provided under the MIT License. You may use it freely in commercial and non-commercial projects.

---

## Support

For integration help, bug reports, or feature requests, please contact the TextBuddy support team at:  
Email: support@textbuddy.io  
Website: https://www.textbuddy.io
