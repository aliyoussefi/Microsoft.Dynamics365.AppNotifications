# Microsoft.Dynamics365.AppNotifications

# Summary

This tool will create notification records which will be displayed in an environment that has In-App Notifications turned on.

# Definitions

BroadcastAppNotification - This loops through all Read/Write users and create a notification.

# Setup

Begin by activating In-App Notifications by following these instructions:

[Send in-app notifications within model-driven apps - Power Apps | Microsoft Docs](https://docs.microsoft.com/en-us/powerapps/developer/model-driven-apps/clientapi/send-in-app-notifications)

## Enable in-app notification feature

To use the in-app notification feature, you need enable the `AllowNotificationsEarlyAccess` app setting in model-driven app.

1. Sign in to your model-driven app.
2. Select the app where you want to use this feature.
3. Select **F12** button on your keyboard to open the browser console.
4. In the browser console, copy the code below. Enter your app unique name in the `AppUniqueName` parameter. Press **Enter**.

```
fetch(window.origin + "/api/data/v9.1/SaveSettingValue()",{ method: "POST",    headers: {'Content-Type': 'application/json'},   body: JSON.stringify({AppUniqueName: "Your app unique name", SettingName:"AllowNotificationsEarlyAccess", Value: "true"})   });
```

## Enable Security Privilege

Users will need the **prvReadAppNotification** 

# Contribute

This is a proof of concept but happy to have contributions to make the tool better. :)
