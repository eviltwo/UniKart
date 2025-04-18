# UniKart (In Development)
UniKart is a simple asset that includes a controller for driving a kart. It allows smooth movement over slopes, and supports jumping and drifting. Works with Unity 6.

UniKart is open source and available for free under the MIT License on this GitHub repository. If you'd like to support me, please consider visiting my [Asset Store](https://assetstore.unity.com/publishers/12117) page.

![UniKart](https://github.com/user-attachments/assets/c714b856-5db4-46be-a484-2cf3b0862fc7)

# Features
- **Basic Actions**
  - Moves forward in the Kart's Z-axis direction.
  - Supports jumping, drifting, and boosting.
  - External physical forces can be applied.
- **Simple Physics Components**
  - Uses only a Rigidbody and a SphereCollider.
  - Does not execute Physics.Raycast().
- **Smooth Movement**
  - Automatically detects slopes and adjusts rotation accordingly.
  - Implements custom interpolation to ensure smooth motion, even during acceleration and drifting.
- **Proper Camera Behavior**
  - Camera movement is tuned based on commercial kart racing games.
  - The horizon remains level (no rotation on the roll axis), reducing motion sickness.

![kart_collider](https://github.com/user-attachments/assets/cf3ffd87-918f-46f4-98a3-2d1ff827880a)

# Support My Work
I’m a solo indie developer. Your financial support is greatly appreciated and helps me continue working on this project.
- [Asset Store](https://assetstore.unity.com/publishers/12117)
- [Steam](https://store.steampowered.com/curator/45066588)
- [GitHub Sponsors](https://github.com/sponsors/eviltwo)
