# Basic Runtime Building System

### Buildable Prefab Hierarchy
- Pivot root
  - Child with BuildingSystemObject, Renderer, Collider, Rigidbody
    - Other child game objects, Renderer, Collider
  - Other child game objects

### Usage
- Create buildable item with prefab reference manually or with generator script
- Call BuildingSystem SetBuildObject(ItemBuildable) or ItemBuildable SingleClick()

### Todo
- Prefabs, example
- Contextual buildable snapping
- Configurable buildable itemization
- Optimizations (material, baking, lighting, etc)

