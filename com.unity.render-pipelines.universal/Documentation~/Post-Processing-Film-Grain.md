# Film Grain

The Film Grain effect simulates the random optical texture of photographic film, usually caused by small particles being present on the physical film.

## Using Film Grain

**Film Grain** uses the [Volume](Volumes.html) framework, so to enable and modify **Film Grain** properties, you must add a **Film Grain** override to a [Volume](Volumes.html) in your Scene. To add **Film Grain** to a Volume:

1. In the Scene or Hierarchy view, select a GameObject that contains a Volume component to view it in the Inspector.
2. In the Inspector, navigate to **Add Override > Post-processing** and click on **Film Grain**. Universal RP now applies **Film Grain** to any Camera this Volume affects.

## Properties

![](Images/Post-processingFilmGrain1.png)

| **Property**  | **Description**                                              |
| ------------- | ------------------------------------------------------------ |
| **Type**      | Use the drop-down to select the type of grain to use. You can select from a list of presets that Universal RP includes, or select **Custom** to provide your own grain Texture. |
| **Texture**   | Assign a Texture that this effect uses as a custom grain Texture.This property is only available when **Type** is set to **Custom**. |
| **Intensity** | Use the slider to set the strength of the Film Grain effect. |
| **Response**  | Use the slider to set the noisiness response curve. The higher you set this value, the less noise there is in brighter areas. |