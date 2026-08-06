# Rahal

Rahal is a cross-platform travel guide application developed in Unity using C#. The application allows users to create an account, log in, explore places in Cairo, London and Paris, filter places by category, like places, view liked and uploaded places in a profile, open locations in Google Maps and upload new places with images.

The app uses Firebase Authentication for user registration and login, Cloud Firestore for storing place and user data, Cloudinary for storing uploaded images, and Native Gallery for selecting images from the user’s device.

## Third-Party Plugins and Services

- Firebase Authentication
- Cloud Firestore
- Firebase Unity SDK
- Native Gallery
- Cloudinary
- External Dependency Manager for Unity

## How to Run

1. Clone or download the repository.
2. Open the project using Unity 6.
3. Allow Unity to restore packages and dependencies.
4. Ensure the Firebase configuration files are available in the Assets folder.
5. Open the Login scene.
6. Add all required scenes to the Unity Build Profile.
7. Run inside Unity or build for Android.
8. For iOS, export the Unity project and compile it using Xcode on macOS.