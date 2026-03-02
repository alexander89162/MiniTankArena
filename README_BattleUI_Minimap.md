# BattleUI + Minimap Quick Guide

This project includes a BattleUI minimap system with:
- Rotating radar/compass overlay
- 2D minimap mode
- Top-down terrain texture mapping
- Terrain texture generator tool

## 1) Setup BattleUI in a scene

In Unity:
1. Open your target scene.
2. Go to **Tools -> Battle UI -> Setup In Active Scene**.
3. This creates/updates BattleUI and minimap wiring in the current scene.

## 2) Use 2D minimap mode

Select the BattleUI object (with `HUDController`) and configure:
- `Use 2D Minimap` = true
- `Minimap Size` as desired (default 500x500)

When 2D mode is enabled, the minimap camera feed is disabled and the UI map texture is used.

## 3) Generate top-down terrain map texture

In Unity:
1. Open a scene that has an active Terrain.
2. Go to **Tools -> Battle UI -> Generate Top-Down Minimap Texture**.
3. Choose a save path under `Assets/`.

The tool will:
- Bake terrain into a top-down texture
- Include height + slope shading for displacement readability
- Import the texture with minimap-friendly settings
- Auto-assign it to all `MinimapRadarOverlay` components in the scene
- Force `use2DMinimap` on for scene HUD controllers

## 4) Minimap overlay controls

On `MinimapRadarOverlay` you can tune:
- Top-down world bounds and zoom
- Map rotation with player
- Compass ring ticks/labels
- Cardinal letter styling and placement
- Overlay readability profile

## 5) Typical workflow

1. Run **Setup In Active Scene** once.
2. Run **Generate Top-Down Minimap Texture** whenever terrain changes significantly.
3. Press Play and adjust `MinimapRadarOverlay` + `HUDController` inspector values.
